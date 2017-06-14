namespace QueueMonitoring.Library
{
    using System.Messaging;
    using System.Threading.Tasks;

    public interface IMessageCountService
    {
        Task<int> GetCountAsync(MessageQueue queue);
    }

    public class MessageCountService : IMessageCountService
    {
        public async Task<int> GetCountAsync(MessageQueue queue)
        {
            return await Task.Run(() => CountAllMessages(queue));
        }

        private static int CountAllMessages(MessageQueue queue)
        {
            var count = 0;
            var enumerator = queue.GetMessageEnumerator2();
            while (enumerator.MoveNext()) count++;
            return count;
        }
    }
}