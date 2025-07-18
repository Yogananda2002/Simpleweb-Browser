using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;

namespace SimpleWebBrowser.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _address = "https://www.hw.ac.uk";
        private string _content = "";
        private string _status = "Ready";
        private string _statusCode = "";
        private string _pageTitle = "";
        private readonly HttpClient _httpClient = new();

        public MainWindowViewModel()
        {
            ReloadCommand = new RelayCommand(async () => await ReloadPage());
            LoadFavorites();
            LoadHistory();
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public string StatusCode
        {
            get => _statusCode;
            set
            {
                _statusCode = value;
                OnPropertyChanged(nameof(StatusCode));
            }
        }

        public string PageTitle
        {
            get => _pageTitle;
            set
            {
                _pageTitle = value;
                OnPropertyChanged(nameof(PageTitle));
            }
        }

        public ICommand ReloadCommand { get; }
        public ObservableCollection<FavoriteItem> Favorites { get; } = new();
        public ObservableCollection<HistoryItem> History { get; } = new();

        public async Task Navigate()
        {
            try
            {
                Status = "Loading...";
                var response = await _httpClient.GetAsync(Address);
                
                if (response == null)
                {
                    StatusCode = "Status: No Response";
                    Status = "Error: No response from server";
                    return;
                }

                var html = await response.Content.ReadAsStringAsync();
                Content = html;
                Status = "Ready";
                StatusCode = $"Status: {(int)response.StatusCode} {response.StatusCode}";
                PageTitle = $"Title: {ExtractTitle(html)}";

                History.Add(new HistoryItem
                {
                    Url = Address,
                    Title = ExtractTitle(html),
                    Visited = DateTime.Now
                });
                SaveHistory();
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
                StatusCode = "Status: Error";
                PageTitle = "Title: N/A";
                Content = $"Error loading page: {ex.Message}";
            }
        }

        private async Task ReloadPage()
        {
            if (!string.IsNullOrEmpty(Address))
            {
                await Navigate();
            }
        }

        public async Task GoHome()
        {
            Address = "https://www.hw.ac.uk";
            await Navigate();
        }

        private string ExtractTitle(string html)
        {
            var match = Regex.Match(html, @"<title>\s*(.+?)\s*</title>");
            return match.Success ? match.Groups[1].Value : "No title";
        }

        private void LoadFavorites()
        {
            var path = GetStoragePath("favorites.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var items = JsonConvert.DeserializeObject<FavoriteItem[]>(json);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        Favorites.Add(item);
                    }
                }
            }
        }

        private void LoadHistory()
        {
            var path = GetStoragePath("history.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var items = JsonConvert.DeserializeObject<HistoryItem[]>(json);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        History.Add(item);
                    }
                }
            }
        }

        private void SaveHistory()
        {
            var path = GetStoragePath("history.json");
            var json = JsonConvert.SerializeObject(History, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        private string GetStoragePath(string filename)
        {
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SimpleWebBrowser");
            Directory.CreateDirectory(appData);
            return Path.Combine(appData, filename);
        }
    }

    public class FavoriteItem
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
    }

    public class HistoryItem
    {
        public string? Url { get; set; }
        public string? Title { get; set; }
        public DateTime Visited { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;
        private EventHandler? _canExecuteChanged;

        public event EventHandler? CanExecuteChanged
        {
            add => _canExecuteChanged += value;
            remove => _canExecuteChanged -= value;
        }

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged() => _canExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}