using ArionLibrary.Config;
using ArionLibrary.Controllers;
using System.Timers;
using System.Windows.Controls;
using NLog;

namespace ArionCameraXrayDefender.Models
{
    internal static class MainManager
    {
        public static CConfig Config;
        public static Frame XrayFrame;
        public static MotorController Controller;
        public static MotorController Door1;
        public static MotorController Door2;
        //public static DetectorPage DetectorPage;
        public static Timer XrayUpdateTimer;
        public static XRayControllerRs232 XRay;
        public static STRAZHADAPTER Guardian;
        public static Logger Log;

        public static bool OpenDoor1()
        {
            if (Door1.GetCoordinates() == 1) return true;
            return false;
        }

        public static bool OpenDoor2()
        {
            if (Door2.GetCoordinates() == 1) return true;
            return false;
        }

        public static bool GetEmergencyStop()
        {
            return true;
        }

        public static void XraySet(Frame frame)
        {
            XrayFrame = frame;
        }
        public static void XrayNavigate(Page pageToMove)
        {
            XrayFrame.Navigate(pageToMove);
        }
        public static void XrayGoBack()
        {
            XrayFrame.GoBack();
        }
    }
}
