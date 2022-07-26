namespace Services.SpineNotificationServices
{
    public interface ISpineNotificationServices
    {
        Task PostToTopic(string topic, string message);
        Task PostToTopic<T>(string topic, T payload);
    }
}