namespace QueueMonitoring.IntegrationTests
{
    using System.Diagnostics;
    using System.Linq;
    using FluentAssertions;
    using Library;
    using Xunit;

    public class MessageCountServiceTests : IClassFixture<MsmqFixture>
    {
        public MessageCountServiceTests(MsmqFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly MsmqFixture _fixture;
        private QueueRepository GetRepository()
        {
            return _fixture.GetRepository();
        }

        [Fact]
        public void CountingMessagesInEachQueue()
        {
            var coonAndFriends = GetRepository().GetGroupings().Single();

            var coonQueue = coonAndFriends.Queues.Single(x => x.Name.Contains("the_coon"));
            coonQueue.MessagesCount.Should().Be(2);
            coonQueue.PoisonMessagesCount.Should().Be(1);
        }

        [Fact]
        public void SpeedTest_ShouldLoadQueuesFast()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var groupings = new QueueRepository(_fixture.GetMessageCountService /*, groupingFilter: "collectionorderprocessing"*/).GetGroupings().ToList();

            stopwatch.Stop();

            Assert.False(stopwatch.ElapsedMilliseconds > 100, $"Loading took {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}
