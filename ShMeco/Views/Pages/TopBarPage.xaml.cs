using ArionCameraXrayDefender.Models;
using ArionCameraXrayDefender.Views.Windows;
using ArionControlLibrary;
using ArionLibrary.User;
using System.Windows;
using System.Windows.Controls;
using ArionLibrary.Controllers;

namespace ArionCameraXrayDefender.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для TopBarPage.xaml
    /// </summary>
    public partial class TopBarPage : Page
    {
        public MainWindow Mw;

        public TopBarPage()
        {
            InitializeComponent();
        }

        private void BtnCloseApplication_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBoxOwn.Show("Вопрос", "Закрыть программу?", MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;
            
            if (Threads.VideoRider != null)
                if (Threads.VideoRider.IsOpen)
                    Threads.VideoRider.Close();
            MainManager.XrayUpdateTimer.Stop();
            MainManager.XRay.MMustQuit = 0;
            Mw.Close();
        }

        private void BtnMinimizeApplication_OnClick(object sender, RoutedEventArgs e)
        {
            if (Mw.WindowState == WindowState.Normal)
                Mw.WindowState = WindowState.Minimized;
        }

        private void BtnSettings_OnClick(object sender, RoutedEventArgs e)
        {
            if (UsersManager.CurrentUser.Level > 0)
            {
                var settings = new SettingsWindow();
                settings.Show();
            }
            else MessageBoxOwn.Show("У вас нет прав доступа");
        }

        private void BtnUser_OnClick(object sender, RoutedEventArgs e)
        {
            if (UsersManager.CurrentUser.Level < 2)
            {
                MessageBoxOwn.Show("У вас нет прав доступа");
                return;
            }
            var usersWindow = new UsersWindow();
            usersWindow.Show();
        }

        private void BtnAuthorization_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsersManager.CurrentUser.Login))
            {
                var authorization = new AuthorizationWindows();
                var result = authorization.Show();
                BtnAuthorization.Content = result == UsersManager.USER_LOGIN_RESULT.SUCCESS ? UsersManager.CurrentUser.Fullname : "Пользователь не выбран";
            }
            else
            {
                UsersManager.Logout();
                BtnAuthorization.Content = "Пользователь не выбран";
            }
        }
    }
}