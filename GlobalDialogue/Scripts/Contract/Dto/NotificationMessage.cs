namespace Blackhole.GlobalDialogue.Contract
{
    public enum NotificationType
    {
        Warning,
        Failed,
        Success,
        Default
    }

    public readonly struct NotificationMessage
    {
        public readonly NotificationType NotificationType;
        public readonly string Message;
        public NotificationMessage(NotificationType type, string message)
        {
            this.NotificationType = type;
            this.Message = message;
        }
    }
}