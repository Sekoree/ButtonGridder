using System.Collections.ObjectModel;
using System.Net.Http;
using Avalonia.Controls;
using Avalonia.Media;
using AvaloniaEdit.Document;
using ButtonGridder.Models;

namespace ButtonGridder.Entities;

//JSON serializable class for a button that triggers an HTTP request
public class TriggerButton
{
    public int GridColumn { get; set; }
    public int GridRow { get; set; }
    public int GridColumnSpan { get; set; }
    public int GridRowSpan { get; set; }
    public required string Title { get; set; }
    public required string BackgroundColor { get; set; }
    public required string TriggerUrl { get; set; }
    public required string TriggerHttpMethod { get; set; }
    public string TriggerBody { get; set; } = string.Empty;
    public string TriggerBodyType { get; set; } = string.Empty;
    
    public static TriggerButton ToSerializable(TriggerButtonModel model)
    {
        var colorName = model.BackgroundColorBrush.Color.ToString();
        var method = model.TriggerHttpMethod.Method;
        return new TriggerButton
        {
            GridColumn = model.GridColumn,
            GridRow = model.GridRow,
            GridColumnSpan = model.GridColumnSpan,
            GridRowSpan = model.GridRowSpan,
            Title = model.Title,
            BackgroundColor = colorName,
            TriggerUrl = model.TriggerUrl,
            TriggerHttpMethod = method,
            TriggerBody = model.TriggerBody.Text,
            TriggerBodyType = model.TriggerBodyType
        };
    }
    
    public static TriggerButtonModel ToModel(TriggerButton button, ObservableCollection<TriggerButtonModel> parent, Grid parentGrid, SolidColorBrush parentBackgroundBrush, SolidColorBrush parentTitleBrush)
    {
        var color = Color.Parse(button.BackgroundColor);
        var method = HttpMethod.Parse(button.TriggerHttpMethod);
        return new TriggerButtonModel(parent, parentGrid, parentBackgroundBrush, parentTitleBrush)
        {
            GridColumn = button.GridColumn,
            GridRow = button.GridRow,
            GridColumnSpan = button.GridColumnSpan,
            GridRowSpan = button.GridRowSpan,
            Title = button.Title,
            BackgroundPickerColor = color,
            TriggerUrl = button.TriggerUrl,
            TriggerHttpMethod = method,
            TriggerBody = new TextDocument(button.TriggerBody),
            TriggerBodyType = button.TriggerBodyType
        };
    }
}