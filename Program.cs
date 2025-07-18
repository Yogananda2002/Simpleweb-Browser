using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SimpleWebBrowser.ViewModels;
using SimpleWebBrowser.Views;

namespace SimpleWebBrowser;

public class Program
{
    public static void Main(string[] args) 
        => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}