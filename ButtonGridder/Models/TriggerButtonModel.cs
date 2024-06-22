using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using ButtonGridder.Views;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ButtonGridder.Models;

public partial class TriggerButtonModel : ObservableObject
{
    private static readonly HttpClient Client = new();

    #region Postion / Basics

    [ObservableProperty] private int _gridColumn;

    [ObservableProperty] private int _gridRow;

    [ObservableProperty] private int _gridColumnSpan = 1;

    [ObservableProperty] private int _gridRowSpan = 1;

    [ObservableProperty] private string _title = "A Button";

    #endregion

    #region Colors / Font

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(BackgroundColorBrush))]
    private Color _backgroundPickerColor = Color.Parse("#333");

    public SolidColorBrush BackgroundColorBrush => new(BackgroundPickerColor);

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(TitleColorBrush))]
    private Color _titleColor;

    public SolidColorBrush TitleColorBrush => new(TitleColor);

    [ObservableProperty] private int _titleFontSize = 16;

    [ObservableProperty] private FontFamily _titleFontFamily = FontManager.Current.DefaultFontFamily;
    public List<FontFamily> Fonts => [..FontManager.Current.SystemFonts.OrderBy(x => x.Name)];

    #endregion

    #region Request

    [ObservableProperty] private string _triggerUrl = string.Empty;

    [ObservableProperty] private HttpMethod _triggerHttpMethod = HttpMethod.Get;

    [ObservableProperty] private bool _hasBody;

    [ObservableProperty] private TextDocument _triggerBody = new();

    [ObservableProperty] private string _triggerBodyType = "application/json";

    #endregion

    #region Debug

    [ObservableProperty] private HttpStatusCode _triggerLastResponseCode = 0;

    [ObservableProperty] private string _triggerLastResponse = string.Empty;

    [ObservableProperty] private string _triggerLastResponseTimes = string.Empty;

    #endregion

    [ObservableProperty] private bool _isEditing;

    private readonly ObservableCollection<TriggerButtonModel> _parentCollection;
    private readonly Grid _parentGrid;

    public int MaxGridColumn
    {
        get
        {
            if (_parentGrid.ColumnDefinitions.Count == 0)
                return 1;
            return _parentGrid.ColumnDefinitions.Count - GridColumnSpan + 1;
        }
    }

    public int MaxGridRow
    {
        get
        {
            if (_parentGrid.RowDefinitions.Count == 0)
                return 1;
            return _parentGrid.RowDefinitions.Count - GridRowSpan + 1;
        }
    }

    public List<HttpMethod> HttpMethods =>
    [
        HttpMethod.Get,
        HttpMethod.Post,
        HttpMethod.Put,
        HttpMethod.Delete,
        HttpMethod.Head,
        HttpMethod.Options,
        HttpMethod.Patch,
        HttpMethod.Trace,
        HttpMethod.Connect
    ];

    public TriggerButtonModel(ObservableCollection<TriggerButtonModel> parentCollection, Grid parentGrid)
    {
        _parentCollection = parentCollection;
        _parentGrid = parentGrid;
        if (Application.Current?.ActualThemeVariant == ThemeVariant.Light)
        {
            BackgroundPickerColor = Color.Parse("#33000000");
            TitleColor = Colors.Black;
        }
        else
        {
            BackgroundPickerColor = Color.Parse("#33FFFFFF");
            TitleColor = Colors.White;
        }
    }

    public async Task Edit(Window parentWindow)
    {
        var editWindow = new AddEditTrigger
        {
            DataContext = this
        };
        await editWindow.ShowDialog(parentWindow);
    }

    public void Delete()
    {
        _parentCollection.Remove(this);
    }

    public async Task Trigger()
    {
        if (string.IsNullOrWhiteSpace(TriggerUrl))
            return;

        try
        {
            var start = Stopwatch.GetTimestamp();
            var request = new HttpRequestMessage(TriggerHttpMethod, TriggerUrl);
            if (HasBody)
                request.Content = new StringContent(TriggerBody.Text, Encoding.UTF8, TriggerBodyType);
            var endRequestBuild = Stopwatch.GetTimestamp();
            var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var endRequest = Stopwatch.GetTimestamp();
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var endRead = Stopwatch.GetTimestamp();
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                TriggerLastResponseCode = response.StatusCode;
                TriggerLastResponse = content;
                var buildTime = TimeSpan.FromTicks(endRequestBuild - start).Milliseconds;
                var requestTime = TimeSpan.FromTicks(endRequest - endRequestBuild).Milliseconds;
                var totalTime = TimeSpan.FromTicks(endRequest - start).Milliseconds;
                var responseTime = TimeSpan.FromTicks(endRead - endRequest).Milliseconds;
                TriggerLastResponseTimes =
                    $"Build: {buildTime}ms, Request: {requestTime}ms, Read: {responseTime}ms, Total: {totalTime}ms";
            });
        }
        catch (Exception e)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                TriggerLastResponseCode = 0;
                TriggerLastResponse = e.Message;
            });
        }
    }
}