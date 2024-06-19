using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ButtonGridder.ViewModels;

namespace ButtonGridder.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void NameText_OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        //get parent of the sender, its a StackPanel, then get the TextBox child named "EditNameTextBox", make the sender TextBlock Invisible and the TextBox Visible
        var textBlock = sender as TextBlock;
        if (textBlock == null) return;
        var stackPanel = textBlock.Parent as StackPanel;
        if (stackPanel == null) return;
        var textBox = stackPanel.Children.FirstOrDefault(c => c.Name == "EditNameTextBox") as TextBox;
        if (textBox == null) return;
        textBlock.IsVisible = false;
        textBox.IsVisible = true;
        textBox.Focus();
        textBox.SelectAll();
    }

    private void EditNameTextBox_OnKeyDown(object? sender, KeyEventArgs e)
    {
        //reverse the process of the NameText_OnDoubleTapped method when the Enter key is pressed
        if (e.Key != Key.Enter) 
            return;
        var textBox = sender as TextBox;
        if (textBox == null) return;
        var stackPanel = textBox.Parent as StackPanel;
        if (stackPanel == null) return;
        var textBlock = stackPanel.Children.FirstOrDefault(c => c.Name == "NameText") as TextBlock;
        if (textBlock == null) return;
        textBlock.IsVisible = true;
        textBox.IsVisible = false;
    }

    private void EditNameTextBox_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        //reverse the process of the NameText_OnDoubleTapped method when the TextBox loses focus
        var textBox = sender as TextBox;
        if (textBox == null) return;
        var stackPanel = textBox.Parent as StackPanel;
        if (stackPanel == null) return;
        var textBlock = stackPanel.Children.FirstOrDefault(c => c.Name == "NameText") as TextBlock;
        if (textBlock == null) return;
        textBlock.IsVisible = true;
        textBox.IsVisible = false;
    }
}
