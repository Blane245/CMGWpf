using System.Diagnostics;

namespace CMGWpf.PlayFunctions.Utilities
{
    public static class DebugLog
    {
        // Set to true to enable debug output from PlayEngine, false to disable
        private static readonly bool EnableDebugOutput = true;

        /// <summary>
        /// Conditional debug output - only writes when EnableDebugOutput is true
        /// </summary>
        public static void Write(string message)
        {
            if (EnableDebugOutput)
            {
                Debug.WriteLine(message);
            }
        }

    }
}
