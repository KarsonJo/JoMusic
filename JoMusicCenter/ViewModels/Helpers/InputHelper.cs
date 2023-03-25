using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoMusicCenter.ViewModels
{
    public class InputHelper
    {
        public delegate void InputHelpEventHandler();

        public InputHelpEventHandler? InputChanged;

        private string inputText = "";
        public string InputText
        {
            get => inputText;
            set
            {
                if (Equals(inputText, value))
                {
                    return;
                }
                inputText = value;
                InputChanged?.Invoke();
            }
        }

        /// <summary>
        /// 说明输入是否被接受
        /// </summary>
        public bool NoConflicts { get; set; } = true;


        public bool Valid => NoConflicts && !string.IsNullOrEmpty(InputText);
    }
}
