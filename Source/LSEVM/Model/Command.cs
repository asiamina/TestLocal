using System;
using System.Windows.Input;

namespace VSIXProjectThesis.Model{

    internal class Command : ICommand
    {

        private readonly Func<object, bool> m_canExecute = null;
        private readonly Action<object> m_exec = null;

#pragma warning disable 0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore 0067

        public Command(Action<object> exec, Func<object, bool> canExecute = null){
            this.m_canExecute = canExecute;
            this.m_exec = exec;
        }

        public bool CanExecute(object parameter){
            if (this.m_canExecute != null)
                return this.m_canExecute(parameter);
            else
                return true;
        }

        public void Execute(object parameter){
            if (this.m_exec != null)
                this.m_exec(parameter);
        }

    }

}