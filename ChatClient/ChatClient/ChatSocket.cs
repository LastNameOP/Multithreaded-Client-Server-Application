using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection.Emit;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ChatClient;
using System.Security.Cryptography;
using System.Threading;
using System.Text.RegularExpressions;
using System.Windows.Markup;

namespace ChatClient
{
    class ChatSocket : INotifyPropertyChanged
    {
        #region Интерфейс MVVM
        // Интерфейс 
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Chat { get; set; } = "Чат";
        public string Message { get; set; }
        public string Nick { get; set; } = "Unnamed";
        public string IP { get; set; } = "255.255.255.255";
        public string ConnectLabel { get; set; } = "Not Connected";
        public string ConnectLabelColor { get; set; } = "Red";
        public bool ConnectButtonEnabled { get; set; } = true;
        public bool NickBoxEnabled { get; set; } = true;
        public bool IpBoxEnabled { get; set; } = true;

        private Model addCommand;
        private Model addConnection;
        public Model AddCommand
        {
            get
            {
                return addCommand ??
                  (addCommand = new Model(async obj =>
                  {
                      await SendMessageAsync();
                  }));
            }
        }
        public Model AddConnection
        {
            get
            {
                return addConnection ??
                  (addConnection = new Model(obj =>
                  {
                      if (!isConnect && ConnectLabel != "Connect")
                      {
                          Connect();
                      }
                  }));
            }
        }
        public ChatSocket() {}
        // Интерфейс
        #endregion
        const int port = 8888;
        static bool isConnect = false;
        NetworkStream stream;
        string userName;
        TcpClient client = null;
        void Connect()
        {
            try
            {
                if (CheckIP(IP))
                {
                    userName = Nick;
                    client = new TcpClient(IP, port);
                    stream = client.GetStream();
                    ConnectLabel = "Connected";
                    OnPropertyChanged("ConnectLabel");
                    ConnectLabelColor = "LimeGreen";
                    OnPropertyChanged("ConnectLabelColor");
                    isConnect = true;
                    ConnectButtonEnabled = false;
                    OnPropertyChanged("ConnectButtonEnabled");
                    NickBoxEnabled = false;
                    OnPropertyChanged("NickBoxEnabled");
                    IpBoxEnabled = false;
                    OnPropertyChanged("IpBoxEnabled");

                    stream = client.GetStream();
                    byte[] data = new byte[64]; 

                    if (stream.DataAvailable)
                    {
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        do
                        {
                            bytes = stream.Read(data, 0, data.Length);
                            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        }
                        while (stream.DataAvailable);
                        string message = builder.ToString();
                        Chat += "\r\n" + message;
                        OnPropertyChanged("Chat");
                    }
                }
            } catch(Exception ex)
            {
                
            }
        }
        void SendMessage(string messageTo)
        {
            
            try
            {
                // ввод сообщения
                string message = Message;
                message = String.Format("[{2}]{0}: {1}", userName, message, DateTime.Now.ToShortTimeString());
                byte[] data = Encoding.Unicode.GetBytes(message);
                Message = String.Empty;
                OnPropertyChanged("Message");
                // отправка сообщения
                stream.Write(data, 0, data.Length);

                stream = client.GetStream();
                data = new byte[64]; 
                while (true)
                {
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    message = builder.ToString();
                    Chat += "\r\n" + message;
                    OnPropertyChanged("Chat");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }
        async Task SendMessageAsync()
        {
            await Task.Run(() => SendMessage(Message)); // Возможно это ненадо
        }
        bool CheckIP(string ip)
        {
            IPAddress checkIP;
            return IPAddress.TryParse(ip, out checkIP);
        }
    }
}
