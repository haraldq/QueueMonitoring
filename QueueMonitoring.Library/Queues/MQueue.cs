namespace QueueMonitoring.Library.Queues
{
    public abstract class MQueue
    {
        protected MQueue(string name, uint messagesCount)
        {
            Name = name;
            MessagesCount = messagesCount;
        }

        public string Name { get; }
        public uint MessagesCount { get; }
    }
}