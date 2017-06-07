namespace QueueMonitoring.Mvvm.QueueGroupings
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Library;

    public class MqGroupingViewModel : ViewModelBase
    {
        public MqGroupingViewModel(MqGrouping mqGrouping)
        {
            Queues = new ObservableCollection<MQueueViewModel>(mqGrouping.Queues.Select(x => new MQueueViewModel(x)));
            Name = mqGrouping.Name;
        }
        public ObservableCollection<MQueueViewModel> Queues { get; }
        public int MessagesCount => Queues.Sum(x => x.MessagesCount);
        public string Name { get; }

        private MQueueViewModel _selectedMQueue;

        public MQueueViewModel SelectedMQueue
        {
            get => _selectedMQueue;
            set
            {
                _selectedMQueue = value;
                OnPropertyChanged(nameof(SelectedMQueue));
            }
        }
    }
}