namespace QueueMonitoring.Library
{
    using System.Collections.Generic;
    using System.Linq;

    public class MqGrouping
    {
        public MqGrouping(List<MQueue> queues, string name)
        {
            Queues = queues;
            Name = name;
        }

        public List<MQueue> Queues { get; }

        public int MessagesCount => Queues.Sum(x => x.MessagesCount);
        public string Name { get; }
    }
}