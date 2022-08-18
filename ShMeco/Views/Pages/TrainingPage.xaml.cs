using ArionCameraXrayDefender.Models;
using ArionLibrary.Utilities;
using System;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Timer = System.Timers.Timer;

namespace ArionCameraXrayDefender.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для TrainingPage.xaml
    /// </summary>
    public partial class TrainingPage : Page
    {
        public XRayPage Xr;
        private readonly Timer _timerTraining = new Timer(1000);
        private int _time;

        public TrainingPage()
        {
            InitializeComponent();
            _timerTraining.Elapsed += TimerTrainingOnElapsed;
        }

        private void CloseTraining_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Close()
        {
            Xr.IsEnabled = true;
            Xr.OpenWindowTraining = false;
            MainManager.XrayGoBack();
        }


        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            BtnStartTraining();
        }

        private async void BtnStartTraining()
        {
            BtnStart.IsEnabled = false;
            BtnStop.IsEnabled = true;

            MainManager.XRay.Send($"WU:4,{ControlKv.Value} ");
            Thread.Sleep(100);

            GetWarmUpTime();
            await LabelTime.Dispatcher.BeginInvoke(new Action(() => LabelTime.Content = _time.ConvertTime()));

            MainManager.XRay.XRayOn();
            MainManager.XRay.TrainingInProgress = true;

            _timerTraining.Start();

            //MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            //if (mainWindow != null && MainManager.XRay.NeedTrain && mainWindow.GetOpenDoor1() && mainWindow.GetOpenDoor2() && mainWindow.GetEmergencyStop())
            //{
            //    MainManager.XRay.Send($"WU:4,{ControlKv.Value} ");
            //    Thread.Sleep(500);
            //    XRayController.ComPort.Send("HV:1 ");
            //}
        }

        private void GetWarmUpTime()
        {
            while (MainManager.XRay.Time == 0)
            {
                MainManager.XRay.Send("WT ");
                _time = MainManager.XRay.Time + 15;
            }
        }
        private void TimerTrainingOnElapsed(object sender, ElapsedEventArgs e)
        {
            LabelTime.Dispatcher.BeginInvoke(new Action(() => LabelTime.Content = _time.ConvertTime()));
            if (_time == 0)
            {
                MainManager.XRay.TrainingInProgress = false;
                MainManager.XRay.TotalOff();
                _timerTraining.Stop();
                MessageBox.Show("Тренировка окончена");
                MainManager.XRay.IsEnable = false;
                Dispatcher.BeginInvoke(new Action(Close));
            }
            else
                _time--;


            // TODO Отреагировать на то что тренировку отключили с кнопки
            var status = MainManager.XRay.Send("SR:12 ");
            if (status == "117")
            {
                _time = 0;
            }
        }

        private void BtnStop_OnClick(object sender, RoutedEventArgs e)
        {
            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;
            MainManager.XRay.Off();
            _timerTraining.Stop();
            MainManager.XRay.IsEnable = false;
            MainManager.XRay.TrainingInProgress = false;
        }
    }
}
