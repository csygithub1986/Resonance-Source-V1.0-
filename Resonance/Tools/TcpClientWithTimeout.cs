using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace Resonance
{
    /// <summary>
    /// 带超时的TCP连接类
    /// </summary>
    class TcpClientWithTimeout
    {
        //protected string _hostname;
        protected IPAddress _ipAddress;
        protected int _port;
        protected int _timeout_milliseconds;
        protected TcpClient tcpClient;
        protected bool connected;
        protected Exception exception;

        public TcpClientWithTimeout(IPAddress ipAddress, int port, int timeout_milliseconds)
        {
            _ipAddress = ipAddress;
            _port = port;
            _timeout_milliseconds = timeout_milliseconds;
        }

        public TcpClient Connect()
        {
            // kick off the thread that tries to connect
            connected = false;
            exception = null;
            Thread thread = new Thread(new ThreadStart(BeginConnect));
            thread.IsBackground = true; // 作为后台线程处理
            // 不会占用机器太长的时间
            thread.Start();

            // 等待如下的时间
            thread.Join(_timeout_milliseconds);

            if (connected == true)
            {
                // 如果成功就返回TcpClient对象
                thread.Abort();
                return tcpClient;
            }
            if (exception != null)
            {
                // 如果失败就抛出错误
                thread.Abort();
                throw exception;
            }
            else
            {
                // 同样地抛出错误
                thread.Abort();
                string message = string.Format("TcpClient connection to {0}:{1} timed out",
                  _ipAddress, _port);
                throw new TimeoutException(message);
            }
        }

        protected void BeginConnect()
        {
            try
            {
                tcpClient = new System.Net.Sockets.TcpClient(AddressFamily.InterNetwork);
                tcpClient.Connect(_ipAddress, _port);
                //tcpClient = new TcpClient(_hostname, _port);
                // 标记成功，返回调用者
                connected = true;
            }
            catch (Exception ex)
            {
                // 标记失败
                exception = ex;
            }
        }

    }
}
