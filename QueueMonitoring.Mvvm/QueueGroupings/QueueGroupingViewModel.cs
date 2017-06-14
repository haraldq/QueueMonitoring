namespace QueueMonitoring.Mvvm.QueueGroupings
{
    using System.Collections.Generic;
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

            
            QueueRepository = new QueueRepository(new MessageCountService(null));//,groupingFilter:"collectionorderprocessing");

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
            QueueRepository.MoveToSubqueue(selectedMQueue.Path, SubQueueType.Poison, selectedMQueue.SelectedMessage.InternalMessageId);
        }

        public IQueueRepository QueueRepository { get; }

        public RelayCommand MoveToPoisonQueueCommand { get; }

        private ObservableCollection<MqGroupingViewModel> _queueGroupings;
        public ObservableCollection<MqGroupingViewModel> QueueGroupings {
            get { return _queueGroupings; }
            set
            {
                if (_queueGroupings != value)
                {
                    _queueGroupings = value;
                    OnPropertyChanged(nameof(QueueGroupings));
                }
            } }

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