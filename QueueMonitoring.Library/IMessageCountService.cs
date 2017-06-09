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
            //var key = $@"{Environment.MachineName.ToLower()}\{queue.QueueName}";
            //return _messageCount.ContainsKey(key) ? _messageCount[key] : 0;

            return GetMessageCount(queue);
        }

        private Message PeekWithoutTimeout(MessageQueue q, Cursor cursor, PeekAction action)
        {
            Message ret = null;
            try
            {
                ret = q.Peek(new TimeSpan(1), cursor, action);
            }
            catch (MessageQueueException mqe)
            {
                if (!mqe.Message.ToLower().Contains("timeout"))
                {
                    throw;
                }
            }
            return ret;
        }

        private int GetMessageCount(MessageQueue q)
        {
            int count = 0;
            Cursor cursor = q.CreateCursor();

            Message m = PeekWithoutTimeout(q, cursor, PeekAction.Current);
            if (m != null)
            {
                count = 1;
                while ((m = PeekWithoutTimeout(q, cursor, PeekAction.Next)) != null)
                {
                    count++;
                }
            }
            return count;
        }
    }
}