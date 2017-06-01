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
        public string Path => ".\\" + InternalName;

        public string SubQueuePath(SubQueueType? subQueueType)
        {
            if (!subQueueType.HasValue)
                return Path;
            return Path + ";" + subQueueType.Value;
        }
    }
}