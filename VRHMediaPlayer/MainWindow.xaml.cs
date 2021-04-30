using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace VRHMediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer timer;
        private readonly OpenFileDialog ofd;
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;

        public MainWindow()
        {
            InitializeComponent();
            MediaPlayer.Volume = VolumeSlider.Value;

            ofd = new OpenFileDialog
            {
                Multiselect = false,
                RestoreDirectory = true,
                Title = "Open Media File...",
                Filter = "Audio Files|*.mp3|Video Files|*.mp4;*.avi|All Files|*.*",
                FilterIndex = 3
            };

            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if ((MediaPlayer.Source != null) && (MediaPlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                Slider.Minimum = 0;
                Slider.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                Slider.Value = MediaPlayer.Position.TotalSeconds;
           
                CurrentTime.Content = MediaPlayer.Position.TotalMinutes.ToString("00") + ":" + MediaPlayer.Position.Seconds.ToString("00");
                Duration.Content = MediaPlayer.NaturalDuration.TimeSpan.TotalMinutes.ToString("00") + ":" + MediaPlayer.NaturalDuration.TimeSpan.Seconds.ToString("00");
            }

        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Play();
            if (mediaPlayerIsPlaying)
            {
                Splash.Visibility = Visibility.Hidden;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            CurrentTime.Content = "--:--";
            Duration.Content = "--:--";
            mediaPlayerIsPlaying = false;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            bool? dr = ofd.ShowDialog();
            if (dr.HasValue && dr.Value)
            {
                MediaPlayer.Source = new Uri(ofd.FileName);
            }
            mediaPlayerIsPlaying = true;
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            VolumeSlider.Value = VolumeSlider.Minimum;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaPlayer.Volume = VolumeSlider.Value;
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            MediaPlayer.Position = TimeSpan.FromSeconds(Slider.Value);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurrentTime.Content = TimeSpan.FromSeconds(Slider.Value).ToString(@"mm\:ss");
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
        }
    }
}
