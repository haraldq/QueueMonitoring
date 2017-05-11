namespace QueueMonitoring.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Messaging;

    public class MsmqFixture : IDisposable
    {
        public string Grouping = "coon_and_friends";
        public string[] QueueNames = { "the_coon", "mint_berry_crunch", "mosquito", "mysterion" };
        private static readonly Dictionary<string, MessageQueue> Queues = new Dictionary<string, MessageQueue>();

        public MsmqFixture()
        {
            CreateQueuesAndMessages();
        }

        public void CreateQueuesAndMessages()
        {
            foreach (var queueName in QueueNames)
            {
                var privateQueuePath = $".\\Private$\\{Grouping}.{queueName}";

                MessageQueue queue;

                if (MessageQueue.Exists(privateQueuePath))
                {
                    queue = new MessageQueue(privateQueuePath);

                    ClearQueue(queue);
                }
                else queue = MessageQueue.Create(privateQueuePath);

                Queues[queueName] = queue;

            }

            Queues[QueueNames[0]].Send("Fear not everyone! Coon is here to save the day.");
            Queues[QueueNames[0]].Send("Dude, seriously? I'm gonna kick the shit out of you if you don't stop!");
            Queues[QueueNames[0]].Send("South Park is safe. Until next time.");
            Queues[QueueNames[1]].Send("Shabladoo!");
        }

        private static void ClearQueue(MessageQueue queue)
        {
            queue.Purge();
        }

        public void Dispose()
        {
            foreach (var queue in Queues.Values)   
            {
                ClearQueue(queue);
            }

            Queues.Clear();
        }
    }
}