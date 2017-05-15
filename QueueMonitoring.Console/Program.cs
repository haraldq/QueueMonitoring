namespace QueueMonitoring.Console
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Xml.Linq;
    using IntegrationTests;
    using Library;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var queueRepository = new QueueRepository();

            new MsmqFixture().CreateQueuesAndMessages();

            var observable = queueRepository.GetGroupings().ToObservable();

            observable.Subscribe(ProcessMQueue);

            Console.ReadKey();
        }

        private static void ProcessMQueue(MqGrouping grouping)
        {
            //Console.WriteLine($"### {grouping.}");
            foreach (var queue in grouping.Queues)
            {
                
            }
            //Console.WriteLine($"### {mQueue.Name,-25} Messages: {mQueue.MessagesCount}");
            //foreach (var grouping in mQueue.Messages.GroupBy(x => x.SubType))
            //{
            //    string group = grouping.Key.HasValue ? grouping.Key.ToString() : "Main";

            //    Console.WriteLine($"============ {group} ============");

            //    foreach (var message in grouping)
            //    {
            //        Console.WriteLine($"{XElement.Parse(message.Body)}");
            //    }
            //    Console.WriteLine();
            //}
        }
    }
}