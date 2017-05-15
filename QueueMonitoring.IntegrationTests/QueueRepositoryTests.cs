namespace QueueMonitoring.IntegrationTests
{
    using System.Collections.Generic;
    using System.Linq;
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

        private static MqGrouping GetCoonMembersGrouping(bool includeSubQueues = true)
        {
            var repository = new QueueRepository(groupingFilter: "coon_and_friends_members");

            return repository.GetGroupings(includeSubQueues).Single();
        }
    }
}