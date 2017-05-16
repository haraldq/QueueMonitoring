namespace QueueMonitoring.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Messaging;
    using System.Reflection;
    using FluentAssertions;
    using Library;
    using Xunit;

    public class QueueRepositoryTests : IClassFixture<MsmqFixture>
    {
        private readonly MsmqFixture _fixture;

        public QueueRepositoryTests(MsmqFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void GetGroupingShouldBeIdempotent()
        {
            var mQueues = GetCoonMembersGrouping().Queues;
            var queue = mQueues.Single(x => x.Name == _fixture.QueueNames[0]);
            queue.MessagesCount.Should().Be(3);

            queue = GetCoonMembersGrouping().Queues.Single(x => x.Name == _fixture.QueueNames[0]);
            queue.MessagesCount.Should().Be(3);
        }

        [Fact]
        public void GetGroupingShouldGetAllQueuesInGrouping()
        {
            GetCoonMembersGrouping().Queues.Should().HaveCount(4);
        }

        [Fact]
        public void GetGroupingShouldGetMessageBody()
        {
            var queue = GetCoonMembersGrouping().Queues.Single(x => x.Name == _fixture.QueueNames[0]);

            queue.MessagesCount.Should().Be(3);
            queue.Messages[2].Body.Should().NotBeEmpty();
        }

        [Fact]
        public void GetGroupingShouldGetMessageCount()
        {
            var queue = GetCoonMembersGrouping().Queues.Single(x => x.Name == _fixture.QueueNames[0]);

            queue.MessagesCount.Should().Be(3);
        }

        [Fact]
        public void GetGroupingShouldShowPoisonMessages()
        {
            var queue = GetCoonMembersGrouping().Queues.Single(x => x.Name == _fixture.QueueNames[0]);

            queue.MessagesCount.Should().Be(3);
            queue.Messages.Where(x => x.SubType == null).Should().HaveCount(2);
            queue.Messages.Where(x => x.SubType == MqSubType.Poison).Should().HaveCount(1);
        }

        [Fact]
        public void GetGroupingShouldGetAllGroupings()
        {
            var repository = new QueueRepository(groupingFilter: "coon_and_friends_[a-zA-Z]*");

            var groupings = repository.GetGroupings().ToList();

            groupings.Should().HaveCount(2);
            groupings.Single(x => x.Name == "coon_and_friends_members").Should().NotBeNull();
            groupings.Single(x => x.Name == "coon_and_friends_enemies").Should().NotBeNull();
        }

        [Fact]
        public void Bug_MessageQueueNameTooLong()
        {
            var privateQueueSuffix = ".\\Private$";
            var name = "queue_with_a_name_that_is_too_long.when_doing_the_format_name_getter_method_in_queue_api_which_makes_the_whole_prog_break";
            var path = $"{privateQueueSuffix}\\{name}";

            try
            {
                if (MessageQueue.Exists(path))
                    MessageQueue.Delete(path);
                MessageQueue.Create(path, true);

                var q = new MessageQueue(path + ";poison");

                var fn = typeof(MessageQueue).GetField("formatName", BindingFlags.NonPublic | BindingFlags.Instance);
                var value = fn.GetValue(q);
                fn.SetValue(q, path);
                
                var enumerator = q.GetMessageEnumerator2();
                while (enumerator.MoveNext()) { }

                //var groupings = new QueueRepository(groupingFilter: "queue_with_a_name_that_is_too_long").GetGroupings().ToList();
            }
            finally
            {
                MessageQueue.Delete(path);
            }
        }

        private static MqGrouping GetCoonMembersGrouping(bool includeSubQueues = true)
        {
            var repository = new QueueRepository(groupingFilter: "coon_and_friends_members");

            return repository.GetGroupings(includeSubQueues).Single();
        }
    }
}