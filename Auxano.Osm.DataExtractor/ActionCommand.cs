using System;
using System.Windows.Input;

namespace Auxano.Osm.DataExtractor
{
    public class ActionCommand
        : ICommand
    {
        private readonly Action<object> action;

        private bool isEnabled;

        public ActionCommand(Action<object> action)
        {
            this.action = action;
        }

        public event EventHandler CanExecuteChanged;

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set
            {
                isEnabled = value;
                var eventHandler = this.CanExecuteChanged;
                if (eventHandler != null) eventHandler.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter)
        {
            return this.isEnabled;
        }

        public void Execute(object parameter)
        {
            this.action(parameter);
        }
    }
}