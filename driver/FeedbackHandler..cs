using System;
using HidLibrary;

namespace Driver.Feedback
{
    public class FeedbackHandler
    {
        private HidDevice _wheelDevice;
        private HidDevice _joyDevice;

        public FeedbackHandler(HidDevice wheel, HidDevice joy)
        {
            _wheelDevice = wheel;
            _joyDevice = joy;
        }

        public void ProcessFeedback(byte leftForce, byte rightForce)
        {
            // Усиливаем вибрацию в 2 раза, ограничивая значение 255 (0xFF)
            byte amplifiedLeft = (byte)Math.Min(leftForce * 4, 0xFF);
            byte amplifiedRight = (byte)Math.Min(rightForce * 4, 0xFF);

            //Console.WriteLine($"Получена обратная связь: Left = {amplifiedLeft}, Right = {amplifiedRight}");

            // Пример отправки данных на руль
            byte[] wheelCommand1 = new byte[] { 0x00, 0x51, 0x00, amplifiedLeft, 0x00, amplifiedRight, 0x00, 0x00 };
            byte[] wheelCommand2 = new byte[] { 0x00, 0xFA, 0xFE, 0x00, 0x00, 0x00, 0x00, 0x00 };
            _wheelDevice.Write(wheelCommand1);
            _wheelDevice.Write(wheelCommand2);

            // Пример отправки данных на джойстик
            byte[] joyCommand = new byte[] { 0x00, 0x00, 0x20, amplifiedRight, amplifiedLeft };
            _joyDevice.Write(joyCommand);
        }

    }
}
