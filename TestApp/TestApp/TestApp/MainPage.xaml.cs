﻿using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using System.Net.Http;
using Newtonsoft.Json;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using HtmlAgilityPack;
using System.IO;


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

        private readonly string _fileName = "sav.json";

        public PropertiesJson()
        {
            updated_at = "";
            app_id = "com.dominigames.fe2";
            screen_width = "1792";
            screen_height = "828";
            payment_model = "freemium";
            platform = "android";
            installed_games = new string[] { };
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
        }
    }

    public partial class MainPage : ContentPage
    {
        private static readonly HttpClient client = new();

        public MainPage()
        {
            InitializeComponent();
            On<iOS>().SetUseSafeArea(true);

            var a = new PropertiesJson();
            a.LoadFromFile();

            string jsonString = JsonConvert.SerializeObject(a, Formatting.Indented);

            //jsonString = @"{
            //""game_id"":""cc12"",
            //""platform_version"":""ios/android/amazon"",
            //""game_version"":""freemium/free2play""
            //}";

            Entry1.Text = "192.168.4.140:8000";
            Entry2.Text = jsonString;
        }

        private async void BtnSend_Clicked(object sender, EventArgs e)
        {
            string inputJsonString = Entry2.Text;
            var inputJson = JsonConvert.DeserializeObject<PropertiesJson>(inputJsonString);

            //Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(inputJsonString);

            //FormUrlEncodedContent form = new FormUrlEncodedContent(dict);

            HttpContent content = new StringContent(inputJsonString);
            HttpRequestMessage request = new()
            {
                //RequestUri = new Uri("https://moregamesdmg.com/public/index.php"),
                RequestUri = new Uri("http://" + Entry1.Text + "/external/get-slider"),
                Method = HttpMethod.Get,
                Content = content,
            };

            string ur = request.ToString();

            try
            {
                HttpResponseMessage response = await client.GetAsync(request.ToString());
                string result = await response.Content.ReadAsStringAsync();

                HtmlDocument document = new();
                document.LoadHtml(result);
                var container = document.DocumentNode.Descendants("meta").FirstOrDefault(x => x.Attributes.Contains("name") && x.Attributes["name"].Value == "updated_at");
                if (container != null)
                {
                    inputJson.updated_at = container.Attributes["content"].Value;
                    inputJson.SaveToFile();

                    //await DisplayAlert("", container.Attributes["content"].Value, "X");
                }

                var htmlSource = new HtmlWebViewSource
                {
                    Html = result
                };

                WebView1.Source = htmlSource;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Exception", ex.Message, "X");
            }
        }
    }
}
