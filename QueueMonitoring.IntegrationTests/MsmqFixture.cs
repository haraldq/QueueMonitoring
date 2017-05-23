namespace QueueMonitoring.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Messaging;
    using Library;

    public class MsmqFixture : IDisposable
    {
        private static readonly Dictionary<string, MessageQueue> Queues = new Dictionary<string, MessageQueue>();

        private IMessageCountService _messageCountService;
        public string Grouping = "coon_and_friends_members";
        public string[] QueueNames = {"the_coon", "mint_berry_crunch", "mosquito", "mysterion"};


        public MsmqFixture()
        {
            CreateQueuesAndMessages();
        }

        public IMessageCountService GetMessageCountService => _messageCountService ?? (_messageCountService = new MessageCountService(PowerShellMethods.GetMsmqMessageCount()));

        public void Dispose()
        {
            foreach (var queue in Queues.Values)
            {
                ClearQueue(new MessageQueue($".\\{queue.QueueName};poison"));
                ClearQueue(queue);
            }

            Queues.Clear();
        }

        public void CreateQueuesAndMessages()
        {
            var privateQueuePath = ".\\Private$";

            Queues["the_coon"] = GetFreshQueue($"{privateQueuePath}\\coon_and_friends_members.the_coon");
            Queues["mint_berry_crunch"] = GetFreshQueue($"{privateQueuePath}\\coon_and_friends_members.mint_berry_crunch");
            Queues["mosquito"] = GetFreshQueue($"{privateQueuePath}\\coon_and_friends_members.mosquito");
            Queues["mysterion"] = GetFreshQueue($"{privateQueuePath}\\coon_and_friends_members.mysterion");
            Queues["professor_chaos"] = GetFreshQueue($"{privateQueuePath}\\coon_and_friends_enemies.professor_chaos");
            Queues["cthulhu"] = GetFreshQueue($"{privateQueuePath}\\coon_and_friends_enemies.cthulhu");
            Queues["captain_hindsight"] = GetFreshQueue($"{privateQueuePath}\\coon_and_friends_enemies.captain_hindsight");


            SendMessage(Queues["the_coon"], "Fear not everyone! Coon is here to save the day.");
            SendMessage(Queues["the_coon"], "Dude, seriously?");
            SendMessage(Queues["the_coon"], "South Park is safe. Until next time.");
            SendMessage(Queues["mint_berry_crunch"], "Shabladoo!");
            SendMessage(Queues["captain_hindsight"], "My work here is done! I'm off to find others in need!");

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
            else
            {
                queue = MessageQueue.Create(privateQueuePath, true);
            }

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
    }
}