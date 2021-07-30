using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Provider;
using YamlDotNet.Serialization;
using Xamarin.Essentials;
using Android.Media;
using Android.Webkit;

namespace MyDevices.Androids
{
    class DeviceMqtt
    {
        MqttHA ha = null;
        MainActivity activity = null;
        AudioManager audioManager;

        Dictionary<string, string> topic = null;
        Dictionary<string, string> batteryTopic = null;
        Dictionary<string, string> lightSensorTopic = null;
        Dictionary<string, string> volumeSensorTopic = null;
        WebView webView = null;

        public float LightSensorValue = 0;

        public DeviceMqtt(MainActivity activity, WebView webView)
        {
            this.activity = activity;
            this.webView = webView;
            audioManager = activity.GetSystemService(Context.AudioService) as AudioManager;

            StartHttpServer();
        }

        void StartHttpServer()
        {
            HttpServer httpServer = new HttpServer();
            Dictionary<string, Action<string>> action = new Dictionary<string, Action<string>>();
            action["mqtt_host"] = (value) =>
            {
                ha = new MqttHA(value, "1883", "", "", httpServer.ip, "android");
                ConnectMQTT();
            };
            action["home_url"] = (value) =>
            {
                this.webView.LoadUrl(value);
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
            string name = "小米平板";
            string batteryName = "小米平板电量";
            string lightSensorName = "小米平板光照传感器";
            string volumeSensorName = "小米平板音量";
            topic = ha.GetTopic(name);
            batteryTopic = ha.GetTopic(batteryName);
            lightSensorTopic = ha.GetTopic(lightSensorName);
            volumeSensorTopic = ha.GetTopic(volumeSensorName);
            Dictionary<string, Action<string>> action = new Dictionary<string, Action<string>>();
            // 屏幕
            action.Add(topic["command"], (value) =>
            {
                Console.WriteLine(value);
                ha.Publish(topic["state"], value);
                if (value == "OFF")
                {
                    Settings.System.PutInt(activity.ContentResolver, Settings.System.ScreenBrightness, 1);
                    this.PublishInfo();
                }
            });
            action.Add(topic["brightness"], (value) =>
            {
                Settings.System.PutInt(activity.ContentResolver, Settings.System.ScreenBrightness, int.Parse(value));
                this.PublishInfo();
            });

            // 系统音量
            action.Add(volumeSensorTopic["command"], (value) =>
            {
                ha.Publish(volumeSensorTopic["state"], value);
                audioManager.SetStreamVolume(Stream.System, int.Parse(value), VolumeNotificationFlags.PlaySound);
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
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Xamarin.Essentials.TextToSpeech.SpeakAsync(dict["tts"].ToString());
                    });
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

        /// <summary>
        /// 发送光照传感器
        /// </summary>
        /// <param name="value"></param>
        public void PublishLightSensor(string value)
        {
            if (lightSensorTopic != null)
            {
                ha.Publish(lightSensorTopic["state"], value);
            }
        }
    
        /// <summary>
        /// 发送平板信息
        /// </summary>
        void PublishInfo()
        {
            if (topic != null)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("brightness", Settings.System.GetInt(activity.ContentResolver, Settings.System.ScreenBrightness).ToString());
                ha.PublishJson(topic["brightness"], dict);
            }
        }
    }
}