using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArionLibrary.Controllers;

namespace ArionLibraryConsoleTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            byte slave = 1;
            MotorController controller = new MotorController(slave, new SerialPort("COM3", 38400));
            while (true)
            {
                Console.Write($"{string.Join(" ", controller.Command(24582, 4))}\r");
                Thread.Sleep(100);
            }
        }
    }
}
