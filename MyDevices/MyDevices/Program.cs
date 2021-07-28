using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace MyDevices
{
    class Program
    {
        static MqttHA ha = null;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            StartHttpServer();

            // Windows 语音识别
            // KeyboardHelper.win_keypress(KeyboardHelper.vbKeyH);
        }

        static void StartHttpServer()
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

        static void ConnectMQTT()
        {
            if (ha != null && ha.mqttClient != null && ha.mqttClient.IsConnected)
            {
                return;
            }
            string name = "小米平板";
            string batteryName = "小米平板电量";
            string lightSensorName = "小米平板光照传感器";
            string volumeSensorName = "小米平板音量";
            Dictionary<string, string> topic = ha.GetTopic(name);
            Dictionary<string, string> batteryTopic = ha.GetTopic(batteryName);
            Dictionary<string, string> lightSensorTopic = ha.GetTopic(lightSensorName);
            Dictionary<string, string> volumeSensorTopic = ha.GetTopic(volumeSensorName);
            Dictionary<string, Action<string>> action = new Dictionary<string, Action<string>>();
            // 屏幕
            action.Add(topic["command"], (value) =>
            {
                Console.WriteLine(value);
                ha.Publish(topic["state"], value);
            });
            action.Add(topic["brightness"], (value) =>
            {
                Console.WriteLine(value);
            });

            // 系统音量
            action.Add(volumeSensorTopic["command"], (value) =>
            {
                ha.Publish(volumeSensorTopic["state"], value);
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
                }
            });

            ha.Connect(action, (args) =>
            {
                // 屏幕
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add("name", name);
                dict.Add("icon", "mdi:android");
                dict.Add("state_topic", topic["state"]);
                dict.Add("json_attributes_topic", topic["attributes"]);
                dict.Add("brightness_command_topic", topic["brightness"]);
                dict.Add("command_topic", topic["command"]);

                ha.Config("light", topic["object_id"], dict);

                // 电量
                Dictionary<string, object> batteryDict = new Dictionary<string, object>();
                batteryDict.Add("name", batteryName);
                // batteryDict.Add("icon", "mdi:android");
                batteryDict.Add("state_topic", batteryTopic["state"]);
                batteryDict.Add("device_class", "battery");
                batteryDict.Add("unit_of_measurement", "%");

                ha.Config("sensor", batteryTopic["object_id"], batteryDict);

                // 光照传感器
                Dictionary<string, object> lightSensorDict = new Dictionary<string, object>();
                lightSensorDict.Add("name", lightSensorName);
                lightSensorDict.Add("state_topic", lightSensorTopic["state"]);
                lightSensorDict.Add("device_class", "illuminance");
                lightSensorDict.Add("unit_of_measurement", "lx");

                ha.Config("sensor", lightSensorTopic["object_id"], lightSensorDict);

                // 系统音量
                Dictionary<string, object> volumeSensorDict = new Dictionary<string, object>();
                volumeSensorDict.Add("name", volumeSensorName);
                volumeSensorDict.Add("state_topic", volumeSensorTopic["state"]);
                volumeSensorDict.Add("command_topic", volumeSensorTopic["command"]);
                volumeSensorDict.Add("min", "1");
                volumeSensorDict.Add("max", "7");

                ha.Config("number", volumeSensorTopic["object_id"], volumeSensorDict);

            });
        }
    }
}
