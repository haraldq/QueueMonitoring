namespace QueueMonitoring.Wpf
{
    using System;
    using System.Collections.ObjectModel;
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
        private Stopwatch _stopwatch;
        private QueueRepository _repository;
        public MainWindow()
        {
            InitializeComponent();
            InitializeTreeview();
        }
        
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
                 ((TreeViewItem)QueueTreeView
                    .ItemContainerGenerator
                    .ContainerFromItem(QueueTreeView.Items[0])).Focus();
            
        }

        private void ProcessMqGrouping(MqGrouping grouping)
        {
            QueueTreeView.Items.Add(grouping);
        }

        private void OnProcessMessage(MqMessage message)
        {
            MessageListView.Items.Add(message);
        }

        private void OnProcessPoisonMessage(MqMessage message)
        {
            PoisonMessageListView.Items.Add(message);
        }
        
        private void QueueTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var queue = e.NewValue as MQueue;
            if (queue != null)
            {
                MessageListView.Items.Clear();
                PoisonMessageListView.Items.Clear();

                var observableMessages = _repository.LoadQueue(queue).Messages.ToObservable().SubscribeOn(Scheduler.Default).ObserveOnDispatcher();
                observableMessages.Subscribe(OnProcessMessage);

                var observablePoisonMessages = _repository.LoadQueue(queue).PoisonMessages.ToObservable().SubscribeOn(Scheduler.Default).ObserveOnDispatcher();
                observablePoisonMessages.Subscribe(OnProcessPoisonMessage);
            }
        }
    }
}