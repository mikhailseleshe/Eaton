namespace Device
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var device = new Device(12345);
            device.Start();
        }
    }
}