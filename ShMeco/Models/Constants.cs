using Accord.Video.FFMPEG;
using System;
using System.IO;

namespace ArionCameraXrayDefender.Models
{
    internal class Constants
    {
        public const int MECH_MOTORS = 1;
        public const int BASIC_USER_LEVEL = 1;
        public const int POWER_USER_LEVEL = 2;
        public const int ADMIN_LEVEL = 3;

        public static string CONFIG_FILE = AppDomain.CurrentDomain.BaseDirectory + @"Data\App\Config.cfg";
        public static string CONFIG_FILE_JSON = AppDomain.CurrentDomain.BaseDirectory + @"Data\App\Config.json";
        public static string LogsFolder = AppDomain.CurrentDomain.BaseDirectory + @"Logs\";
        public static string Users_file = AppDomain.CurrentDomain.BaseDirectory + @"Data\App\User.dat";
        public static string LogsFile = AppDomain.CurrentDomain.BaseDirectory + @"Logs\logs.txt";
        public static string ProgsFile = AppDomain.CurrentDomain.BaseDirectory + @"Data\Progs\progs.xml";
        public static string PROGRAMS_FILE_JSON = AppDomain.CurrentDomain.BaseDirectory + @"Data\Programs\programs.json";
        public static string XRayError_Logfile = AppDomain.CurrentDomain.BaseDirectory + @"Logs\XRayErrorsLog.txt";
        public static string IOBoxErrors_Gulmay = AppDomain.CurrentDomain.BaseDirectory + @"Errors\Gulmay.xml";
        public const int TIME_OUT = 300;
        /// <summary>
        /// Ссылка на потоковое вещание камеры
        /// </summary>
        public const string IURL_STREAM = "rtsp://admin:sura450t@192.168.10.22:554/ISAPI/Streaming/Channels/101";

        public static void ExistFile(string path)
        {
            if (!File.Exists(path))
                File.Create(path);
        }
    }

    internal static class Threads
    {
        public static VideoFileReader VideoRider;
    }
}
