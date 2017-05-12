namespace QueueMonitoring.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Messaging;
    using Library;


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

                var queue = GetFreshQueue(privateQueuePath);

                Queues[queueName] = queue;

            }
            SendMessage(Queues[QueueNames[0]], "Fear not everyone! Coon is here to save the day.");
            SendMessage(Queues[QueueNames[0]], "Dude, seriously?");
            SendMessage(Queues[QueueNames[0]], "South Park is safe. Until next time.");
            SendMessage(Queues[QueueNames[1]], "Shabladoo!");

            MoveFirstMessageToPoison(Queues[QueueNames[0]]);
        }

        private static void SendMessage(MessageQueue queue, string body)
        {
            queue.Send(body, MessageQueueTransactionType.Single);
        }

        private static MessageQueue GetFreshQueue(string privateQueuePath)
        {
            MessageQueue queue;

            if (MessageQueue.Exists(privateQueuePath))
            {
                queue = new MessageQueue(privateQueuePath, QueueAccessMode.SendAndReceive);

                ClearQueue(queue);
            }
            else queue = MessageQueue.Create(privateQueuePath, true);

            return queue;
        }

        private static void ClearQueue(MessageQueue queue)
        {
            queue.Purge();
        }

        private static void MoveFirstMessageToPoison(MessageQueue q)
        {
            var message = q.Peek();
            q.MoveToSubQueue("poison", message);
        }

        public void Dispose()
        {
            foreach (var queue in Queues.Values)
            {
                ClearQueue(new MessageQueue($".\\{queue.QueueName};poison"));
                ClearQueue(queue);
            }

            Queues.Clear();
        }
    }
}