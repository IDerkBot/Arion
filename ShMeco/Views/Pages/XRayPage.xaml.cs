using ArionCameraXrayDefender.Models;
using ArionCameraXrayDefender.Views.Windows;
using ArionControlLibrary;
using ArionLibrary.Controllers;
using ArionLibrary.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Timer = System.Timers.Timer;

namespace ArionCameraXrayDefender.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для XRayPage.xaml
    /// </summary>
    public partial class XRayPage : Page
    {
        #region Переменые

        private readonly List<string> _errorList = new List<string>();
        private bool _isError;
        private bool _isErrorForIsError;
        internal static bool SpOpen;
        private readonly Timer _updTimer = new Timer();
        private const int UPD_DELAY = 1000;

        public bool OpenWindowTraining;
        public MainWindow Mw;

        #endregion

        #region Инициализация

        public XRayPage()
        {
            InitializeComponent();
            MainManager.XRay = new XRayControllerRs232(LogManager.GetLogger("XRay"));
        }
        private void XRayPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MainManager.XRay.Connect();
                SpOpen = true;

                MainManager.XRay.GetTargets();

                if (MainManager.XRay.TargetKv == 0 && MainManager.XRay.TargetMa == 0 &&
                    MainManager.XRay.TargetTime == 0)
                    SpOpen = false;
            }
            catch (Exception ex)
            {
                MainManager.Log.Error(ex.ToString());
            }
        }

        #endregion

        #region Timer

        /// <summary>
        /// Старт таймера обновления 
        /// </summary>
        public void StartUpdate()
        {
            MainManager.Log.Info("Запуск таймера для отображения главного окна и считвания показателей с рентгена");
            MainManager.XrayUpdateTimer = _updTimer;
            MainManager.XrayUpdateTimer.Interval = UPD_DELAY;
            MainManager.XrayUpdateTimer.Elapsed += UpdTimer_Tick;
            MainManager.XrayUpdateTimer.Enabled = true;
        }

        private void GetData()
        {
            if (MainManager.XRay.TrainingInProgress) return;
            if (OpenWindowTraining) return;
            // Получение актуальных значений
            if (MainManager.XRay.IsEnable)
            {
                MainManager.Log.Info("Получение актуальных значений рентгена");
                if (ControlKv.Change == false && ControlMa.Change == false && ControlTime.Change == false)
                {
                    MainManager.XRay.GetTargets();
                    MainManager.XRay.GetActuals();

                    var convertTime = MainManager.XRay.ActualTime.ConvertTime();

                    if (ControlKv.Change == false)
                        ControlKv.Value = MainManager.XRay.ActualKv;
                    if (ControlMa.Change == false)
                        ControlMa.Value = MainManager.XRay.ActualMa;
                    if (ControlTime.Change == false)
                        ControlTime.UpdateActualValue(convertTime);
                }
            }
            else
            {
                MainManager.Log.Info("Получение целевых значений рентгена");
                bool enableControls = true;
                ControlKv.Dispatcher.BeginInvoke(new Action(() => enableControls = ControlKv.IsEnabled));
                if (enableControls == false)
                {
                    ControlKv.Dispatcher.BeginInvoke(new Action(() => ControlKv.IsEnabled = true));
                    ControlMa.Dispatcher.BeginInvoke(new Action(() => ControlMa.IsEnabled = true));
                    ControlTime.Dispatcher.BeginInvoke(new Action(() => ControlTime.IsEnabled = true));
                }
                MainManager.XRay.GetTargets();

                if (ControlKv.Change == false)
                    ControlKv.Value = MainManager.XRay.TargetKv;
                if (ControlMa.Change == false)
                    ControlMa.Value = MainManager.XRay.TargetMa;
                if (ControlTime.Change == false)
                    ControlTime.Value = MainManager.XRay.TargetTime;
            }
        }

        private void ChangeVisibleControls()
        {
            if (!MainManager.XRay.IsEnable) return;
            //ReSharper disable once CompareOfFloatsByEqualityOperator
            if (MainManager.XRay.TargetMa == Math.Round(MainManager.XRay.ActualMa, 1) && MainManager.XRay.ActualKv == MainManager.XRay.TargetKv)
            {
                ControlKv.ChangeEnable(true);
                ControlMa.ChangeEnable(true);
                ControlTime.ChangeEnable(true);
            }
            else
            {
                ControlKv.ChangeEnable(false);
                ControlMa.ChangeEnable(false);
                ControlTime.ChangeEnable(false);
            }
        }

        /// <summary>
        /// Получение статуса рентгена
        /// </summary>
        private void GetStatus()
        {
            var status = MainManager.XRay.Send("SR:12 ");
            if (string.IsNullOrWhiteSpace(status))
            {
                LabelStatus.ChangeContentAsync("Рентген отключен");
                this.ChangeEnable(false);
                return;
            }

            _errorList.Add(MainManager.XRay.StatusMessage(status));

            _errorList.RemoveAll(string.IsNullOrWhiteSpace);
            if (_errorList.Count(x => x.Contains("Готов")) > 1)
                _errorList.RemoveAll(z => z == "Готов");

            _errorList.Distinct().ToList().ForEach(statusXray =>
            {
                if (string.IsNullOrEmpty(statusXray)) return;

                // Если статус != Готов
                if (!statusXray.Contains("Готов"))
                {
                    MainManager.XRay.NeedTraining = statusXray == "Нужна тренировка";
                    _isError = _isErrorForIsError = true;
                    LabelStatus.ChangeContentAsync(statusXray);
                }
                else
                {
                    if (_isError)
                    {
                        if (!_isErrorForIsError)
                            _isError = false;
                        _isErrorForIsError = false;
                    }
                    else
                    {
                        LabelStatus.ChangeContentAsync(statusXray);
                        MainManager.XRay.NeedTraining = false;
                    }
                }
                _errorList.Remove(statusXray);
            });
        }

        private void UpdTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if(Mw.IsShow == false)
                    Mw.Dispatcher.BeginInvoke(new Action(() => Mw.ShowDialog()));
            }
            catch (Exception ex)
            {
                MainManager.Log.Error(ex.ToString());
            }

            if (OpenWindowTraining) return;
            if (!MainManager.XRay.Send("SR:01 ").Contains("SR:01"))
            {
                this.ChangeEnable(false);
                LabelStatus.ChangeContentAsync("Ошибка связи COM-порта RSR-232");
                return;
            }

            // Если тренировка не требуется
            if (MainManager.XRay.NeedTraining == false)
            {
                BtnTraining.ChangeEnable(false);
                BtnStart.ChangeEnable(true);
                BtnStop.ChangeEnable(false);
                if (OpenWindowTraining == false) this.ChangeEnable(true);
            }
            else if (OpenWindowTraining == false) this.ChangeEnable(true);
            else
            {
                BtnStart.ChangeEnable(false);
                BtnStop.ChangeEnable(false);
                BtnTraining.ChangeEnable(true);
            }

            var thread = new Thread(GetStatus) { Name = "Get Status" };
            thread.Start();

            //Получение данных
            var threadGetData = new Thread(GetData) { Name = "Get Data" };
            threadGetData.Start();

            var thread2 = new Thread(ChangeVisibleControls) { Name = "Change Enable Controls" };
            thread2.Start();

            if (MainManager.XRay.NeedTraining)
            {
                BtnTraining.ChangeEnable(true);
                BtnStart.ChangeEnable(false);
                BtnStop.ChangeEnable(false);
            }
            else if (BtnTraining.IsEnabled)
            {
                BtnTraining.ChangeEnable(false);
                BtnStart.ChangeEnable(true);
                BtnStop.ChangeEnable(false);
            }

            // Проверка на открытость порта, если он закрылся то выводим в статус ошибку
            if (SpOpen == false)
            {
                LabelStatus.ChangeContentAsync("Ошибка COM-порта RS - 232");
                this.ChangeEnable(false);
            }
            else
            {
                SpOpen = true;
                this.ChangeEnable(true);
            }
        }
        #endregion

        #region XRayController Btns

        /// <summary>
        /// Кнопка включение рентгена
        /// </summary>
        private void BtnXrOn_Click(object sender, RoutedEventArgs e)
        {
            MainManager.XRay.XRayOn();
            BtnStart.ChangeEnable(false);
            BtnStop.ChangeEnable(true);

            
            //if (MainManager.OpenDoor1() && MainManager.OpenDoor2() && MainManager.GetEmergencyStop())
            //    ComPort.XRayOn();

            //if (mainWindow.GetOpenDoor1()) MessageBox.Show("Дверь номер 1 открыта!");
            //if (mainWindow.GetOpenDoor2()) MessageBox.Show("Дверь номер 2 открыта!");
            //if (mainWindow.GetEmergencyStop()) MessageBox.Show("Аварийная остановка!");
        }

        /// <summary>
        /// Кнопка выключения рентгена
        /// </summary>
        private void BtnXrayOff_Click(object sender, RoutedEventArgs e)
        {
            MainManager.XRay.Off();
            BtnStart.ChangeEnable(true);
            BtnStop.ChangeEnable(false);
        }

        /// <summary>
        /// Кнопка тренировки
        /// </summary>
        private void BtnTraining_OnClick(object sender, RoutedEventArgs e)
        {
            this.ChangeEnable(false);
            OpenWindowTraining = true;
            var tr = new TrainingPage { Xr = this };
            MainManager.XrayNavigate(tr);
        }

        private void ControlKv_OnSendChange(object sender, EventArgs e)
        { if (ControlKv.Send == false) MainManager.XRay.SendKv(ControlKv.Value); }

        private void ControlMa_OnSendChange(object sender, EventArgs e)
        { if (ControlMa.Send == false) MainManager.XRay.SendMa(ControlMa.Value*10); }

        private void ControlTime_OnSendChange(object sender, EventArgs e)
        { if (ControlTime.Send == false) MainManager.XRay.SendTime(ControlTime.Value/6); }

        #endregion

        #region Close

        /// <summary>
        /// Выключение RS
        /// </summary>
        public void SpClose()
        {
            //mustExit = true;
            if (!MainManager.XRay.ComPort.IsOpen) return;
            MainManager.XRay.TotalOff();
            MainManager.XRay.ComPort.Close();
        }

        #endregion
    }
}