using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        private bool mediaPlayerIsPlaying = false,
                     repeat = false,
                     mute = false;

        private double currentVolume = 0;
        private readonly List<string> songs_in_playlist = new();
        private bool previous = false,
                     fullscreen = false;

        private string theme = "default";
        private readonly string TITLE = "VRHPlayer";

        public MainWindow()
        {
            InitializeComponent();

            ofd_playlist = new OpenFileDialog // file dialog for opening multiple media files
            {
                Multiselect = true,
                RestoreDirectory = true,
                Title = "Open Files...",
                Filter = "Media Files|*.mp3;*.avi;*.mp4;*.ogg|Audio Files|*.mp3|Video Files|*.mp4;*.avi|All Files|*.*",
                FilterIndex = 1
            };

            ofd_one = new OpenFileDialog // file dialog for opening only one file
            {
                Multiselect = false,
                RestoreDirectory = true,
                Title = "Open Media Files...",
                Filter = "Media Files|*.mp3;*.avi;*.mp4;*.ogg",
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
            if ((MediaPlayer.Source != null) && (MediaPlayer.NaturalDuration.HasTimeSpan))
            {
                Slider.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                Slider.Value = MediaPlayer.Position.TotalSeconds;

                CurrentTime.Content = MediaPlayer.Position.ToString(@"mm\:ss");
                Duration.Content = MediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }
            else
            {
                if (repeat) // if repeat is pressed
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
                if (Playlist.Items.IsEmpty)
                {
                    bool? dialog = ofd_one.ShowDialog();
                    if (dialog.HasValue && dialog.Value)
                    {
                        Playlist.Items.Add(System.IO.Path.GetFileNameWithoutExtension(ofd_one.FileName));
                        string song = ofd_one.FileName;
                        songs_in_playlist.Add(song);
                        Playlist.SelectedItem = Playlist.Items.GetItemAt(0);

                        mediaPlayerIsPlaying = true;
                    }
                }

                if (mediaPlayerIsPlaying)
                {
                    Slider.Minimum = 0;
                    PlayPauseButton.Content = FindResource("Pause");
                    MediaPlayer.Play();
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
            MediaPlayer.Close();
            Slider.Value = 0;
            Slider.IsEnabled = false;
            PlayPauseButton.Content = FindResource("Play");
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            bool? dialog = ofd_playlist.ShowDialog();
            if (dialog.HasValue && dialog.Value)
            {
                string[] songs = ofd_playlist.FileNames;
                songs_in_playlist.Clear();
                songs_in_playlist.AddRange(songs);

                string[] lines = ofd_playlist.SafeFileNames;
                Playlist.Items.Clear();
                foreach (var line in lines)
                {
                    Playlist.Items.Add(System.IO.Path.GetFileNameWithoutExtension(line));
                }
                MediaPlayer.Source = new Uri(ofd_playlist.FileName);

                Playlist.SelectedItem = Playlist.Items.GetItemAt(0);
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

                if (theme == "default")
                {
                    MuteButton.ClearValue(Button.BackgroundProperty);
                }
                else if (theme == "tomato")
                {
                    MuteButton.Background = Brushes.Tomato;
                }
                else
                {
                    MuteButton.Background = Brushes.Lime;
                }
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaPlayer.Volume = VolumeSlider.Value;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurrentTime.Content = TimeSpan.FromSeconds(Slider.Value).ToString(@"mm\:ss");
            MediaPlayer.Position = TimeSpan.FromSeconds(Slider.Value);


            if ((Slider.Value == Slider.Maximum) && (repeat))
            {
                MediaPlayer.Position = TimeSpan.Zero;
                MediaPlayer.Play();
            }
        }

        private void Playlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Playlist.Items.IsEmpty)
            {
                MediaPlayer.Source = new Uri(songs_in_playlist.ElementAt(Playlist.SelectedIndex));
                this.Title = TITLE + " - " + Playlist.SelectedItem.ToString();
                previous = false;
                Slider.Minimum = 0;
                PlayPauseButton.Content = FindResource("Pause");
                MediaPlayer.Play();
                Slider.IsEnabled = true;
                Splash.Visibility = Visibility.Hidden;

                // the playlist has to be hidden while a video is playing
                if (songs_in_playlist.ElementAt(Playlist.SelectedIndex).EndsWith(".mp4") ||
                    songs_in_playlist.ElementAt(Playlist.SelectedIndex).EndsWith(".ogg"))
                    Playlist.Visibility = Visibility.Hidden;
                else
                    Playlist.Visibility = Visibility.Visible;
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (Playlist.SelectedIndex == 0)
            {
                MediaPlayer.Position = TimeSpan.Zero;
            }
            else if (!previous)
            {
                MediaPlayer.Position = TimeSpan.Zero;
                previous = true;
            }
            else if (Playlist.SelectedIndex > 0)
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

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            if (!repeat)
            {
                RepeatButton.Background = Brushes.Red;
                repeat = true;
            }
            else
            {
                if (theme == "default")
                {
                    RepeatButton.ClearValue(Button.BackgroundProperty);
                }
                else if (theme == "tomato")
                {
                    RepeatButton.Background = Brushes.Tomato;
                }
                else
                {
                    RepeatButton.Background = Brushes.Lime;
                }
                repeat = false;
            }
        }

        private void Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (!fullscreen)
            {
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
                Menu.Visibility = Visibility.Hidden;
                PlayerGrid.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;
                Menu.Visibility = Visibility.Visible;
                PlayerGrid.Margin = new Thickness(0, 22, 0, 0);
            }

            fullscreen = !fullscreen;
        }

        private void DefaultTheme_Click(object sender, RoutedEventArgs e)
        {
            TomatoTheme.IsChecked = false;
            LimeTheme.IsChecked = false;
            theme = "default";

            PlayPauseButton.ClearValue(Button.BackgroundProperty);
            PreviousButton.ClearValue(Button.BackgroundProperty);
            StopButton.ClearValue(Button.BackgroundProperty);
            NextButton.ClearValue(Button.BackgroundProperty);
            RepeatButton.ClearValue(Button.BackgroundProperty);
            OpenButton.ClearValue(Button.BackgroundProperty);
            FullscreenButton.ClearValue(Button.BackgroundProperty);
            MuteButton.ClearValue(Button.BackgroundProperty);

            VolumeSlider.ClearValue(Slider.ForegroundProperty);
            Slider.ClearValue(Slider.ForegroundProperty);
        }

        private void Slider_Dragging(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            int position = Convert.ToInt32(Slider.Value);
            MediaPlayer.Position = new TimeSpan(0, 0, 0, position, 0);
        }

        // as it's a *simple* media player, the themes are made simple
        private void TomatoTheme_Click(object sender, RoutedEventArgs e)
        {
            DefaultTheme.IsChecked = false;
            LimeTheme.IsChecked = false;
            theme = "tomato";

            PlayPauseButton.Background = Brushes.Tomato;
            PreviousButton.Background = Brushes.Tomato;
            StopButton.Background = Brushes.Tomato;
            NextButton.Background = Brushes.Tomato;
            RepeatButton.Background = Brushes.Tomato;
            OpenButton.Background = Brushes.Tomato;
            FullscreenButton.Background = Brushes.Tomato;
            MuteButton.Background = Brushes.Tomato;

            VolumeSlider.Foreground = Brushes.Tomato;
            Slider.Foreground = Brushes.Tomato;
        }

        private void LimeTheme_Click(object sender, RoutedEventArgs e)
        {
            DefaultTheme.IsChecked = false;
            TomatoTheme.IsChecked = false;
            theme = "lime";

            PlayPauseButton.Background = Brushes.Lime;
            PreviousButton.Background = Brushes.Lime;
            StopButton.Background = Brushes.Lime;
            NextButton.Background = Brushes.Lime;
            RepeatButton.Background = Brushes.Lime;
            OpenButton.Background = Brushes.Lime;
            FullscreenButton.Background = Brushes.Lime;
            MuteButton.Background = Brushes.Lime;

            VolumeSlider.Foreground = Brushes.Lime;
            Slider.Foreground = Brushes.Lime;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("VRHPlayer is a simple media player created in WPF and C#.", "About the player...", MessageBoxButton.OK);
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (repeat)
            {
                MediaPlayer.Position = TimeSpan.Zero;
                MediaPlayer.Play();
            }
            else if ((Playlist.SelectedIndex + 1) == (Playlist.Items.Count))
            {
                MediaPlayer.Stop();
                MediaPlayer.Position = TimeSpan.Zero;
                PlayPauseButton.Content = FindResource("Play");
            }
            else
            {
                Next_Click(sender, e);
            }
        }
    }
}
