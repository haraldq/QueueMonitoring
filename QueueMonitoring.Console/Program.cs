namespace QueueMonitoring.Console
{
    using System;
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

            var observable = queueRepository.GetQueuesWithGrouping("coon_and_friends").ToObservable();

            observable.Subscribe(ProcessMQueue);

            Console.ReadKey();
        }

        private static void ProcessMQueue(MQueue mQueue)
        {
            Console.WriteLine($"{mQueue.Name,-25} Messages: {mQueue.MessagesCount}");
            foreach (var message in mQueue.Messages)
            {
                Console.WriteLine($"    {XElement.Parse(message.Body)}");

            }
        }
    }
}