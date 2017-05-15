namespace QueueMonitoring.Wpf
{
    using System;
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
            
            var observable = repository.GetGrouping("coon_and_friends").Queues.ToObservable();

            observable.Subscribe(ProcessMQueue);
        }

        private void ProcessMQueue(MQueue mQueue)
        {
            QueueTreeView.Items.Add(new TreeViewItem {Header = mQueue.Name});
        }
    }
}