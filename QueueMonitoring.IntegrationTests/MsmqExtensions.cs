namespace QueueMonitoring.IntegrationTests
{
    using System;
    using System.ComponentModel;
    using System.Messaging;
    using System.Runtime.InteropServices;
    using System.Transactions;

    //From: https://github.com/ayende/rhino-esb/blob/master/Rhino.ServiceBus/Msmq/MsmqExtensions.cs
    public static class MsmqExtensions
    {
        public static void MoveToSubQueue(
            this MessageQueue queue,
            string subQueueName,
            Message message)
        {
            var fullSubQueueName = @"DIRECT=OS:.\" + queue.QueueName + ";" + subQueueName;
            IntPtr queueHandle = IntPtr.Zero;
            var error = NativeMethods.MQOpenQueue(fullSubQueueName, NativeMethods.MQ_MOVE_ACCESS,
                NativeMethods.MQ_DENY_NONE, ref queueHandle);
            if (error != 0)
                throw new Win32Exception(error);
            try
            {
                Transaction current = Transaction.Current;
                IDtcTransaction transaction = null;
                if (current != null && queue.Transactional)
                {
                    transaction = TransactionInterop.GetDtcTransaction(current);
                }

                error = NativeMethods.MQMoveMessage(queue.ReadHandle, queueHandle,
                    message.LookupId, transaction);
                if (error != 0)
                    throw new Win32Exception(error);
            }
            finally
            {
                error = NativeMethods.MQCloseQueue(queueHandle);
                if (error != 0)
                    throw new Win32Exception(error);

            }
        }

    }
}