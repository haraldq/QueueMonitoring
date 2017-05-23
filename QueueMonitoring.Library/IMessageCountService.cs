namespace QueueMonitoring.Library
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Messaging;

    public interface IMessageCountService
    {
        uint GetCount(MessageQueue queue);
    }

    public class MessageCountService : IMessageCountService
    {
        private readonly Dictionary<string, int> _messageCount;

        public MessageCountService(Dictionary<string, int> messageCount)
        {
            _messageCount = messageCount;
        }

        public uint GetCount(MessageQueue queue)
        {
            uint count = 0;
            var countKvp = _messageCount.SingleOrDefault(x => x.Key.EndsWith(queue.QueueName));
            if (!countKvp.Equals(new KeyValuePair<string, int>()))
                count = (uint) countKvp.Value;
            return count;
        }
    }
}