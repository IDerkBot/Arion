using ArionCameraXrayDefender.Models;
using ArionControlLibrary;
using ArionLibrary.Controllers;
using System;
using System.IO.Ports;
using System.Timers;
using System.Windows;
using Timer = System.Timers.Timer;

namespace ArionCameraXrayDefender.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ManipulatorPage.xaml
    /// </summary>
    public partial class ManipulatorPage
    {
        private bool _back;
        private int _x1;
        private bool _isMainController;
        private Timer _timer;
        private bool _notFound;

        #region Init

        public ManipulatorPage()
        {
            InitializeComponent();
            MainManager.Controller = new MotorController(1, new SerialPort("COM3", 38400));
            MainManager.Controller.Homing();

            MainManager.Door1 = new MotorController(2, new SerialPort("COM3", 38400));
            MainManager.Door2 = new MotorController(3, new SerialPort("COM3", 38400));
            
            var timer = new Timer(200);
            timer.Elapsed += TimerOnElapsed;
            timer.Start();
        }

        #endregion

        #region Buttons

        private void BtnSpeed1_OnClick(object sender, RoutedEventArgs e)
        {
            int speed = int.Parse(BtnSpeed1.Content.ToString());
            MainManager.Controller.SetSpeed(speed);
        }
        private void BtnSpeed2_OnClick(object sender, RoutedEventArgs e)
        {
            int speed = int.Parse(BtnSpeed2.Content.ToString());
            MainManager.Controller.SetSpeed(speed);
        }
        private void BtnSpeed3_OnClick(object sender, RoutedEventArgs e)
        {
            int speed = int.Parse(BtnSpeed3.Content.ToString());
            MainManager.Controller.SetSpeed(speed);
        }
        private void BtnCalibration_OnClick(object sender, RoutedEventArgs e)
        {
            if (_back)
            {
                _back = false;
                MainManager.Controller.GoToPosition(0);
                return;
            }

            _back = true;
            MainManager.Controller.GoToPosition(500);
        }

        #endregion

        #region Events

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            LoadData();

            if (_isMainController) return;

            //MainManager.Controller.GetState();
            _x1 = MainManager.Controller.GetCoordinates();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                X1.Text = $"{_x1}";
            }));
            if (_x1 == 0)
            {
                _isMainController = true;
                LabelStatus.ChangeContentAsync("Манипулятор не найден");
                _notFound = true;
                _timer = new Timer(1000);
                _timer.Elapsed += TimerStatus;
                _timer.Start();
            }
            else if (MainManager.Controller.isHoming)
                LabelStatus.ChangeContentAsync("Возвращение");
            else if (MainManager.Controller.isGo)
                LabelStatus.ChangeContentAsync("Перемещение");
            else
            {
                if (_notFound)
                {
                    MainManager.Controller.GetLimits();
                    _notFound = false;
                }
                LabelStatus.ChangeContentAsync("Готов");
            }
        }

        private void TimerStatus(object sender, ElapsedEventArgs e)
        {
            _x1 = MainManager.Controller.GetCoordinates();
            if (_x1 != 0)
            {
                _isMainController = false;
                _timer.Dispose();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Загрузка данных
        /// </summary>
        public void LoadData()
        {
            BtnSpeed1.ChangeContentAsync(MainManager.Config.ProgramSettings.Button1Velocity);
            BtnSpeed2.ChangeContentAsync(MainManager.Config.ProgramSettings.Button2Velocity);
            BtnSpeed3.ChangeContentAsync(MainManager.Config.ProgramSettings.Button3Velocity);
        }

        #endregion

        private void ClSpeed_OnSendChange(object sender, EventArgs e)
        {
            MainManager.Controller.SetSpeed((int)ClSpeed.Value);
        }
    }
}
