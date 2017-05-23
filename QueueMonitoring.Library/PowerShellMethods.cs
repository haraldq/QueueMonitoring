namespace QueueMonitoring.Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management;

    public class PowerShellMethods
    {
        public static Dictionary<string, int> GetMsmqMessageCount()
        {
            return GetMsmqMessageCount("", Environment.MachineName, "", "");
        }

        private static Dictionary<string, int> GetMsmqMessageCount(string queuePath, string machine,
            string username, string password)
        {
            var path = $@"\\{machine}\root\CIMv2";
            ManagementScope scope;
            if (string.IsNullOrEmpty(username))
            {
                scope = new ManagementScope(path);
            }
            else
            {
                var options = new ConnectionOptions {Username = username, Password = password};
                scope = new ManagementScope(path, options);
            }
            scope.Connect();
            var queryString = $@"SELECT * FROM Win32_PerfFormattedData_msmq_MSMQQueue";
            var query = new ObjectQuery(queryString);

            var searcher = new ManagementObjectSearcher(scope, query);

            var queuesCount =
                from ManagementObject queue in searcher.Get()
                select new {MessagesInQueue = (int) (ulong) queue.GetPropertyValue("MessagesInQueue"), Name = queue.GetPropertyValue("Name").ToString()};

            var dictionary = queuesCount.ToDictionary(key => key.Name, val => val.MessagesInQueue);

            return dictionary;
        }
    }
}