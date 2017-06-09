namespace QueueMonitoring.Mvvm.QueueGroupings
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
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

            _stopwatch.Start();

            var queueRepository = new QueueRepository(new MessageCountService(/*PowerShellMethods.GetMsmqMessageCount()*/null),groupingFilter:"collectionorderprocessing");
            QueueGroupings = new ObservableCollection<MqGroupingViewModel>(queueRepository.GetGroupings().ToList().Select(x => new MqGroupingViewModel(x)));

            _stopwatch.Stop();

            QueueRepository = queueRepository;

            MoveToPoisonQueueCommand = new RelayCommand(MoveToPoisonQueue);
        }

        private void MoveToPoisonQueue()
        {
            var selectedMQueue = SelectedGrouping.SelectedMQueue;
            QueueRepository.MoveToSubqueue(selectedMQueue.Path, SubQueueType.Poison, selectedMQueue.SelectedMessage.InternalMessageId);
        }

        public IQueueRepository QueueRepository { get; }

        public RelayCommand MoveToPoisonQueueCommand { get; }

        public ObservableCollection<MqGroupingViewModel> QueueGroupings { get; set; }

        public MqGroupingViewModel SelectedGrouping
        {
            get => _selectedGrouping;
            set
            {
                _selectedGrouping = value;
                OnPropertyChanged(nameof(SelectedGrouping));
            }
        }

        public string LoadTime => $"Time loading: {_stopwatch.ElapsedMilliseconds} ms";
    }
}