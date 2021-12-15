using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using HtmlAgilityPack;
using System.IO;
using System.Text;
using System.Web;
using System.Net;
using System.Threading.Tasks;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

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
        public int launch_count { get; set; }
        public string device { get; set; }

        private readonly string _fileName;

        public PropertiesJson()
        {
            updated_at = "";
            app_id = "com.dominigames.fe2";
            screen_width = "1792";
            screen_height = "828";
            payment_model = "freemium";
            platform = "android";
            installed_games = new string[] { };
            launch_count = 0;
            device = "tablet";

            _fileName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/sav.json";
        }

        public void SaveToFile()
        {
            string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(_fileName, jsonString);
        }

        public void LoadFromFile()
        {
            if (!File.Exists(_fileName))
                return;

            string jsonString = File.ReadAllText(_fileName);

            if (jsonString.Length == 0)
                return;

            PropertiesJson temp = JsonConvert.DeserializeObject<PropertiesJson>(jsonString);

            updated_at = temp.updated_at;
            app_id = temp.app_id;
            screen_width = temp.screen_width;
            screen_height = temp.screen_height;
            payment_model = temp.updated_at;
            platform = temp.updated_at;
            installed_games = temp.installed_games;
            launch_count = temp.launch_count;
            device = temp.device;
        }
    }

    public partial class MainPage : ContentPage
    {

        private static readonly HttpClient client = new();

        public MainPage()
        {
            InitializeComponent();

            On<iOS>().SetUseSafeArea(true);

            HasPermission();

            var a = new PropertiesJson();
            a.LoadFromFile();
            //a.launch_count = ++a.launch_count;

            if (Device.Idiom == TargetIdiom.Phone)
            { a.device = "mobile"; }
            else
            { a.device = "tablet"; }
            
            a.SaveToFile();

            string jsonString = JsonConvert.SerializeObject(a, Formatting.Indented);

            Entry1.Text = "192.168.4.140:8000";
            Entry2.Text = jsonString;

        }

        private async void BtnSend_Clicked(object sender, EventArgs e)
        {
            string inputJsonString = Entry2.Text;
            var inputJson = JsonConvert.DeserializeObject<PropertiesJson>(inputJsonString);

            inputJson.platform = Picker1.SelectedItem.ToString();
            inputJson.payment_model = Picker2.SelectedItem.ToString();

            string encoded = WebUtility.UrlEncode(JsonConvert.SerializeObject(inputJson));

            inputJson.SaveToFile();

            // Test save/load
            //PropertiesJson load = new();
            //load.LoadFromFile();
            //await DisplayAlert("Loaded", JsonConvert.SerializeObject(load), "X");

            //HttpContent content = new StringContent(encoded, Encoding.UTF8, "application/json");
            HttpRequestMessage request = new()
            {
                RequestUri = new Uri("http://" + Entry1.Text + "/external/get-slider?json=" + encoded),
                Method = HttpMethod.Get,
                //Content = content,
            };

            try
            {
                HttpResponseMessage response = await client.SendAsync(request);
                string result = await response.Content.ReadAsStringAsync();

                HtmlDocument document = new();
                document.LoadHtml(result);
                var container = document.DocumentNode.Descendants("meta").FirstOrDefault(x => x.Attributes.Contains("name") && x.Attributes["name"].Value == "updated_at");
                if (container != null)
                {
                    inputJson.updated_at = container.Attributes["content"].Value;
                    inputJson.SaveToFile();
                }

                var htmlSource = new HtmlWebViewSource
                {
                    Html = result
                };

                WebView1.Source = htmlSource;
                Popup();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Exception", ex.Message, "X");
            }
        }

        private void Popup()
        {
            if (!this.popuplayout.IsVisible)
            {
                this.popuplayout.IsVisible = !this.popuplayout.IsVisible;
                this.popuplayout.Scale = 1;
                btnClose.IsVisible = true;
            }
            else
            {
                this.popuplayout.IsVisible = !this.popuplayout.IsVisible;
                this.popuplayout.Scale = 0;
                btnClose.IsVisible = false;
            }
        }

        private void BtnClose_Clicked(object sender, EventArgs e)
        {
            Popup();
        }

        private async void HasPermission()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location))
                    {
                        await DisplayAlert("Need storage", "Gunna need that storage", "OK");
                    }

                    status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
                }

                if (status == PermissionStatus.Granted)
                {

                }
                else if (status != PermissionStatus.Unknown)
                {

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Exception", ex.Message, "X");
            }
        }
    }
}
