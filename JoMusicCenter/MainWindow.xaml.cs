using JoMusicCenter.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JoMusicCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly System.Windows.Threading.DispatcherTimer m_ClosePopupTimer = new () { Interval = TimeSpan.FromMilliseconds(1) };
        MainWindowViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            new WindowResizer(this);
            viewModel = (MainWindowViewModel)DataContext;

            m_ClosePopupTimer.Tick += ClosePopupTimer_Tick;

            Closing += viewModel.MainWindow_Closing;
        }
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var dialogue = new SettingsWindow();
            dialogue.Owner = this;
            dialogue.Show();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                //BorderThickness = new System.Windows.Thickness(0);
                WindowState = WindowState.Normal;

            }
            else
            {
                //https://stackoverflow.com/questions/29391063/wpf-maximized-window-bigger-than-screen
                //https://www.reddit.com/r/csharp/comments/921k9l/fixing_8_pixel_overhang_on_maximized_window_state/
                //BorderThickness = new System.Windows.Thickness(7);
                WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //public class BoolToVisibilityConverter : IValueConverter
        //{
        //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //    {
        //        return value != null && (bool)value ? Visibility.Visible : Visibility.Collapsed;
        //    }

        //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            viewModel.MediaProgress = ((Slider)sender).Value;
            viewModel.Dragging = false;
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            viewModel.Dragging = true;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!viewModel.Dragging)
            {
                var v = ((Slider)sender).Value;

                //绑定设置也会唤起ValueChanged事件
                //如果是这种情况（值是相等的），则不重设
                //否则会卡音
                if (Math.Abs(v - viewModel.MediaProgress)> 0.01)
                {
                    Debug.WriteLine("Slider changed");
                    viewModel.MediaProgress = ((Slider)sender).Value;
                }
            }
        }

        //https://stackoverflow.com/questions/653524/selecting-a-textbox-item-in-a-listbox-does-not-change-the-selected-item-of-the-l
        protected void SelectCurrentItem(object sender, KeyboardFocusChangedEventArgs e)
        {
            ListViewItem item = (ListViewItem)sender;
            item.IsSelected = true;
        }

        /// <summary>
        /// MVVM另一种实现绑定事件的方式
        /// https://stackoverflow.com/questions/50944934/interaction-triggers-for-listboxitem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (ListViewItem)sender;
            //防止双击在子物体上
            if (item.DataContext is SongViewModel model)
            {
                model.SwitchSongCommand.Execute(null);
            }
            //((SongViewModel)item.DataContext).SwitchSongCommand.Execute(null);
        }

        //keep popup: https://stackoverflow.com/questions/46671458/wpf-net-popup-open-on-hover-and-keep-open-if-mouse-is-over
        private void ClosePopupTimer_Tick(object? sender, EventArgs e)
        {
            volumePopup.IsOpen = false;
        }

        private void PopupMouseOverControls_MouseEnter(object sender, MouseEventArgs e)
        {
            m_ClosePopupTimer.Stop();
            volumePopup.IsOpen = true;
        }
        private void PopupMouseOverControls_MouseLeave(object sender, MouseEventArgs e)
        {
            m_ClosePopupTimer.Start();
        }
    }
}
