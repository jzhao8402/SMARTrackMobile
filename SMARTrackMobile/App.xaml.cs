// This is the App.xaml.cs file

using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using System;

namespace SMARTrackMobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}