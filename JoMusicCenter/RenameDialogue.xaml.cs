using JoMusicCenter.Utilities;
using JoMusicCenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JoMusicCenter
{
    /// <summary>
    /// RenameDialogue.xaml 的交互逻辑
    /// </summary>
    public partial class RenameDialogue : Window
    {
        public RenameDialogue(ICloseable dataContext, EventHandler? closeHandler = null)
        {
            InitializeComponent();
            DataContext = dataContext;
            dataContext.CloseRequest += (s, e) => Close();
            if (closeHandler != null)
            {
                dataContext.CloseRequest += closeHandler;
            }
        }
    }
}
