namespace QueueMonitoring.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Messaging;

    public class QueueRepository
    {
        private const string PrivateQueueIdentifier = "private$\\";
        private string _grouping;

        public IEnumerable<MQueue> GetQueuesWithGrouping(string grouping)
        {
            _grouping = grouping;

            var queues = MessageQueue.GetPrivateQueuesByMachine(".").Where(x => x.QueueName.StartsWith(PrivateQueueIdentifier + grouping));

            return queues.Select(CreateMQueues);
        }

        private MQueue CreateMQueues(MessageQueue q)
        {
            var messages = GetMessage(q);
            return new MQueue(
                q.QueueName.Replace($"{PrivateQueueIdentifier}{_grouping}.", "").Trim(),
                messages);
        }

        private static List<MqMessage> GetMessage(MessageQueue q)
        {
            var enumerator = q.GetMessageEnumerator2();

            var messages = new List<Message>();

            while (enumerator.MoveNext())
            {
                messages.Add(enumerator.Current);
            }

            return messages.Select(CreateMqMessages).ToList();
        }

        private static MqMessage CreateMqMessages(Message m)
        {
            string messageBody;
            using (var reader = new StreamReader(m.BodyStream))
            {
                messageBody = reader.ReadToEnd();   
            }

            return new MqMessage(messageBody);
        }
    }

    public class MQueue
    {
        public MQueue(string name, List<MqMessage> messages)
        {
            Name = name;
            Messages = messages;
        }

        public string Name { get; }
        public List<MqMessage> Messages { get; }
        public int MessagesCount => Messages.Count;
    }

    public class MqMessage
    {
        public MqMessage(string body)
        {
            Body = body;
        }

        public string Body { get; }

        public MqSubType? SubType { get; private set; }
    }

    public enum MqSubType
    {
        Retry,
        Poison
    }
}