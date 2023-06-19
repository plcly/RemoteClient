using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using RemoteClient.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace RemoteClientBlazor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            var jsonPath = "AppSettings.json";
            if (!File.Exists(jsonPath))
            {
                throw new Exception("AppSettings.json missing!");
            }
            string json = File.ReadAllText(jsonPath) ?? string.Empty;
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip
            };
            AppSettings settings = JsonSerializer.Deserialize<AppSettings>(json, options);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddMudServices();
            serviceCollection.AddSingleton<AppSettings>(settings);
            var provider = serviceCollection.BuildServiceProvider();
            Resources.Add("services", provider);
            Ioc.Default.ConfigureServices(provider);


            base.OnStartup(e);
        }
    }
}
