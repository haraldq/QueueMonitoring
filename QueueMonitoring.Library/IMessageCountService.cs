namespace QueueMonitoring.Library
{
    using System;
    using System.Collections.Generic;
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
            var key = $@"{Environment.MachineName.ToLower()}\{queue.QueueName}";
            return _messageCount.ContainsKey(key) ? _messageCount[key] : 0;
        }
    }
}