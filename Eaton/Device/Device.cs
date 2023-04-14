using ProtoBuf;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Device
{
    public class Device
    {
        private const int MIN_TCP_PORT_NUMBER = 0;
        private const int MAX_TCP_PORT_NUMBER = 65535;
        private const string INCORRECT_PORT_NUMBER_EXCEPTION = "Incorrect port number";
        private const int MESSAGE_SIZE = 63;
        private const int MEASUREMENT_MAX_VALUE = 1000;
        private const int MESSAGE_DELAY_MAX_VALUE = 3000;

        private Socket sender;
        private Random random = new Random();
        private Message message = new Message { SenderID = Guid.NewGuid() };

        public Device(int port, IPEndPoint endPoint = null)
        {
            if (port < MIN_TCP_PORT_NUMBER || port > MAX_TCP_PORT_NUMBER)
                throw new Exception(INCORRECT_PORT_NUMBER_EXCEPTION);

            if (endPoint == null)
                endPoint = GetEndPoint(port);

            sender = new Socket(GetIPAddress().AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(endPoint);
        }

        public void Start()
        {
            try
            {
                Console.WriteLine("Device is running...");

                while (true)
                {
                    byte[] bytes;

                    message.Measurements = random.Next(MEASUREMENT_MAX_VALUE);

                    using (var stream = new MemoryStream())
                    {
                        Serializer.Serialize(stream, message);
                        bytes = stream.ToArray();
                    }

                    sender.Send(bytes);

                    var callbackMessage = new byte[MESSAGE_SIZE];

                    sender.Receive(callbackMessage);

                    Console.WriteLine(Encoding.ASCII.GetString(callbackMessage, 0, MESSAGE_SIZE));

                    Thread.Sleep(random.Next(MESSAGE_DELAY_MAX_VALUE));
                }
            }
            finally 
            {
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
        }

        private IPEndPoint GetEndPoint(int port)
        {
            return new IPEndPoint(GetIPAddress(), port);
        }

        private IPAddress GetIPAddress()
        {
            var ipHost = Dns.GetHostEntry(Dns.GetHostName());
            return ipHost.AddressList[0];
        }
    }
}
