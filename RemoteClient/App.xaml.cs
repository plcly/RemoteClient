using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
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

namespace RemoteClient
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
            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                .AddTransient<MainWindowViewModel>()
                .AddTransient<AddOrUpdateServerViewModel>()
                .AddSingleton<AppSettings>(settings)
                .BuildServiceProvider());

            base.OnStartup(e);
        }
    }
}
