namespace QueueMonitoring.Library.Queues
{
    using System.Collections.Generic;

    public class SubQueue : MQueue
    {
        public SubQueue(string name, List<MqMessage> messages) : base(name, messages)
        {
        }
    }
}