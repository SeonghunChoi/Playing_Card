namespace PlayingCard.GamePlay.Model.Message
{
    public enum MainMenuMessageType
    {
        Start,
        Exit,
        Menu,
        Network,
    }

    /// <summary>
    /// UIMainMenu 버튼 액션 처리용 message
    /// </summary>
    public struct MainMenuMessage
    {
        public MainMenuMessageType messageType;
        public int value;

        public MainMenuMessage(MainMenuMessageType messageType, int value)
        {
            this.messageType = messageType;
            this.value = value;
        }
    }
}
