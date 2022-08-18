using ArionCameraXrayDefender.Views.Windows;
using System.Windows;
using Application = System.Windows.Application;

namespace ArionCameraXrayDefender
{
    public enum CyclesType { Manual = 0, Automatic = 1 }

    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            new SplashWindow().Open("Инициализация рентгена и манипулятора");
            
            Current.MainWindow = new MainWindow();
            Current.MainWindow.Show();
        }
    }
}
