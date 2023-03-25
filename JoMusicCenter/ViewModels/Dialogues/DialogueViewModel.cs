using JoMusicCenter.Commands;
using JoMusicCenter.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace JoMusicCenter.ViewModels
{
    /// <summary>
    /// 一个携带主输入框的对话框数据
    /// </summary>
    public abstract class DialogueViewModel : CommitableObject
    {

        protected InputHelper mainInput = new();

        public string InputText
        {
            get => mainInput.InputText;
            set
            {
                mainInput.InputText = value;

            }
        }

        private string? mainNotification;

        public string? MainNotification
        {
            get => mainNotification;
            set
            {
                mainNotification = value;
                OnPropertyChanged(nameof(MainNotification));
            }
        }

        public virtual bool Valid => mainInput.Valid;


        public DialogueViewModel()
        {
            MainNotification = "";

            mainInput.InputChanged += () =>
            {
                OnPropertyChanged(nameof(InputText));
                MainInputChanged();
                OnPropertyChanged(nameof(Valid));
            };
        }

        protected virtual void MainInputChanged()
        {

        }
    }
}
