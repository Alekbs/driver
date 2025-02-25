using System;
using System.Threading;
using Driver.Input;
using Driver.Virtual;
using Driver.Feedback;
using HidLibrary;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            // Инициализируем устройства
            var deviceManager = new HidDeviceManager();
            if (!deviceManager.InitializeDevices())
            {
                return;
            }

            // Инициализируем процессоры ввода
            var wheelProcessor = new WheelInputProcessor();
            var joyProcessor = new JoyInputProcessor();

            // Инициализируем виртуальный контроллер
            using var virtualController = new VirtualControllerManager();

            // Инициализируем обработчик обратной связи
            var feedbackHandler = new FeedbackHandler(deviceManager.WheelDevice, deviceManager.JoyDevice);

            // Подписываемся на обратную связь от виртуального контроллера
            virtualController.FeedbackReceived += (lForce, rForce) =>
            {
                feedbackHandler.ProcessFeedback(lForce, rForce);
            };

            // Подписываемся на события обработки данных от руля
            wheelProcessor.OnWheelDataProcessed += (data) =>
            {

                // Пример обработки данных. Если data[7] == 0x1F, считаем, что кнопка LeftShoulder нажата.
                bool rightThumbPressed = data[6] == 0x1F;

                // Обработка педалей
                // Если педаль не нажата, значение равно 0x80.
                // Для тормоза (левый триггер): 0x80 -> 0 (нет нажатия), 0xFF -> 255 (полностью нажато)
                byte rawBrake =data[2];
                byte leftTrigger = (byte)Math.Clamp((rawBrake - 0x80) * (255.0 / (0xFF - 0x80)), 0, 255);

                // Для газа (правый триггер): 0x80 -> 0 (нет нажатия), 0x00 -> 255 (полностью нажато)
                byte rawGas = data[2];
                byte rightTrigger = (byte)Math.Clamp((0x80 - rawGas) * (255.0 / 0x80), 0, 255);

                short leftThumbX = ConvertByteToShortAxis(data[1]);
                short leftThumbY = 0;
                

                virtualController.UpdateControllerStateWeel(
                    
                    rightThumbPressed,
                    leftThumbX,
                    leftThumbY,
                    leftTrigger,
                    rightTrigger);
                    
            };

            // Пример для джойстика – можно комбинировать данные с руля или обрабатывать отдельно.
            joyProcessor.OnJoyDataProcessed += (data) =>
            {
                





                short rightThumbX = ConvertByteToShortAxisJoy(data[1]);
                short rightThumbY = ConvertByteToShortAxisJoy((byte)(255 - data[2]));
                Console.WriteLine(data[5]);
                bool rightThumbPressed = data[4] == 0x20;
                virtualController.UpdateControllerStateJoy(
                    rightThumbX,
                    rightThumbY);
                // Если требуется, здесь можно обновлять дополнительные элементы контроллера.
                // Например, обновлять правый стик или другой набор кнопок.
            };

            // Запускаем чтение данных в отдельных потоках
            var wheelThread = new Thread( () =>
            {
                while (true)
                {
                    var data = deviceManager.WheelDevice.Read();
                    if (data.Data.Length > 0)
                    {
                        wheelProcessor.ProcessInput(data.Data);
                    }
                    

                }
            });

            var joyThread = new Thread( () =>
            {
                while (true)
                {
                    var data = deviceManager.JoyDevice.Read();
                    if (data.Data.Length > 0)
                    {
                        joyProcessor.ProcessInput(data.Data);
                    }
                    

                }
            });

            wheelThread.Start();
            joyThread.Start();

            Console.WriteLine("Нажмите Enter для выхода...");
            Console.ReadLine();

            // Остановка потоков – рекомендуется использовать корректное завершение, а не Abort.
            wheelThread.Interrupt();
            joyThread.Interrupt();
        }
        // Преобразование байта (0-255) в диапазон short (-32768 ... 32767)
        static short ConvertByteToShortAxis(byte value)
        {
            // Сначала переводим байт в диапазон 0..1 (float), затем масштабируем к диапазону оси
            float normalized = value / 255f; // значение от 0 до 1
                                             // Масштабируем: -32768 + normalized * (32767 - (-32768)) = -32768 + normalized * 65535
            short axisValue = (short)(-32768 + normalized * 65535);
            return axisValue;
        }
        static short ConvertByteToShortAxisJoy(byte value)
        {
            byte x = value; // или другой индекс, где хранится значение
            byte result;
            if (x >= 0x80)  // x от 0x80 до 0xFF
            {
                // При x = 0x80 → 0, при x = 0xFF → 127
                result = (byte)(x - 0x80);
            }
            else  // x от 0x00 до 0x7F
            {
                // При x = 0x00 → 128, при x = 0x7F → 255
                result = (byte)(0x80 + x);
            }
            return ConvertByteToShortAxis(result);
        }
    }
}
