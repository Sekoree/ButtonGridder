using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using ButtonGridder.ViewModels;

namespace ButtonGridder.Entities;

//JSON serializable class for a grid of buttons
public class ButtonGrid
{
    public string Title { get; set; } = "Untitled";
    public int GridColumns { get; set; } = 10;
    public int GridRows { get; set; } = 6;
    public List<TriggerButton> Buttons { get; set; } = new();
    
    public static ButtonGrid ToSerializable(ButtonGridViewModel model)
    {
        try
        {
            return new ButtonGrid
            {
                Title = model.Title,
                GridColumns = model.GridColumns,
                GridRows = model.GridRows,
                Buttons = model.Buttons.Select(TriggerButton.ToSerializable).ToList()
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null!;
        }
    }
    
    public static ButtonGridViewModel ToModel(ButtonGrid grid, ObservableCollection<ButtonGridViewModel> parent, SolidColorBrush parentBackgroundBrush, SolidColorBrush parentTitleBrush)
    {
        var model = new ButtonGridViewModel(parent, grid.Title)
        {
            GridColumns = grid.GridColumns,
            GridRows = grid.GridRows
        };
        foreach (var button in grid.Buttons) 
            model.Buttons.Add(TriggerButton.ToModel(button, model.Buttons, model.ButtonGrid, parentBackgroundBrush, parentTitleBrush));
        return model;
    }
}