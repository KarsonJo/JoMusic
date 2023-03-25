//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Controls.Primitives;
//using System.Windows.Input;

//namespace JoMusicCenter
//{
//    public static class SelectItemOnClickButton
//    {
//        public static readonly DependencyProperty EnableProperty = DependencyProperty.RegisterAttached(
//            "Enable",
//            typeof(bool),
//            typeof(SelectItemOnClickButton),
//            new FrameworkPropertyMetadata(false, OnEnableChanged));


//        public static bool GetEnable(FrameworkElement frameworkElement)
//        {
//            return (bool)frameworkElement.GetValue(EnableProperty);
//        }


//        public static void SetEnable(FrameworkElement frameworkElement, bool value)
//        {
//            frameworkElement.SetValue(EnableProperty, value);
//        }


//        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            if (d is ListViewItem listBoxItem)
//                listBoxItem.PreviewGotKeyboardFocus += ListBoxItem_PreviewGotKeyboardFocus;
//        }

//        private static void ListBoxItem_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
//        {
//            var listBoxItem = (ListViewItem)sender;
//            listBoxItem.IsSelected = true;
//        }
//    }
//}
