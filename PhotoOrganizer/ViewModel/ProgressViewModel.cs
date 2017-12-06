using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace PhotoOrganizer.ViewModel
{
    public class ProgressViewModel : ViewModelBase
    {
        public Progress<ProgressValueAndTitle> Progress { get; }
        public CancellationTokenSource CancellationTokenSource { get; }

        public ProgressViewModel(Progress<ProgressValueAndTitle> progress = null, CancellationTokenSource cancellationTokenSource = null, 
            double minimum = 0, double maximum = 100)
        {
            Progress = progress;
            CancellationTokenSource = cancellationTokenSource;
            Minimum = minimum;
            Maximum = maximum;
            IsIndeterminate = Progress == null;
            _cancelCommand = new RelayCommand(Cancel, CanCancel);
            SubscribeToProgressChange();
        }

        public double Minimum { get; }
        public double Maximum { get; }
        public bool IsIndeterminate { get; }

        public ICommand CancelCommand => _cancelCommand;

        public string Title
        {
            get => _title;
            private set => Set(() => Title, ref _title, value);
        }

        public double Value
        {
            get => _value;
            private set => Set(() => Value, ref _value, value);
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set => Set(() => IsCompleted, ref _isCompleted, value);
        }

        private double _value;
        private bool _isCompleted;
        private readonly RelayCommand _cancelCommand;
        private string _title;

        private bool CanCancel()
        {
            return CancellationTokenSource != null && !CancellationTokenSource.IsCancellationRequested;
        }

        private void Cancel()
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
                _cancelCommand.RaiseCanExecuteChanged();
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
            UnsubscribeFromProgressChange();
        }

        private void SubscribeToProgressChange()
        {
            if (Progress != null)
            {
                Progress.ProgressChanged += OnProgressChanged;
            }
        }

        private void UnsubscribeFromProgressChange()
        {
            if (Progress != null)
            {
                Progress.ProgressChanged -= OnProgressChanged;
            }
        }

        private void OnProgressChanged(object sender, ProgressValueAndTitle progress)
        {
            Value = progress.Value;
            Title = progress.Title;
            if (Value >= Maximum)
            {
                Complete();
            }
        }

        public void Complete()
        {
            IsCompleted = true;
            UnsubscribeFromProgressChange();
        }
    }
}
