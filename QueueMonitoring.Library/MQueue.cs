namespace QueueMonitoring.Library
{
    using System.Collections.Generic;

    public class MQueue
    {
        public MQueue(string name, string internalName, int messagesCount)
        {
            Name = name;
            InternalName = internalName;
            MessagesCount = messagesCount;
            Messages = new List<MqMessage>();
        }

        public string Name { get; }
        public string InternalName { get; }
        public int MessagesCount { get; }
        public string Path => ".\\" + InternalName;
        public List<MqMessage> Messages { get; set; }

        public string SubQueuePath(SubQueueType? subQueueType)
        {
            if (!subQueueType.HasValue)
                return Path;
            return Path + ";" + subQueueType.Value;
        }
    }
}