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

        public void UpdateControllerStateWeel( bool rightshoulder, bool leftshoulder, bool up, bool down, bool left, bool right, short leftThumbX, byte leftTrigger, byte rightTrigger)
        {
            // Устанавливаем состояние кнопки
            //Console.WriteLine($"Кнопка {button} = {buttonPressed}");
            _controller.SetButtonState(Xbox360Button.RightShoulder, rightshoulder);
            _controller.SetButtonState(Xbox360Button.LeftShoulder, leftshoulder);
            _controller.SetButtonState(Xbox360Button.Up, up);
            _controller.SetButtonState(Xbox360Button.Down, down);
            _controller.SetButtonState(Xbox360Button.Left, left);
            _controller.SetButtonState(Xbox360Button.Right, right);

            _controller.SetAxisValue(Xbox360Axis.LeftThumbX, leftThumbX);
            // Устанавливаем значение левого триггера
            _controller.SetSliderValue(Xbox360Slider.LeftTrigger, leftTrigger);
            _controller.SetSliderValue(Xbox360Slider.RightTrigger, rightTrigger);
        }

        public void UpdateControllerStateJoy(short rightThumbX, short rightThumbY, short leftThumbY, bool rightThumb, bool leftThumb, bool buttonA, bool buttonB, bool buttonX, bool buttonY)
        {

            _controller.SetAxisValue(Xbox360Axis.LeftThumbY, leftThumbY);
            _controller.SetAxisValue(Xbox360Axis.RightThumbX, rightThumbX);
            _controller.SetAxisValue(Xbox360Axis.RightThumbY, rightThumbY);
            _controller.SetButtonState(Xbox360Button.RightThumb, rightThumb);
            _controller.SetButtonState(Xbox360Button.LeftThumb, leftThumb);
            _controller.SetButtonState(Xbox360Button.A, buttonA);
            _controller.SetButtonState(Xbox360Button.B, buttonB);
            _controller.SetButtonState(Xbox360Button.X, buttonX);
            _controller.SetButtonState(Xbox360Button.Y, buttonY);

        }

        public void Dispose()
        {
            _controller?.Disconnect();
            _client?.Dispose();
        }
    }
}
