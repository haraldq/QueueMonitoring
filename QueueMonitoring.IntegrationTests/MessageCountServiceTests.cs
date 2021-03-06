﻿namespace QueueMonitoring.IntegrationTests
{
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Library;
    using Xunit;

    [Collection("Msmq Collection")]
    public class MessageCountServiceTests
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
        public async void CountingMessagesInEachQueue()
        {
            var coonAndFriends = (await GetRepository().GetGroupingsAsync()).Single();

            var coonQueue = coonAndFriends.Queues.Single(x => x.Name.Contains("the_coon"));
            coonQueue.MessagesCount.Should().Be(2);
            coonQueue.PoisonMessagesCount.Should().Be(1);
        }

        [Fact]
        public async Task SpeedTest_ShouldLoadQueuesFast()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
             
            await new QueueRepository(_fixture.GetMessageCountService).GetGroupingsAsync();

            stopwatch.Stop();

            Assert.False(stopwatch.ElapsedMilliseconds > 3000, $"Loading took {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}
