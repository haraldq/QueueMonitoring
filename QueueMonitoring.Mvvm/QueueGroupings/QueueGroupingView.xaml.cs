namespace QueueMonitoring.Mvvm.QueueGroupings
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Library;

    /// <summary>
    ///     Interaction logic for QueueGroupingView.xaml
    /// </summary>
    public partial class QueueGroupingView : UserControl
    {
        public QueueGroupingView()
        {
            InitializeComponent();
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var vm = DataContext as QueueGroupingViewModel;

            var grouping = e.NewValue as MqGroupingViewModel;
            if (grouping != null)
            {
                vm.SelectedGrouping = grouping;
                return;
            }

            var queue = e.NewValue as MQueueViewModel;
            if (queue != null)
            {
                vm.SelectedGrouping.SelectedMQueue = queue;
                if (queue.MessagesCount > 0)
                {
                    int index = 1;
                    foreach (var mqMessage in vm.QueueRepository.MessagesFor(queue.Path, queue.SubqueuePath))//, SubQueueType.Poison))
                    {
                        queue.Messages.Add(new MqMessageViewModel(mqMessage, index++));
                    }
                    foreach (var mqMessage in vm.QueueRepository.MessagesFor(queue.Path, queue.SubqueuePath, SubQueueType.Poison))
                    {
                        queue.PoisonMessages.Add(new MqMessageViewModel(mqMessage, index++));
                    }
                }
            }
        }
    }
}