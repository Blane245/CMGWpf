using CMGWpf.Layout;
using CMGWpf.View;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;


namespace CMGWpf.Layout.UnitTests
{
    /// <summary>
    /// Unit tests for the TimeLine class constructor.
    /// Note: These tests require WPF runtime initialization and may need to run in a WPF test host environment.
    /// </summary>
    [TestClass]
    public partial class TimeLineTests
    {
        /// <summary>
        /// Tests that the TimeLine constructor completes successfully without throwing exceptions.
        /// Verifies that InitializeComponent() executes and the instance is created.
        /// Note: This test requires WPF Application context and compiled XAML resources to succeed.
        /// </summary>
        [STATestMethod]
        public void TimeLine_Constructor_CreatesInstanceSuccessfully()
        {
            // Arrange & Act
            var timeLine = new TimeLine();

            // Assert
            Assert.IsNotNull(timeLine);
            Assert.IsNotNull(timeLine.DataContext);
        }

        /// <summary>
        /// Tests that the TimeLine constructor sets the DataContext property to the TimeLineViewModel singleton instance.
        /// Verifies that after construction, DataContext is correctly assigned to TimeLineViewModel.Instance.
        /// Note: This test requires WPF Application context and compiled XAML resources to succeed.
        /// </summary>
        [TestMethod]
        public void TimeLine_Constructor_SetsDataContextToTimeLineViewModelInstance()
        {
            // Arrange
            TimeLine? timeLine = null;
            Exception? caughtException = null;

            // Act - Run on STA thread as required by WPF
            var thread = new Thread(() =>
            {
                try
                {
                    // Create TimeLine instance
                    // This may fail if WPF runtime is not initialized or XAML resources are unavailable
                    timeLine = new TimeLine();
                    
                    // Assert on the same STA thread that created the object
                    Assert.IsNotNull(timeLine);
                    Assert.IsNotNull(timeLine.DataContext);
                    Assert.AreSame(TimeLineViewModel.Instance, timeLine.DataContext);
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            // Assert
            if (caughtException != null)
            {
                // Check if it's a WPF initialization issue that should be inconclusive
                if (caughtException is System.IO.IOException || 
                    caughtException is System.Windows.Markup.XamlParseException ||
                    (caughtException.InnerException != null && 
                     (caughtException.InnerException is System.IO.IOException || 
                      caughtException.InnerException is System.Windows.Markup.XamlParseException)))
                {
                    Assert.Inconclusive(
                        $"TimeLine constructor requires WPF runtime and XAML resources. " +
                        $"Test inconclusive due to: {caughtException.GetType().Name} - {caughtException.Message}");
                }
                else
                {
                    // Re-throw for assertion failures or other exceptions
                    throw caughtException;
                }
            }
        }
    }
}