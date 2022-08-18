using System;
using System.Timers;
using System.Windows;

namespace ArionCameraXrayDefender.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        private readonly Timer _timer = new Timer(3000);

        public SplashWindow()
        {
            InitializeComponent();
        }

        public void Open()
        {
            InitializeComponent();
            Show();
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        public void Open(string message)
        {
            Show();
            LblMessage.Content = message;

            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        public void Open(string message, string status)
        {
            InitializeComponent();
            LblMessage.Content = message;
            LblStatus.Content = status;
            Show();
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(Close));
        }
    }
}
