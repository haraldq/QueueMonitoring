namespace QueueMonitoring.Mvvm.QueueGroupings
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.Eventing.Reader;
    using System.Linq;
    using System.Windows;
    using Library;

    public class QueueGroupingViewModel : ViewModelBase
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private MqGroupingViewModel _selectedGrouping;

        public QueueGroupingViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;

            QueueRepository = new QueueRepository(new MessageCountService());

            MoveToPoisonQueueCommand = new RelayCommand(MoveToPoisonQueue);
        }

        public async void LoadQueues()
        {
            _stopwatch.Start();
            IsLoading = true;

            var mqGroupings = await QueueRepository.GetGroupingsAsync();
            QueueGroupings = new ObservableCollection<MqGroupingViewModel>(mqGroupings.ToList().Select(x => new MqGroupingViewModel(x)));

            _stopwatch.Stop();

            IsLoading = false;
            LoadTime = $"Time loading: {_stopwatch.ElapsedMilliseconds} ms";
        }


        private void MoveToPoisonQueue()
        {
            var selectedMQueue = SelectedGrouping.SelectedMQueue;
            var selectedMessages = selectedMQueue.Messages.Where(x => x.IsSelected).Select(x => x.InternalMessageId);

            QueueRepository.MoveToSubqueue(selectedMQueue.Path, SubQueueType.Poison, selectedMessages);

            RebindSelectedMqueueMessages();
        }

        private void RebindSelectedMqueueMessages()
        {
            SelectedGrouping.SelectedMQueue.Messages.Clear();
            SelectedGrouping.SelectedMQueue.PoisonMessages.Clear();
            int index = 1;
            foreach (var mqMessage in QueueRepository.MessagesFor(SelectedGrouping.SelectedMQueue.Path, SelectedGrouping.SelectedMQueue.SubqueuePath))
            {
                SelectedGrouping.SelectedMQueue.Messages.Add(new MqMessageViewModel(mqMessage, index++));
            }

            index = 1;
            foreach (var mqMessage in QueueRepository.MessagesFor(SelectedGrouping.SelectedMQueue.Path, SelectedGrouping.SelectedMQueue.SubqueuePath, SubQueueType.Poison))
            {
                SelectedGrouping.SelectedMQueue.PoisonMessages.Add(new MqMessageViewModel(mqMessage, index++));
            }
        }

        public IQueueRepository QueueRepository { get; }

        public RelayCommand MoveToPoisonQueueCommand { get; }

        private ObservableCollection<MqGroupingViewModel> _queueGroupings;
        public ObservableCollection<MqGroupingViewModel> QueueGroupings
        {
            get { return _queueGroupings; }
            set
            {
                if (_queueGroupings != value)
                {
                    _queueGroupings = value;
                    OnPropertyChanged(nameof(QueueGroupings));
                }
            }
        }

        public MqGroupingViewModel SelectedGrouping
        {
            get => _selectedGrouping;
            set
            {
                _selectedGrouping = value;
                OnPropertyChanged(nameof(SelectedGrouping));
            }
        }

        private string _loadTime;
        public string LoadTime
        {
            get => _loadTime;
            set
            {
                _loadTime = value;
                OnPropertyChanged(nameof(LoadTime));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }
}