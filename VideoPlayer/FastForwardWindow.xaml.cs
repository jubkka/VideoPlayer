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
using System.Xml.Linq;

namespace VideoPlayer
{
    /// <summary>
    /// Логика взаимодействия для FastForwardWindow.xaml
    /// </summary>
    public partial class FastForwardWindow : Window
    {
        public MainWindow mainWindow { get; set; }
        public MediaElement mediaPlayer { get; set; }

        public Duration time { get; set; }

        private TimeSpan TimeSpanDuration;

        private TimeSpan goTime = new TimeSpan();

        private int hours;
        private int minutes;
        private int seconds;

        public FastForwardWindow()
        {
            InitializeComponent();
        }

        private void Hours_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            string text = textBox.Text;

            if (int.TryParse(text, out int result) && text.Length <= TimeSpanDuration.TotalHours.ToString().Length && result <= TimeSpanDuration.TotalHours)
            {
                if (hours <= TimeSpanDuration.TotalHours) hours = result;
            }
            else
            {
                hours= 0;
                textBox.Text = "00";
            }
        }
        private void Minutes_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            string text = textBox.Text;

            if (int.TryParse(text, out int result) && text.Length <= TimeSpanDuration.TotalMinutes.ToString().Length && result <= TimeSpanDuration.TotalMinutes)
            {
                if (minutes <= TimeSpanDuration.TotalMinutes) minutes = result;
            }
            else
            {
                minutes = 0;
                textBox.Text = "00";
            }
        }
        private void Seconds_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            string text = textBox.Text;

            if (int.TryParse(text, out int result) && text.Length <= TimeSpanDuration.TotalSeconds.ToString().Length && result <= TimeSpanDuration.TotalSeconds)
            {
                if (seconds <= TimeSpanDuration.TotalSeconds) seconds = result;
            }
            else
            {
                seconds = 0;
                textBox.Text = "00";
            }
        }

        private void FastForwadClose(object sender, EventArgs e) => mainWindow.IsEnabled = true;

        private void ButtonTime_Click(object sender, RoutedEventArgs e)
        {
            goTime = new TimeSpan(hours, minutes, seconds);

            mediaPlayer.Position = goTime;

            Close();
        }

        private void FastForwardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TimeSpanDuration = time.TimeSpan;
        }
    }


}
