using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using ButtonGridder.Entities;
using ButtonGridder.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ButtonGridder.ViewModels;

public partial class ButtonGridViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title;
    
    public ObservableCollection<TriggerButtonModel> Buttons { get; } = new();

    public ItemsControl ButtonControl { get; set; } = new();
    public Grid ButtonGrid { get; set; } = new();

    [ObservableProperty] private bool _isEditing = false;


    [ObservableProperty] private int _gridColumns = 10;
    [ObservableProperty] private int _gridRows = 6;
    
    [ObservableProperty] private string _baseUrl = string.Empty;
    
    private readonly ObservableCollection<ButtonGridViewModel> _parentCollection;

    public int MinGridColumns
    {
        get
        {
            if (Buttons.Count == 0)
                return 1;
            var minColumns = Buttons.Max(b => b.GridColumn + b.GridColumnSpan);
            return minColumns;
        }
    }
    public int MinGridRows
    {
        get
        {
            if (Buttons.Count == 0)
                return 1;
            var minRows = Buttons.Max(b => b.GridRow + b.GridRowSpan);
            return minRows;
        }
    }

    public ButtonGridViewModel(ObservableCollection<ButtonGridViewModel> parent, string title = "Untitled")
    {
        _parentCollection = parent;
        Title = title;
        var path = new CompiledBindingPathBuilder();
        path.SetRawSource(Buttons);
        var propInfo = 
            new ClrPropertyInfo(
                "Buttons", 
                o => Buttons, 
                null, 
                typeof(ObservableCollection<TriggerButtonModel>));
        path.Property(propInfo, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor);
        var binding = new CompiledBindingExtension(path.Build());
        ButtonControl.Bind(ItemsControl.ItemsSourceProperty, binding);
        ButtonControl.ItemsPanel = new FuncTemplate<Panel?>(() =>
        {
            ButtonControl.Loaded += (_, _) =>
            {
                ButtonGrid.ColumnDefinitions.Clear();
                ButtonGrid.RowDefinitions.Clear();
                for (var i = 0; i < _gridColumns; i++)
                {
                    ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                for (var i = 0; i < _gridRows; i++)
                {
                    ButtonGrid.RowDefinitions.Add(new RowDefinition());
                }
                ButtonControl.InvalidateVisual();
                ButtonGrid.InvalidateVisual();
            };
            return ButtonGrid;
        });
    }

    partial void OnIsEditingChanged(bool value)
    {
        ButtonGrid.ShowGridLines = value;
        foreach (var button in Buttons) 
            button.IsEditing = value;
    }

    partial void OnGridColumnsChanged(int value)
    {
        if (value < MinGridColumns)
            GridColumns = MinGridColumns;
        var oldValue = ButtonGrid.ColumnDefinitions.Count;
        if (oldValue > value && Buttons.Max(x => x.GridColumn) < value)
            for (var i = oldValue - 1; i >= value; i--)
                ButtonGrid.ColumnDefinitions.RemoveAt(i);
        else
            for (var i = oldValue; i < value; i++)
                ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
    }

    partial void OnGridRowsChanged(int value)
    {
        if (value < MinGridRows)
            GridRows = MinGridRows;
        var oldValue = ButtonGrid.RowDefinitions.Count;
        if (oldValue > value && Buttons.Max(x => x.GridRow) < value)
            for (var i = oldValue - 1; i >= value; i--)
                ButtonGrid.RowDefinitions.RemoveAt(i);
        else
            for (var i = oldValue; i < value; i++)
                ButtonGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
    }

    public async Task AddButton(Window parentWindow)
    {
        var newButton = new TriggerButtonModel(Buttons, ButtonGrid)
        {
            TriggerUrl = BaseUrl,
            IsEditing = IsEditing
        };
        Buttons.Add(newButton);
        await newButton.Edit(parentWindow);
    }
    
    public void Delete()
    {
        _parentCollection.Remove(this);
    }
}

//JsonContext for serializing/deserializing ButtonGridViewModel
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ButtonGrid))]
[JsonSerializable(typeof(List<ButtonGrid>))]
[JsonSerializable(typeof(TriggerButton))]
[JsonSerializable(typeof(List<TriggerButton>))]
public partial class ButtonGridJsonContext : JsonSerializerContext
{
}