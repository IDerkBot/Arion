using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArionCameraXrayDefender.Models;
using ArionCameraXrayDefender.Views.Windows;

namespace ArionCameraXrayDefender.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для DetectorConnectPage.xaml
    /// </summary>
    public partial class DetectorConnectPage : Page
    {
        public DetectorConnectPage()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(MainManager.Guardian.IsConnected.ToString());
            MessageBox.Show(MainManager.Guardian.Snap().ToString());
        }

        private void BtnAveraging_OnClick(object sender, RoutedEventArgs e)
        {
            //MainManager.Guardian.Averaging(int.Parse(TbAveraging.Text));
        }

        private void BtnExposure_OnClick(object sender, RoutedEventArgs e)
        {
            //MainManager.Guardian.Exposure(int.Parse(TbExposure.Text));
        }

        private void BtnNewFile_OnClick(object sender, RoutedEventArgs e)
        {
            MainManager.Guardian.NewFile();
        }

        private void BtnSaveDCM_OnClick(object sender, RoutedEventArgs e)
        {
            //MainManager.Guardian.SaveDCM(TbSaveDCMFolder.Text, TbSaveDCMPath.Text);
        }

        private void BtnConnect_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MainManager.Guardian.ServerConnect();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
