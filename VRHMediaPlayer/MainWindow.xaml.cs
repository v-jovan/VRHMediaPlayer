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
    /// Created by: Jovan Vrhovac
    /// Subject: Human-Computer Interaction
    /// </summary>

    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer timer;
        private readonly OpenFileDialog ofd_one, ofd_playlist;

        private bool mediaPlayerIsPlaying = false,
                     repeat = false,
                     mute = false,
                     previous = false,
                     fullscreen = false;

        private double currentVolume = 0;
        private readonly List<string> mediaInPlaylist = new();

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
                        mediaInPlaylist.Add(song);
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
                    if (mediaInPlaylist.ElementAt(Playlist.SelectedIndex).EndsWith(".mp4") ||
                    mediaInPlaylist.ElementAt(Playlist.SelectedIndex).EndsWith(".ogg"))
                    {
                        Playlist.Visibility = Visibility.Hidden;
                        Notes.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        Playlist.Visibility = Visibility.Visible;
                        Notes.Visibility = Visibility.Visible;
                    }
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
            MediaPlayer.Stop();
            Slider.Value = 0;
            Slider.IsEnabled = false;
            PlayPauseButton.Content = FindResource("Play");
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            bool? dialog = ofd_playlist.ShowDialog();
            if (dialog.HasValue && dialog.Value)
            {
                string[] mediaFile = ofd_playlist.FileNames;
                mediaInPlaylist.Clear();
                mediaInPlaylist.AddRange(mediaFile);

                string[] lines = ofd_playlist.SafeFileNames;
                Playlist.Items.Clear();
                foreach (var line in lines)
                {
                    Playlist.Items.Add(System.IO.Path.GetFileNameWithoutExtension(line));
                }

                Playlist.SelectedItem = Playlist.Items.GetItemAt(0);
            }

            mediaPlayerIsPlaying = true;

            if (MediaPlayer.Source != null)
            {
                Splash.Visibility = Visibility.Hidden;
                if (mediaInPlaylist.ElementAt(Playlist.SelectedIndex).EndsWith(".mp4") ||
                    mediaInPlaylist.ElementAt(Playlist.SelectedIndex).EndsWith(".ogg"))
                {
                    Playlist.Visibility = Visibility.Hidden;
                    Notes.Visibility = Visibility.Hidden;
                }
                else
                {
                    Playlist.Visibility = Visibility.Visible;
                    Notes.Visibility = Visibility.Visible;
                }
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
                MuteButton.BorderBrush = Brushes.Red;
            }
            else
            {
                mute = false;
                VolumeSlider.Value = currentVolume;
                MediaPlayer.Volume = currentVolume;

                if (theme == "default")
                {
                    MuteButton.ClearValue(Button.BackgroundProperty);
                    MuteButton.ClearValue(Button.BorderBrushProperty);
                }
                else if (theme == "tomato")
                {
                    MuteButton.Background = Brushes.Tomato;
                    MuteButton.BorderBrush = Brushes.Tomato;
                }
                else
                {
                    MuteButton.Background = Brushes.Lime;
                    MuteButton.BorderBrush = Brushes.Lime;
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


            if ((Slider.Value == Slider.Maximum) && repeat && (PlayPauseButton.Content == FindResource("Pause")))
            {
                MediaPlayer.Position = TimeSpan.Zero;
                MediaPlayer.Play();
            }
        }

        private void Playlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Playlist.Items.IsEmpty)
            {
                return;
            }

            this.Title = TITLE + " - " + Playlist.SelectedItem.ToString();
            previous = false;
            Slider.Minimum = 0;
            PlayPauseButton.Content = FindResource("Pause");
            Slider.IsEnabled = true;
            Splash.Visibility = Visibility.Hidden;

            // the playlist has to be hidden while a video is playing
            if (mediaInPlaylist.ElementAt(Playlist.SelectedIndex).EndsWith(".mp4") ||
                mediaInPlaylist.ElementAt(Playlist.SelectedIndex).EndsWith(".ogg"))
            {
                Playlist.Visibility = Visibility.Hidden;
                Notes.Visibility = Visibility.Hidden;
            }
            else
            {
                Playlist.Visibility = Visibility.Visible;
                Notes.Visibility = Visibility.Visible;
            }

            MediaPlayer.Source = new Uri(mediaInPlaylist.ElementAt(Playlist.SelectedIndex));
            MediaPlayer.Play();
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
                Playlist.SelectedIndex++;
            }
            else
            {
                Playlist.SelectedIndex = 0;
            }
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            if (!repeat)
            {
                RepeatButton.Background = Brushes.Red;
                RepeatButton.BorderBrush = Brushes.Red;
                repeat = true;
            }
            else
            {
                if (theme == "default")
                {
                    RepeatButton.ClearValue(Button.BackgroundProperty);
                    RepeatButton.ClearValue(Button.BorderBrushProperty);
                }
                else if (theme == "tomato")
                {
                    RepeatButton.Background = Brushes.Tomato;
                    RepeatButton.BorderBrush = Brushes.Tomato;
                }
                else
                {
                    RepeatButton.Background = Brushes.Lime;
                    RepeatButton.BorderBrush = Brushes.Lime;
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
                PlayerGrid.Margin = new Thickness(0, 30, 0, 0);
            }

            fullscreen = !fullscreen;
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            MediaPlayer.Pause();
        }

        private void Slider_Dragging(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            int position = Convert.ToInt32(Slider.Value);
            MediaPlayer.Position = new TimeSpan(0, 0, 0, position, 0);
            MediaPlayer.Play();
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (repeat && (PlayPauseButton.Content == FindResource("Pause")))
            {
                MediaPlayer.Position = TimeSpan.Zero;
                MediaPlayer.Play();
            }
            else
            {
                MediaPlayer.Position = TimeSpan.Zero;
                Slider.Value = 0;
                Next_Click(sender, e);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("VRHPlayer is a simple media player created in WPF and C#.", "About the player...", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // as it's a *simple* media player, the themes are made simple
        private void DefaultTheme_Click(object sender, RoutedEventArgs e)
        {
            TomatoTheme.IsChecked = false;
            LimeTheme.IsChecked = false;
            theme = "default";

            PlayPauseButton.ClearValue(Button.BackgroundProperty);
            PlayPauseButton.ClearValue(Button.BorderBrushProperty);
            PreviousButton.ClearValue(Button.BackgroundProperty);
            PreviousButton.ClearValue(Button.BorderBrushProperty);
            StopButton.ClearValue(Button.BackgroundProperty);
            StopButton.ClearValue(Button.BorderBrushProperty);
            NextButton.ClearValue(Button.BackgroundProperty);
            NextButton.ClearValue(Button.BorderBrushProperty);
            RepeatButton.ClearValue(Button.BackgroundProperty);
            RepeatButton.ClearValue(Button.BorderBrushProperty);
            OpenButton.ClearValue(Button.BackgroundProperty);
            OpenButton.ClearValue(Button.BorderBrushProperty);
            FullscreenButton.ClearValue(Button.BackgroundProperty);
            FullscreenButton.ClearValue(Button.BorderBrushProperty);
            MuteButton.ClearValue(Button.BackgroundProperty);
            MuteButton.ClearValue(Button.BorderBrushProperty);

            VolumeSlider.ClearValue(Slider.ForegroundProperty);
            Slider.ClearValue(Slider.ForegroundProperty);
        }

        private void TomatoTheme_Click(object sender, RoutedEventArgs e)
        {
            DefaultTheme.IsChecked = false;
            LimeTheme.IsChecked = false;
            theme = "tomato";

            PlayPauseButton.Background = Brushes.Tomato;
            PlayPauseButton.BorderBrush = Brushes.Tomato;
            PreviousButton.Background = Brushes.Tomato;
            PreviousButton.BorderBrush = Brushes.Tomato;
            StopButton.Background = Brushes.Tomato;
            StopButton.BorderBrush = Brushes.Tomato;
            NextButton.Background = Brushes.Tomato;
            NextButton.BorderBrush = Brushes.Tomato;
            RepeatButton.Background = Brushes.Tomato;
            RepeatButton.BorderBrush = Brushes.Tomato;
            OpenButton.Background = Brushes.Tomato;
            OpenButton.BorderBrush = Brushes.Tomato;
            FullscreenButton.Background = Brushes.Tomato;
            FullscreenButton.BorderBrush = Brushes.Tomato;
            MuteButton.Background = Brushes.Tomato;
            MuteButton.BorderBrush = Brushes.Tomato;

            VolumeSlider.Foreground = Brushes.Tomato;
            Slider.Foreground = Brushes.Tomato;
        }

        private void LimeTheme_Click(object sender, RoutedEventArgs e)
        {
            DefaultTheme.IsChecked = false;
            TomatoTheme.IsChecked = false;
            theme = "lime";

            PlayPauseButton.Background = Brushes.Lime;
            PlayPauseButton.BorderBrush = Brushes.Lime;
            PreviousButton.Background = Brushes.Lime;
            PreviousButton.BorderBrush = Brushes.Lime;
            StopButton.Background = Brushes.Lime;
            StopButton.BorderBrush = Brushes.Lime;
            NextButton.Background = Brushes.Lime;
            NextButton.BorderBrush = Brushes.Lime;
            RepeatButton.Background = Brushes.Lime;
            RepeatButton.BorderBrush = Brushes.Lime;
            OpenButton.Background = Brushes.Lime;
            OpenButton.BorderBrush = Brushes.Lime;
            FullscreenButton.Background = Brushes.Lime;
            FullscreenButton.BorderBrush = Brushes.Lime;
            MuteButton.Background = Brushes.Lime;
            MuteButton.BorderBrush = Brushes.Lime;

            VolumeSlider.Foreground = Brushes.Lime;
            Slider.Foreground = Brushes.Lime;
        }
    }
}
