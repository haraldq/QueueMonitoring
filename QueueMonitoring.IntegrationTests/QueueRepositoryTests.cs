namespace QueueMonitoring.IntegrationTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Messaging;
    using System.Reflection;
    using FluentAssertions;
    using Library;
    using Library.Queues;
    using Moq;
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
        public void GetGroupingShouldGetMessageCount()
        {
            var queue = GetCoonMembersGrouping().Queues.Single(x => x.Name == _fixture.QueueNames[0]);

            queue.MessagesCount.Should().Be(3);
        }

        [Fact]
        public void GetGroupingShouldGetAllGroupings()
        {
            var repository = new QueueRepository(new Mock<IMessageCountService>().Object,groupingFilter: "coon_and_friends_[a-zA-Z]*");

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

                var groupings = new QueueRepository(new Mock<IMessageCountService>().Object, groupingFilter: "queue_with_a_name_that_is_too_long").GetGroupings().ToList();

                groupings.Should().HaveCount(1);
            }
            finally
            {
                MessageQueue.Delete(path);
            }
        }


        private MqGrouping GetCoonMembersGrouping()
        {
            var repository = new QueueRepository(_fixture.GetMessageCountService, groupingFilter: "coon_and_friends_members");

            return repository.GetGroupings().Single();
        }
    }
}