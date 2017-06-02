namespace QueueMonitoring.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Messaging;
    using FluentAssertions;
    using Library;
    using Moq;
    using Xunit;

    public class QueueRepositoryTests : IClassFixture<MsmqFixture>
    {
        public QueueRepositoryTests(MsmqFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly MsmqFixture _fixture;


        private QueueRepository GetRepository()
        {
            return new QueueRepository(_fixture.GetMessageCountService, groupingFilter: "coon_and_friends_members");
        }

        private MqGrouping GetCoonMembersGrouping()
        {
            var repository = GetRepository();

            return repository.GetGroupings().Single();
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
        public void GetGroupingShouldGetAllGroupings()
        {
            var repository = new QueueRepository(new Mock<IMessageCountService>().Object, groupingFilter: "coon_and_friends_[a-zA-Z]*");

            var groupings = repository.GetGroupings().ToList();

            groupings.Should().HaveCount(2);
            groupings.Single(x => x.Name == "coon_and_friends_members").Should().NotBeNull();
            groupings.Single(x => x.Name == "coon_and_friends_enemies").Should().NotBeNull();
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
        public void GetMessagesShouldEqualQueueCount()
        {
            var queue = GetCoonMembersGrouping().Queues.Single(x => x.Name == _fixture.QueueNames[0]);

            var messages = GetRepository().MessagesFor(queue);
            var poisonMessages = GetRepository().MessagesFor(queue, SubQueueType.Poison);

            messages.Union(poisonMessages).Should().HaveCount(queue.MessagesCount);

            //var loadedMqueue = GetRepository().MessagesFor(queue);

            //loadedMqueue.Messages.Union(loadedMqueue.PoisonMessages).Should().HaveCount(queue.MessagesCount);
        }

        [Fact]
        public void GetMessagesShouldHaveBody()
        {
            var queue = GetCoonMembersGrouping().Queues.Single(x => x.Name == _fixture.QueueNames[0]);
            
            GetRepository().MessagesFor(queue).FirstOrDefault(x => x.Body.Contains("South Park is safe. Until next time.")).Should().NotBeNull();
            GetRepository().MessagesFor(queue, SubQueueType.Poison).SingleOrDefault(x => x.Body.Contains("Fear not everyone! Coon is here to save the day.")).Should().NotBeNull();
        }

        [Fact]
        public void GetMessagesShouldHaveMetadataProperties()
        {
            var queue = GetCoonMembersGrouping().Queues.Single(x => x.Name == _fixture.QueueNames[0]);
            
            var message = GetRepository().MessagesFor(queue).FirstOrDefault();
            message.Body.Should().NotBeNullOrEmpty();
            message.SentAt.Should().BeBefore(DateTime.Now).And.BeAfter(DateTime.MinValue);
            message.ArrivedAt.Should().BeBefore(DateTime.Now).And.BeAfter(DateTime.MinValue);
        }

        [Fact]
        public void MovingMessageToSubqueue()
        {
            var queue = GetCoonMembersGrouping().Queues.Single(x => x.Name == _fixture.QueueNames[0]);
            var repository = GetRepository();
            var message = repository.MessagesFor(queue).FirstOrDefault();
            message.SubQueueType.Should().BeNull();

            repository.MoveToSubqueue(queue, SubQueueType.Poison, message);

            repository.MessagesFor(queue).Should().HaveCount(1);
            var poisonMessages = repository.MessagesFor(queue, SubQueueType.Poison).ToList();
            poisonMessages.Should().HaveCount(2);

            var poisonMessage = poisonMessages.SingleOrDefault(x => x.InternalMessageId == message.InternalMessageId);
            poisonMessage.Should().NotBeNull();
            poisonMessage.SubQueueType.Value.Should().Be(SubQueueType.Poison);

            // cleanup
            repository.MoveFromSubqueue(queue, SubQueueType.Poison, poisonMessage);
        }

        [Fact]
        public void MovingMessageFromSubqueueToRegularQueue()
        {
            var queue = GetCoonMembersGrouping().Queues.Single(x => x.Name == _fixture.QueueNames[0]);
            var repository = GetRepository();
            var message = repository.MessagesFor(queue, SubQueueType.Poison).FirstOrDefault();

            repository.MoveFromSubqueue(queue, SubQueueType.Poison, message);

            repository.MessagesFor(queue).Should().HaveCount(3);
            repository.MessagesFor(queue, SubQueueType.Poison).Should().HaveCount(0);
            var m = repository.MessagesFor(queue).Single(x => x.InternalMessageId == message.InternalMessageId);

            // cleanup
            repository.MoveToSubqueue(queue, SubQueueType.Poison, m);
        }
    }
}