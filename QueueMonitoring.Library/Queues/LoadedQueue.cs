namespace QueueMonitoring.Library.Queues
{
    using System.Collections.Generic;
    using System.Linq;

    public class LoadedMqueue : MQueue
    {
        private readonly IEnumerable<MqMessage> _messages;

        public List<MqMessage> Messages => _messages.Where(x => !x.SubQueueType.HasValue).ToList();
        public IEnumerable<MqMessage> PoisonMessages => _messages.Where(x => x.SubQueueType == SubQueueType.Poison).ToList();

        public LoadedMqueue(MQueue mq, IEnumerable<MqMessage> messages) : base(mq.Name, mq.InternalName, mq.MessagesCount)
        {
            _messages = messages;
        }
    }
}