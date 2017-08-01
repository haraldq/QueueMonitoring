namespace QueueMonitoring.Mvvm.QueueGroupings
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Library;

    public class MQueueViewModel : ViewModelBase
    {
        private readonly IQueueRepository _queueRepository;
        private ObservableCollection<MqMessageViewModel> _messages;
        private ObservableCollection<MqMessageViewModel> _poisonMessages;
        private MqMessageViewModel _selectedMessage;

        public RelayCommand MoveToPoisonQueueCommand { get; }
        public RelayCommand MoveToDefaultQueueCommand { get; }

        public MQueueViewModel(MQueue mQueue, IQueueRepository queueRepository)
        {
            _queueRepository = queueRepository;

            MessagesCount = mQueue.MessagesCount;
            PoisonMessagesCount = mQueue.PoisonMessagesCount;
            Name = mQueue.Name;
            Messages = new ObservableCollection<MqMessageViewModel>();
            PoisonMessages = new ObservableCollection<MqMessageViewModel>();
            Path = mQueue.Path;
            SubqueuePath = mQueue.SubQueuePath(SubQueueType.Poison);

            MoveToPoisonQueueCommand = new RelayCommand(MoveToPoisonQueue);
            MoveToDefaultQueueCommand = new RelayCommand(MoveToDefaultQueue);
        }

        private void MoveToPoisonQueue()
        {
            var selectedMessages = Messages.Where(x => x.IsSelected).Select(x => x.InternalMessageId);

            _queueRepository.MoveToSubqueue(Path, SubQueueType.Poison, selectedMessages);

            RebindSelectedMqueueMessages();
        }

        private void MoveToDefaultQueue()
        {
            var selectedMessages = PoisonMessages.Where(x => x.IsSelected).Select(x => x.InternalMessageId);

            _queueRepository.MoveFromSubqueue(Path, SubQueueType.Poison, selectedMessages);

            RebindSelectedMqueueMessages();
        }

        public void RebindSelectedMqueueMessages()
        {
            PopulateMessages(() => Messages, Path, SubqueuePath);
            PopulateMessages(() => PoisonMessages, Path, SubqueuePath, SubQueueType.Poison);
        }

        private void PopulateMessages(Func<ObservableCollection<MqMessageViewModel>> f, string path, string subqueuePath, SubQueueType? subQueueType = null)
        {
            f().Clear();

            var index = 1;
            foreach (var mqMessage in _queueRepository.MessagesFor(path, subqueuePath, subQueueType))
            {
                f().Add(new MqMessageViewModel(mqMessage, index++));
            }
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