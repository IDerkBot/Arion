using ArionCameraXrayDefender.Models;
using ArionControlLibrary;
using ArionLibrary.User;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace ArionCameraXrayDefender.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для UsersWindow.xaml
    /// </summary>
    public partial class UsersWindow : Window
    {
        private bool _createMode;
        private bool _editMode;
        private User _editUser;

        public UsersWindow()
        {
            InitializeComponent();
            if (UsersManager.Load())
                DGridUsers.ItemsSource = UsersManager.Users;
        }
        /// <summary>
        /// Закрытие окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
        /// <summary>
        /// Действие на нажатие кнопки добавить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCreate_OnClick(object sender, RoutedEventArgs e)
        {
            ClearData();
            ChangeEnable(false);

            TbLogin.Focus();
            _editUser = new User();
            DataContext = _editUser;

            BtnEdit.Content = "Сохранить";
            BtnCancel.Visibility = Visibility.Visible;
            _createMode = true;
        }
        /// <summary>
        /// Действие на нажатие кнопки удалить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = DGridUsers.SelectedItem as User;
            UsersManager.DeleteUser(selectedItem.Login);
        }
        /// <summary>
        /// Действие на нажатие кнопки изменить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnEdit_OnClick(object sender, RoutedEventArgs e)
        {
            if (_editMode || _createMode)
            {
                int index = GetIndex();

                if (index > -1 && UsersManager.Users[index].Level > UsersManager.CurrentUser.Level)
                {
                    MessageBoxOwn.Show("Предупреждение", "Вы не можете вносить изменения в данные пользователя с уровнем доступа выше Вашего", MessageBoxButton.OK);
                    return;
                }

                SaveUser();
                BtnEdit.Content = "Редактировать";
                _editMode = false;
                ChangeEnable(true);
                BtnCancel.Visibility = Visibility.Collapsed;

                return;
            }

            BtnCancel.Visibility = Visibility.Visible;
            _editMode = true;
            BtnEdit.Content = "Сохранить";
            ChangeEnable(false);
            DataContext = _editUser;
        }
        private int GetIndex()
        {
            var selectedItem = DGridUsers.SelectedItem as User;
            return DGridUsers.ItemsSource.Cast<User>().ToList().IndexOf(selectedItem);
        }
        private void ChangeEnable(bool enable)
        {
            TbLogin.IsReadOnly = enable;
            TbFullname.IsReadOnly = enable;
            BtnLevelPlus.IsEnabled = !enable;
            BtnLevelMinus.IsEnabled = !enable;
            TbPassword.IsReadOnly = enable;
            BtnLevelMinus.IsEnabled = !enable;
            BtnLevelPlus.IsEnabled = !enable;
            TgBtIsEnable.IsEnabled = !enable;
        }
        /// <summary>
        /// Проверка данных
        /// </summary>
        /// <returns></returns>
        private bool ValidateUserData()
        {
            if (TbLogin.Text.Length == 0)
            {
                TbLogin.Focus();
                MessageBox.Show("Логин не введен!", "Внимание");
                return false;
            }

            // Если создание нового пользователя
            if (_createMode)
            {
                if (UsersManager.UserExist(TbLogin.Text))
                {
                    TbLogin.Focus();
                    MessageBox.Show("Такой пользователь уже существует!", "Внимание");
                    return false;
                }
            }

            if (TbPassword.Text.Length == 0)
            {
                TbPassword.Focus();
                MessageBox.Show("Пароль не введен!", "Внимание");
                return false;
            }
            return true;
        }
        /// <summary>
        /// Сохранение пользователей
        /// </summary>
        /// <returns></returns>
        private bool SaveUser()
        {
            if (ValidateUserData())
            {
                User user = _editUser;
                bool updated = UsersManager.Update(user);
                _editUser = user;
                DataContext = _editUser;
                if (!updated)
                    updated = UsersManager.AddUser(user);
                return updated;
            }
            return false;
        }
        private void TextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Checker.EnglishChar(e);
        }
        /// <summary>
        /// Действие при выборе пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DGridUsers_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = DGridUsers.SelectedItem as User;

            _editUser = selectedItem;
            DataContext = _editUser;
        }
        /// <summary>
        /// Очистка значений полей
        /// </summary>
        private void ClearData()
        {
            _editUser = new User();
        }
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (_editMode) _editMode = false;
            if (_createMode) _createMode = false;
            BtnEdit.Content = "Редактировать";
            BtnCancel.Visibility = Visibility.Collapsed;

            User selectedUser = DGridUsers.SelectedItem as User;
            _editUser = selectedUser;
            DataContext = _editUser;

            ChangeEnable(true);
        }
        private void BtnLevelPlus_OnClick(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TbLevel.Text, out var l))
            {
                if (l < UsersManager.MaxLevelForSet)
                {
                    TbLevel.Text = (l + 1).ToString();
                    _editUser.Level++;
                }
            }
        }
        private void BtnLevelMinus_OnClick(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TbLevel.Text, out var l))
            {
                if (l > UsersManager.MIN_LEVEL)
                {
                    TbLevel.Text = (l - 1).ToString();
                    _editUser.Level--;
                }
            }
        }

        public void GetKeyboardValue(string value, string nameControl)
        {
            //PbPassword.Password = value;

            ((TextBox)FindName(nameControl)).Text = value;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            VirtualKeyboard virtualKeyboard;
            if (TbLogin.IsFocused)
            {
                virtualKeyboard = new VirtualKeyboard(TbLogin);
                TbLogin.Text = virtualKeyboard.Show();
            }
            else if (TbFullname.IsFocused)
            {
                virtualKeyboard = new VirtualKeyboard(TbFullname);
                TbFullname.Text = virtualKeyboard.Show();
            }
            else if (TbPassword.IsFocused)
            {
                virtualKeyboard = new VirtualKeyboard(TbPassword);
                TbPassword.Text = virtualKeyboard.Show();
            }
        }
    }
}