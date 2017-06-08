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
        private readonly IQueueRepository _queueRepository; 

        public IQueueRepository QueueRepository { get; }

        public QueueGroupingViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return; 

            _stopwatch.Start();

            _queueRepository = new QueueRepository(new MessageCountService(PowerShellMethods.GetMsmqMessageCount()));
            QueueGroupings = new ObservableCollection<MqGroupingViewModel>(_queueRepository.GetGroupings().ToList().Select(x => new MqGroupingViewModel(x)));

            _stopwatch.Stop();

            QueueRepository = _queueRepository;
        }

        public ObservableCollection<MqGroupingViewModel> QueueGroupings { get; set; }

        private MqGroupingViewModel _selectedGrouping;

        public MqGroupingViewModel SelectedGrouping
        {
            get => _selectedGrouping;
            set
            {
                _selectedGrouping = value;
                OnPropertyChanged(nameof(SelectedGrouping));
            }
        }

        private readonly Stopwatch _stopwatch = new Stopwatch();

        public string LoadTime => $"Time loading: {_stopwatch.ElapsedMilliseconds} ms";
    }
}