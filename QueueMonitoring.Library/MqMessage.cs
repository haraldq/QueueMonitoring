namespace QueueMonitoring.Library
{
    using System;

    public class MqMessage
    {
        public MqMessage(string internalMessageId, string body, DateTime sentAt, DateTime arrivedAt, SubQueueType? subQueueType = null)
        {
            InternalMessageId = internalMessageId;
            ArrivedAt = arrivedAt;
            SentAt = sentAt;
            Body = body;
            SubQueueType = subQueueType;
        }
        public string InternalMessageId { get; }
        public DateTime ArrivedAt { get; }
        public DateTime SentAt { get; }
        public string Body { get; }

        public SubQueueType? SubQueueType { get; }
    }
}