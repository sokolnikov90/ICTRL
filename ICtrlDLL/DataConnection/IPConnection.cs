using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ICtrlDLL
{
    public class IPConnection
    {
        public delegate void ReadDelegate(string message, bool isComplite);
        public event ReadDelegate ReadEvent;

        private Thread thread;
        private TcpClient tcpClient;
        private NetworkStream networkStream;

        public bool Connect(string ip, int port)
        {
            try
            {
                Disconnect();

                AutoResetEvent autoResetEvent = new AutoResetEvent(false);

                tcpClient = new TcpClient();

                
                tcpClient.BeginConnect(ip,
                                       port,
                                       new AsyncCallback(
                                           delegate(IAsyncResult asyncResult)
                                           {
                                               try
                                               {
                                                   tcpClient.EndConnect(asyncResult);
                                               }
                                               catch { }

                                               autoResetEvent.Set();
                                           }
                                       ),
                                       tcpClient);
            
                if (!autoResetEvent.WaitOne())
                    throw new Exception();

                networkStream = tcpClient.GetStream();

                thread = new Thread(new ThreadStart(Read));

                thread.IsBackground = true;
                thread.Name = "ReadThread";
                thread.Start();

                return true;
            }
            catch (Exception e)
            {
                ICtrl.logger.Info("Connect(...) exception:");

                ICtrl.logger.Info("ip: " + ip);
                ICtrl.logger.Info("port: " + port);

                ICtrl.logger.Info(e.Message);
                ICtrl.logger.Info(e.Source);
                ICtrl.logger.Info(e.StackTrace);

                DBConnection.Instance.Disconnect();

                Environment.Exit(0);
            }

            return false;
        }

        public void Disconnect()
        {
            try
            {
                if (networkStream != null)
                    networkStream.Close();

                if (tcpClient != null)
                    tcpClient.Close();

                if (thread != null)
                    thread.Abort();
            }
            catch (Exception e)
            {
                ICtrl.logger.Info("Disconnect(...) exception:");
                ICtrl.logger.Info(e.Message);
                ICtrl.logger.Info(e.Source);
                ICtrl.logger.Info(e.StackTrace);

                DBConnection.Instance.Disconnect();

                Environment.Exit(0);
            }
        }

        private void Read()
        {
            string buf = "";

            byte[] read = new byte[65535];

            try
            {
                while (true)
                {
                    int readLength = networkStream.Read(read, 0, read.Length);

                    if (readLength <= 0)
                        break;

                    buf += Encoding.Default.GetString(read, 0, readLength);

                    int start = 0;
                    int end = 0;

                    while (start != -1 && end != -1)
                    {
                        start = buf.IndexOf("<Message>");

                        if (start == -1)
                            break;

                        end = buf.IndexOf("</Message>", start);

                        if (end == -1)
                            break;

                        end += 10;

                        string message = buf.Substring(start, end - start);

                        start = message.LastIndexOf("<Message>");
                        message = message.Substring(start);

                        ReadEvent.Invoke(message, true);

                        buf = buf.Substring(end);
                    }
                }
            }
            catch { }
        }

        public void Write(string message)
        {
            ICtrl.logger.Info("ICtrl send message:" + message);
            if (networkStream == null)
            {
                ICtrl.logger.Info("networkStream == null");
                networkStream = tcpClient.GetStream();
            }
            try
            {
                networkStream.Write(Encoding.Default.GetBytes(message), 0, message.Length);
            }
            catch (Exception e)
            {
                ICtrl.logger.Info("ICtrl send message ERROR:" + e.ToString());
            }
        }
    }
}
