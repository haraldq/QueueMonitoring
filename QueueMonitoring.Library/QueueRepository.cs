namespace QueueMonitoring.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Messaging;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Queues;

    public class QueueRepository
    {
        private const string PrivateQueueIdentifier = "private$\\";
        private readonly IMessageCountService _messageCountService;
        private readonly string _groupingDelimiter;
        private readonly string _groupingFilter;

        public QueueRepository(IMessageCountService messageCountService, string groupingDelimiter = ".", string groupingFilter = "\\d*")
        {
            _messageCountService = messageCountService;
            _groupingDelimiter = groupingDelimiter;
            _groupingFilter = groupingFilter;
        }
        public IEnumerable<MqGrouping> GetGroupings()
        {
            var queues = MessageQueue.GetPrivateQueuesByMachine(".");

            foreach (var group in queues.GroupBy(x => GetGroupingName(x.QueueName)))
            {
                var name = group.Key;

                if (string.IsNullOrEmpty(name) || ShouldBeFilteredOut(name))
                    continue;

                yield return new MqGrouping(group.Select(x => GetMQueues(x, name)).ToList(), name);
            }
        }

        private bool ShouldBeFilteredOut(string group)
        {
            return !Regex.IsMatch(group, _groupingFilter);
        }

        private string GetGroupingName(string queueName)
        {
            if (!queueName.Contains(_groupingDelimiter))
                return null;

            queueName = queueName.Replace(PrivateQueueIdentifier, "").Trim();

            var indexOfGroupingDelimiter = queueName.IndexOf(_groupingDelimiter, StringComparison.Ordinal);

            return queueName.Substring(0, indexOfGroupingDelimiter);
        }
        private MQueue GetMQueues(MessageQueue q, string group)
        {
            var name = q.QueueName.Replace($"{PrivateQueueIdentifier}{group}.", "").Trim();
            var internalName = q.QueueName;

            var baseQueue = new MQueue(name, internalName, _messageCountService.GetCount(q));

            return baseQueue;
        }

        private static void HackFixMsmqFormatNameBug(string path, MessageQueue subq)
        {
            var fn = typeof(MessageQueue).GetField("formatName", BindingFlags.NonPublic | BindingFlags.Instance);
            var formatName = "DIRECT=OS:" + path;
            if (fn != null)
                fn.SetValue(subq, formatName);
        }

        private static IEnumerable<MqMessage> GetMessageInternal(MessageQueue q)
        {
            var enumerator = q.GetMessageEnumerator2();

            var messages = new List<Message>();

            while (enumerator.MoveNext())
                messages.Add(enumerator.Current);

            return messages.Select(CreateMqMessages);
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

        public LoadedMqueue LoadQueue(MQueue mq)
        {
            var path = ".\\" + mq.InternalName;
            var q = new MessageQueue(path);

            var poisonPath = path + ";poison";
            var pq = new MessageQueue(poisonPath);
            HackFixMsmqFormatNameBug(poisonPath, pq);

            return new LoadedMqueue(mq, GetMessageInternal(q), GetMessageInternal(pq));
        }
    }
}