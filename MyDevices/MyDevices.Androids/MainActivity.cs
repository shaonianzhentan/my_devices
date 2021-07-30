using Android.App;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Webkit;
using AndroidX.AppCompat.App;

namespace MyDevices.Androids
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        DeviceMqtt mq = null;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);


            WebView webView = this.FindViewById<WebView>(Resource.Id.wv);
            webView.Settings.AllowFileAccess = true;
            webView.Settings.AllowContentAccess = true;
            webView.Settings.AllowFileAccessFromFileURLs = true;
            webView.Settings.AllowUniversalAccessFromFileURLs = true;
            webView.Settings.JavaScriptEnabled = true;
            webView.Settings.SetRenderPriority(Android.Webkit.WebSettings.RenderPriority.High);
            webView.ScrollbarFadingEnabled = true;
            webView.SetWebViewClient(new PodWebViewClient());

            mq = new DeviceMqtt(this, webView);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        #region 传感器
        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {

        }

        public void OnSensorChanged(SensorEvent e)
        {
            // 光照传感器
            if (e.Sensor.Name.Contains("Light Sensor"))
            {
                mq.PublishLightSensor(e.Values[0].ToString());
            }
        }
        #endregion

        public class PodWebViewClient : WebViewClient

        {
            [System.Obsolete]
            public override bool ShouldOverrideUrlLoading(WebView view, string url)

            {
                view.LoadUrl(url);
                return true;
            }
        }
    }
}