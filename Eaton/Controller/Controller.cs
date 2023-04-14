using ProtoBuf;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Controller
{
    public class Controller
    {
        private const int MIN_TCP_PORT_NUMBER = 0;
        private const int MAX_TCP_PORT_NUMBER = 65535;
        private const string INCORRECT_PORT_NUMBER_EXCEPTION = "Incorrect port number";
        private const int MESSAGE_SIZE = 23;

        private Socket tcpListener;
        private List<Task> listenerTasks = new List<Task>();
        private List<Socket> listenerSockets = new List<Socket>();
        private static int messagesReceived = 0;

        public Controller(int listenersCount, int port, IPEndPoint endPoint = null)
        {
            if (port < MIN_TCP_PORT_NUMBER || port > MAX_TCP_PORT_NUMBER)
                throw new Exception(INCORRECT_PORT_NUMBER_EXCEPTION);

            if (endPoint == null)
                endPoint = GetEndPoint(port);

            tcpListener = new Socket(GetIPAddress().AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            tcpListener.Bind(endPoint);
            tcpListener.Listen(listenersCount);
        }

        public void Start()
        {
            try
            {
                Console.WriteLine("Controller is running...");

                while (true)
                {
                    var socket = tcpListener.Accept();

                    if (socket.Connected)
                    {
                        listenerSockets.Add(socket);
                        listenerTasks.Add(Task.Run(() => ListenerAction(socket)));
                    }
                }
            }
            finally
            {
                tcpListener.Shutdown(SocketShutdown.Both);
                tcpListener.Close();
            }
        }

        private static void ListenerAction(Socket socket)
        {
            try
            {
                while (true)
                {
                    var bytes = new byte[MESSAGE_SIZE];
                    Message message;

                    int messageSize = socket.Receive(bytes);

                    using (var stream = new MemoryStream())
                    {
                        stream.Write(bytes, 0, messageSize);
                        stream.Seek(0, SeekOrigin.Begin);

                        message = Serializer.Deserialize<Message>(stream);
                    }

                    Interlocked.Increment(ref messagesReceived);

                    Console.WriteLine("Received message from {0} with measurements {1}", message.SenderID, message.Measurements);
                    Console.WriteLine("Received {0} messages", messagesReceived);

                    var callbackMessage = string.Format("Data received successfull by controller at {0}", DateTime.Now.ToString());

                    bytes = Encoding.ASCII.GetBytes(callbackMessage);

                    socket.Send(bytes);
                }
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
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
