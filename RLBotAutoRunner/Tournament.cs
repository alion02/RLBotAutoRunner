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
