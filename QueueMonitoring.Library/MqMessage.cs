namespace QueueMonitoring.Library
{
    public class MqMessage
    {
        public MqMessage(string body)
        {
            Body = body;
        }

        public string Body { get; }
    }
}