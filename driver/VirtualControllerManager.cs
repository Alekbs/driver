﻿using System;
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

        public void UpdateControllerStateWeel( bool buttonPressed, short leftThumbX, short leftThumbY, byte leftTrigger, byte rightTrigger)
        {
            // Устанавливаем состояние кнопки
            //Console.WriteLine($"Кнопка {button} = {buttonPressed}");
            _controller.SetButtonState(Xbox360Button.Right, buttonPressed);


            _controller.SetAxisValue(Xbox360Axis.LeftThumbX, leftThumbX);
            _controller.SetAxisValue(Xbox360Axis.LeftThumbY, leftThumbY);

            // Устанавливаем значение левого триггера
            _controller.SetSliderValue(Xbox360Slider.LeftTrigger, leftTrigger);
            _controller.SetSliderValue(Xbox360Slider.RightTrigger, rightTrigger);
        }

        public void UpdateControllerStateJoy(short rightThumbX, short rightThumbY)
        {


            _controller.SetAxisValue(Xbox360Axis.RightThumbX, rightThumbX);
            _controller.SetAxisValue(Xbox360Axis.RightThumbY, rightThumbY);

        }

        public void Dispose()
        {
            _controller?.Disconnect();
            _client?.Dispose();
        }
    }
}
