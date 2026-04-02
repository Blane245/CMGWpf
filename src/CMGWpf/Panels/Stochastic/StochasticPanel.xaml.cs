using CMGWpf.Model;
using CMGWpf.Services;
using CMGWpf.Types;
using CMGWpf.Utilities;
using CMGWpf.View;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows.Controls;
using System.Windows.Data;

namespace CMGWpf.Panels.Stochastic
{
    /// <summary>
    /// Interaction logic for StochasticPanel.xaml
    /// </summary>
    public partial class StochasticPanel : UserControl
    {
        public StochasticPanel()
        {
            InitializeComponent();
            DataContextChanged += StochasticPanel_DataContextChanged;
        }

        private void StochasticPanel_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            // Unsubscribe from old DataContext if exists
            if (e.OldValue is GeneratorViewModel oldVm)
            {
                oldVm.PropertyChanged -= ViewModel_PropertyChanged;
            }

            // Subscribe to new DataContext
            if (e.NewValue is GeneratorViewModel newVm)
            {
                newVm.PropertyChanged += ViewModel_PropertyChanged;
                UpdateDynamicColumns(newVm);
                newVm.Messages.Clear();
            }
        }

        private async Task LoadEnsembleNamesAsync(GeneratorViewModel vm)
        {
            vm.Messages = [];
            try
            {
                vm.Messages.Add(new Message { Text = "Loading ensembles...", Error = false });
                var ensembles = await EnsembleUtilities.GetEnsembleListAsync();
                vm.Messages.Add(new Message { Text = $"Loaded {ensembles.Count} ensembles", Error = false });
            }
            catch (HttpRequestException ex)
            {
                vm.Messages.Add(new Message { Text = $"HTTP Error: {ex.Message}", Error = true }  );
            }
            catch (TaskCanceledException ex)
            {
                vm.Messages.Add(new Message { Text = $"Request timeout error {ex.Message} - check if DB server is running at {GlobalService.Instance.DbServer}:{GlobalService.Instance.DbPort}", Error = true });
            }
            catch (Exception ex)
            {
                vm.Messages.Add(new Message { Text = $"Error loading ensemble names: {ex.Message}", Error = true });
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Regenerate columns when StochasticComposition changes
            if (e.PropertyName == nameof(GeneratorViewModel.StochasticComposition))
            {
                if (sender is GeneratorViewModel vm)
                {
                    UpdateDynamicColumns(vm);
                }
            }
        }

        private void UpdateDynamicColumns(GeneratorViewModel viewModel)
        {
            // Remove existing dynamic voice columns (keep Move, Time, and Sum)
            for (int i = CompositionDataGrid.Columns.Count - 2; i >= 2; i--)
            {
                CompositionDataGrid.Columns.RemoveAt(i);
            }

            // Add columns for each active voice
            ObservableCollection<Voice> voices = viewModel.StochasticGenerator!.Voices;
            ObservableCollection<Voice> activeVoices = [.. voices.Where(v => !v.Muted)];
            foreach ((var voice, var i) in activeVoices.Select((v, i) => (v, i)))
            {
                if (voice.Muted) continue;

                string voiceName = voice.Name;

                var column = new DataGridTextColumn
                {
                    Header = CreateColumnHeader(voiceName, i, activeVoices.Count, viewModel),
                    IsReadOnly = true,
                    Binding = new Binding($"Values[{i}]")
                };

                // Insert before the Sum column (which is the last column)
                CompositionDataGrid.Columns.Insert(CompositionDataGrid.Columns.Count - 1, column);
            }
        }

        private StackPanel CreateColumnHeader(string voiceName, int columnIndex, int totalColumns, GeneratorViewModel viewModel)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };

            // Voice name label
            var nameLabel = new Label
            {
                Content = voiceName,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 0, 0, 2)
            };
            stackPanel.Children.Add(nameLabel);

            // Button panel
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };

            // Move Left button
            var moveLeftButton = new Button
            {
                Content = "◀",
                Width = 20,
                Height = 20,
                Margin = new System.Windows.Thickness(2, 0, 2, 0),
                IsEnabled = columnIndex > 0,
                ToolTip = "Move Composition Values Left"
            };
            moveLeftButton.Click += (s, e) => MoveColumn(columnIndex, -1, viewModel);
            buttonPanel.Children.Add(moveLeftButton);

            // Move Right button
            var moveRightButton = new Button
            {
                Content = "▶",
                Width = 20,
                Height = 20,
                Margin = new System.Windows.Thickness(2, 0, 2, 0),
                IsEnabled = columnIndex < totalColumns - 1,
                ToolTip = "Move Composition values Right"
            };
            moveRightButton.Click += (s, e) => MoveColumn(columnIndex, 1, viewModel);
            buttonPanel.Children.Add(moveRightButton);

            stackPanel.Children.Add(buttonPanel);

            return stackPanel;
        }

        private void MoveColumn(int currentIndex, int direction, GeneratorViewModel viewModel)
        {
            // This should swap voices composition values in the StochasticGenerator.Composition array, leaving the voices themselves unchanged, and trigger regeneration of the composition both in the generator and the UI. The direction parameter indicates whether to move left (-1) or right (+1).

            if (viewModel.StochasticGenerator == null) return;

            var voices = viewModel.StochasticGenerator.Voices;
            int newIndex = currentIndex + direction;

            // Validate new index
            if (newIndex < 0 || newIndex >= voices.Count) return;

            // Find the actual voice indices (accounting for muted voices)
            int actualCurrentIndex = -1;
            int actualNewIndex = -1;
            int unmutedCount = 0;

            for (int i = 0; i < voices.Count; i++)
            {
                if (!voices[i].Muted)
                {
                    if (unmutedCount == currentIndex)
                        actualCurrentIndex = i;
                    if (unmutedCount == newIndex)
                        actualNewIndex = i;
                    unmutedCount++;
                }
            }

            if (actualCurrentIndex == -1 || actualNewIndex == -1) return;

            // Swap corresponding composition columns
            var composition = viewModel.StochasticGenerator.Composition;
            for (int row = 0; row < composition.Length; row++)
            {
                (composition[row][actualNewIndex], composition[row][actualCurrentIndex]) = (composition[row][actualCurrentIndex], composition[row][actualNewIndex]);
            }

            // Notify changes to refresh the UI
            viewModel.NotifyGeneratorChanged(nameof(GeneratorViewModel.StochasticGenerator));
            viewModel.NotifyGeneratorChanged(nameof(GeneratorViewModel.StochasticComposition));
        }

        private void MoveRowUp_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is GeneratorViewModel.StochasticCompositionRow row &&
                DataContext is GeneratorViewModel viewModel)
            {
                MoveRow(row, -1, viewModel);
            }
        }

        private void MoveRowDown_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is GeneratorViewModel.StochasticCompositionRow row &&
                DataContext is GeneratorViewModel viewModel)
            {
                MoveRow(row, 1, viewModel);
            }
        }

        private void MoveRow(GeneratorViewModel.StochasticCompositionRow row, int direction, GeneratorViewModel viewModel)
        {
            if (viewModel.StochasticGenerator == null) return;

            var composition = viewModel.StochasticGenerator.Composition;
            if (composition.Length == 0) return;

            // Find the current row index by time
            double deltaT = viewModel.StochasticGenerator.GetDeltaT();
            if (deltaT == 0) return;

            int currentIndex = (int)Math.Round(row.Time / deltaT);
            int newIndex = currentIndex + direction;

            // Validate new index
            if (newIndex < 0 || newIndex >= composition.Length) return;

            // Swap the composition rows
            (composition[newIndex], composition[currentIndex]) = (composition[currentIndex], composition[newIndex]);

            // Notify changes to refresh the UI
            viewModel.NotifyGeneratorChanged(nameof(GeneratorViewModel.StochasticGenerator));
            viewModel.NotifyGeneratorChanged(nameof(GeneratorViewModel.StochasticComposition));
        }
    }
}
