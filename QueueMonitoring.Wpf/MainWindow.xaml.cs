namespace QueueMonitoring.Wpf
{
    using System;
    using System.Diagnostics;
    using System.Linq;
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

            var repository = new QueueRepository(new MessageCountService(PowerShellMethods.GetMsmqMessageCount()));

            var observable = repository.GetGroupings().ToObservable().SubscribeOn(Scheduler.Default).ObserveOnDispatcher();
            
            observable.Subscribe(ProcessMqGrouping, OnCompleted);
        }

        private void OnCompleted()
        {
            _stopwatch.Stop();
            this.DebugTxt.Text = "Rendering took " + _stopwatch.ElapsedMilliseconds + " ms.";
        }

        private void ProcessMqGrouping(MqGrouping grouping)
        {
            var groupingNode = new TreeViewItem { Header = grouping.Name };
            QueueTreeView.Items.Add(groupingNode);
            var observable = grouping.Queues.ToObservable().SubscribeOn(Scheduler.Default).ObserveOnDispatcher();

            observable.Subscribe(x => OnProcessMqueue(x, groupingNode));
        }

        private void OnProcessMqueue(MQueue queue, TreeViewItem groupingNode)
        {
            //TODO: hämta MessagesCount utan att hämta alla messages, hämta messages vid klick 

            groupingNode.Items.Add(new TreeViewItem { Header = queue.Name + " " + queue.MessagesCount });
        }
    }
}