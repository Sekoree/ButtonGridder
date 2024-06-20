using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
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
    public string TitleColor { get; set; } = string.Empty;
    public int TitleFontSize { get; set; } = 16;
    public string TitleFontFamily { get; set; } = string.Empty;
    public required string TriggerUrl { get; set; }
    public required string TriggerHttpMethod { get; set; }
    public bool HasBody { get; set; } = false;
    public string TriggerBody { get; set; } = string.Empty;
    public string TriggerBodyType { get; set; } = string.Empty;
    
    public static TriggerButton ToSerializable(TriggerButtonModel model)
    {
        var bgColorName = model.BackgroundColorBrush.Color.ToString();
        var titleColorName = model.TitleColorBrush.Color.ToString();
        var fontFamilyName = model.TitleFontFamily.Name;
        var method = model.TriggerHttpMethod.Method;
        return new TriggerButton
        {
            GridColumn = model.GridColumn,
            GridRow = model.GridRow,
            GridColumnSpan = model.GridColumnSpan,
            GridRowSpan = model.GridRowSpan,
            Title = model.Title,
            BackgroundColor = bgColorName,
            TitleColor = titleColorName,
            TitleFontSize = model.TitleFontSize,
            TitleFontFamily = fontFamilyName,
            TriggerUrl = model.TriggerUrl,
            TriggerHttpMethod = method,
            HasBody = model.HasBody,
            TriggerBody = model.TriggerBody.Text,
            TriggerBodyType = model.TriggerBodyType
        };
    }
    
    public static TriggerButtonModel ToModel(TriggerButton button, ObservableCollection<TriggerButtonModel> parent, Grid parentGrid)
    {
        var bgColor = Color.Parse(button.BackgroundColor);
        if (string.IsNullOrWhiteSpace(button.TitleColor)) 
            button.TitleColor = Application.Current?.ActualThemeVariant == ThemeVariant.Light ? "#000000" : "#FFFFFF";
        var titleColor = Color.Parse(button.TitleColor);
        var fontFamily = FontManager.Current.SystemFonts.FirstOrDefault(f => f.Name == button.TitleFontFamily) ??
                         FontManager.Current.DefaultFontFamily;
        var method = HttpMethod.Parse(button.TriggerHttpMethod);
        return new TriggerButtonModel(parent, parentGrid)
        {
            GridColumn = button.GridColumn,
            GridRow = button.GridRow,
            GridColumnSpan = button.GridColumnSpan,
            GridRowSpan = button.GridRowSpan,
            Title = button.Title,
            BackgroundPickerColor = bgColor,
            TitleColor = titleColor,
            TitleFontSize = button.TitleFontSize,
            TitleFontFamily = fontFamily,
            TriggerUrl = button.TriggerUrl,
            TriggerHttpMethod = method,
            HasBody = button.HasBody,
            TriggerBody = new TextDocument(button.TriggerBody),
            TriggerBodyType = button.TriggerBodyType
        };
    }
}