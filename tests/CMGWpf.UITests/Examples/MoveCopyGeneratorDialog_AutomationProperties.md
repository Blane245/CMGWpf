# Adding AutomationProperties to MoveCopyGeneratorDialog

To make the MoveCopyGeneratorDialog testable, add AutomationProperties to the controls.

## Updated XAML

Here's how to modify `src\CMGWpf\Dialogs\MoveCopyGeneratorDialog.xaml`:

```xml
<Window x:Class="CMGWpf.Dialogs.MoveCopyGeneratorDialog"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:CMGWpf.Dialogs"
        xmlns:viewmodel="clr-namespace:CMGWpf.View"
        d:DataContext="{d:DesignInstance Type=viewmodel:GeneratorViewModel}"
        mc:Ignorable="d"
        WindowStyle="None"
        Background="White"
        ResizeMode="CanResize"
        Title="MoveCopyGeneratorDialog"
        AutomationProperties.AutomationId="MoveCopyGeneratorDialog">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        <StackPanel Grid.Row="0" Grid.Column="0" Margin="0" Background="Aquamarine">
            <Label Content="{Binding MoveCopyMode}" Foreground="Black" FontWeight="Bold" VerticalAlignment="Center"/>
            <Label Content="Generator" Foreground="Black" FontWeight="Bold" VerticalAlignment="Center"/>
        </StackPanel>
        <StackPanel Grid.Row="0" Grid.Column="1" Margin="0" Background="Aquamarine">
            <Button x:Name="CloseDialog" 
                    Width="20" 
                    HorizontalAlignment="Right" 
                    VerticalAlignment="Center" 
                    Content="X" 
                    Command="{Binding GeneratorCancelCommand}" 
                    Style="{StaticResource WindowButton}"
                    AutomationProperties.AutomationId="CloseDialogButton"
                    AutomationProperties.Name="Close"/>
        </StackPanel>
        <StackPanel Grid.Row="1" Grid.ColumnSpan="2" Orientation="Horizontal">
            <Label Content="Move Track To:" VerticalAlignment="Center" Margin="0"/>
            <ComboBox ItemsSource="{Binding Tracks}" 
                      SelectedItem="{Binding SelectedTrack}" 
                      Width="150"
                      AutomationProperties.AutomationId="TrackComboBox"
                      AutomationProperties.Name="Select Target Track"/>
        </StackPanel>
        <StackPanel Grid.Row="2" Grid.Column="0" Orientation="Horizontal">
            <Button x:Name="MoveCopyButton" 
                    Content="OK" 
                    Width="50" 
                    Command="{Binding MoveCopyCommand}" 
                    Margin="0"
                    AutomationProperties.AutomationId="MoveCopyOkButton"
                    AutomationProperties.Name="OK"/>
            <Button x:Name="CancelButton" 
                    Content="Cancel" 
                    Width="50" 
                    Command="{Binding GeneratorCancelCommand}" 
                    Margin="5,0,0,0"
                    AutomationProperties.AutomationId="MoveCopyCancelButton"
                    AutomationProperties.Name="Cancel"/>
        </StackPanel>
    </Grid>
</Window>
```

## What Changed?

Added `AutomationProperties.AutomationId` and `AutomationProperties.Name` to:

1. **Window** - "MoveCopyGeneratorDialog"
2. **CloseDialog Button** - "CloseDialogButton"
3. **Track ComboBox** - "TrackComboBox"
4. **OK Button** - "MoveCopyOkButton"
5. **Cancel Button** - "MoveCopyCancelButton"

## Why Both AutomationId and Name?

- **AutomationId**: Unique identifier for finding elements in tests (preferred for tests)
- **Name**: Human-readable name for accessibility tools and screen readers

## Testing These Controls

```csharp
// Find the dialog
var dialog = MainWindow!.ModalWindows.FirstOrDefault();

// Find controls by AutomationId
var trackCombo = dialog.FindFirstDescendant(cf => cf.ByAutomationId("TrackComboBox"))?.AsComboBox();
var okButton = dialog.FindFirstDescendant(cf => cf.ByAutomationId("MoveCopyOkButton"))?.AsButton();
var cancelButton = dialog.FindFirstDescendant(cf => cf.ByAutomationId("MoveCopyCancelButton"))?.AsButton();

// Interact with them
trackCombo!.Select("Track 1");
okButton!.Click();
```
