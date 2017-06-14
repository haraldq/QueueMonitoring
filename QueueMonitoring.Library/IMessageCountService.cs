namespace QueueMonitoring.Library
{
    using System;
    using System.Collections.Generic;
    using System.Messaging;
    using System.Threading.Tasks;

    public interface IMessageCountService
    {
        Task<int> GetCountAsync(MessageQueue queue);
    }

    public class MessageCountService : IMessageCountService
    {
        private readonly Dictionary<string, int> _messageCount;

        public MessageCountService(Dictionary<string, int> messageCount)
        {
            _messageCount = messageCount;
        }

        public async Task<int> GetCountAsync(MessageQueue queue)
        {
            //TODO: do not use GetAllMessages()!
            return await Task.Run(() => queue.GetAllMessages().Length);
        }

        //private async Task<Message> PeekWithoutTimeout(MessageQueue q, Cursor cursor, PeekAction action)
        //{
        //    Message ret = null;
        //    try
        //    {
        //        ret = q.Peek(new TimeSpan(1), cursor, action);
        //    }
        //    catch (MessageQueueException mqe)
        //    {
        //        if (!mqe.Message.ToLower().Contains("timeout"))
        //        {
        //            throw;
        //        }
        //    }
        //    return ret;
        //}

        //private async Task<int> GetMessageCount(MessageQueue q)
        //{
        //    int count = 0;
        //    Cursor cursor = q.CreateCursor();

        //    var m = await PeekWithoutTimeout(q, cursor, PeekAction.Current);
        //    if (m != null)
        //    {
        //        count = 1;
        //        while ((m = await PeekWithoutTimeout(q, cursor, PeekAction.Next)) != null)
        //        {
        //            count++;
        //        }
        //    }
        //    return count;
        //}
    }
}