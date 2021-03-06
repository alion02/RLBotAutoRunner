﻿using System;
using System.IO;
using System.Collections.Generic;

namespace RLBotAutoRunner
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                    throw new ArgumentException($"Received {args.Length} arguments, expected 1. Usage: 'RLBotAutoRunner.exe <path to configuration file>'");

                // TODO: Detect and report invalid config
                var config = new INIParser(args[0], INIMode.SpacedEquals);
                var botFilter = config["Autorunner Configuration", "header_filter"];
                var teamSize = int.Parse(config["Autorunner Configuration", "team_size"]);
                var breakLengthParsed = double.TryParse(config["Autorunner Configuration", "break_length"], out var breakLengthSeconds);
                Enum.TryParse(config["Autorunner Configuration", "size_override"], out SizeOverride sizeOverride);
                var tourneyTypeString = config["Tournament Configuration", "type"];

                var matchRunner = new MatchRunner(config, breakLengthParsed ? new TimeSpan((long)(breakLengthSeconds * TimeSpan.TicksPerSecond)) : TimeSpan.Zero);
                var tourneyType = Enum.TryParse(tourneyTypeString, out Tournament.Type type) ? type : throw new InvalidDataException($"Configuration specified tournament type '{tourneyTypeString}', which doesn't exist.");

                Console.WriteLine($"Searching for metadata files containing section '[{botFilter}]'...\n");
                var teams = new List<Team>();
                foreach (var file in Directory.EnumerateFiles(@".\", "*.metadata", SearchOption.AllDirectories))
                {
                    var dir = Path.GetDirectoryName(file);
                    string FullPath(string path) => Path.GetFullPath(Path.Combine(dir, path));

                    var ini = new INIParser(file, INIMode.SpacedEquals);
                    Console.WriteLine($"Found metadata file '{file}'. Showing stripped contents:");
                    Console.WriteLine(ini.ToString(INIMode.SpacedEquals));

                    var name = ini[botFilter, "team"];
                    var cfgs = ini[botFilter, "cfgs"];
                    var sel = ini[botFilter, "cfg_selection"];
                    var res = ini[botFilter, "resources"];
                    var size = ini[botFilter, "size"];

                    if (name?.Length > 0 && cfgs?.Length > 0)
                    {
                        // TODO: Error handling
                        Console.WriteLine($"Metadata file contains appropriate section. Adding '{name}' to list of participants...");

                        var parsedRes = new List<IUniqueResource>();
                        foreach (var r in res != null ? res.Split(';') : new string[0])
                            if (UniqueResource.TryParse(FullPath(r), out var p))
                                parsedRes.Add(p);

                        var fullCfgs = cfgs.Split(';');
                        for (int i = 0; i < fullCfgs.Length; ++i)
                            fullCfgs[i] = FullPath(fullCfgs[i]);

                        teams.Add(new Team(
                            name,
                            int.TryParse(size, out var desiredSize) && (sizeOverride == SizeOverride.Any || (sizeOverride == SizeOverride.HandicapOnly && desiredSize < teamSize)) ? desiredSize : teamSize,
                            GetCfgPool(Enum.TryParse(sel, out CfgSelectionMode desiredSel) ? desiredSel : CfgSelectionMode.SequentialRepeat, fullCfgs),
                            parsedRes
                        ));
                    }
                }
                
                Tournament.Run(teams.ToArray(), matchRunner, tourneyType, config);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        static IEnumerable<string> GetCfgPool(CfgSelectionMode mode, params string[] cfgs)
        {
            IEnumerable<string> FirstOnly()
            {
                while (true)
                    yield return cfgs[0];
            }
            IEnumerable<string> Sequential()
            {
                for (int i = 0; i < cfgs.Length; ++i)
                    yield return cfgs[i];
            }
            IEnumerable<string> RandomUnique()
            {
                int left = cfgs.Length;
                var cfgCopy = new string[left];
                cfgs.CopyTo(cfgCopy, 0);
                while (left > 0)
                {
                    int i = Program.Random.Next(left);
                    yield return cfgCopy[i];
                    cfgCopy[i] = cfgCopy[--left];
                }
            }
            IEnumerable<string> Random()
            {
                yield return cfgs[Program.Random.Next(cfgs.Length)];
            }
            IEnumerable<string> Repeat(IEnumerable<string> source)
            {
                while (true)
                    foreach (var cfg in source)
                        yield return cfg;
            }
            IEnumerable<string> Throw(IEnumerable<string> source)
            {
                foreach (var cfg in source)
                    yield return cfg;

                throw new InvalidOperationException("A request was made for more configs than provided by entry.");
            }
            switch (mode)
            {
                case CfgSelectionMode.FirstOnly:
                    return FirstOnly();
                case CfgSelectionMode.SequentialRepeat:
                    return Repeat(Sequential());
                case CfgSelectionMode.SequentialThrow:
                    return Throw(Sequential());
                case CfgSelectionMode.RandomUniqueRepeat:
                    return Repeat(RandomUnique());
                case CfgSelectionMode.RandomUniqueThrow:
                    return Throw(RandomUnique());
                case CfgSelectionMode.Random:
                    return Random();
                default:
                    throw new NotImplementedException();
            }
        }

        enum SizeOverride
        {
            Disabled,
            HandicapOnly,
            Any
        }

        enum CfgSelectionMode
        {
            FirstOnly,
            SequentialRepeat,
            SequentialThrow,
            RandomUniqueRepeat,
            RandomUniqueThrow,
            Random,
            CustomRepeat,
            CustomThrow
        }

        public static readonly Random Random = new Random();
    }
}
