using ArionCameraXrayDefender.Models;
using ArionLibrary.Program;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArionCameraXrayDefender.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ProgramsWindow.xaml
    /// </summary>
    public partial class ProgramsWindow : Window
    {
        private readonly ObservableCollection<ProgramInfoDataSource> _programs;
        private ProgramInfoDataSource _currentProgram;
        private bool _editMode;
        private bool _createMode;

        public ProgramsWindow(ref ObservableCollection<ProgramInfoDataSource> programs)
        {
            InitializeComponent();
            _programs = programs;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Заполняет список программ
                DGridPrograms.ItemsSource = _programs;
                FirstProgram();
            }
            catch
            {
                //
            }
        }

        /// <summary>
        /// Получение первой программы
        /// </summary>
        private void FirstProgram()
        {
            if (_programs.Count <= 0) return;
            _currentProgram = _programs[0];
            DataContext = _currentProgram;
        }

        /// <summary>
        /// Кнопка создания программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            DGridPrograms.UnselectAllCells();

            TbName.Focus();
            _currentProgram = new ProgramInfoDataSource();
            DataContext = _currentProgram;
            BtnEdit.Content = "Сохранить";

            _createMode = true;

            ChangeEnable(false);
        }

        private void ChangeEnable(bool enable)
        {
            TbName.IsReadOnly = enable;
            TbAngle.IsReadOnly = enable;
            TbFrameCount.IsReadOnly = enable;
            TbHeight.IsReadOnly = enable;
            TbMa.IsReadOnly = enable;
            TbTiming.IsReadOnly = enable;
            TbVolt.IsReadOnly = enable;
        }

        private void Test(object sender, KeyEventArgs e)
        {
            //TextBox txt = sender as TextBox;
            //if (e.Key == Key.Enter)
            //{
            //    //int currentRowIdx = itms.Items.CurrentPosition;
            //    //itms.Items.MoveCurrentToNext();
            //    CheckTxtBxDetail(txt.Name);
            //    txt.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            //    //currentRowIdx = itms.Items.CurrentPosition;
            //}
        }

        private void OnTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox txt)
                CheckTxtBxDetail(txt.Name);
        }

        #region Buttons
        /// <summary>
        /// Просмотр и изменение программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnChange_Click(object sender, RoutedEventArgs e)
        {
            if (_editMode || _createMode)
            {
                ChangeEnable(true);

                CheckAllDetails();

                if (MessageBox.Show("Сохранить изменения?", "Вопрос", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    ProgramInfoDataSource program = _currentProgram;
                    bool updated = ProgramsManager.Update(program);
                    _currentProgram = program;
                    DataContext = _currentProgram;
                    if (!updated)
                        ProgramsManager.Add(program);

                    _editMode = false;
                    _createMode = false;
                    BtnEdit.Content = "Редактировать";
                    BtnCancel.Visibility = Visibility.Collapsed;
                }
                DGridPrograms.UnselectAllCells();

                return;
            }

            BtnCancel.Visibility = Visibility.Visible;
            ChangeEnable(false);
            _editMode = true;
            BtnEdit.Content = "Сохранить";
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = DGridPrograms.SelectedItem as ProgramInfoDataSource;
            ProgramsManager.Delete(selectedItem?.Id);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
        #endregion

        #region Проверка ввода
        private void TbText_Input(object sender, TextCompositionEventArgs e) => Checker.RussiaChar(e);
        private void TbInt_Input(object sender, TextCompositionEventArgs e) => Checker.IntChar(e);
        private void TbFloat_Input(object sender, TextCompositionEventArgs e) => Checker.FloatChar(e);
        #endregion

        private void CheckAllDetails()
        {
            CheckTxtBxDetail(TbName.Name);
            CheckTxtBxDetail(TbAngle.Name);
            CheckTxtBxDetail(TbHeight.Name);
            CheckTxtBxDetail(TbTiming.Name);
            CheckTxtBxDetail(TbVolt.Name);
            CheckTxtBxDetail(TbMa.Name);
            CheckTxtBxDetail(TbFrameCount.Name);
        }

        private void CheckTxtBxDetail(string name)
        {
            TextBox txt = GridPrgDetails.FindName(name) as TextBox;

            int x;

            switch (name)
            {
                // Угол
                case "TbAngle":
                    if (int.TryParse(txt?.Text, out x))
                    {
                        _currentProgram.Angle = x;
                    }
                    break;
                // Выдержка
                case "TbTiming":
                    if (int.TryParse(txt?.Text, out x))
                    {
                        _currentProgram.Timing = x;
                    }
                    break;
                // Высота
                case "TbHeight":
                    if (int.TryParse(txt?.Text, out x))
                    {
                        _currentProgram.Height = x;
                    }
                    break;
                // Напряжение
                case "TbVolt":
                    if (int.TryParse(txt?.Text, out x))
                    {
                        _currentProgram.Kv = x;
                    }
                    break;
                // 
                case "TbFrameCount":
                    if (int.TryParse(txt?.Text, out x))
                    {
                        _currentProgram.Count = x;
                    }
                    else
                    {
                        //MessageBox.Show("Значение должно быть числом");
                        txt.Text = "0";
                    }
                    break;
                // Название программы
                case "TbName":
                    {
                        _currentProgram.Name = TbName.Text;
                        break;
                    }
                case "TbMa":
                    if (float.TryParse(txt?.Text, out var y))
                    {
                        _currentProgram.Ma = y;
                    }
                    break;
            }
        }

        private void DGridPrograms_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentProgram = DGridPrograms.SelectedItem as ProgramInfoDataSource;
            DataContext = _currentProgram;
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (_editMode) _editMode = false;
            if (_createMode) _createMode = false;
            BtnEdit.Content = "Редактировать";
            BtnCancel.Visibility = Visibility.Collapsed;

            ProgramInfoDataSource selectedProgram = DGridPrograms.SelectedItem as ProgramInfoDataSource;
            _currentProgram = selectedProgram;
            DataContext = _currentProgram;

            ChangeEnable(true);
        }
    }
}
