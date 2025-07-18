using Avalonia.Controls;
using Avalonia.Interactivity;
using SimpleWebBrowser.ViewModels;

namespace SimpleWebBrowser.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private async void Go_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                await vm.Navigate();
            }
        }

        private async void Home_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                await vm.GoHome();
            }
        }
    }
}