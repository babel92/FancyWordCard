﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using WordCard;


namespace FancyWordCard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TextBlock CurrentShownLabel;
        private Dictionary Dict;
        private int MouseStatus;
        private double MouseX;
        private double MouseY;
        private DispatcherTimer Timer;
        private int TimerCounter;
        private int Interval;

        private Color MainColor;

        private const string RegKeyPrefix = "HKEY_CURRENT_USER\\Software\\FancyWordCard\\";

        private string GetSetting(string Setting)
        {
            object ret = Registry.GetValue(RegKeyPrefix, Setting, null);
            return (string)ret;
        }

        private void SetSetting(string Setting, string Value)
        {
            Registry.SetValue(RegKeyPrefix, Setting, Value);
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SwitchLabel()
        {
            var hidanim1 = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.1)));
            Storyboard.SetTargetName(hidanim1, CurrentShownLabel.Name);
            Storyboard.SetTargetProperty(hidanim1, new PropertyPath(TextBlock.OpacityProperty));

            var hidanim2 = new ThicknessAnimation(new Thickness(10, 10, 0, 0), new Thickness(-200, 10, 0, 0), new Duration(TimeSpan.FromSeconds(0.3)));
            Storyboard.SetTargetName(hidanim2, CurrentShownLabel.Name);
            Storyboard.SetTargetProperty(hidanim2, new PropertyPath(TextBlock.MarginProperty));

            if (CurrentShownLabel == label1)
                CurrentShownLabel = label2;
            else
                CurrentShownLabel = label1;

            var curanim1 = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.3)));
            Storyboard.SetTargetName(curanim1, CurrentShownLabel.Name);
            Storyboard.SetTargetProperty(curanim1, new PropertyPath(TextBlock.OpacityProperty));

            var curanim2 = new ThicknessAnimation(new Thickness(200, 10, 0, 0), new Thickness(10, 10, 0, 0), new Duration(TimeSpan.FromSeconds(0.3)));
            Storyboard.SetTargetName(curanim2, CurrentShownLabel.Name);
            Storyboard.SetTargetProperty(curanim2, new PropertyPath(TextBlock.MarginProperty));

            var story = new Storyboard();
            story.Children.Add(curanim1);
            story.Children.Add(curanim2);
            story.Children.Add(hidanim1);
            story.Children.Add(hidanim2);
            story.Begin(grid);
        }

        private void Show(String text)
        {
            
            SwitchLabel();
            CurrentShownLabel.Text = text;
        }

        private void InitUI()
        {
            label2.Margin = new Thickness(-200, 0, 0, 0);
            label2.Opacity = 0;
            MainColor=Color.FromArgb(0xaa,0xc8,0xc8,0xff);
            CurrentShownLabel = label1;
            this.Background = new SolidColorBrush(MainColor);

            string interval=GetSetting("Interval");
            if(interval==null)
            {
                SetSetting("Inverval",((double)Interval/10).ToString());
                IntSldr.Value = Interval / 5;
            }
            else
            {
                IntSldr.Value = Double.Parse(interval)*2;
            }

            string pos = GetSetting("Pos");
            string size=GetSetting("Size");

            if (pos != null && size != null)
            {
                string[] szarr=size.Split(' ');
                string[] posarr = pos.Split(' ');
                this.Width = Double.Parse(szarr[0]);
                this.Height = Double.Parse(szarr[1]);
                this.Left = Double.Parse(posarr[0]);
                this.Top = Double.Parse(posarr[1]);
            }

            var curanim1 = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.3)));
            Storyboard.SetTargetName(curanim1, CurrentShownLabel.Name);
            Storyboard.SetTargetProperty(curanim1, new PropertyPath(Label.OpacityProperty));

            var curanim2 = new ThicknessAnimation(new Thickness(200, 10, 0, 0), new Thickness(10, 10, 0, 0), new Duration(TimeSpan.FromSeconds(0.3)));
            Storyboard.SetTargetName(curanim2, CurrentShownLabel.Name);
            Storyboard.SetTargetProperty(curanim2, new PropertyPath(Label.MarginProperty));

            var story = new Storyboard();
            story.Children.Add(curanim1);
            story.Children.Add(curanim2);
            story.Begin(grid);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (TimerCounter > 0)
                TimerCounter--;
            else 
            {
                Show(Dict.GetRandomEntry());
                TimerCounter = Interval;
            }
            
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            string dict = GetSetting("Dict");
            if (dict == null)
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Title = "Please select dictionary file";
                dlg.Filter = "All files (*.*)|*.*";
                Nullable<bool> result = dlg.ShowDialog();

                // Get the selected file name and display in a TextBox 
                if (result == true)
                {
                    // Open document 
                    dict = dlg.FileName;
                    SetSetting("Dict", dict);
                }
            }
            
            Dict = new Dictionary(dict);

            Interval = 20;
            InitUI();
            label1.Text = Dict.GetRandomEntry();

            TimerCounter = 10;
            Timer = new System.Windows.Threading.DispatcherTimer();
            Timer.Tick += new EventHandler(dispatcherTimer_Tick);
            Timer.Interval = new TimeSpan(1000000);
            Timer.Start();
        }

        private void Window_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            label1.Width = label2.Width = e.NewSize.Width-20;
            label1.Height = label2.Height = e.NewSize.Height-20;
        }

        private void Window_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            MouseStatus = 1;
            MouseX = e.GetPosition(this).X;
            MouseY = e.GetPosition(this).Y;
        }

        private void Window_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            MouseStatus = 0;
        }

        private void Window_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (MouseStatus == 1)
            {
                this.Left += e.GetPosition(this).X - MouseX;
                this.Top += e.GetPosition(this).Y - MouseY;
            }
        }

        private void Window_MouseRightButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            menu.IsOpen = true;
        }

        private void Window_MouseEnter_1(object sender, MouseEventArgs e)
        {
            Timer.Stop();
            Color target=MainColor;
            target.A=0xff;
            ColorAnimation opanim = new ColorAnimation(target ,new Duration(TimeSpan.FromSeconds(0.2)));
            this.Background.BeginAnimation(SolidColorBrush.ColorProperty, opanim);
        }

        private void Window_MouseLeave_1(object sender, MouseEventArgs e)
        {
            Timer.Start();
            ColorAnimation opanim = new ColorAnimation(MainColor, new Duration(TimeSpan.FromSeconds(0.2)));
            this.Background.BeginAnimation(SolidColorBrush.ColorProperty, opanim);
        }

        private void MenuItem_Checked_1(object sender, RoutedEventArgs e)
        {
            this.Topmost = true;
        }

        private void MenuItem_Unchecked_1(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            SetSetting("Interval", ((double)Interval / 10).ToString());
            SetSetting("Pos",String.Format("{0} {1}",this.Left,this.Top));
            SetSetting("Size",String.Format("{0} {1}",this.Width,this.Height));
            Application.Current.Shutdown();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Please select dictionary file";
            dlg.Filter = "All files (*.*)|*.*";
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                SetSetting("Dict", dlg.FileName);
                Dict = new Dictionary(dlg.FileName);
            }
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IntInd==null)
                return;
            Interval = (int)(e.NewValue * 5);
            IntInd.Content = e.NewValue / 2 + "s";
        }
    }
}
