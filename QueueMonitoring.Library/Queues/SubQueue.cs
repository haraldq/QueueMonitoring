namespace QueueMonitoring.Library.Queues
{
    using System.Collections.Generic;

    public class SubQueue : MQueue
    {
        public SubQueue(string name, uint messageCount) : base(name, messageCount)
        {
        }
    }
}