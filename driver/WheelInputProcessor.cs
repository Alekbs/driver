using System;

namespace Driver.Input
{
    public class WheelInputProcessor : InputProcessor
    {
        public event Action<byte[]> OnWheelDataProcessed;

        public override void ProcessInput(byte[] data)
        {
            // Преобразование и обработка данных от руля
            // Можно добавить разбор конкретных байтов, формирование структуры и т.д.
            //Console.WriteLine("Обработка данных руля.");
            //Console.WriteLine(BitConverter.ToString(data));
            OnWheelDataProcessed?.Invoke(data);
        }
    }
}
