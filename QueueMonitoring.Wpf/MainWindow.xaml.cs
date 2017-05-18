namespace QueueMonitoring.Wpf
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Library;

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

        private void InitializeTreeview()
        {
            var repository = new QueueRepository();

            var observable = repository.GetGroupings().ToObservable();

            observable.Subscribe(ProcessMQueue);
        }

        private void ProcessMQueue(MqGrouping grouping)
        {
            var groupingNode = new TreeViewItem { Header = grouping.Name };

            foreach (var queue in grouping.Queues.OrderByDescending(x => x.MessagesCount))
            {
                groupingNode.Items.Add(new TreeViewItem {Header = queue.Name + " " + queue.MessagesCount});
            }

            QueueTreeView.Items.Add(groupingNode);

        }
    }
}