namespace QueueMonitoring.Library.Queues
{
    public class MQueue
    {
        public MQueue(string name, string internalName, int messagesCount)
        {
            Name = name;
            InternalName = internalName;
            MessagesCount = messagesCount;
        }

        public string Name { get; }
        public string InternalName { get; }
        public int MessagesCount { get; }
    }
}