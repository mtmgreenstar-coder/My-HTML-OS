using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace HtmlOsWindows;

public sealed class MainForm : Form
{
    private readonly WebView2 browser = new();
    private readonly string appData = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "HTML OS", "WebView2");
    private bool fullScreen;
    private FormBorderStyle previousBorder;
    private FormWindowState previousWindowState;

    public MainForm()
    {
        Text = "HTML OS";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 620);
        Size = new Size(1280, 820);
        BackColor = Color.FromArgb(8, 11, 20);
        KeyPreview = true;

        string iconPath = Path.Combine(AppContext.BaseDirectory, "assets", "html_os_icon.ico");
        if (File.Exists(iconPath))
        {
            try { Icon = new Icon(iconPath); } catch { }
        }

        browser.Dock = DockStyle.Fill;
        Controls.Add(browser);
        Shown += async (_, _) => await InitializeBrowserAsync();
        KeyDown += HandleShortcuts;
    }

    private async Task InitializeBrowserAsync()
    {
        try
        {
            Directory.CreateDirectory(appData);
            string assetsFolder = Path.Combine(AppContext.BaseDirectory, "assets");
            string htmlFile = Path.Combine(assetsFolder, "index.html");

            if (!File.Exists(htmlFile))
            {
                ShowStartupError("فایل اصلی HTML OS پیدا نشد. لطفاً تمام فایل‌های کنار برنامه را در یک پوشه نگه دارید.");
                return;
            }

            var environment = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: null,
                userDataFolder: appData);

            await browser.EnsureCoreWebView2Async(environment);
            browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
            browser.CoreWebView2.Settings.IsStatusBarEnabled = false;
            browser.CoreWebView2.Settings.IsZoomControlEnabled = true;

            browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "htmlos.local", assetsFolder, CoreWebView2HostResourceAccessKind.Allow);

            browser.CoreWebView2.DownloadStarting += HandleDownloadStarting;
            browser.CoreWebView2.NewWindowRequested += (_, e) =>
            {
                e.Handled = true;
                if (Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == "https" || uri.Scheme == "http"))
                {
                    try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true }); }
                    catch { }
                }
            };

            browser.Source = new Uri("https://htmlos.local/index.html");
        }
        catch (Exception ex)
        {
            ShowStartupError(
                "اجرای موتور نمایش ویندوز ممکن نشد. ممکن است Microsoft Edge WebView2 Runtime روی ویندوز نصب نباشد.\n\nجزئیات: " +
                ex.Message);
        }
    }

    private void HandleDownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            FileName = Path.GetFileName(e.ResultFilePath),
            Title = "ذخیره فایل",
            OverwritePrompt = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            e.ResultFilePath = dialog.FileName;
            e.Handled = true;
        }
        else
        {
            e.Cancel = true;
        }
    }

    private void HandleShortcuts(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F11)
        {
            ToggleFullScreen();
            e.Handled = true;
        }
        else if (e.Control && e.KeyCode == Keys.R)
        {
            browser.Reload();
            e.Handled = true;
        }
        else if (e.Alt && e.KeyCode == Keys.F4)
        {
            Close();
        }
    }

    private void ToggleFullScreen()
    {
        if (!fullScreen)
        {
            previousBorder = FormBorderStyle;
            previousWindowState = WindowState;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            fullScreen = true;
        }
        else
        {
            FormBorderStyle = previousBorder;
            WindowState = previousWindowState;
            fullScreen = false;
        }
    }

    private void ShowStartupError(string message)
    {
        MessageBox.Show(this, message, "HTML OS", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
