namespace QueueMonitoring.Mvvm.QueueGroupings
{
    using System;
    using Library;

    public class MqMessageViewModel
    {
        public MqMessageViewModel(MqMessage mqMessage, int index)
        {
            Index = index;
            Body = mqMessage.Body;
            ArrivedAt = mqMessage.ArrivedAt;
            SentAt = mqMessage.SentAt;
            InternalMessageId = mqMessage.InternalMessageId;
        }
        public int Index { get; }
        public DateTime ArrivedAt { get; }
        public DateTime SentAt { get; }
        public string Body { get; }
        public string InternalMessageId { get; }
    }
}