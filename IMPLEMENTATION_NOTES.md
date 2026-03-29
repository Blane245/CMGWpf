# AttributeDescriptor Pattern Implementation

## Overview
Implemented a flexible, data-driven pattern for managing algorithm attributes in the Algorithmic Generator. This pattern decouples the Attribute UserControl from the specific generator implementation and allows each attribute (Note, Attack, Speed, Duration, Volume, Pan) to independently manage its own algorithm type and parameters.

## Key Components

### 1. AttributeDescriptor Class (`Model/AttributeDescriptor.cs`)
- **Purpose**: Wraps an Algorithm with its metadata (name, units)
- **Properties**:
  - `Name`: Display name (e.g., "Note", "Attack")
  - `Units`: Unit label (e.g., "(0-127)", "(note.cents)")
  - `Algorithm`: The current algorithm instance
  - `SelectedType`: The ALGORITHMTYPE enum value that drives algorithm creation
- **Key Feature**: When `SelectedType` changes, it automatically creates a new algorithm instance using `AlgorithmFactory`
- **Change Notification**: Implements `INotifyPropertyChanged` for two-way binding

### 2. GeneratorViewModel Updates (`View/GeneratorViewModel.cs`)
- **Added 6 AttributeDescriptor properties**:
  - `NoteAttribute`: "(note.cents)"
  - `AttackAttribute`: "(0-127)"
  - `SpeedAttribute`: "(BPM)"
  - `DurationAttribute`: "(note value)"
  - `VolumeAttribute`: "(dB)"
  - `PanAttribute`: "(-1,+1)"
- **Lazy Initialization**: Each property is created on first access and only when AlgorithmicGenerator exists
- **Two-Way Sync**: PropertyChanged handlers update the underlying Algorithmic generator's algorithm properties when the descriptor's Algorithm changes

### 3. Attribute UserControl Updates
#### XAML (`Panels/Attribute.xaml`)
- **DataContext**: Now expects an `AttributeDescriptor` instance
- **Simplified Bindings**:
  - `Name` → Displays attribute name
  - `Units` → Displays units
  - `SelectedType` → Binds to ComboBox for algorithm selection
  - `Algorithm` → Content of ContentControl for algorithm-specific UI
- **ComboBox ItemsSource**: Uses RelativeSource to access `AlgorithmTypes` from the parent's DataContext (GeneratorViewModel)

#### Code-Behind (`Panels/Attribute.xaml.cs`)
- **Simplified**: All DependencyProperty code removed
- **No logic**: Pure view - all behavior is in the AttributeDescriptor

### 4. AlgorithmicPanel Updates (`Panels/AlgorithmicPanel.xaml`)
- **Uncommented attribute section**
- **6 Attribute instances**: Each with DataContext bound to corresponding descriptor property
- **Example**:
  ```xaml
  <panels:Attribute DataContext="{Binding NoteAttribute}"/>
  ```

## Benefits

1. **Decoupling**: Attribute UserControl knows nothing about Algorithmic generator
2. **Reusability**: Same control works for all 6 attributes with different metadata
3. **Type Safety**: No reflection or string-based property access
4. **MVVM Compliant**: All logic in ViewModel/Model layers
5. **Extensible**: Easy to add new attributes or algorithm types
6. **Two-Way Binding**: Changes in UI automatically update the model
7. **No Dependency Properties**: Simpler code in UserControl

## How It Works

1. **User opens AlgorithmicPanel**: GeneratorViewModel lazily creates 6 AttributeDescriptor instances
2. **Each descriptor wraps**: A specific algorithm from the Algorithmic generator (e.g., NoteAlgorithm)
3. **User selects algorithm type**: ComboBox changes `SelectedType` on descriptor
4. **Descriptor creates new algorithm**: Via `AlgorithmFactory.CreateAlgorithm()`
5. **PropertyChanged fires**: ViewModel handler updates `AlgorithmicGenerator.NoteAlgorithm` (or other)
6. **UI updates**: ContentControl displays algorithm-specific parameters

## Future Extensions

### Algorithm-Specific UI Panels
To display different UI for each algorithm type, add DataTemplates in AlgorithmicPanel resources:

```xaml
<UserControl.Resources>
    <DataTemplate DataType="{x:Type model:Constant}">
        <StackPanel Orientation="Horizontal">
            <Label Content="Value:"/>
            <xctk:DoubleUpDown Value="{Binding Value}"/>
        </StackPanel>
    </DataTemplate>
    
    <DataTemplate DataType="{x:Type model:Oscillator}">
        <StackPanel Orientation="Horizontal">
            <Label Content="Center:"/>
            <xctk:DoubleUpDown Value="{Binding Center}"/>
            <Label Content="Frequency:"/>
            <xctk:DoubleUpDown Value="{Binding Frequency}"/>
            <!-- etc -->
        </StackPanel>
    </DataTemplate>
    <!-- Add templates for Wiener, Markovian, etc -->
</UserControl.Resources>
```

The ContentControl in Attribute.xaml will automatically select the correct template based on the Algorithm's type.

## Testing Checklist

- [ ] Open Algorithmic Generator dialog
- [ ] Verify all 6 attributes display with correct names and units
- [ ] Change algorithm type for each attribute
- [ ] Verify UI updates show algorithm-specific fields (when DataTemplates added)
- [ ] Save and load project - verify algorithms persist correctly
- [ ] Verify changes update the underlying Algorithmic generator
