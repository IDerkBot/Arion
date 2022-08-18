using System.Windows.Input;

namespace ArionCameraXrayDefender.Models
{
    internal static class Checker
    {
        internal static void RussiaChar(TextCompositionEventArgs e)
        {
            char ch = char.Parse(e.Text);
            if ((ch >= 'А' && ch <= 'Я') || (ch >= 'а' && ch <= 'я') || (ch >= '0' && ch <= '9') || ch == (char)Key.Back || ch == '-' || ch == '.')
            {
            }
            else
                e.Handled = true;
        }
        internal static void EnglishChar(TextCompositionEventArgs e)
        {
            char ch = char.Parse(e.Text);
            if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == (char)Key.Back || ch == '-' || ch == '.')
            {
            }
            else
                e.Handled = true;
        }

        internal static void IntChar(TextCompositionEventArgs e)
        {
            char ch = char.Parse(e.Text);
            if ((ch >= '0' && ch <= '9') || ch == (char)Key.Back)
            {
            }
            else
                e.Handled = true;
        }

        internal static void FloatChar(TextCompositionEventArgs e)
        {
            char ch = char.Parse(e.Text);
            if ((ch >= '0' && ch <= '9') || ch == ',' || ch == '.' || ch == (char)Key.Back)
            {
            }
            else
                e.Handled = true;
        }
    }
}
