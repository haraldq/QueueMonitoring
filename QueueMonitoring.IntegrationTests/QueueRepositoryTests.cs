namespace QueueMonitoring.IntegrationTests
{
    using System.Linq;
    using FluentAssertions;
    using Library;
    using Xunit;

    public class QueueRepositoryTests : IClassFixture<MsmqFixture>
    {
        public QueueRepositoryTests(MsmqFixture fixture)
        {
            _grouping = fixture.Grouping;
            _queueNames = fixture.QueueNames;
        }

        private readonly string _grouping;
        private readonly string[] _queueNames;

        [Fact]
        public void GetQueuesShouldBeIdempotent()
        {
            var repository = new QueueRepository();

            var queue = repository.GetQueuesWithGrouping(_grouping).Single(x => x.Name == _queueNames[0]);
            queue.MessagesCount.Should().Be(3);

            queue = repository.GetQueuesWithGrouping(_grouping).Single(x => x.Name == _queueNames[0]);
            queue.MessagesCount.Should().Be(3);
        }

        [Fact]
        public void ShouldGetAllQueuesWithGrouping()
        {
            var repository = new QueueRepository();

            repository.GetQueuesWithGrouping(_grouping).Should().HaveCount(4);
        }

        [Fact]
        public void ShouldGetMessageBody()
        {
            var repository = new QueueRepository();

            var queue = repository.GetQueuesWithGrouping(_grouping).Single(x => x.Name == _queueNames[0]);

            queue.MessagesCount.Should().Be(3);
            queue.Messages[2].Body.Should().Contain("South Park is safe. Until next time.");
        }

        [Fact]
        public void ShouldGetMessageCount()
        {
            var repository = new QueueRepository();

            var queue = repository.GetQueuesWithGrouping(_grouping).Single(x => x.Name == _queueNames[0]);

            queue.MessagesCount.Should().Be(3);
        }
    }
}