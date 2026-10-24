using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UseHelp;

public partial class MainWindow : Window
{
    private const string ProgramVersion = "1.2";
    private const string OnlineHelpUrl = "https://eggyuidocs.wordpress.com/";

    public MainWindow()
    {
        InitializeComponent();
        InfoLabel.Text = $"程序版本：{ProgramVersion}\n运行时版本：{RuntimeInformation.FrameworkDescription}";
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadHelpFromXml();
        }
        catch (Exception ex)
        {
            // Loading failed: offer the online help, then close.
            if (MessageBox.Show($"加载帮助内容失败：{ex.Message}\n是否要打开联机帮助？", "提示",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                VisitOnlineHelp();
            }
            Close();
        }
    }

    /// <summary>
    /// Loads HelpContent.xml and renders each category as an expander whose
    /// content lists the item answers, preserving the original FAQ structure.
    /// </summary>
    private void LoadHelpFromXml()
    {
        string baseDir = AppContext.BaseDirectory;
        string[] candidates =
        {
            Path.Combine(baseDir, "HelpContent.xml"),
            Path.Combine(baseDir, "Resources", "HelpContent.xml"),
            Path.Combine(Directory.GetCurrentDirectory(), "HelpContent.xml")
        };

        string? filePath = candidates.FirstOrDefault(File.Exists);
        if (filePath is null)
        {
            return; // No help file found; do not block the app.
        }

        var doc = XDocument.Load(filePath);
        var root = doc.Root;
        if (root is null)
        {
            return;
        }

        foreach (var category in root.Elements("Category"))
        {
            string catText = category.Attribute("Text")?.Value ?? "未命名分类";

            var expander = new Expander
            {
                Header = catText,
                Margin = new Thickness(0, 0, 0, 8),
                IsExpanded = false
            };

            var panel = new StackPanel();
            foreach (var item in category.Elements("Item"))
            {
                string itemText = item.Attribute("Text")?.Value ?? item.Value ?? string.Empty;
                panel.Children.Add(new TextBlock
                {
                    Text = itemText,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(4, 4, 4, 4),
                    FontSize = 13
                });
            }

            expander.Content = panel;
            FaqPanel.Children.Add(expander);
        }
    }

    private void OnlineHelpButton_Click(object sender, RoutedEventArgs e) => VisitOnlineHelp();

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private void VisitOnlineHelp()
    {
        try
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = OnlineHelpUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"联机帮助访问失败：{ex.Message}", "提示",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
