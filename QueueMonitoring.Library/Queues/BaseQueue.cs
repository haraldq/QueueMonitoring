namespace QueueMonitoring.Library.Queues
{
    using System.Collections.Generic;

    public class BaseQueue : MQueue
    {

        public BaseQueue(string name, uint messageCount) : base(name, messageCount)
        {
        }
        
    }
}