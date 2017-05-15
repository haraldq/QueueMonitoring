namespace QueueMonitoring.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Messaging;
    using System.Text.RegularExpressions;

    public class QueueRepository
    {
        private const string PrivateQueueIdentifier = "private$\\";
        private readonly string _groupingDelimiter;
        private readonly string _groupingFilter;

        public QueueRepository(string groupingDelimiter = ".", string groupingFilter = "*")
        {
            _groupingDelimiter = groupingDelimiter;
            _groupingFilter = groupingFilter;
        }

        public IEnumerable<MqGrouping> GetGroupings(bool includeSubQueues = true)
        {
            var queues = MessageQueue.GetPrivateQueuesByMachine(".");

            foreach (var group in queues.GroupBy(x => GetGroupingName(x.QueueName)))
            {
                var name = group.Key;

                if (string.IsNullOrEmpty(name) || ShouldBeFilteredOut(name))
                    continue;

                yield return new MqGrouping(group.Select(x => GetMQueues(x, includeSubQueues, name)).ToList(), name);
            }
        }

        private bool ShouldBeFilteredOut(string group)
        {
            bool success = Regex.IsMatch(group, _groupingFilter);
            if (success)
                return false;

            return true;
        }


        private string GetGroupingName(string queueName)
        {
            if (!queueName.Contains(_groupingDelimiter))
                return null;

            queueName = queueName.Replace(PrivateQueueIdentifier, "").Trim();

            var indexOfGroupingDelimiter = queueName.IndexOf(_groupingDelimiter, StringComparison.Ordinal);

            return queueName.Substring(0, indexOfGroupingDelimiter);
        }
        private static MQueue GetMQueues(MessageQueue q, bool includeSubQueues, string group)
        {
            var messages = GetMessages(q, includeSubQueues);
            return new MQueue(
                q.QueueName.Replace($"{PrivateQueueIdentifier}{group}.", "").Trim(),
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
                messages.Add(enumerator.Current);

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

    public class MqGrouping
    {
        public MqGrouping(List<MQueue> queues, string name)
        {
            Queues = queues;
            Name = name;
        }

        public List<MQueue> Queues { get; }

        public int MessageCount => Queues.Sum(x => x.MessagesCount);
        public string Name { get; }
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