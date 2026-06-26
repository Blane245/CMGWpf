using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace CMGWpf.Layout
{

    // This control provide a visual representation of all of the track generators that appear in the current timeline view. It is responsible for rendering the generators in the track display canvas, and for handling user interactions with the generators. Each generator is displayed as a rectangle starting at its start time and ending at this end time, with the height being 1/3 of the track height. The rectangle contains the name of the generator. When the rectangle is right-clicked, it provides a menu of several functions. The background color of the rectangle depends on the generator's type. The generator functions include: 
    //  1. Edit the generator's properties, including start time, end time, and generator-specific properties
    //  2. Copy the generator to a track of the user's choice. The start and end times are maintained and a unique name is created for the new generator.
    //  3. Move the generator to another track of the user's choice. The generator's name, start and end times are maintained.
    //  4. Mute the generator.
    //  5. Play the generator's audio output in a play dialog. See the Play Window for more information on this dialog.
    //  6. Delete the generator with confirmation.
    // In addition to these functions, the generator rectangle can be left-clicked/dragged to moved the generator rectangle up and down within the track display canvas to avoid obscuration with other generators. Left-click near the start or stop time of the rectangle and drag to change the start or stop time of the generator.
    // The background color of the generator rectangle is determined by its location in with with respect to the TimeInterval property of the TimeLine. When the generator's start and end times are within the TimeInterval start and end times, the rectangle is highlighted, indicating is selected. This is a filter used by the Play function.
    public partial class TrackDisplay : UserControl
    {
        public TrackDisplay()
        {
            InitializeComponent();

            // DataContext will be set by parent (TrackViewModel from Track.xaml)
            // When DataContext changes, wrap generators in GeneratorViewModels
            DataContextChanged += TrackDisplay_DataContextChanged;
        }

        private void TrackDisplay_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is TrackViewModel trackVM)
            {
                // Wrap each Generator in a GeneratorViewModel
                var generatorVMs = new ObservableCollection<GeneratorViewModel>();
                foreach (var generator in trackVM.Track.Generators)
                {
                    GeneratorViewModel vm = new GeneratorViewModel(generator, trackVM);
                    generatorVMs.Add(vm);
                    vm.UpdateColor();
                }

            }
        }

        private void GeneratorBorder_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Attach native mouse event handlers directly to the Border (like TimeLineViewModel does with Rectangle)
            if (sender is Border border && border.DataContext is GeneratorViewModel vm)
            {
                // Register the border with the ViewModel so it can manage capture
                vm.AttachMouseHandlers(border);
            }
        }
    }
}
