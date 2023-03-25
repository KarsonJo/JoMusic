using JoMusicCenter.Utilities;
using JoMusicCenter.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JoMusicCenter.ViewModels
{
    public abstract class CommitableObject : NotifyPropertyChangedObject, ICloseable
    {
        public event EventHandler? CloseRequest;

        //protected Action? DialogueCommitted;
        public event Action? DialogueCommitted;
        private ICommand? dialogueCommand;

        public ICommand DialogueCommand
        {
            get
            {
                if (dialogueCommand == null)
                {
                    dialogueCommand = new RelayCommand(null,
                        async p =>
                        {
                            await CommitCommand(p);
                            DialogueCommitted?.Invoke();
                        });
                }
                return dialogueCommand;
            }
        }

        protected abstract Task CommitCommand(object? p);

        protected void OnCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
