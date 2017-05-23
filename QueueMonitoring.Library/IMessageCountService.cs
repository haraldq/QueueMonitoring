namespace QueueMonitoring.Library
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Messaging;

    public interface IMessageCountService
    {
        int GetCount(MessageQueue queue);
    }

    public class MessageCountService : IMessageCountService
    {
        private readonly Dictionary<string, int> _messageCount;

        public MessageCountService(Dictionary<string, int> messageCount)
        {
            _messageCount = messageCount;
        }

        public int GetCount(MessageQueue queue)
        {
            int count = 0;
            var countKvp = _messageCount.SingleOrDefault(x => x.Key.EndsWith(queue.QueueName));
            if (!countKvp.Equals(new KeyValuePair<string, int>()))
                count = (int) countKvp.Value;
            return count;
        }
    }
}