namespace QueueMonitoring.Mvvm.QueueGroupings
{
    using System;
    using Library;

    public class MqMessageViewModel
    {
        public MqMessageViewModel(MqMessage mqMessage)
        {
            Body = mqMessage.Body.Substring(0, 100) + "...";
            ArrivedAt = mqMessage.ArrivedAt;
            SentAt = mqMessage.SentAt;
        }
        public DateTime ArrivedAt { get; }
        public DateTime SentAt { get; }
        public string Body { get; }
    }
}