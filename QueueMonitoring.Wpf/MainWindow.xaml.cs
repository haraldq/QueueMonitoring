namespace QueueMonitoring.Wpf
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Library;
    using Library.Queues;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private QueueRepository _repository;
        public MainWindow()
        {
            InitializeComponent();
            InitializeTreeview();
        }

        private Stopwatch _stopwatch;

        private void InitializeTreeview()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            _repository = new QueueRepository(new MessageCountService(PowerShellMethods.GetMsmqMessageCount()));

            var observable = _repository.GetGroupings().ToObservable().SubscribeOn(Scheduler.Default).ObserveOnDispatcher();

            observable.Subscribe(ProcessMqGrouping, OnCompleted);
        }

        private void OnCompleted()
        {
            _stopwatch.Stop();

            DebugTxt.Text = "Rendering took " + _stopwatch.ElapsedMilliseconds + " ms.";

            if (!QueueTreeView.Items.IsEmpty)
                ((TreeViewItem)QueueTreeView.Items[0]).Focus();
        }

        private void ProcessMqGrouping(MqGrouping grouping)
        {
            var groupingNode = new TreeViewItem { Header = grouping.Name + " " + grouping.MessageCount, Tag = grouping.Name };
            QueueTreeView.Items.Add(groupingNode);
            var observable = grouping.Queues.ToObservable().SubscribeOn(Scheduler.Default).ObserveOnDispatcher();

            observable.Subscribe(x => OnProcessMqueue(x, groupingNode));
        }

        private void OnProcessMqueue(MQueue queue, TreeViewItem groupingNode)
        {
            var queueNode = new TreeViewItem { Header = queue.Name + " " + queue.MessagesCount, Tag = queue };
            queueNode.Selected += QueueNodeOnSelected;
            groupingNode.Items.Add(queueNode);
        }

        private void QueueNodeOnSelected(object sender, RoutedEventArgs routedEventArgs)
        {
            var queueNode = (TreeViewItem)sender;

            MessageListView.Items.Clear();
            PoisonMessageListView.Items.Clear();

            var observableMessages = _repository.LoadQueue((MQueue)queueNode.Tag).Messages.ToObservable().SubscribeOn(Scheduler.Default).ObserveOnDispatcher();
            observableMessages.Subscribe(OnProcessMessage);

            var observablePoisonMessages = _repository.LoadQueue((MQueue)queueNode.Tag).PoisonMessages.ToObservable().SubscribeOn(Scheduler.Default).ObserveOnDispatcher();
            observablePoisonMessages.Subscribe(OnProcessPoisonMessage);
        }

        private void OnProcessMessage(MqMessage message)
        {
            MessageListView.Items.Add(message);
        }

        private void OnProcessPoisonMessage(MqMessage message)
        {
            PoisonMessageListView.Items.Add(message);
        }
    }
}