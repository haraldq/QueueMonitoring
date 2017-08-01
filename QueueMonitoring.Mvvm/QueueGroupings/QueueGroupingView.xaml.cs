namespace QueueMonitoring.Mvvm.QueueGroupings
{
    using System;
    using System.Collections.ObjectModel;
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

            if (vm == null)
                return;

            var grouping = e.NewValue as MqGroupingViewModel;
            if (grouping != null)
            {
                vm.SelectedGrouping = grouping;
                return;
            }

            var queue = e.NewValue as MQueueViewModel;
            if (queue == null)
                return;

            vm.SelectedGrouping.SelectedMQueue = queue;
            if (queue.MessagesCount <= 0 && queue.PoisonMessagesCount <= 0)
                return;
            
            PopulateMessages(vm, () => queue.Messages,  queue.Path, queue.SubqueuePath);
            PopulateMessages(vm, () => queue.PoisonMessages, queue.Path, queue.SubqueuePath, SubQueueType.Poison);
        }

        private static void PopulateMessages(QueueGroupingViewModel vm, Func<ObservableCollection<MqMessageViewModel>> f,string path, string subqueuePath, SubQueueType? subQueueType = null)
        {
            int index = 1;
            foreach (var mqMessage in vm.QueueRepository.MessagesFor(path,subqueuePath,subQueueType))
            {
                f().Add(new MqMessageViewModel(mqMessage, index++));
            }
        }
    }
}