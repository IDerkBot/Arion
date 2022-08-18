using ArionCameraXrayDefender.Models;
using ArionCameraXrayDefender.Views.Windows;
using ArionControlLibrary;
using ArionLibrary.Controllers;
using ArionLibrary.Program;
using ArionLibrary.User;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ArionCameraXrayDefender.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProgramsPage.xaml
    /// </summary>
    public partial class ProgramsPage : Page
    {
        private ObservableCollection<ProgramInfoDataSource> _programs;
        public ProgramsPage()
        {
            InitializeComponent();
            _programs = ProgramsManager.Programs;
            CbPrograms.ItemsSource = _programs;
        }

        private void BtnEdit_OnClick(object sender, RoutedEventArgs e)
        {
            if (UsersManager.CurrentUser.Level > 1)
            {
                ProgramsWindow listOfProgramsWindow = new ProgramsWindow(ref _programs);
                listOfProgramsWindow.Show();
            }
            else
            {
                MessageBoxOwn.Show("У вас нет прав доступа");
            }
        }

        public void StartUpdate()
        {
            //_updTimer.Interval = TimeSpan.FromMilliseconds(upd_delay);
            //_updTimer.Tick += UpdTimer_Tick;
            //_updTimer.IsEnabled = true;
        }
        private void UpdTimer_Tick(object sender, EventArgs e)
        {
            //if (_master == null && _flag)
            //{
            //    //grdManip.IsEnabled = BtnCycleStart.IsEnabled = btnCyclePause.IsEnabled = false;
            //    //lbl_Err.Content = "Нет связи с шаговым двигателем.";
            //}
            //else
            //{
            //    //lbl_Err.Content = "";
            //    //grdManip.IsEnabled = true;
            //}

            //if (_mayRead)
            //    Read_MB();

            //if (_mayReadTcp)
            //    Read_TCP();
        }

        Thread _run;

        /// <summary>
        /// Роботизированный цикл
        /// </summary>
        private void BtnCycles_OnClick(object sender, RoutedEventArgs e)
        {
            //_flag = false;
            var item = CbPrograms.SelectedItem as ProgramInfoDataSource;
            if (item == null)
                return;

            new Thread(() =>
            {
                for (int i = 0; i < item.Count; i++)
                {
                    MainManager.Controller.GoToPosition(item.Height-200);

                    while (MainManager.Controller.lastPosition != item.Height)
                    {
                    }
                    MainManager.XRay.SendKv(item.Kv);
                    Thread.Sleep(100);
                    MainManager.XRay.SendMa(item.Ma*10);
                    Thread.Sleep(100);
                    MainManager.XRay.XRayOn();


                    Thread.Sleep(10000);
                    MainManager.XRay.Off();
                }
            }).Start();

            //BtnCycleStart.IsEnabled = stop = false;
            // Находим индекс программы и запускаем её выполнение в отдельном потоке

            _run = new Thread(Run);
            _run.SetApartmentState(ApartmentState.STA);
            _run.Name = "Acquire video 2";
            _run.Start();
        }

        /// <summary>
        /// Функция рoботизированного цикла, запускаемое в отдельном потоке
        /// </summary>
        private void Run()
        {
            //_sAdapt.ServerConnect();
            //bool startSnap = false;
            //int cntSnap = 0;

            //try
            //{
            //    for (int i = 0; i < item?.Count && !stop; ++i)
            //    {
            //        GetToPosition = false;

            //        //Thread.Sleep(500);
            //        if (!Motion_LOAD((ushort)(item.Height + 1), item.Angle * i))
            //            return;

            //        if (_master != null && !_xRay.Target_LOAD(item.KV, item.MA, item.Timing))
            //        {
            //            //lbl_Err.Dispatcher.BeginInvoke(new Action(() => lbl_Err.Content = "Проверьте рентгенаппарат"));
            //            //BtnCycleStart.Dispatcher.BeginInvoke(new Action(() => BtnCycleStart.IsEnabled = true));
            //            return;
            //        }

            //        _sAdapt.Snap();
            //        //MessageBox.Show("Захват изображения");
            //        Thread.Sleep(2000);
            //        if (_sAdapt.Command == V01.SDK_NOTIFY_IMAGE_READY)
            //        {
            //            cntSnap++;
            //            if (cntSnap == 1)
            //                break;
            //        }
            //    }
            //    //BtnCycleStart.Dispatcher.BeginInvoke(new Action(() => BtnCycleStart.IsEnabled = true));
            //    stop = true;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        }
    }
}
