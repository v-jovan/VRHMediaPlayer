using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly OpenFileDialog ofd_one, ofd_playlist;
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;
        private bool repeat = false;
        private bool mute = false;
        private double currentVolume = 0;
        private List<string> songs_in_playlist = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            MediaPlayer.Volume = VolumeSlider.Value;

            ofd_playlist = new OpenFileDialog
            {
                Multiselect = true,
                RestoreDirectory = true,
                Title = "Open Files...",
                Filter = "Audio Files|*.mp3|Video Files|*.mp4;*.avi|All Files|*.*",
                FilterIndex = 1
            };

            ofd_one = new OpenFileDialog
            {
                Multiselect = false,
                RestoreDirectory = true,
                Title = "Open Music Files...",
                Filter = "Audio Files|*.mp3",
                FilterIndex = 1
            };

            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();

            Slider.IsEnabled = false;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if ((MediaPlayer.Source != null) && (MediaPlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider) && (mediaPlayerIsPlaying))
            {
                Slider.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                Slider.Value = MediaPlayer.Position.TotalSeconds;
           
                CurrentTime.Content = MediaPlayer.Position.ToString(@"mm\:ss");
                Duration.Content = MediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }
            else
            {
                if (repeat)
                {
                    MediaPlayer.Position = TimeSpan.Zero;
                    MediaPlayer.Play();
                }
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (PlayPauseButton.Content == FindResource("Play"))
            {
                if ((Playlist.Items.IsEmpty) && (!mediaPlayerIsPlaying))
                {
                    bool? dr = ofd_one.ShowDialog();
                    if (dr.HasValue && dr.Value)
                    {
                        Playlist.Items.Add(System.IO.Path.GetFileNameWithoutExtension(ofd_one.FileName));
                        MediaPlayer.Source = new Uri(ofd_one.FileName);
                    }
                    mediaPlayerIsPlaying = true;

                }
                Slider.Minimum = 0;
                PlayPauseButton.Content = FindResource("Pause");
                MediaPlayer.Play();
                if (mediaPlayerIsPlaying)
                {
                    Slider.IsEnabled = true;
                    Splash.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                PlayPauseButton.Content = FindResource("Play");
                MediaPlayer.Pause();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            CurrentTime.Content = "--:--";
            Duration.Content = "--:--";
            mediaPlayerIsPlaying = false;
            Slider.Value = 0;
            Slider.IsEnabled = false;
            PlayPauseButton.Content = FindResource("Play");
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            bool? dr = ofd_playlist.ShowDialog();
            if (dr.HasValue && dr.Value)
            {

                string[] songs = ofd_playlist.FileNames;
                songs_in_playlist.AddRange(songs);

                string[] lines = ofd_playlist.SafeFileNames;
                foreach (var line in lines)
                {
                    Playlist.Items.Add(System.IO.Path.GetFileNameWithoutExtension(line));
                }
                MediaPlayer.Source = new Uri(ofd_playlist.FileName);
            }
            mediaPlayerIsPlaying = true;

            if (MediaPlayer.Source != null)
            {
                Splash.Visibility = Visibility.Hidden;
            }

        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!mute)
            {
                currentVolume = VolumeSlider.Value;
                mute = true;
                VolumeSlider.Value = VolumeSlider.Minimum;
                MediaPlayer.Volume = VolumeSlider.Minimum;
                MuteButton.Background = Brushes.Red;
            }
            else
            {
                mute = false;
                VolumeSlider.Value = currentVolume;
                MediaPlayer.Volume = currentVolume;
                MuteButton.ClearValue(Button.BackgroundProperty);
            }
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

        private void Playlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MediaPlayer.Source = new Uri(songs_in_playlist.ElementAt(Playlist.SelectedIndex));
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (Playlist.SelectedIndex > 0)
            {
                Playlist.SelectedItem = --Playlist.SelectedIndex;
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (Playlist.SelectedIndex < Playlist.Items.Count - 1)
            {
                Playlist.SelectedItem = ++Playlist.SelectedIndex;
            }
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            PlayPauseButton.Content = FindResource("Play");
        }

    }

}
