using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using Microsoft.International.Converters.PinYinConverter;

namespace MyDevices
{
    class MqttHA
    {
        public string ip { get; set; }
        public string host { get; set; }
        public int port { get; set; }
        public string user { get; set; }
        public string password { get; set; }

        string model { get; set; }

        public IMqttClient mqttClient = null;

        public MqttHA(string host, string port, string user, string password, string ip, string model)
        {
            this.host = host;
            this.port = int.Parse(port);
            this.user = user;
            this.password = password;
            this.ip = ip;
            this.model = model;
        }

        public string PinYin(string name)
        {
            string pinyin = "";
            foreach (char item in name)
            {
                try
                {
                    ChineseChar cc = new ChineseChar(item);
                    if (cc.Pinyins.Count > 0 && cc.Pinyins[0].Length > 0)
                    {
                        string temp = cc.Pinyins[0].ToString();
                        pinyin += temp.Substring(0, temp.Length - 1);
                    }
                }
                catch (Exception)
                {
                    pinyin += item.ToString();
                }
            }
            pinyin = pinyin.Replace(" ", "_");
            return pinyin;
        }

        public Dictionary<string, string> GetTopic(string name)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            string pinyin = this.PinYin(name);
            string topic = $"{this.model}/{this.ip.Replace(".", "_")}/{pinyin}/";
            dict["state"] = $"{topic}state";
            dict["attributes"] = $"{topic}attributes";
            dict["command"] = $"{topic}command";
            dict["brightness"] = $"{topic}brightness";
            dict["object_id"] = pinyin;
            return dict;
        }

        async public void Connect(Dictionary<string, Action<string>> subscribeList, Action<dynamic> connected)
        {
            // 连接MQTT服务
            string clientId = Guid.NewGuid().ToString();
            var options = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithCleanSession(true)
                .WithWillDelayInterval(5)
                .WithTcpServer(host, port);
            if (!string.IsNullOrEmpty(user))
            {
                options.WithCredentials(user, password);
            };
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();
            mqttClient.UseConnectedHandler((action) =>
            {
                // 连接成功
                foreach(var item in subscribeList)
                {
                    mqttClient.SubscribeAsync(item.Key);
                }
                connected(action);
            });
            mqttClient.UseDisconnectedHandler((action) =>
            {
                // 连接中断了啊
            });
            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                string topic = e.ApplicationMessage.Topic;
                string payload = e.ApplicationMessage.Payload == null ? "" : System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                if (subscribeList.Keys.Contains(topic))
                {
                    subscribeList[topic](payload);
                }
            });
            this.mqttClient = mqttClient;
            await mqttClient.ConnectAsync(options.Build(), CancellationToken.None);
        }

        // 配置
        public void Config(string component, string object_id, Dictionary<string, object> dict)
        {
            string model = this.model;
            string unique_id = $"{model}-{object_id}";
            // 实体唯一ID
            dict.Add("unique_id", unique_id);
            // 设备信息
            dict.Add("device", new
            {
                identifiers = "635147515",
                manufacturer = "shaonianzhentan",
                model,
                name = "我的设备",
                sw_version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()
            });
            string payload = JsonConvert.SerializeObject(dict);
            this.Publish($"homeassistant/{component}/{unique_id}/config", payload);
        }

        // 发布
        public void Publish(string topic, string payload)
        {
            mqttClient.PublishAsync(topic, payload);
        }

        public void PublishJson(string topic, Dictionary<string, string> payload)
        {
            mqttClient.PublishAsync(topic, JsonConvert.SerializeObject(payload));
        }
    }
}