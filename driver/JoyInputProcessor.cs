using System;

namespace Driver.Input
{
    public class JoyInputProcessor : InputProcessor
    {
        public event Action<byte[]> OnJoyDataProcessed;

        public override void ProcessInput(byte[] data)
        {
            // Преобразование и обработка данных от джойстика
            //Console.WriteLine(BitConverter.ToString(data));
            OnJoyDataProcessed?.Invoke(data);
        }
    }
}
