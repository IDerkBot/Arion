using Accord.Video.FFMPEG;
using ArionCameraXrayDefender.Models;
using ArionControlLibrary.Utilities;
using System.Windows.Controls;

namespace ArionCameraXrayDefender.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для VideoStreamPage.xaml
    /// </summary>
    public partial class VideoStreamPage : Page
    {
        public VideoStreamPage()
        {
            InitializeComponent();

            Threads.VideoRider = new VideoFileReader();

            var ipAndPort = Constants.IURL_STREAM.Split('@')[1].Split('/')[0].Split(':');
            var ip = ipAndPort[0];

            if (ArionLibrary.Utilities.Net.PingIP(ip))
            {
                Threads.VideoRider.Open(Constants.IURL_STREAM);

                VideoCamera videoCamera = new VideoCamera();
                videoCamera.UpdateImage(Threads.VideoRider, Img);
            }
        }
    }
}
