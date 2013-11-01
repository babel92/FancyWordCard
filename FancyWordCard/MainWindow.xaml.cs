using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
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

        private DoubleAnimation hidanim1;
        private ThicknessAnimation hidanim2;
        private DoubleAnimation curanim1;
        private ThicknessAnimation curanim2;

        private void SwitchLabel()
        {

            CurrentShownLabel.BeginAnimation(TextBlock.OpacityProperty, hidanim1);
            CurrentShownLabel.BeginAnimation(TextBlock.MarginProperty, hidanim2);

            if (CurrentShownLabel == label1)
                CurrentShownLabel = label2;
            else
                CurrentShownLabel = label1;

            CurrentShownLabel.BeginAnimation(TextBlock.OpacityProperty, curanim1);
            CurrentShownLabel.BeginAnimation(TextBlock.MarginProperty, curanim2);
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
            CurrentShownLabel = label1;

            MainColor = Color.FromArgb(0xaa, 0xc8, 0xc8, 0xff);
            string color = GetSetting("Color");
            if (color != null)
            {
                string[] rgb = color.Split(' ');
                MainColor.R = Byte.Parse(rgb[0]);
                MainColor.G = Byte.Parse(rgb[1]);
                MainColor.B = Byte.Parse(rgb[2]);
            }
            RSldr.Value = MainColor.R;
            GSldr.Value = MainColor.G;
            BSldr.Value = MainColor.B;

            string interval=GetSetting("Interval");
            if(interval==null)
            {
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
            var curanim2 = new ThicknessAnimation(new Thickness(200, 10, 0, 0), new Thickness(10, 10, 0, 0), new Duration(TimeSpan.FromSeconds(0.3)));

            CurrentShownLabel.BeginAnimation(TextBlock.OpacityProperty, curanim1);
            CurrentShownLabel.BeginAnimation(TextBlock.MarginProperty, curanim2);
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
            if (dict == null||!File.Exists(dict))
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

            hidanim1 = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.1)));
            hidanim2 = new ThicknessAnimation(new Thickness(10, 10, 0, 0), new Thickness(-200, 10, 0, 0), new Duration(TimeSpan.FromSeconds(0.3)));

            curanim1 = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.3)));
            curanim2 = new ThicknessAnimation(new Thickness(200, 10, 0, 0), new Thickness(10, 10, 0, 0), new Duration(TimeSpan.FromSeconds(0.3)));

            InitUI();
            label1.Text = Dict.GetRandomEntry();

            TimerCounter = Interval;
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

        private void UpdateBGColor()
        {
            this.Background = new SolidColorBrush(MainColor);
        }

        private void RSldr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color clr=new Color();
            byte tmp = (byte)e.NewValue;
            clr.A = 0xff;
            clr.R = (byte)0xff;
            clr.G = (byte)(0xff - tmp);
            clr.B = (byte)(0xff - tmp);
            MainColor.R = tmp;
            RLabel.Content = tmp;
            RLabel.Background = new SolidColorBrush(clr);
            UpdateBGColor();
        }

        private void GSldr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color clr = new Color();
            byte tmp = (byte)e.NewValue;
            clr.A = 0xff;
            clr.G = (byte)0xff;
            clr.R = (byte)(0xff - tmp);
            clr.B = (byte)(0xff - tmp);
            MainColor.G = tmp;
            GLabel.Content = tmp;
            GLabel.Background = new SolidColorBrush(clr);
            UpdateBGColor();
        }

        private void BSldr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color clr = new Color();
            byte tmp = (byte)e.NewValue;
            clr.A = 0xff;
            clr.B = (byte)0xff;
            clr.G = (byte)(0xff - tmp);
            clr.R = (byte)(0xff - tmp);
            MainColor.B = tmp;
            BLabel.Content = tmp;
            BLabel.Background = new SolidColorBrush(clr);
            UpdateBGColor();
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {
            SetSetting("Interval", ((double)Interval / 10).ToString());
            SetSetting("Pos", String.Format("{0} {1}", this.Left, this.Top));
            SetSetting("Size", String.Format("{0} {1}", this.Width, this.Height));
            SetSetting("Color", String.Format("{0} {1} {2}", MainColor.R, MainColor.G, MainColor.B));
        }

        private void Window_Drop_1(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            SetSetting("Dict", files[0]);
            Dict = new Dictionary(files[0]);
        }
    }
}
