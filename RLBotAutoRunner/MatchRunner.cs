using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RLBotAutoRunner
{
    public class MatchRunner
    {
        public MatchRunner(INIParser baseIni, TimeSpan? breakLength = null)
        {
            this.baseIni = baseIni;
            this.breakLength = breakLength ?? TimeSpan.Zero;
        }

        private readonly INIParser baseIni;
        private readonly TimeSpan breakLength;

        public MatchResult Run(Team blue, Team orange)
        {
            Console.WriteLine($"Starting match: {blue.Name} vs {orange.Name}");

            int players = blue.Size + orange.Size;
            var teams = new[] { blue, orange };
            baseIni.Append("Team Configuration", ("Team Blue Name", blue.Name), ("Team Orange Name", orange.Name));
            baseIni["Match Configuration", "num_participants"] = players.ToString();

            int num = -1;
            int teamNum = -1;
            var vals = new List<(string, string)>();
            foreach (var team in teams)
            {
                ++teamNum;
                var cfgGetter = team.CfgPool.GetEnumerator();
                for (int i = 0; i < team.Size; ++i)
                {
                    ++num;
                    cfgGetter.MoveNext();
                    vals.Add(($"participant_config_{num}", cfgGetter.Current));
                    vals.Add(($"participant_team_{num}", teamNum.ToString()));
                    vals.Add(($"participant_type_{num}", "rlbot"));
                }

                foreach (var res in team.Resources)
                    res.Start();
            }
            baseIni.Append("Participant Configuration", vals.ToArray());

            File.WriteAllText("rlbot.cfg", baseIni.ToString(INIMode.SpacedEquals), Encoding.ASCII);

            var path = Path.GetFullPath("run.bat");
            var rlbot = Process.Start(new ProcessStartInfo() { FileName = path, WorkingDirectory = Path.GetDirectoryName(path), UseShellExecute = true });

            var demos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Rocket League\TAGame\Demos");
            var watcher = new FileSystemWatcher(demos);
            var file = watcher.WaitForChanged(WatcherChangeTypes.Created).Name;

            var sw = new Stopwatch();

            Console.WriteLine("Match ended. Shutting down...");
            Keyboard.SendKeystroke(Keys.Q, rlbot.MainWindowHandle);
            if (!rlbot.WaitForExit(8000))
            {
                Console.WriteLine("Injector unresponsive, trying harder...");
                rlbot.CloseMainWindow();
                if (!rlbot.WaitForExit(5000))
                {
                    Console.WriteLine("Injector completely unresponsive, killing...");
                    rlbot.Kill();
                }
            }
            rlbot.Dispose();

            foreach (var team in teams)
                foreach (var res in team.Resources)
                    res.Stop();

            var sleepTime = breakLength - sw.Elapsed;
            if (sleepTime > TimeSpan.Zero)
                Thread.Sleep(sleepTime);

            // TODO: Read in score from the file and return it
            return new MatchResult(blue, orange, 0, 0);
        }
    }
}