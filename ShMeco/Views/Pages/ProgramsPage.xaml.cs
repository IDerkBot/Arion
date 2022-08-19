using ArionCameraXrayDefender.Models;
using ArionCameraXrayDefender.Views.Windows;
using ArionControlLibrary;
using ArionLibrary.Program;
using ArionLibrary.User;
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

        /// <summary>
        /// Роботизированный цикл
        /// </summary>
        private void BtnCycles_OnClick(object sender, RoutedEventArgs e)
        {
            var item = CbPrograms.SelectedItem as ProgramInfoDataSource;
            if (item == null)
                return;

            new Thread(() =>
            {
                for (int i = 0; i < item.Count; i++)
                {
                    MainManager.Guardian.Exposure(item.Timing);
                    MainManager.Guardian.Averaging(item.Count);

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
                    MainManager.Guardian.Snap();
                    Thread.Sleep(2000);

                    MainManager.XRay.Off();
                }
            }).Start();
        }
    }
}
