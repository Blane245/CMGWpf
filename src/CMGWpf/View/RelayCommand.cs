using System.Windows.Input;

namespace CMGWpf.View
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Predicate<T>? canExecute;
        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (canExecute == null)
                return true;

            if (parameter is T t)
                return canExecute(t);

            if (parameter == null && default(T) == null)
                return canExecute(default(T)!);

            return false;
        }

        public void Execute(object? parameter)
        {
            if (parameter is T t)
            {
                execute(t);
            }
            else if (parameter == null && default(T) == null)
            {
                execute(default(T)!);
            }
        }
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

    }
}
