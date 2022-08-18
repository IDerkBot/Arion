using ArionCameraXrayDefender.Models;
using ArionLibrary.User;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArionControlLibrary;

namespace ArionCameraXrayDefender.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationWindows.xaml
    /// </summary>
    public partial class AuthorizationWindows
    {
        private UsersManager.USER_LOGIN_RESULT _result = UsersManager.USER_LOGIN_RESULT.ABORT;
        private VirtualKeyboard _keyboard;

        public AuthorizationWindows()
        {
            InitializeComponent();

            var list = UsersManager.GetUserList(true, true);
            CbUsers.ItemsSource = list;
        }

        public new UsersManager.USER_LOGIN_RESULT Show()
        {
            ShowDialog();
            return _result;
        }

        private void BtnEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(CbUsers.SelectedItem is User activeUser))
                return;
            if (UsersManager.Login(activeUser.Login, PbPassword.Password))
            {
                _result = UsersManager.USER_LOGIN_RESULT.SUCCESS;
                Close();
            }
            else
            {
                PbPassword.Background = ColorPalete.SolidRed;
                PbPassword.Foreground = ColorPalete.SolidWhite;
            }
        }
        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PbPassword_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            PbPassword.Background = FindResource("SolidControlColor") as SolidColorBrush;
            PbPassword.Foreground = FindResource("SolidDarkBlue") as SolidColorBrush;
        }

        private void CbUsers_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PbPassword.Focus();
        }

        private void BtnKeyboard_OnClick(object sender, RoutedEventArgs e)
        {
            _keyboard = new VirtualKeyboard(PbPassword);
            PbPassword.Password = _keyboard.Show();
        }
    }
}
