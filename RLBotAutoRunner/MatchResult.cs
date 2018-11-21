namespace RLBotAutoRunner
{
    public readonly struct MatchResult
    {
        public MatchResult(int blueScore, int orangeScore)
        {
            BlueScore = blueScore;
            OrangeScore = orangeScore;
        }

        public int BlueScore { get; }
        public int OrangeScore { get; }
    }
}