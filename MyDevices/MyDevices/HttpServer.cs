using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MyDevices
{
    public class HttpServer
    {
        /// <summary>
        /// 当前IP
        /// </summary>
        public string ip { get; set; }
        /// <summary>
        /// 监听地址
        /// </summary>
        public string url { get; set; }
        HttpListener httpListenner { get; set; }
        public HttpServer()
        {
            this.ip = this.getIP();
            this.httpListenner = new HttpListener();
            httpListenner = new HttpListener();
            httpListenner.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            this.url = $"http://{this.ip}:8124/";
            httpListenner.Prefixes.Add(this.url);
            httpListenner.Start();
        }

        public void Listenner(Dictionary<string, Action<string>> action)
        {
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    while (true)
                    {
                        try
                        {
                            HttpListenerContext context = httpListenner.GetContext();
                            HttpListenerRequest request = context.Request;
                            HttpListenerResponse response = context.Response;
                            string path = request.Url.LocalPath;
                            string key = request.QueryString["key"];
                            string value = request.QueryString["value"];
                            Dictionary<string, object> dict = new Dictionary<string, object>();
                            dict.Add(key, value);
                            dict.Add("path", path);
                            dict.Add("time", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            // 执行命令
                            if (action.ContainsKey(key))
                            {
                                action[key](value);
                            }
                            string responseString = JsonConvert.SerializeObject(dict);
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                            //对客户端输出相应信息.
                            response.ContentType = "application/json; charset=utf-8";
                            response.ContentLength64 = buffer.Length;
                            System.IO.Stream output = response.OutputStream;
                            output.Write(buffer, 0, buffer.Length);
                            //关闭输出流，释放相应资源
                            output.Close();
                        }
                        catch (System.Exception ex)
                        {
                            System.Console.WriteLine(ex);
                        }

                    }
                }
                catch (Exception)
                {
                    // httpListenner.Stop();
                }
            })).Start();
        }

        public void Stop()
        {
            if (this.httpListenner.IsListening)
            {
                this.httpListenner.Stop();
            }
        }

        /// <summary>
        /// 广播当前接口
        /// </summary>
        public void Broadcast()
        {
            UdpClient udpClient = new UdpClient();
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("ip", this.ip);
            dict.Add("api", this.url);
            string data = JsonConvert.SerializeObject(dict);
            udpClient.Send(System.Text.Encoding.UTF8.GetBytes(data), data.Length, new IPEndPoint(IPAddress.Broadcast, 9234));
        }

        string getIP()
        {
            return "10.1.1.214";
            //string ip = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
            //    .Select(p => p.GetIPProperties())
            //    .SelectMany(p => p.UnicastAddresses)
            //    .Where(p => p.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(p.Address))
            //    .FirstOrDefault()?.Address.ToString();
            //return ip;
        }
    }
}
