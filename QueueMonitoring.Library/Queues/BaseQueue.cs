namespace QueueMonitoring.Library.Queues
{
    using System.Collections.Generic;

    public class BaseQueue : MQueue
    {

        public BaseQueue(string name, List<MqMessage> messages, SubQueue poisonQueue) : base(name, messages)
        {
            PoisonQueue = poisonQueue;
        }

        public SubQueue PoisonQueue { get; }
    }
}