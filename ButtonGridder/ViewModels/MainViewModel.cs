using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ButtonGridder.Entities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ButtonGridder.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ObservableCollection<ButtonGridViewModel> ButtonGrids { get; } = new();

    [ObservableProperty] private ButtonGridViewModel? _selectedButtonGrid;
    
    private SolidColorBrush _backgroundColorBrush = new(Color.Parse("#333"));
    private SolidColorBrush _titleColorBrush = new(Color.Parse("#FFF"));
    
    private bool _isEditing = false;
    
    public MainViewModel()
    {
        var btnGrid = new ButtonGridViewModel(ButtonGrids);
        ButtonGrids.Add(btnGrid);
        SelectedButtonGrid = btnGrid;
    }

    public void EnableEditing()
    {
        foreach (var buttonGrid in ButtonGrids)
            buttonGrid.IsEditing = !_isEditing;
        _isEditing = !_isEditing;
    }

    public void AddButtonGrid()
    {
        var btnGrid = new ButtonGridViewModel(ButtonGrids);
        ButtonGrids.Add(btnGrid);
        SelectedButtonGrid = btnGrid;
    }

    public async Task SaveButtonGrid(Window parent)
    {
        if (SelectedButtonGrid is null)
            return;
        var fpo = new FilePickerSaveOptions()
        {
            Title = "Save ButtonGrid",
            SuggestedFileName = $"{SelectedButtonGrid.Title}.btng",
            DefaultExtension = "btng",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("ButtonGrid config (.btng)")
                {
                    Patterns = new[] { "*.btng" }
                }
            }
        };
        var file = await parent.StorageProvider.SaveFilePickerAsync(fpo);
        if (file is null)
            return;

        var path = file.TryGetLocalPath();
        if (path is null)
            return;

        await using var content = File.Create(path);
        var gridData = ButtonGrid.ToSerializable(SelectedButtonGrid);
        await JsonSerializer.SerializeAsync(content, gridData, ButtonGridJsonContext.Default.ButtonGrid);
    }

    public async Task SaveButtonGridCollection(Window parent)
    {
        if (ButtonGrids.Count == 0)
            return;
        var fpo = new FilePickerSaveOptions()
        {
            Title = "Save ButtonGrid Collection",
            SuggestedFileName = "ButtonGridCollection.btngc",
            DefaultExtension = "btngc",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("ButtonGrid Collection config (.btngc)")
                {
                    Patterns = new[] { "*.btngc" }
                }
            }
        };
        var file = await parent.StorageProvider.SaveFilePickerAsync(fpo);
        if (file is null)
            return;

        var path = file.TryGetLocalPath();
        if (path is null)
            return;

        await using var content = File.Create(path);
        var gridData = ButtonGrids.Select(ButtonGrid.ToSerializable).ToList();
        await JsonSerializer.SerializeAsync(content, gridData, ButtonGridJsonContext.Default.ListButtonGrid);
        
    }
    
    public async Task LoadButtonGrid(Window parent)
    {
        var fpo = new FilePickerOpenOptions()
        {
            Title = "Load ButtonGrid",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("ButtonGrid config (.btng)")
                {
                    Patterns = new[] { "*.btng" }
                }
            }
        };
        var files = await parent.StorageProvider.OpenFilePickerAsync(fpo);
        if (files.Count == 0)
            return;

        foreach (var file in files)
        {
            var path = file.TryGetLocalPath();
            if (path is null)
                continue;

            await using var content = File.OpenRead(path);
            try
            {
                var gridData =
                    await JsonSerializer.DeserializeAsync(content, ButtonGridJsonContext.Default.ButtonGrid);
                if (gridData is null)
                    continue;
                var asModel = ButtonGrid.ToModel(gridData, ButtonGrids, _backgroundColorBrush, _titleColorBrush);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ButtonGrids.Add(asModel);
                    asModel.IsEditing = _isEditing;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public async Task LoadButtonGridCollection(Window parent)
    {
        var fpo = new FilePickerOpenOptions()
        {
            Title = "Load ButtonGrid Collection",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("ButtonGrid Collection config (.btngc)")
                {
                    Patterns = new[] { "*.btngc" }
                }
            }
        };
        var files = await parent.StorageProvider.OpenFilePickerAsync(fpo);
        if (files.Count == 0)
            return;

        foreach (var file in files)
        {
            var path = file.TryGetLocalPath();
            if (path is null)
                continue;

            await using var content = File.OpenRead(path);
            try
            {
                var gridData =
                    await JsonSerializer.DeserializeAsync(content,
                        ButtonGridJsonContext.Default.ListButtonGrid);
                if (gridData is null)
                    continue;
                var asModels = gridData.Select(x => ButtonGrid.ToModel(x, ButtonGrids, _backgroundColorBrush, _titleColorBrush));
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var buttonGridViewModels = asModels as ButtonGridViewModel[] ?? asModels.ToArray();
                    ButtonGrids.AddRange(buttonGridViewModels);
                    foreach (var model in buttonGridViewModels)
                        model.IsEditing = _isEditing;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}