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
        public void GetQueuesShouldBeIdempotent()
        {
            var queue = GetQueues().Single(x => x.Name == _fixture.QueueNames[0]);
            queue.MessagesCount.Should().Be(3);

            queue = GetQueues().Single(x => x.Name == _fixture.QueueNames[0]);
            queue.MessagesCount.Should().Be(3);
        }

        [Fact]
        public void ShouldGetAllQueuesWithGrouping()
        {
            GetQueues().Should().HaveCount(4);
        }

        [Fact]
        public void ShouldGetMessageBody()
        {
            var queue = GetQueues().Single(x => x.Name == _fixture.QueueNames[0]);

            queue.MessagesCount.Should().Be(3);
            queue.Messages[2].Body.Should().Contain("South Park is safe. Until next time.");
        }

        [Fact]
        public void ShouldGetMessageCount()
        {
            var queue = GetQueues().Single(x => x.Name == _fixture.QueueNames[0]);

            queue.MessagesCount.Should().Be(3);
        }

        [Fact]
        public void ShouldShowPoisonMessages()
        {
            _fixture.MoveFirstMessageToPoison(_fixture.QueueNames[0]);
        }

        private IEnumerable<MQueue> GetQueues()
        {
            var repository = new QueueRepository();

            return repository.GetQueuesWithGrouping(_fixture.Grouping);
        }
    }
}