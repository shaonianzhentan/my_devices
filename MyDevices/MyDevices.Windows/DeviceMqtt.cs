using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;
using System.Speech.Synthesis;

namespace MyDevices.Windows
{
    class DeviceMqtt
    {
        MqttHA ha = null;
        public DeviceMqtt()
        {
            StartHttpServer();
        }

        void StartHttpServer()
        {
            HttpServer httpServer = new HttpServer();
            Dictionary<string, Action<string>> action = new Dictionary<string, Action<string>>();
            action["mqtt_host"] = (value) =>
            {
                ha = new MqttHA(value, "1883", "", "", httpServer.ip, "windows");
                ConnectMQTT();
            };
            action["home_url"] = (value) =>
            {

            };
            action["ha_api"] = (value) =>
            {

            };
            httpServer.Listenner(action);
            httpServer.Broadcast();
        }

        void ConnectMQTT()
        {
            if (ha != null && ha.mqttClient != null && ha.mqttClient.IsConnected)
            {
                return;
            }
            string name = "我的电脑";
            Dictionary<string, string> topic = ha.GetTopic(name);
            Dictionary<string, Action<string>> action = new Dictionary<string, Action<string>>();
            // 屏幕
            action.Add(topic["command"], (value) =>
            {
                Console.WriteLine(value);
                ha.Publish(topic["state"], value);
                switch (value)
                {
                    case "关机":

                        break;
                    case "重启":

                        break;
                    case "打开爱奇艺":

                        break;
                }
            });

            // 相关功能
            action.Add(ha.ip, (value) =>
            {
                var yamlReader = new System.IO.StringReader(value);
                Deserializer yamlDeserializer = new Deserializer();
                Dictionary<string, object> dict = yamlDeserializer.Deserialize<Dictionary<string, object>>(yamlReader);
                // 设置TTS
                if (dict.ContainsKey("tts"))
                {
                    string tts = dict["tts"].ToString();
                    using (SpeechSynthesizer reader = new SpeechSynthesizer())
                    {
                        reader.Speak(tts);
                        reader.Dispose();
                    }
                }
            });


            ha.Connect(action, (args) =>
            {
                // 屏幕
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("name", name);
                dict.Add("icon", "mdi:desktop-mac-dashboard");
                dict.Add("state_topic", topic["state"]);
                dict.Add("json_attributes_topic", topic["attributes"]);
                dict.Add("command_topic", topic["command"]);
                dict.Add("options", new string[] { "关机", "重启" });

                ha.Config("select", topic["object_id"], dict);
            });
        }
    }
}
