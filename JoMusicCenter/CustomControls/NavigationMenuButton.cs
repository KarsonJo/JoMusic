using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace JoMusicCenter.CustomControls
{
    public class NavigationMenuButton : RadioButton
    {
        
        public Geometry Icon
        {
            get { return (Geometry)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(Geometry), typeof(NavigationMenuButton));





        public ICommand PlayCommand
        {
            get { return (ICommand)GetValue(PlayCommandProperty); }
            set { SetValue(PlayCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlayCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlayCommandProperty =
            DependencyProperty.Register("PlayCommand", typeof(ICommand), typeof(NavigationMenuButton));



        public ICommand UnpinCommand
        {
            get { return (ICommand)GetValue(UnpinCommandProperty); }
            set { SetValue(UnpinCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UnpinCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnpinCommandProperty =
            DependencyProperty.Register("UnpinCommand", typeof(ICommand), typeof(NavigationMenuButton));




    }
}
