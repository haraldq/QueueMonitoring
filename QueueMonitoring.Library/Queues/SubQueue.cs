namespace QueueMonitoring.Library.Queues
{
    public class SubQueue : MQueue
    {
        public SubQueue(string name, string internalName, int messageCount) : base(name, internalName, messageCount)
        {
        }
    }
}