using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MenuItem = System.Windows.Controls.MenuItem;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Linq;
using System.Text.Json;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Media;

namespace VideoPlayer
{
    public partial class MainWindow : Window
    {
        private string maxTimeVideo;
        private double previousVolume;

        private DispatcherTimer timer = new DispatcherTimer();

        private string path;
        private string directory;

        private List<string> files;
        private List<string> lastOpenFiles = new List<string>();

        FastForwardWindow fastForwardWindow;

        private int currentPlayingIndex;

        public MainWindow()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromSeconds(1); // интервал срабатывания таймера
            timer.Tick += timer_Tick; // задаем действия
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timelineSlider.Value = Player.Position.TotalSeconds; // двигаем таймлайн вместе с видео
            timeVideo.Content = Player.Position.ToString(@"hh\:mm\:ss") + " / " + maxTimeVideo; // отображаем время
        }

        private void StopVideo(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void PlayVideo(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void PauseVideo(object sender, RoutedEventArgs e)
        {
            Pause();
        }

        private void DecreaseSpeed(object sender, RoutedEventArgs e)
        {
            if (Player.SpeedRatio != 0.25) Player.SpeedRatio -= 0.25;
        }

        private void IncreaseSpeed(object sender, RoutedEventArgs e)
        {
            if (Player.SpeedRatio != 2) Player.SpeedRatio += 0.25;
        }

        private void PreviousVideo(object sender, RoutedEventArgs e)
        {
            if (files != null)
            {
                if (currentPlayingIndex - 1 >= 0)
                {
                    currentPlayingIndex -= 1;
                    Player.Source = new Uri(files[currentPlayingIndex]);
                    Start();
                }
                else
                {
                    currentPlayingIndex = files.Count - 1;
                    Player.Source = new Uri(files[files.Count - 1]);
                    Start();
                }
            }
        }

        private void NextVideo(object sender, RoutedEventArgs e)
        {
            if (files.Count != 0)
            {
                if (currentPlayingIndex + 1 < files.Count)
                {
                    currentPlayingIndex += 1;
                    Player.Source = new Uri(files[currentPlayingIndex]);
                    Start();
                }
                else
                {
                    currentPlayingIndex = 0;
                    Player.Source = new Uri(files[0]);
                    Start();
                }
            }
        }

        private void VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Player != null)
            {
                Player.Volume = Math.Round(sliderVolume.Value) / 100;

                if (Player.Volume > 0) 
                {
                    MuteButton.ToolTip = "Выключить звук";
                    MuteButton.Style = (Style)MuteButton.FindResource("normalButton");
                    muteButtonInMenu.Header = "Выключить звук";

                    volumeButton.Source = BitmapFrame.Create(new Uri(@"E:\Users\zxc\source\repos\VideoPlayer\VideoPlayer\volumeOn.png"));
                } 
                else 
                {
                    MuteButton.ToolTip = "Включить звук";
                    muteButtonInMenu.Header = "Включить звук";
                    MuteButton.Style = (Style)MuteButton.FindResource("selectedButton");

                    volumeButton.Source = BitmapFrame.Create(new Uri(@"E:\Users\zxc\source\repos\VideoPlayer\VideoPlayer\volumeOff.png"));
                }
            }
        }

        private void Mute(object sender, RoutedEventArgs e)
        {
            MuteVideo();
        }

        private void MuteVideo()
        {
            if (Player.Volume == 0) sliderVolume.Value = previousVolume;
            else
            {
                previousVolume = sliderVolume.Value;
                sliderVolume.Value = 0;
            }
        }

        private void media_MediaOpened(object sender, RoutedEventArgs e)
        {
            Logo.Visibility = Visibility.Collapsed;
            Player.Visibility = Visibility.Visible;

            timelineSlider.Value = 0;

            timelineSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
            maxTimeVideo = Player.NaturalDuration.TimeSpan.ToString();
            timeVideo.Content = Player.Position.ToString(@"hh\:mm\:ss") + " / " + maxTimeVideo;
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            Pause();
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Start();
        }


        private void timelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        { 
            Player.Position = TimeSpan.FromSeconds(timelineSlider.Value);
            timeVideo.Content = Player.Position.ToString(@"hh\:mm\:ss") + " / " + maxTimeVideo;
        }

        private void Player_MediaEnded(object sender, RoutedEventArgs e)
        {
            Player.Position = TimeSpan.FromSeconds(0);

            Start();
        }

        private void sliderVolume_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0) sliderVolume.Value += 5;
            else sliderVolume.Value -= 5;
        }

        public void Start()
        {
            if (Player.Source != null)  // проверка, есть ли ссылка на видео
            {
                Player.Play(); //запускаем плеер
                timer.Start(); // запускаем таймер
                StateVideo.Content = "Воспроизведение"; // обновляем строку состояния
                AddLastOpenVideos(Player.Source.ToString(), false); // добавляем последние открытое видео
            }

            //Стили

            PlayButton.Style = (Style)PlayButton.FindResource("selectedButton");
            StopButton.Style = (Style)StopButton.FindResource("normalButton");
            PauseButton.Style = (Style)PauseButton.FindResource("normalButton");
        }

        private void Pause()
        {
            Player.Pause(); //останавливаем плеер
            timer.Stop(); // останавливаем таймер

            //Стили

            PlayButton.Style = (Style)PlayButton.FindResource("normalButton");
            StopButton.Style = (Style)StopButton.FindResource("normalButton");
            PauseButton.Style = (Style)PauseButton.FindResource("selectedButton");

            StateVideo.Content = "Пауза"; // обновляем состояние
        }

        private void Stop()
        {
            timelineSlider.Value = 0; // сбрасываем значение таймлайна

            Player.Stop(); // остановка
            timer.Stop();

            StateVideo.Content = "Остановлено"; //обновляем состояние
            Player.Position = TimeSpan.FromSeconds(0); // начинаем воспроизведение сначало

            //Стили

            PlayButton.Style = (Style)PlayButton.FindResource("normalButton");
            StopButton.Style = (Style)StopButton.FindResource("selectedButton");
            PauseButton.Style = (Style)PauseButton.FindResource("normalButton");
        }

        private void mainArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (timer.IsEnabled) Pause();
            else Start();
        }

        private void OpenFastFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Видео файлы (*.mp4;*.wmv)|*.mp4;*.wmv|Аудио файлы (*.mp3;*.wav)|*.mp3;*.wav"; //фильтры

            if (ofd.ShowDialog() == true)
            {
                path = ofd.FileName; // путь к выбранному файлу

                //Махинация для определения директории файла
                directory = ofd.FileName.Remove(ofd.FileName.Length - ofd.SafeFileName.Length, ofd.SafeFileName.Length);

                Player.Source = new Uri(path); // задаем видео 

                Start(); //запускаем
                ActiveControlPanel(); //включаем панель 
                ClearPlayListVideo(); // очищаем список воспроизведения
            }

        }

        private void CloseVideo(object sender, RoutedEventArgs e) 
        {
            Pause(); //останавливаем
            Player.Close();
            Player.Source = null;

            timelineSlider.Value = 0;

            timeVideo.Content = "00:00:00 / 00:00:00";

            //Стили

            PlayButton.IsEnabled = false;
            PauseButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            SpeedButton1.IsEnabled = false;
            SpeedButton2.IsEnabled = false;

            PlayImageButton.Style = (Style)PlayImageButton.FindResource("NonActiveImageButton");
            PauseImageButton.Style = (Style)PauseImageButton.FindResource("NonActiveImageButton");
            PauseButton.Style = (Style)PauseImageButton.FindResource("normalButton");
            StopImageButton.Style = (Style)StopImageButton.FindResource("NonActiveImageButton");
            SpeedImageButton1.Style = (Style)SpeedImageButton1.FindResource("NonActiveImageButton");
            SpeedImageButton2.Style = (Style)SpeedImageButton2.FindResource("NonActiveImageButton");
        }

        private void CloseApplication(object sender, RoutedEventArgs e) 
        {
            Close();
        }

        private void ActiveControlPanel()
        {
            PlayButton.IsEnabled = true;
            PauseButton.IsEnabled = true;
            StopButton.IsEnabled = true;
            SpeedButton1.IsEnabled = true;
            SpeedButton2.IsEnabled = true;

            PlayImageButton.Style = (Style)PlayImageButton.FindResource("ActiveImageButton");
            PauseImageButton.Style = (Style)PauseImageButton.FindResource("ActiveImageButton");
            StopImageButton.Style = (Style)StopImageButton.FindResource("ActiveImageButton");
            SpeedImageButton1.Style = (Style)SpeedImageButton1.FindResource("ActiveImageButton");
            SpeedImageButton2.Style = (Style)SpeedImageButton2.FindResource("ActiveImageButton");
        }

        private void DirectoryFile(object sender, RoutedEventArgs e)
        {
            if (Player.Source != null) // если есть ссылка на видео
            {
                string argument = Player.Source.ToString().Remove(Player.Source.ToString().LastIndexOf("/"));
                Process.Start("explorer.exe", argument); // запускаем проводник
            }
        }

        private void Window_Topmost(object sender, RoutedEventArgs e)
        {
            if (Topmost) Topmost = false;
            else Topmost = true;
        }

        private void Window_Fullscreen(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void HideTimeLineSlider(object sender, RoutedEventArgs e)
        {
            if (timelineSlider.Visibility == Visibility.Collapsed) timelineSlider.Visibility = Visibility.Visible;
            else timelineSlider.Visibility = Visibility.Collapsed;
        }

        private void HideMenu(object sender, RoutedEventArgs e)
        {
            if (TopMenu.Visibility == Visibility.Collapsed) TopMenu.Visibility = Visibility.Visible;
            else TopMenu.Visibility = Visibility.Collapsed;
        }

        private void HideControlPanel(object sender, RoutedEventArgs e)
        {
            if (ControlPanel.Visibility == Visibility.Collapsed) ControlPanel.Visibility = Visibility.Visible;
            else ControlPanel.Visibility = Visibility.Collapsed;
        }

        private void HideStatusBar(object sender, RoutedEventArgs e)
        {
            if (StatusBar.Visibility == Visibility.Collapsed) StatusBar.Visibility = Visibility.Visible;
            else StatusBar.Visibility = Visibility.Collapsed;
        }

        private void HidePlayList(object sender, RoutedEventArgs e)
        {
            if (PlayListBox.Visibility == Visibility.Collapsed) PlayListBox.Visibility = Visibility.Visible;
            else PlayListBox.Visibility = Visibility.Collapsed;
        }

        private void StartButtonInMenu(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled) Pause();
            else Start();
        }

        private void StopButtonInMenu(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void IncreaseSpeedButtonInMenu(object sender, RoutedEventArgs e)
        {
            if (Player.SpeedRatio != 2) Player.SpeedRatio += 0.25; // увеличиваем скорость воспроизведения 
        }

        private void DecreaseButtonInMenu(object sender, RoutedEventArgs e)
        {
            if (Player.SpeedRatio != 0.25) Player.SpeedRatio -= 0.25;
        }

        private void NormalSpeedButtonInMenu(object sender, RoutedEventArgs e)
        {
            Player.SpeedRatio = 1;
        }

        private void IncreaseSoundButtonInMenu(object sender, RoutedEventArgs e)
        {
            sliderVolume.Value += 10;
        }

        private void DecreaseSoundButtonInMenu(object sender, RoutedEventArgs e)
        {
            sliderVolume.Value -= 10;
        }

        private void ChangeSpeedInMenu(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)e.Source;

            Player.SpeedRatio = double.Parse(item.Header.ToString().Replace("x",""));
        }

        private void MuteButtonInMenu(object sender, RoutedEventArgs e)
        { 
            MuteVideo();
        }

        private void OpenDirectoryButtonInMenu(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog ofd = new CommonOpenFileDialog();
            ofd.IsFolderPicker = true; // указываем, что будем указывать папки

            files = new List<string>();

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string[] filesMP4 = Directory.GetFiles(ofd.FileName, "*.mp4"); // выбираем .mp4 файлы
                string[] filesWMV = Directory.GetFiles(ofd.FileName, "*.wmv"); // выбираем .wmv файлы

                files = filesWMV.Concat(filesMP4).ToList(); // объединяем все в один масивчик

                directory = ofd.FileName; // директория этих файлов

                AddPlayListVideo(files);
                ActiveControlPanel();

                if (files.Count != 0) Player.Source = new Uri(files[0]); // запускаем первое видео

                currentPlayingIndex = 0;
                Start();
            }
        }

        private void OpenManyVideos(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Видео файлы (*.mp4;*.wmv)|*.mp4;*.wmv|Аудио файлы (*.mp3;*.wav)|*.mp3;*.wav";
            ofd.Multiselect = true;

            files = new List<string>();

            if (ofd.ShowDialog() == true)
            {
                files = ofd.FileNames.ToList();

                directory = ofd.FileName.Remove(ofd.FileName.LastIndexOf(@"\"));

                if (files.Count != 0) Player.Source = new Uri(files[0]);

                AddPlayListVideo(files);
                ActiveControlPanel();

                currentPlayingIndex = 0;
                Start();
            }
        }

        private void AddPlayListVideo(List<string> asd)
        {
            if (asd != null) 
            {
                PlayListBox.Visibility = Visibility.Visible; // показываем список воспроизведения 

                ClearPlayListVideo(); //очищаем список

                for (int i = 0; i < asd.Count; i++) 
                {
                    TextBlock nameVideo = new TextBlock(); // добавляем название файла
                    Image image = new Image(); // кнопочка для удаления видео из списка

                    //Настройка элементов
                    nameVideo.Text = $"[{i + 1}]" + " " + asd[i].Remove(0, asd[i].LastIndexOf(@"\") + 1);
                    nameVideo.Foreground = new SolidColorBrush(Colors.LightGray);
                    nameVideo.MouseDown += VideoInPlayList_Click;

                    image.Style = (Style)image.FindResource("RemoveButtonVideo");
                    image.MouseDown += RemoveVideoInPlayList;
                    
                    //Добавляем в панель
                    RemoveButtonsVideo.Children.Add(image);

                    PlayList.Children.Add(nameVideo);
                }
            }
        }

        private void RemoveVideoInPlayList(object sender, MouseButtonEventArgs e)
        {
            //Длинная связь, дабы получить панель с названиями файлов
            Image image = (Image)sender;

            StackPanel stackPanel2 = (StackPanel)image.Parent;

            Grid grid = (Grid)stackPanel2.Parent;

            StackPanel stackPanel1 = (StackPanel)grid.Children[0];

            for (int i = 0; i < stackPanel2.Children.Count; i++) // даем тэг кнопкам, с их помощью будем удалять
            {
                Image images = (Image)stackPanel2.Children[i];
                images.Tag = $"{i}";
            }

            //удаляем по индексу
            stackPanel1.Children.RemoveAt(int.Parse(image.Tag.ToString())); 
            stackPanel2.Children.RemoveAt(int.Parse(image.Tag.ToString()));

            files.RemoveAt(int.Parse(image.Tag.ToString()));

            if (stackPanel1.Children.Count == 0)
            {
                Player.Source = null;
            }
        }

        private void ClearPlayListVideo()
        {
            PlayList.Children.Clear();
            RemoveButtonsVideo.Children.Clear();
        }

        private void VideoInPlayList_Click(object sender, RoutedEventArgs e)
        {
            TextBlock a = (TextBlock)sender;

            try
            {
                Player.Source = new Uri(directory + @"\" + a.Text.ToString().Substring(4));
            }

            catch { Console.WriteLine("Ошибка"); }

            Start();
        }

        private void AddLastOpenVideos(string pathVideo, bool saveFiles)
        {
            if (!saveFiles)
            {
                if (lastOpenFiles.Contains(pathVideo)) return;
                lastOpenFiles.Add(pathVideo);
            }
            
            MenuItem text = new MenuItem();
            text.Style = (Style)text.FindResource("lastOpenVideo");
            text.Header = pathVideo;
            text.Click += Text_Click;

            LastOpenVideos.Items.Add(text);
        }

        private void RefreshLastOpenVideos()
        {
            for (int i = 0; i < lastOpenFiles.Count; i++)
            {
                AddLastOpenVideos(lastOpenFiles[i], true);
            }
        }

        private void Text_Click(object sender, RoutedEventArgs e)
        {
            MenuItem source = (MenuItem)sender;

            try
            {
                Player.Source = new Uri(source.Header.ToString());
            }
            catch { }

            ClearPlayListVideo();
        }

        private void ClearLastOpenVideosButtonInMenu(object sender, RoutedEventArgs e)
        {
            for (int i = 2; i < LastOpenVideos.Items.Count; i++)
            {
                LastOpenVideos.Items.Remove(i);
            }
        }

        async private void SaveData()
        {
            if (Player.Source != null)
            {
                Save save = new Save(Player.Source.ToString(), sliderVolume.Value, Player.SpeedRatio, Player.Position, lastOpenFiles, files);

                using (FileStream fs = new FileStream("E:\\Users\\zxc\\source\\repos\\VideoPlayer\\VideoPlayer\\data.json", FileMode.OpenOrCreate))
                {
                    await JsonSerializer.SerializeAsync(fs, save);
                }
            }
        }

        async private void LoadData()
        {
            using (FileStream fs = new FileStream("E:\\Users\\zxc\\source\\repos\\VideoPlayer\\VideoPlayer\\data.json", FileMode.Open))
            {
                if (fs.Length != 0)
                {
                    if (MessageBox.Show("Загрузить последнее видео?", "Загрузка...", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Save save = await JsonSerializer.DeserializeAsync<Save>(fs);

                        Player.Source = new Uri(save.Path);
                        sliderVolume.Value = save.Volume;
                        Player.SpeedRatio = save.Speed;
                        lastOpenFiles = save.lastOpenFiles;
                        files = save.files;

                        Start();

                        Player.Position = save.TimeLine;

                        RefreshLastOpenVideos();
                        AddPlayListVideo(files);
                    }
                }
            }
        }

        private void FastForward()
        {
            if (Player.Source != null)
            {
                fastForwardWindow = new FastForwardWindow(); // открываем окно "Переход"

                //Передаем ссылки
                fastForwardWindow.mainWindow = this;
                fastForwardWindow.mediaPlayer = Player;
                fastForwardWindow.time = Player.NaturalDuration;

                IsEnabled = false; //главное окно вырубаем

                fastForwardWindow.Show(); //Показываем окно "Переход"
            }
        }

        private void FastForwardButtonInMenu(object sender, RoutedEventArgs e)
        {
            FastForward();
        }

        private void Loaded_App(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void Closing_App(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveData();

            if (fastForwardWindow != null) fastForwardWindow.Close();
        }

        private void FastForwardButton_Click(object sender, RoutedEventArgs e)
        {
            FastForward();
        }
    }


}
