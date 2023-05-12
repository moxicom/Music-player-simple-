using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using TagLib;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class Song
    {
        public string Path { get; set; }
        public string? Title { get; set; }
        public TimeSpan Length { get; set; }
        public string? Album { get; set; }
        public string[]? Artists { get; set; }
        public string? Genre { get; set; }
        public string? Location { get; set; }

        public int Seconds { get; set; }

        public IPicture? Picture { get; set; }

        public Song(string Location)
        {
            this.Location = Location;
            var mp3 = TagLib.File.Create(Location);

            this.Title = mp3.Tag.Title;
            this.Album = mp3.Tag.Album;
            this.Artists = mp3.Tag.Performers;
            this.Genre = mp3.Tag.FirstGenre;
            this.Length = mp3.Properties.Duration;
            this.Picture = mp3.Tag.Pictures.FirstOrDefault();
            this.Path = Location;
            this.Seconds = (int)mp3.Properties.Duration.TotalSeconds;
        }
    }

    public partial class MainWindow : Window
    {
        private List<Song> _songList = new List<Song>();
        private bool _isPlaying;
        private bool _isTrackChosen;
        private MediaPlayer _player;

        public MainWindow()
        {
            InitializeComponent();
            _isPlaying = false;
            _isTrackChosen = false;
            _player = new MediaPlayer();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;
            timer.Start();


        }

        void TimerTick(object sender, EventArgs e)
        {
            if (_player.Source != null)
            {
                CurrentTime.Text = _player.Position.ToString(@"mm\:ss");
                SongSlider.Value = _player.Position.TotalSeconds;
            }
        }


        private void PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                try
                {
                    _player.Pause();
                    _isPlaying = false;
                }
                catch { }

            }
            else
            {
                try
                {
                    if (!_isTrackChosen && _songList.Count > 0)
                    {
                        SetSong(_songList[0]);
                        SongsListBox.SelectedIndex = 0;
                    }
                    _player.Play();
                    _isPlaying = true;
                }
                catch { }
            }
        }

        private void PreviousTrackBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SongsListBox.SelectedIndex = SongsListBox.SelectedIndex - 1;
            }
            catch { }
        }

        private void NextTrackBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SongsListBox.SelectedIndex = (1 + SongsListBox.SelectedIndex) % _songList.Count;
            }
            catch { }
        }

        private void SongSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _player.Position = TimeSpan.FromSeconds(SongSlider.Value);
        }

        private void SongsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                SetSong(_songList[SongsListBox.SelectedIndex]);
                // System.Windows.Forms.MessageBox.Show(SongsListBox.SelectedIndex.ToString());
                _isPlaying = false;
            }
            catch { }
            
        }

        private void SetSong(Song song)
        {
            AllTime.Text = "123123";
            _player.Open(new Uri(song.Path));
            _isTrackChosen = true;
            SongSlider.Minimum = 0;
            SongSlider.Maximum = song.Seconds;
            AllTime.Text = song.Length.ToString().Substring(3, 5);
            SongSlider.Value = 0;
            if (song.Title != null)
            {
                TitleTextBlock.Text = song.Title;
            }
            else
            {
                TitleTextBlock.Text = "Missed";
            }
            int artistsAmount = song.Artists.Length;
            if (artistsAmount > 0)
            {
                GroupTextBlock.Text = "";
                for (int i = 0; i < artistsAmount; ++i)
                {
                    GroupTextBlock.Text += song.Artists[i];
                    if (i != artistsAmount - 1)
                        TitleTextBlock.Text += ", ";
                }
            }
            else
            {
                GroupTextBlock.Text = "Missed";
            }

            if (song.Picture != null)
            {
                MemoryStream stream = new MemoryStream(song.Picture.Data.Data);
                BitmapImage cover = new BitmapImage();
                cover.BeginInit();
                cover.StreamSource = stream;
                cover.EndInit();

                // установите изображение в Image
                SongImage.Source = cover;
            }
            else
            {
                SongImage.Source = new BitmapImage(new Uri("Images/missed.png", UriKind.Relative));
            }
        }

        private void ChooseFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
            {
                string[] files = Directory.GetFiles(folderBrowserDialog.SelectedPath);
                SongsListBox.Items.Clear();
                _songList.Clear();
                foreach (string file in files)
                {
                    // System.Windows.Forms.MessageBox.Show(file);
                    UpdateSongList(file);
                }
            }
        }

        private void UpdateSongList(string filePath)
        {
            try
            {
                Song newSong = new Song(filePath);
                string s = "";
                if (newSong.Artists.Length > 0)
                {
                    for (int i = 0; i < newSong.Artists.Length; ++i)
                    {
                        s += newSong.Artists[i];
                        if (i != newSong.Artists.Length - 1)
                            s += ", ";
                    }
                    s += " - ";
                }
                else
                {
                    s = "Missed - ";
                }

                if (newSong.Title != null && newSong.Title != "")
                {
                    s += newSong.Title;
                }
                else
                {
                    s += "Missed";
                }
                _songList.Add(newSong);
                // System.Windows.Forms.MessageBox.Show(s);
                SongsListBox.Items.Add(s);
            }
            catch { }
        }

        private void AddTrackBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isTrackChosen = false;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                DialogResult result = openFileDialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string file = openFileDialog.FileName;
                    // System.Windows.Forms.MessageBox.Show(file);
                    UpdateSongList(file);
                }
            }
            catch { }
        }

        private void SavePlaylistBtn_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string selectedFilePath = folderDialog.SelectedPath;
                    string newFolderPath = System.IO.Path.Combine(selectedFilePath, "Your playlist");
                    System.IO.Directory.CreateDirectory(newFolderPath);

                    foreach (var file in _songList)
                    {
                        string fileName = System.IO.Path.GetFileName(file.Path);
                        string destinationPath = System.IO.Path.Combine(newFolderPath, fileName);
                        System.IO.File.Copy(file.Path, destinationPath);
                    }
                    System.Windows.Forms.MessageBox.Show("Плейлист сохранен в " + newFolderPath);
                }
                catch { }
            }
        }
    }
}
