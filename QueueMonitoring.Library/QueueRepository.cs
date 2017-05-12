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

        public IEnumerable<MQueue> GetQueuesWithGrouping(string grouping, bool includeSubQueues = true)
        {
            _grouping = grouping;

            var queues = MessageQueue.GetPrivateQueuesByMachine(".").Where(x => x.QueueName.StartsWith(PrivateQueueIdentifier + grouping));

            return queues.Select(x => CreateMQueues(x, includeSubQueues));
        }

        private MQueue CreateMQueues(MessageQueue q, bool includeSubQueues)
        {
            var messages = GetMessages(q, includeSubQueues);
            return new MQueue(
                q.QueueName.Replace($"{PrivateQueueIdentifier}{_grouping}.", "").Trim(),
                messages);
        }

        private static List<MqMessage> GetMessages(MessageQueue q, bool includeSubQueues)
        {
            var list = GetMessageInternal(q);

            return includeSubQueues ? list.Union(AddSubqueue(q, MqSubType.Poison)).ToList() : list.ToList();
        }

        private static IEnumerable<MqMessage> AddSubqueue(MessageQueue q, MqSubType? subType = null)
        {
            var path = $".\\{q.QueueName};{subType.ToString().ToLower()}";
            var subq = new MessageQueue(path);
            return GetMessageInternal(subq, subType);
        }

        private static IEnumerable<MqMessage> GetMessageInternal(MessageQueue q, MqSubType? subType = null)
        {
            var enumerator = q.GetMessageEnumerator2();

            var messages = new List<Message>();

            while (enumerator.MoveNext())
            {
                messages.Add(enumerator.Current);
            }

            return messages.Select(x => CreateMqMessages(x, subType));
        }

        private static MqMessage CreateMqMessages(Message m, MqSubType? subType)
        {
            string messageBody;
            using (var reader = new StreamReader(m.BodyStream))
            {
                messageBody = reader.ReadToEnd();
            }
            return new MqMessage(messageBody, subType);
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
        public MqMessage(string body, MqSubType? subType)
        {
            SubType = subType;
            Body = body;
        }

        public string Body { get; }

        public MqSubType? SubType { get; }
    }

    public enum MqSubType
    {
        Retry,
        Poison
    }
}