namespace QueueMonitoring.Library.Queues
{
    using System.Collections.Generic;
    using System.Linq;

    public class LoadedMqueue : MQueue
    {
        public List<MqMessage> Messages { get; }
        public List<MqMessage> PoisonMessages { get; }

        public LoadedMqueue(MQueue mq, IEnumerable<MqMessage> messages, IEnumerable<MqMessage> poisonMessages) : base(mq.Name, mq.InternalName, mq.MessagesCount)
        {
            Messages = messages.ToList();
            PoisonMessages = poisonMessages.ToList();
        }
    }
}