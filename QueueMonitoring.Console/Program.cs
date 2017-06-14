namespace QueueMonitoring.Console
{
    using System;
    using System.Reactive.Linq;
    using IntegrationTests;
    using Library;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var queueRepository = new QueueRepository(new MessageCountService(PowerShellMethods.GetMsmqMessageCount()));

            new MsmqFixture().CreateQueuesAndMessages();

            var observable = queueRepository.GetGroupingsAsync().ToObservable();

            observable.Subscribe(ProcessMQueue);

            Console.ReadKey();
        }

        private static void ProcessMQueue(MqGrouping grouping)
        {
            Console.WriteLine($"============ {grouping.Name} ============");
            foreach (var mQueue in grouping.Queues)
                Console.WriteLine($"### {mQueue.Name,-25} Messages: {mQueue.MessagesCount}");
        }
    }
}