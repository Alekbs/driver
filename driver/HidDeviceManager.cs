using HidLibrary;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Driver.Input
{
    class HamaV18
    {
        private const string DllName = "hama_v18.dll"; // Имя вашей библиотеки

        // Импортируем функцию InitWheel из DLL
        [DllImport(DllName)]
        public static extern int InitWheel();

        // Импортируем функцию ReadWheelData из DLL
        [DllImport(DllName)]
        public static extern int ReadWheelData(byte[] buffer, int size);

        // Импортируем функцию CloseWheel из DLL
        [DllImport(DllName)]
        public static extern void CloseWheel();
    }
    public class HidDeviceManager
    {
        public HidDevice WheelDevice { get; private set; }
        public HidDevice JoyDevice { get; private set; }


        public bool InitializeDevices()
        {
            WheelDevice = HidDevices.Enumerate(0x11FF, 0x3331).FirstOrDefault();
            if (WheelDevice == null)
            {
                Console.WriteLine("Руль не найден.");
                return false;
            }
            WheelDevice.OpenDevice();

            JoyDevice = HidDevices.Enumerate(0x07B5, 0x0317).FirstOrDefault();
            if (JoyDevice == null)
            {
                Console.WriteLine("Джойстик не найден.");
                return false;
            }
            JoyDevice.OpenDevice();

            if (!WheelDevice.IsConnected || !JoyDevice.IsConnected)
            {
                Console.WriteLine("Не удалось открыть одно из устройств.");
                return false;
            }
            Console.WriteLine("Устройства успешно открыты.");
            int initResult = HamaV18.InitWheel();
            if (initResult == -1)
            {
                Console.WriteLine("Не удалось инициализировать устройство.");
                return false;
            }
            // Буфер для данных
            Console.WriteLine("Устройство инициализировано.");
            return true;
        }
    }
}
