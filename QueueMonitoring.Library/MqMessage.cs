namespace QueueMonitoring.Library
{
    using System;

    public class MqMessage
    {
        public MqMessage(string body, DateTime sentAt, DateTime arrivedAt)
        {
            ArrivedAt = arrivedAt;
            SentAt = sentAt;
            Body = body;
        }

        public DateTime ArrivedAt { get; }
        public DateTime SentAt { get; }
        public string Body { get; }
    }
}