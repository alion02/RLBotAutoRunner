using System;
using System.Collections.Generic;

namespace RLBotAutoRunner
{
    public static class Tournament
    {
        public static IEnumerable<MatchResult> Run(Team[] teams, MatchRunner runner, Type type, INIParser config)
        {
            Program.Random.Shuffle(teams);
            switch (type)
            {
                case Type.RoundRobin:
                    for (int i = 0; i < teams.Length; ++i)
                        for (int j = i + 1; j < teams.Length; ++j)
                            yield return runner.Run(teams[i], teams[j]);
                    break;

                case Type.Gauntlet:
                    var challengerName = config["Tournament Config", "challenger"];
                    var challengerIndex = Array.FindIndex(teams, t => t.Name == challengerName);
                    var challenger = teams[challengerIndex];

                    for (int i = 0; i < challengerIndex; ++i)
                        yield return runner.Run(challenger, teams[i]);
                    for (int i = challengerIndex + 1; i < teams.Length; ++i)
                        yield return runner.Run(challenger, teams[i]);
                    break;

                default:
                    break;
            }
        }

        public enum Type
        {
            RoundRobin,
            SingleElim,
            DoubleElim,
            Swiss,
            Gauntlet,
            Random,
            Manual
        }
    }
}
