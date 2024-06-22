using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DelayReproTest.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly HttpClient Client = new();
    
    [ObservableProperty]
    private string _triggerUrl = string.Empty;
    
    [ObservableProperty]
    private string _triggerLastResponse = string.Empty;
    
    [ObservableProperty]
    private HttpStatusCode _triggerLastResponseCode;
    
    [ObservableProperty]
    private string _triggerLastResponseTimes = string.Empty;
    
    public async Task Trigger()
    {
        if (string.IsNullOrWhiteSpace(TriggerUrl))
            return;

        try
        {
            var start = Stopwatch.GetTimestamp();
            var request = new HttpRequestMessage(HttpMethod.Get, TriggerUrl);
            //if (HasBody)
            //    request.Content = new StringContent(TriggerBody.Text, Encoding.UTF8, TriggerBodyType);
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