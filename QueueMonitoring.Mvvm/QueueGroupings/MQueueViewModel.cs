namespace QueueMonitoring.Mvvm.QueueGroupings
{
    using System.Collections.ObjectModel;
    using Library;

    public class MQueueViewModel : ViewModelBase
    {
        private ObservableCollection<MqMessageViewModel> _messages;

        private ObservableCollection<MqMessageViewModel> _poisonMessages;

        private MqMessageViewModel _selectedMessage;

        public MQueueViewModel(MQueue mQueue)
        {
            MessagesCount = mQueue.MessagesCount;
            PoisonMessagesCount = mQueue.PoisonMessagesCount;
            Name = mQueue.Name;
            Messages = new ObservableCollection<MqMessageViewModel>();
            PoisonMessages = new ObservableCollection<MqMessageViewModel>();
            Path = mQueue.Path;
            SubqueuePath = mQueue.SubQueuePath(SubQueueType.Poison);
        }

        public string Name { get; }
        public int MessagesCount { get; }
        public int PoisonMessagesCount { get; }
        public string Path { get; }
        public string SubqueuePath { get; }

        public MqMessageViewModel SelectedMessage
        {
            get => _selectedMessage;
            set
            {
                _selectedMessage = value;
                OnPropertyChanged(nameof(SelectedMessage));
            }
        }

        public ObservableCollection<MqMessageViewModel> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged(nameof(Messages));
            }
        }

        public ObservableCollection<MqMessageViewModel> PoisonMessages
        {
            get => _poisonMessages;
            set
            {
                _poisonMessages = value;
                OnPropertyChanged(nameof(PoisonMessages));
            }
        }
    }
}