using System.Windows.Media;
using static System.Windows.Media.ColorConverter;

namespace ArionCameraXrayDefender.Models
{
    class ColorPalete
    {
        public static readonly SolidColorBrush SolidLightBlue = new SolidColorBrush((Color)ConvertFromString("#E7EBF0"));
        public static readonly SolidColorBrush SolidRed = new SolidColorBrush(Colors.Red);
        public static readonly SolidColorBrush SolidGreen = new SolidColorBrush(Colors.LimeGreen);
        public static readonly SolidColorBrush SolidYellow = new SolidColorBrush(Colors.Yellow);
        public static readonly SolidColorBrush SolidGrayBlue = new SolidColorBrush((Color)ConvertFromString("#C9DAF2"));
        public static readonly SolidColorBrush SolidDarkBlue = new SolidColorBrush((Color)ConvertFromString("#335D8E"));
        public static readonly SolidColorBrush SolidGray = new SolidColorBrush(Colors.Gray);
        public static readonly SolidColorBrush SolidWhite = new SolidColorBrush(Colors.White);
        public static readonly SolidColorBrush Transparent = new SolidColorBrush(Colors.Transparent);

        public static readonly LinearGradientBrush GradientWhiteBlack =
            new LinearGradientBrush(Colors.White, Colors.Black, 45);
    }
}
