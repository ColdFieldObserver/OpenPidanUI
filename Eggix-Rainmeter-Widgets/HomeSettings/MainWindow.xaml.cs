using Microsoft.Win32;
using RainmeterIntegration;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HomeSettings;

public partial class MainWindow : Window
{
    // --- HomeSettings-specific conventions (intentionally NOT in the generic library) ---
    private const string IniFileName = "home.ini";
    private const string AvatarFileName = "avatar.png";
    private const string PresetsFolderName = "Avatar";
    private const string NicknameSection = "MeterLaunch1Text";
    private const string NicknameKey = "Text";
    private const string NicknameDefault = "Administrator";

    private readonly ISkinContext _skin;
    private readonly IAvatarWriter _avatarWriter;
    private readonly IRainmeterController _rainmeter;

    private string? _pendingAvatarSource;   // local image chosen (absolute path)
    private string? _selectedPreset;        // preset avatar chosen (absolute path)

    public class PresetItem
    {
        public string ImagePath { get; set; } = "";
    }

    public MainWindow()
    {
        InitializeComponent();
        _skin = RainmeterServices.CreateSkinContext(new SkinLayout(IniFileName));
        _avatarWriter = RainmeterServices.CreateAvatarWriter();
        _rainmeter = RainmeterServices.CreateController();
        LoadInitial();
    }

    private string AvatarPath => Path.Combine(_skin.SkinDirectory, AvatarFileName);
    private string PresetsFolder => Path.Combine(_skin.SkinDirectory, PresetsFolderName);

    private void LoadInitial()
    {
        NicknameBox.Text = _skin.Settings.GetValue(NicknameSection, NicknameKey, NicknameDefault);

        if (File.Exists(AvatarPath))
        {
            AvatarPreview.Source = LoadBitmap(AvatarPath);
        }

        var presets = new ObservableCollection<PresetItem>();
        if (Directory.Exists(PresetsFolder))
        {
            foreach (var file in Directory.GetFiles(PresetsFolder, "*.png")
                         .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
            {
                presets.Add(new PresetItem { ImagePath = file });
            }
        }
        PresetList.ItemsSource = presets;

        StatusText.Text = $"皮肤目录：{_skin.SkinDirectory}";
    }

    private void PickLocalButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.webp",
            Title = "选择头像图片"
        };
        if (dlg.ShowDialog() != true)
        {
            return;
        }

        _pendingAvatarSource = dlg.FileName;
        _selectedPreset = null;
        PresetList.SelectedIndex = -1;
        ShowPreview(_pendingAvatarSource);
    }

    private void PresetList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (PresetList.SelectedItem is not PresetItem item)
        {
            return;
        }

        _selectedPreset = item.ImagePath;
        _pendingAvatarSource = null;
        ShowPreview(item.ImagePath);
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (Directory.Exists(_skin.SkinDirectory))
        {
            Process.Start("explorer.exe", _skin.SkinDirectory);
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _skin.Settings.SetValue(NicknameSection, NicknameKey, (NicknameBox.Text ?? "").Trim());

            var avatarSource = _pendingAvatarSource ?? _selectedPreset;
            if (avatarSource is not null)
            {
                // Default to circular crop; fall back to square if circular fails
                // (e.g. an unsupported source format).
                try
                {
                    _avatarWriter.WriteAvatar(avatarSource, AvatarPath, 256, circular: true);
                }
                catch
                {
                    _avatarWriter.WriteAvatar(avatarSource, AvatarPath, 256, circular: false);
                }
            }

            if (_rainmeter.IsAvailable)
            {
                _rainmeter.Refresh(_skin.ConfigName);
                MessageBox.Show("已保存，Rainmeter 皮肤已刷新。", "完成",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("已保存修改到 home.ini，但未检测到 Rainmeter（无法自动刷新）。",
                    "完成", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("保存失败：" + ex.Message, "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Renders the cropped result to memory and shows it as the live preview, so
    /// the user sees the precise avatar that will be saved. The default is a
    /// circular crop; if that fails it falls back to a square crop, then to a
    /// plain file load (e.g. for an unsupported source format).
    /// </summary>
    private void ShowPreview(string sourcePath)
    {
        try
        {
            AvatarPreview.Source = BitmapFromBytes(_avatarWriter.RenderAvatar(sourcePath, 256, circular: true));
            return;
        }
        catch { /* try square fallback */ }

        try
        {
            AvatarPreview.Source = BitmapFromBytes(_avatarWriter.RenderAvatar(sourcePath, 256, circular: false));
            return;
        }
        catch { /* try raw file load */ }

        try { AvatarPreview.Source = LoadBitmap(sourcePath); }
        catch { AvatarPreview.Source = null; }
    }

    private static BitmapImage BitmapFromBytes(byte[] bytes)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
        bmp.StreamSource = new MemoryStream(bytes);
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    /// <summary>
    /// Loads an image so the file handle is released immediately. The default
    /// BitmapImage behaviour (BitmapCacheOption.OnDemand) keeps the source file
    /// open until garbage collection, which would block overwriting avatar.png
    /// on save. With OnLoad + Freeze the file is decoded into memory and closed.
    /// </summary>
    private static BitmapImage LoadBitmap(string path)
    {
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
        bmp.UriSource = new Uri(path);
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }
}
