using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace Services.SpineNotificationServices;

public class SpineNotificationService : ISpineNotificationServices
{
    private readonly ServiceBusClient _serviceBusClient;

    public SpineNotificationService(ServiceBusClient serviceBusClient)
    {
        _serviceBusClient = serviceBusClient;
    }

    public async Task PostToTopic(string topic, string message)
    {
        var topicSender = _serviceBusClient.CreateSender(topic);
        await topicSender.SendMessageAsync(new ServiceBusMessage(message));
    }

    public async Task PostToTopic<T>(string topic, T payload)
    {
        await PostToTopic(topic, JsonConvert.SerializeObject(payload));
    }
}
