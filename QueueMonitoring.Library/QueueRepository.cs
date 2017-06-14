namespace QueueMonitoring.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Messaging;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class QueueRepository : IQueueRepository
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

        public async Task<IEnumerable<MqGrouping>> GetGroupingsAsync()
        {
            var queues = MessageQueue.GetPrivateQueuesByMachine(".");

            //foreach (var group in queues.GroupBy(x => GetGroupingName(x.QueueName)))
            //{
            //    var name = group.Key;

            //    if (string.IsNullOrEmpty(name) || ShouldBeFilteredOut(name))
            //        continue;

            //     yield return CreateGroupingAsync(group, name);
            //}

            var list = new List<MqGrouping>();
            foreach (var group in queues.GroupBy(x => GetGroupingName(x.QueueName)))
            {
                var name = group.Key;

                if (string.IsNullOrEmpty(name) || ShouldBeFilteredOut(name))
                    continue;
                //list.Add(new MqGrouping(new List<MQueue>(), name));
                list.Add(await CreateGroupingAsync(group.ToList(), name));
            }

            return list;
        }

        private async Task<MqGrouping> CreateGroupingAsync(List<MessageQueue> queues, string groupName)
        {
            var list = queues.Select(queue => CreateMQueuesAsync(queue, groupName)).ToList();

            var qs = await Task.WhenAll(list);

            return new MqGrouping(qs.ToList(), groupName);
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
        private async Task<MQueue> CreateMQueuesAsync(MessageQueue q, string group)
        {
            var name = q.QueueName.Replace($"{PrivateQueueIdentifier}{group}.", "").Trim();
            var internalName = q.QueueName;

            var messagesCountTask = await _messageCountService.GetCountAsync(q);

            var poisonMessagesCountTask = await _messageCountService.GetCountAsync(new MessageQueue(q.Path + ";poison"));

            var baseQueue = new MQueue(name, internalName, messagesCountTask, poisonMessagesCountTask);

            return baseQueue;
        }

        public IEnumerable<MqMessage> MessagesFor(MQueue mq, SubQueueType? subQueueType = null)
        {
            return MessagesFor(mq.Path, mq.SubQueuePath(subQueueType));
        }

        public IEnumerable<MqMessage> MessagesFor(string path, string subqueuePath, SubQueueType? subQueueType = null)
        {
            var propertyFilter = new MessagePropertyFilter { Body = true, SentTime = true, ArrivedTime = true, Id = true };

            if (!subQueueType.HasValue)
            {
                var q = new MessageQueue(path) { MessageReadPropertyFilter = propertyFilter };
                return GetMessagesInternal(q);
            }

            var sq = new MessageQueue(subqueuePath) { MessageReadPropertyFilter = propertyFilter };
            HackFixMsmqFormatNameBug(subqueuePath, sq);

            return GetMessagesInternal(sq, subQueueType);
        }

        private static void HackFixMsmqFormatNameBug(string path, MessageQueue subq)
        {
            var fn = typeof(MessageQueue).GetField("formatName", BindingFlags.NonPublic | BindingFlags.Instance);
            var formatName = "DIRECT=OS:" + path;
            if (fn != null)
                fn.SetValue(subq, formatName);
        }

        private static IEnumerable<MqMessage> GetMessagesInternal(MessageQueue q, SubQueueType? subQueueType = null)
        {
            var enumerator = q.GetMessageEnumerator2();

            var messages = new List<Message>();

            while (enumerator.MoveNext())
                messages.Add(enumerator.Current);

            return messages.Select(x => CreateMqMessages(x, subQueueType));
        }

        private static MqMessage CreateMqMessages(Message m, SubQueueType? subQueueType = null)
        {
            string messageBody;
            using (var reader = new StreamReader(m.BodyStream))
            {
                messageBody = reader.ReadToEnd();
            }
            return new MqMessage(m.Id, messageBody, m.SentTime, m.ArrivedTime, subQueueType);
        }

        public void MoveToSubqueue(string path, SubQueueType toSubQueueType, string internalMessageId)
        {
            var q = new MessageQueue(path);

            var message = q.PeekById(internalMessageId);

            q.MoveToSubQueue(toSubQueueType.ToString().ToLower(), message);
        }

        public void MoveToSubqueue(MQueue defaultMq, SubQueueType toSubQueueType, MqMessage m)
        {
            MoveToSubqueue(defaultMq.Path, toSubQueueType, m.InternalMessageId);
        }

        public void MoveFromSubqueue(MQueue defaultMq, SubQueueType toSubQueueType, MqMessage m)
        {
            var subq = new MessageQueue(defaultMq.SubQueuePath(toSubQueueType));

            var message = subq.PeekById(m.InternalMessageId);

            subq.MoveFromSubQueue(message);
        }
    }

    public interface IQueueRepository
    {
        Task<IEnumerable<MqGrouping>> GetGroupingsAsync();
        IEnumerable<MqMessage> MessagesFor(MQueue mq, SubQueueType? subQueueType = null);
        IEnumerable<MqMessage> MessagesFor(string path, string subqueuePath, SubQueueType? subQueueType = null);
        void MoveToSubqueue(MQueue defaultMq, SubQueueType toSubQueueType, MqMessage m);
        void MoveToSubqueue(string path, SubQueueType toSubQueueType, string internalMessageId);
        void MoveFromSubqueue(MQueue defaultMq, SubQueueType toSubQueueType, MqMessage m);
    }
}