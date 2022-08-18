using ArionCameraXrayDefender.Models;
using ArionLibrary.Config;
using ArionLibrary.User;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Windows;

namespace ArionCameraXrayDefender.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public List<string> ListFChoice;
        public CConfig Config;

        public SettingsWindow()
        {
            InitializeComponent();

            Config = MainManager.Config;
            SetData();

            if (UsersManager.CurrentUser.Level > 5)
            {
                Height = 810;
                CbPort1.Visibility = CbPort2.Visibility = CbPort3.Visibility =
                    CbPort4.Visibility = CbPort5.Visibility = CbPort6.Visibility =
                        LblPort1.Visibility = LblPort2.Visibility = LblPort3.Visibility =
                            LblPort4.Visibility = LblPort5.Visibility = LblPort6.Visibility = Visibility.Visible;
            }

            var activeComPorts = SerialPort.GetPortNames();

            CbPort1.ItemsSource = CbPort2.ItemsSource = CbPort3.ItemsSource = CbPort4.ItemsSource = CbPort5.ItemsSource = CbPort6.ItemsSource = activeComPorts;
            CbPort1.SelectedItem =
                activeComPorts.FirstOrDefault(x => x.Contains(MainManager.Config.ProgramSettings.ComPortXray));
            CbPort2.SelectedItem =
                activeComPorts.FirstOrDefault(x => x.Contains(MainManager.Config.ProgramSettings.ComPortController));
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (CbF1.SelectedItem == CbF2.SelectedItem || CbF1.SelectedItem == CbF2.SelectedItem ||
                CbF2.SelectedItem == CbF3.SelectedItem)
            {
                //MessageBoxOwn.Show("Предупреждение", "Клавиши F1, F2, F3 повторяются", MessageBoxButton.OK);
                return;
            }

            GetData();

            MainManager.Config = Config;
            MainManager.Config.Save();
            Close();
        }

        private void GetData()
        {
            Config.ProgramSettings.TableVelocity = (int)ClTableVelocity.Value;
            Config.ProgramSettings.AutomaticCycleVelocity = (int)ClVelocityAutoCycle.Value;
            Config.ProgramSettings.ManualCycleVelocity = (int)ClVelocityManualCycle.Value;
            Config.ProgramSettings.F1 = CbF1.SelectedIndex;
            Config.ProgramSettings.F2 = CbF2.SelectedIndex;
            Config.ProgramSettings.F3 = CbF3.SelectedIndex;
            Config.ProgramSettings.Button1Velocity = (int)ClVelocityFirstBtn.Value;
            Config.ProgramSettings.Button2Velocity = (int)ClVelocitySecondBtn.Value;
            Config.ProgramSettings.Button3Velocity = (int)ClVelocityThirdBtn.Value;

            if (TgBtnXray.IsChecked != null) Config.XRay.Enabled = (bool)TgBtnXray.IsChecked;
        }

        public void SetData()
        {
            ListFChoice = Config.ProgramSettings.FChoice;

            CbF1.ItemsSource = ListFChoice;
            CbF2.ItemsSource = ListFChoice;
            CbF3.ItemsSource = ListFChoice;

            //ClTableVelocity.Init(0, 100, 1, 1, Config.ProgramSettings.TableVelocity);
            //ClVelocityAutoCycle.Init(0, 100, 1, 1, Config.ProgramSettings.AutomaticCycleVelocity);
            //ClVelocityManualCycle.Init(0, 100, 1, 1, Config.ProgramSettings.ManualCycleVelocity);

            CbF1.SelectedIndex = Config.ProgramSettings.F1;
            CbF2.SelectedIndex = Config.ProgramSettings.F2;
            CbF3.SelectedIndex = Config.ProgramSettings.F3;

            //ClVelocityFirstBtn.Init(2, 100, 1, 1, Config.ProgramSettings.Button1Velocity);
            //ClVelocitySecondBtn.Init(2, 100, 1, 1, Config.ProgramSettings.Button2Velocity);
            //ClVelocityThirdBtn.Init(2, 100, 1, 1, Config.ProgramSettings.Button3Velocity);

            TgBtnXray.IsChecked = Config.XRay.Enabled;
        }
    }
}
