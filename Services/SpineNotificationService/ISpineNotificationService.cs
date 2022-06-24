namespace Services.SpineNotificationService
{
    public interface ISpineNotificationService
    {
        Task PostToTopic(string topic, string message);
        Task PostToTopic<T>(string topic, T payload);
    }
}