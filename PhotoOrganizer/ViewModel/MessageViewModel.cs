using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace PhotoOrganizer.ViewModel
{
    public class MessageViewModel : ViewModelBase
    {
        public string Message { get; }

        private bool _isAcknowledged;

        public bool IsAcknowledged
        {
            get => _isAcknowledged;
            private set => Set(() => IsAcknowledged, ref _isAcknowledged, value);
        }

        private readonly RelayCommand _acknowledgeCommand;
        public ICommand AcknowledgeCommand => _acknowledgeCommand;

        public MessageViewModel(string message)
        {
            Message = message;
            _acknowledgeCommand = new RelayCommand(Acknowledge, CanAcknowledge);
        }

        private bool CanAcknowledge()
        {
            return !IsAcknowledged;
        }

        private void Acknowledge()
        {
            IsAcknowledged = true;
            _acknowledgeCommand.RaiseCanExecuteChanged();
        }
    }
}