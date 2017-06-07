namespace QueueMonitoring.Mvvm
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Annotations;

    public interface IViewModel : INotifyPropertyChanged
    {
    }

    public class ViewModelBase : IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}