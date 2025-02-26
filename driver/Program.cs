using System;
using System.Threading;
using Driver.Input;
using Driver.Virtual;
using Driver.Feedback;
using HidLibrary;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections;

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
                bool rightshoulder = (data[5] & 0x20) != 0; 
                bool leftshoulder = (data[5] & 0x10) != 0;
                // Проверяем состояние кнопок, используя биты в data[5].
                // Инициализация состояния кнопок
                bool up = ((data[5] & 15)  ) == 0;
                bool right = (data[5] & 15) == 2;  
                bool down = (data[5] & 15) == 4;   
                bool left = (data[5] & 15) == 6;  

                // Если 4-й бит установлен (кнопки не нажаты), сбрасываем все кнопки
                if ((data[5] & 0x08) != 0)
                {
                    up = down = left = right = false;
                }




                // Обработка педалей
                // Если педаль не нажата, значение равно 0x80.
                // Для тормоза (левый триггер): 0x80 -> 0 (нет нажатия), 0xFF -> 255 (полностью нажато)
                byte rawBrake =data[1];
                byte leftTrigger = (byte)Math.Clamp((rawBrake - 0x80) * (255.0 / (0xFF - 0x80)), 0, 255);

                // Для газа (правый триггер): 0x80 -> 0 (нет нажатия), 0x00 -> 255 (полностью нажато)
                byte rawGas = data[1];
                byte rightTrigger = (byte)Math.Clamp((0x80 - rawGas) * (255.0 / 0x80), 0, 255);

                short leftThumbX = ConvertByteToShortAxis(data[0]);
                short leftThumbY = 0;
                

                virtualController.UpdateControllerStateWeel(
                    
                    rightshoulder,
                    leftshoulder,
                    up,
                    down,
                    left,
                    right,
                    leftThumbX,
                    leftTrigger,
                    rightTrigger);
                    
            };

            // Пример для джойстика – можно комбинировать данные с руля или обрабатывать отдельно.
            joyProcessor.OnJoyDataProcessed += (data) =>
            {
                
 
                short rightThumbX = ConvertByteToShortAxisJoy(data[1]);
                short leftThumbY = ConvertByteToShortAxisJoy((sbyte)(255 - data[3]));
                short rightThumbY = ConvertByteToShortAxisJoy((sbyte)(255 - data[2]));

                bool rightThumb = (data[5] & 0x10) != 0;

                bool leftThumb = (data[5] & 0x20) != 0; 

                bool buttonA = (data[6] & 0x01) != 0;  // Проверка 1-го бита (00000001)
                bool buttonB = (data[6] & 0x02) != 0;  // Проверка 2-го бита (00000010)
                bool buttonX = (data[6] & 0x04) != 0;  // Проверка 3-го бита (00000100)
                bool buttonY = (data[6] & 0x08) != 0;  // Проверка 4-го бита (00001000)


                virtualController.UpdateControllerStateJoy(
                    rightThumbX,
                    rightThumbY,
                    leftThumbY,
                    rightThumb,
                    leftThumb,
                    buttonA,
                    buttonB,
                    buttonX,
                    buttonY);
                // Если требуется, здесь можно обновлять дополнительные элементы контроллера.
                // Например, обновлять правый стик или другой набор кнопок.
            };
            // Запускаем чтение данных в отдельных потоках

            var wheelThread = Task.Factory.StartNew(async () =>
            {
                byte[] buffer = new byte[64];  // Размер буфера может быть изменен в зависимости от данных устройства
                byte[] dataw = new byte[8];
                byte[] previousData = new byte[dataw.Length]; // Храним предыдущее состояние
                while (true)
                {
                    // Чтение данных с устройства
                    
                    int bytesRead = HamaV18.ReadWheelData(buffer, buffer.Length);
                    if (bytesRead > 0 && !buffer.Take(bytesRead).SequenceEqual(previousData.Take(bytesRead)))
                    {
                        Array.Copy(buffer, dataw, bytesRead);
                        Array.Copy(dataw, previousData, bytesRead);
                        wheelProcessor.ProcessInput(dataw);
                    }


                }
            },TaskCreationOptions.LongRunning);

            var joyThread = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    var data = deviceManager.JoyDevice.Read();
                    if (data.Data.Length > 0)
                    {

                        byte[] byteArray = data.Data; // Ваш исходный массив byte[]
                        sbyte[] sbyteArray = byteArray.Select(b => (sbyte)b).ToArray();
                        joyProcessor.ProcessInput(sbyteArray);
                    }
                    

                }
            }, TaskCreationOptions.LongRunning);

         

            //Console.WriteLine("Нажмите Enter для выхода...");
            Console.ReadLine();
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
        static short ConvertByteToShortAxisJoy(sbyte value)
        {
            return (short)(value * 256);
        }
    }
}
