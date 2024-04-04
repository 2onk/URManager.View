using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace URManager.View.Net
{
    public class ClientTCP
    {
        private readonly string _ip;
        private readonly TcpClient _clientTCP;
        private NetworkStream? _stream;
        private readonly int _dashboardPort = 29999;


        public ClientTCP(string IP)
        {
            _ip = IP;

            _clientTCP = new TcpClient();
            _clientTCP.ReceiveTimeout = 10000;
            _clientTCP.SendTimeout = 500;
        }

        /// <summary>
        /// Connect TCP client
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectToServerAsync()
        {
            var available = await PingAsync();

            if (available)
            {
                await _clientTCP.ConnectAsync(new IPEndPoint(GetIp(_ip), _dashboardPort));
                _stream = _clientTCP.GetStream();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Disconnnect TCP client
        /// </summary>
        public void Disconnect()
        {
            if (_clientTCP is not null)
            {
                _clientTCP.Close();
                _clientTCP.Dispose();
            }
        }

        /// <summary>
        /// Read TCP client stream 
        /// </summary>
        /// <returns>Message as string</returns>
        public string ReceiveMessage()
        {
            try
            {
                if (_stream is not null)
                {
                    byte[] data = new Byte[256];

                    Int32 bytes = _stream.Read(data, 0, data.Length);
                    return System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                }
                else
                {
                    return ("something went wrong");
                }
            }
            catch
            {
                _stream?.Dispose();
                _clientTCP.Close();
                return ("Lost Connection");
            }

        }

        /// <summary>
        /// Send encoded ASCII string to TCP client
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(string msg)
        {
            try
            {
                if (_stream is not null)
                {
                    msg = msg + "\r\n";
                    Byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);
                    _stream.Write(data, 0, data.Length);
                }
            }
            catch
            {
                _stream?.Dispose();
                _clientTCP.Close();
            }
        }

        /// <summary>
        /// Set IP Adress in the TCP client
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private static IPAddress GetIp(string ip)
        {
            return IPAddress.Parse(ip);
        }

        /// <summary>
        /// Ping desired TCP client
        /// </summary>
        /// <returns></returns>
        private async Task<bool> PingAsync()
        {
            Ping ping = new Ping();

            PingReply result = await ping.SendPingAsync(_ip);
            return result.Status == IPStatus.Success;
        }
    }
}
