using System;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Driver.Virtual
{
    public class VirtualControllerManager : IDisposable
    {
        private ViGEmClient _client;
        private IXbox360Controller _controller;

        public event Action<byte, byte> FeedbackReceived;

        public VirtualControllerManager()
        {
            _client = new ViGEmClient();
            _controller = _client.CreateXbox360Controller();
            _controller.Connect();
            _controller.FeedbackReceived += Controller_FeedbackReceived;
            Console.WriteLine("Виртуальный контроллер Xbox 360 запущен.");
        }

        private void Controller_FeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            // Передаем значения вибрации через событие
            FeedbackReceived?.Invoke(e.LargeMotor, e.SmallMotor);
        }

        public void UpdateControllerState(Xbox360Button button, bool buttonPressed, short leftThumbX, short leftThumbY, byte leftTrigger, byte rightTrigger)
        {
            // Устанавливаем состояние кнопки
            //Console.WriteLine($"Кнопка {button} = {buttonPressed}");
            _controller.SetButtonState(button, buttonPressed);


            _controller.SetAxisValue(Xbox360Axis.LeftThumbX, leftThumbX);
            _controller.SetAxisValue(Xbox360Axis.LeftThumbY, leftThumbY);
            _controller.SetButtonState(Xbox360Button.LeftShoulder, buttonPressed);

            // Устанавливаем значение левого триггера
            _controller.SetSliderValue(Xbox360Slider.LeftTrigger, leftTrigger);
            _controller.SetSliderValue(Xbox360Slider.RightTrigger, rightTrigger);
        }

        public void Dispose()
        {
            _controller?.Disconnect();
            _client?.Dispose();
        }
    }
}
