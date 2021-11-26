using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Net;
using System.Net.Http;
using Newtonsoft;
using Newtonsoft.Json;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace TestApp
{
    public class PropertiesJson
    {
        public string updated_at { get; set; }
        public string app_id { get; set; }
        public string screen_width { get; set; }
        public string screen_height { get; set; }
        public string payment_model { get; set; }
        public string platform { get; set; }
        public string[] installed_games { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        private static readonly HttpClient client = new HttpClient();

        public MainPage()
        {
            InitializeComponent();
            On<iOS>().SetUseSafeArea(true);

            var a = new PropertiesJson
            {
                updated_at = "",
                app_id = "com.dominigames.fe2",
                screen_width = "1792",
                screen_height = "828",
                payment_model = "freemium",
                platform = "android",
                installed_games = new string[] { }
            };

            string jsonString = @"{
            ""game_id"":""cc12"",
            ""platform_version"":""ios/android/amazon"",
            ""game_version"":""freemium/free2play""
            }";
            
            //string jsonString = JsonConvert.SerializeObject(a,Formatting.Indented);

            Entry1.Text = "192.168.4.140:8000";
            Entry2.Text = jsonString;
        }

        private async void BtnSend_Clicked(object sender, EventArgs e)
        {
            HttpContent content = new StringContent(Entry2.Text);

            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://moregamesdmg.com/public/index.php");
            //request.RequestUri = new Uri("https://" + Entry1.Text + "/external/get-slider");
            request.Method = HttpMethod.Post;
            request.Content = content;

            try
            {
                HttpResponseMessage response = await client.SendAsync(request);
                string result = await response.Content.ReadAsStringAsync();

                var htmlSource = new HtmlWebViewSource();
                htmlSource.Html = result;
                WebView1.Source = htmlSource;
            }
            catch (Exception ex)
            {
                await DisplayAlert("", ex.Message, "X");
            }
        }
    }
}
