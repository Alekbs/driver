using System;

namespace Driver.Input
{
    public class JoyInputProcessor
    {
        public event Action<sbyte[]> OnJoyDataProcessed;

        public void ProcessInput(sbyte[] data)
        {
            // Преобразование и обработка данных от джойстика

            //Console.WriteLine(); // Переход на новую строку
            OnJoyDataProcessed?.Invoke(data);
        }
    }
}
