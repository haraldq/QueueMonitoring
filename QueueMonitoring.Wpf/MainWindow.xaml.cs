namespace QueueMonitoring.Wpf
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Library;
    using Library.Queues;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<MqMessage> MqMessages { get; } = new ObservableCollection<MqMessage>();

        private Stopwatch _stopwatch;
        private readonly QueueRepository _repository;

        public MainWindow()
        {
            _repository = new QueueRepository(new MessageCountService(PowerShellMethods.GetMsmqMessageCount()));
            
            InitializeComponent();
            InitializeTreeview();
        }

        private void InitializeTreeview()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();


            _repository.GetGroupings()
                .ToObservable()
                .SubscribeOn(Scheduler.Default)
                .ObserveOnDispatcher()
                .Subscribe(ProcessMqGrouping, OnCompleted);

            DataContext = this;
        }

        private void OnCompleted()
        {
            _stopwatch.Stop();

            DebugTxt.Text = "Rendering took " + _stopwatch.ElapsedMilliseconds + " ms.";

            if (!QueueTreeView.Items.IsEmpty)
                ((TreeViewItem)QueueTreeView
                   .ItemContainerGenerator
                   .ContainerFromItem(QueueTreeView.Items[0])).Focus();
        }


        private void ProcessMqGrouping(MqGrouping grouping)
        {
            QueueTreeView.Items.Add(grouping);
        }
        
        private void OnProcessMessage(MqMessage item)
        {
            //MqMessages.Add(item);
            MessageListView.Items.Add(item);
        }

        private void QueueTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var queue = e.NewValue as MQueue;
            if (queue?.MessagesCount > 0)
            {
                MessageStackPanel.Visibility = Visibility.Visible;
                _stopwatch.Restart();
                DebugTxt.Text = "";
                MqMessages.Clear();

                _repository.LoadQueue(queue)
                    .Messages
                    .ToObservable()
                    .SubscribeOn(Scheduler.Default)
                    .ObserveOnDispatcher()
                    .Take(100)
                    .Subscribe(OnProcessMessage, OnMessageCompleted);
            }
            else
                MessageStackPanel.Visibility = Visibility.Hidden;
        }

        private void OnMessageCompleted()
        {
            DebugTxt.Text = "Rendering messages took " + _stopwatch.ElapsedMilliseconds + " ms.";

        }

        private void Selector_OnSelected(object sender, SelectionChangedEventArgs e)
        {
            var queue = QueueTreeView.SelectedValue as MQueue;
            
            if(queue == null)
                return;
            int count = Int32.Parse(((ComboBoxItem) e.AddedItems[0]).Content.ToString());

            _repository.LoadQueue(queue)
                .Messages
                .ToObservable()
                .SubscribeOn(Scheduler.Default)
                .ObserveOnDispatcher()
                .Take(count)
                .Subscribe(OnProcessMessage, OnMessageCompleted);
        }
    }
}