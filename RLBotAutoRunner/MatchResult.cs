namespace RLBotAutoRunner
{
    public class MatchResult
    {
        public MatchResult(Team blue, Team orange, int blueScore, int orangeScore)
        {
            Blue = blue;
            Orange = orange;
            BlueScore = blueScore;
            OrangeScore = orangeScore;
        }

        public Team Blue { get; }
        public Team Orange { get; }
        public int BlueScore { get; }
        public int OrangeScore { get; }
    }
}
