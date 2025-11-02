namespace PlayingCard.ConnectionManagement.Message
{
    public struct ReconnectMessage
    {
        public int CurrentAttempt;
        public int MaxAttempt;

        public ReconnectMessage(int attempt, int maxAttempt)
        {
            CurrentAttempt = attempt;
            MaxAttempt = maxAttempt;
        }
    }
}
