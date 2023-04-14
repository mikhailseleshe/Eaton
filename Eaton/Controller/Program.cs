namespace Controller
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Controller controller = new Controller(10, 12345);
            controller.Start();
        }
    }
}