using HidLibrary;
using System;
using System.Linq;

namespace Driver.Input
{
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
            return true;
        }
    }
}
