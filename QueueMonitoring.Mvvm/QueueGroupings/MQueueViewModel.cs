namespace QueueMonitoring.Mvvm.QueueGroupings
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Library;

    public class MQueueViewModel
    {
        public MQueueViewModel(MQueue mQueue)
        {
            MessagesCount = mQueue.MessagesCount;
            Name = mQueue.Name;
            Messages = new ObservableCollection<MqMessageViewModel>(mQueue.Messages.Select(x => new MqMessageViewModel(x)));
            Path = mQueue.Path;
            SubqueuePath = mQueue.SubQueuePath(SubQueueType.Poison);
        }

        public string Name { get; }
        public ObservableCollection<MqMessageViewModel> Messages { get; set; }
        public int MessagesCount { get; }
        public string Path { get; }
        public string SubqueuePath { get; }
    }
}