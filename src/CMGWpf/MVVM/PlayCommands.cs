using CMGWpf.PlayFunctions;
using CMGWpf.View;
using System.Diagnostics;
using System.Windows;

namespace CMGWpf.MVVM
{
    public class PlayCommands(PlayViewModel vm)
    {
        private readonly PlayViewModel vm = vm;
        #region Play Dialog Commands
        public void Rewind()
        {
            Debug.WriteLine("Rewind command being executed.");

            if (vm.AudioOutput != null && vm.AudioProvider is AudioBufferProvider provider)
            {
                vm.AudioOutput.Pause();
                provider.Reset();
                vm.CurrentPlayPosition = 0;
            }
        }
        public void PlayPause()
        {
            Debug.WriteLine("Play/Pause command being executed.");

            if (vm.AudioOutput == null) return;

            if (vm.IsPlaying)
            {
                vm.AudioOutput.Pause();
                vm.IsPlaying = false;
            }
            else
            {
                vm.AudioOutput.Play();
                vm.IsPlaying = true;
            }
        }
        public void ShowVoicesToggle(PlayDialog dialog)
        {
            if (vm.ShowVoices)
            {
                Debug.WriteLine("[ShowVoices] Creating VoiceDialog...");
                vm.VoiceDialog = new VoiceDialog
                {
                    DataContext = FileViewModel.Instance,
                    Owner = dialog,
                    Width = 300,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.Manual
                };

                // Position it in the upper right corner of the screen containing the owner
                var ownerHandle = new System.Windows.Interop.WindowInteropHelper(dialog).Handle;
                var ownerScreen = System.Windows.Forms.Screen.FromHandle(ownerHandle);
                double left = ownerScreen.WorkingArea.Right - 310;
                double top = ownerScreen.WorkingArea.Top + 10;
                vm.VoiceDialog.Left = left;
                vm.VoiceDialog.Top = top;
                vm.VoiceDialog.Show();
            }
            else vm.VoiceDialog?.Close();
        }
        #endregion

    }
}
