namespace QueueMonitoring.Library.Queues
{
    using System.Collections.Generic;

    public abstract class MQueue
    {
        public MQueue(string name, List<MqMessage> messages)
        {
            Name = name;
            Messages = messages;
        }

        public string Name { get; }
        public List<MqMessage> Messages { get; }
        public int MessagesCount => Messages.Count;
    }
}