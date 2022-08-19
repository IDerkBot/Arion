using ArionCameraXrayDefender.Models;
using ArionCameraXrayDefender.Views.Pages;
using ArionControlLibrary;
using ArionLibrary.Config;
using ArionLibrary.Program;
using ArionLibrary.User;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using NLog;
using NLog.Config;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft;

namespace ArionCameraXrayDefender.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow
    {
        #region Переменные

        public bool IsShow;
        private bool _windowResize;

        #endregion

        #region Window
        public MainWindow()
        {
            MainManager.Guardian = new STRAZHADAPTER();
            InitializeComponent();

            var primaryMonitorArea = SystemParameters.WorkArea;
            Left = primaryMonitorArea.Right - Width;

            LogManager.Configuration = new XmlLoggingConfiguration("NLog.config");

            MainManager.Log = LogManager.GetLogger("Application");
            MainManager.Log.Info("[Start Program]");
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Hide();

            #region Пользователи

            MainManager.Log.Info("Старт считывания пользователей");

            // Предупреждение на прочитанных пользователей
            if (UsersManager.Load() == false)
            {
                UsersManager.CreateDefault();
                MessageBoxOwn.Show("Предупреждение", "Ошибка в чтении пользователей \nАдминистратор по умолчанию \nПароль по умолчанию", MessageBoxButton.OK);
            }

            //Проверяется, если ли пользователь, который может изменять пользователей
            if (UsersManager.ThereIsMaxLevelUser() == false)
            {
                UsersManager.RestoreAdmin();
                MessageBoxOwn.Show("Предупреждение", "Администратор восстановлен" + Environment.NewLine +
                                                     "Администратор по умолчанию" + Environment.NewLine +
                                                     "Пароль по умолчанию", MessageBoxButton.OK);
            }

            //Проверяется, если ли пользователь Arion
            if (!UsersManager.UserExist("Arion"))
            {
                UsersManager.CreateArion();
            }

            // Чтение программ
            if (ProgramsManager.Read() == false)
            {
                MessageBoxOwn.Show("Предупреждение", "Ошибка в чтении программ", MessageBoxButton.OK);
            }

            #endregion

            #region Конфигурационный файл

            MainManager.Log.Info("Считывание конфигурационного файла");

            MainManager.Config = new CConfig();
            MainManager.Config.Load();

            #endregion

            #region Стражник

            MainManager.Log.Info("Запуск стражника");

            try
            {
                MainManager.Guardian.Run();
                var i = MainManager.Guardian.Run();
                if (i < 0)
                {
                    MainManager.Log.Error("Ошибка запуска стражника");
                    MessageBoxOwn.Show("Внимание", "Не удалось запустить программу \"Стражник\"", MessageBoxButton.OK);
                }

                if (i >= 0)
                {
                    new Thread(() =>
                {
                    Thread.Sleep(5000);
                    while (_windowResize == false)
                    {
                        var process = Process.GetProcessesByName("MAIN");
                        if (process.Length == 1)
                        {
                            MoveWindow(process[0].MainWindowHandle, 0, 0, 1500, 1080, true);
                            _windowResize = true;
                        }

                        Thread.Sleep(2000);
                    }
                }).Start();
                }
            }
            catch (Exception ex)
            {
                MainManager.Log.Error("Ошибка запуска стражника");
                MessageBoxOwn.Show($"Не удалось запустить программу \"Стражник\"\n{ex}");
            }

            #endregion

            #region Запуск окон

            MainManager.Log.Info("Запуск верхней панели");
            // Верхнее меню
            var topBar = new TopBarPage { Mw = this };
            TopBarFrame.Navigate(topBar);

            MainManager.Log.Info("Запуск панели видеокамеры");
            // Окно видеокамеры
            VideoFrame.Navigate(new VideoStreamPage());

            MainManager.Log.Info("Запуск панели программ");
            // Окно программ
            ProgramsFrame.Navigate(new ProgramsPage());

            MainManager.Log.Info("Запуск панели манипулятора");
            // Окно манипулятора
            ManipulatorFrame.Navigate(new ManipulatorPage());

            // Окно детектора
            //MainManager.DetectorPage = new DetectorPage();
            //DetectorFrame.Navigate(MainManager.DetectorPage);

            MainManager.Log.Info("Запуск панели рентгена");
            // Окно рентгена
            var xray = new XRayPage { Mw = this };
            MainManager.XraySet(XrayFrame);
            MainManager.XrayNavigate(xray);
            xray.StartUpdate();

            #endregion
        }

        public new void ShowDialog()
        {
            Show();
            IsShow = true;
        }
        #endregion

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
    }
}
