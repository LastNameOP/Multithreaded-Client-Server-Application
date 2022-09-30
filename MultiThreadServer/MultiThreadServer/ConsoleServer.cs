using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MultiThreadServer
{
    // Сделать закрытие потока и стрима при разрыве соединения
    public class ClientObject
    {
        public TcpClient client;
        public ClientObject(TcpClient tcpClient)
        {
            client = tcpClient;
        }

        public void Process()
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                byte[] data = new byte[64]; 
                while (true)
                {
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    Program.history.Add(message);
                    Console.WriteLine(message);
                    // отправляем обратно сообщение 
                    data = Encoding.Unicode.GetBytes(message);
                    foreach (var localclient in Program.clients)
                    {
                        NetworkStream networkStream = localclient.GetStream();
                        networkStream.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
                Thread.CurrentThread.Abort();
                Thread.CurrentThread.Join(500);
            }
        }
        public void SendHistory()
        {
            NetworkStream networkStream = Program.clients.Last().GetStream(); 
            foreach (string message in Program.history)
            {
                byte[] data = Encoding.Unicode.GetBytes(message);
                networkStream.Write(data, 0, data.Length);
            }
        }
    }
}