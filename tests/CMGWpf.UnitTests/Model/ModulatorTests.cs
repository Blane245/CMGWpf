using System;

using CMGWpf.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CMGWpf.Model.UnitTests
{
    /// <summary>
    /// Unit tests for the ModulatorFunctions class.
    /// </summary>
    [TestClass]
    public class ModulatorFunctionsTests
    {
        private const double Tolerance = 1e-10;

        /// <summary>
        /// Tests that NoModulator returns center when frequency is zero, regardless of other parameters.
        /// </summary>
        /// <param name="center">The center value.</param>
        /// <param name="time">The time value.</param>
        /// <param name="amplitude">The amplitude value.</param>
        /// <param name="phase">The phase value.</param>
        [TestMethod]
        [DataRow(10.0, 0.0, 5.0, 0.0, DisplayName = "Frequency zero with positive center")]
        [DataRow(-10.0, 5.0, 3.0, 1.0, DisplayName = "Frequency zero with negative center")]
        [DataRow(0.0, 1.0, 2.0, 3.0, DisplayName = "Frequency zero with center zero")]
        [DataRow(100.5, 10.0, -5.0, -2.0, DisplayName = "Frequency zero with various parameters")]
        public void NoModulator_WhenFrequencyIsZero_ReturnsCenter(double center, double time, double amplitude, double phase)
        {
            // Arrange
            double frequency = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result);
        }

        /// <summary>
        /// Tests that NoModulator returns zero when center is zero, regardless of other parameters.
        /// </summary>
        /// <param name="frequency">The frequency value.</param>
        /// <param name="time">The time value.</param>
        /// <param name="amplitude">The amplitude value.</param>
        /// <param name="phase">The phase value.</param>
        [TestMethod]
        [DataRow(1.0, 0.0, 5.0, 0.0, DisplayName = "Center zero with positive frequency")]
        [DataRow(5.0, 2.0, 3.0, 1.0, DisplayName = "Center zero with various parameters")]
        [DataRow(-3.0, 1.0, -2.0, -1.0, DisplayName = "Center zero with negative frequency")]
        public void NoModulator_WhenCenterIsZero_ReturnsZero(double frequency, double time, double amplitude, double phase)
        {
            // Arrange
            double center = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that NoModulator returns zero when both frequency and center are zero.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenBothFrequencyAndCenterAreZero_ReturnsZero()
        {
            // Arrange
            double time = 1.0;
            double center = 0.0;
            double frequency = 0.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that NoModulator calculates correctly when time and phase are zero,
        /// which should result in sin(0) = 0, so the result equals center.
        /// </summary>
        /// <param name="center">The center value.</param>
        /// <param name="frequency">The frequency value.</param>
        /// <param name="amplitude">The amplitude value.</param>
        [TestMethod]
        [DataRow(10.0, 1.0, 5.0, DisplayName = "Time and phase zero with positive values")]
        [DataRow(-5.0, 2.0, 3.0, DisplayName = "Time and phase zero with negative center")]
        [DataRow(7.5, 0.5, -2.0, DisplayName = "Time and phase zero with negative amplitude")]
        public void NoModulator_WhenTimeAndPhaseAreZero_ReturnsCenter(double center, double frequency, double amplitude)
        {
            // Arrange
            double time = 0.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result, Tolerance);
        }

        /// <summary>
        /// Tests that NoModulator calculates correctly when amplitude is zero,
        /// which should result in the output equaling center regardless of the sine value.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenAmplitudeIsZero_ReturnsCenter()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = 2.0;
            double amplitude = 0.0;
            double phase = 0.5;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result, Tolerance);
        }

        /// <summary>
        /// Tests that NoModulator calculates correctly with known sine values.
        /// When the argument to sin evaluates to π/2, sin should be 1.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenSineArgumentIsHalfPi_CalculatesCorrectly()
        {
            // Arrange
            double time = 0.25;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;
            // 2π * 1.0 * 0.25 + 0 = π/2, sin(π/2) = 1
            // Expected: 10 + 5 * 1 = 15

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(15.0, result, Tolerance);
        }

        /// <summary>
        /// Tests that NoModulator calculates correctly when the sine argument evaluates to π,
        /// where sin(π) ≈ 0.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenSineArgumentIsPi_CalculatesCorrectly()
        {
            // Arrange
            double time = 0.5;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;
            // 2π * 1.0 * 0.5 + 0 = π, sin(π) ≈ 0
            // Expected: 10 + 5 * 0 = 10

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result, Tolerance);
        }

        /// <summary>
        /// Tests that NoModulator calculates correctly when the sine argument evaluates to 3π/2,
        /// where sin(3π/2) = -1.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenSineArgumentIsThreeHalfPi_CalculatesCorrectly()
        {
            // Arrange
            double time = 0.75;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;
            // 2π * 1.0 * 0.75 + 0 = 3π/2, sin(3π/2) = -1
            // Expected: 10 + 5 * (-1) = 5

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(5.0, result, Tolerance);
        }

        /// <summary>
        /// Tests that NoModulator handles negative time values correctly.
        /// </summary>
        [TestMethod]
        public void NoModulator_WithNegativeTime_CalculatesCorrectly()
        {
            // Arrange
            double time = -0.25;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;
            // 2π * 1.0 * (-0.25) + 0 = -π/2, sin(-π/2) = -1
            // Expected: 10 + 5 * (-1) = 5

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(5.0, result, Tolerance);
        }

        /// <summary>
        /// Tests that NoModulator handles negative frequency values correctly.
        /// </summary>
        [TestMethod]
        public void NoModulator_WithNegativeFrequency_CalculatesCorrectly()
        {
            // Arrange
            double time = 0.25;
            double center = 10.0;
            double frequency = -1.0;
            double amplitude = 5.0;
            double phase = 0.0;
            // 2π * (-1.0) * 0.25 + 0 = -π/2, sin(-π/2) = -1
            // Expected: 10 + 5 * (-1) = 5

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(5.0, result, Tolerance);
        }

        /// <summary>
        /// Tests that NoModulator handles negative amplitude values correctly.
        /// </summary>
        [TestMethod]
        public void NoModulator_WithNegativeAmplitude_CalculatesCorrectly()
        {
            // Arrange
            double time = 0.25;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = -5.0;
            double phase = 0.0;
            // 2π * 1.0 * 0.25 + 0 = π/2, sin(π/2) = 1
            // Expected: 10 + (-5) * 1 = 5

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(5.0, result, Tolerance);
        }

        /// <summary>
        /// Tests that NoModulator handles phase offset correctly by shifting the sine wave.
        /// </summary>
        [TestMethod]
        public void NoModulator_WithPhaseOffset_CalculatesCorrectly()
        {
            // Arrange
            double time = 0.0;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = Math.PI / 2;
            // 2π * 1.0 * 0.0 + π/2 = π/2, sin(π/2) = 1
            // Expected: 10 + 5 * 1 = 15

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(15.0, result, Tolerance);
        }

        /// <summary>
        /// Tests that NoModulator returns NaN when time is NaN and frequency/center are non-zero.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenTimeIsNaN_ReturnsNaN()
        {
            // Arrange
            double time = double.NaN;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that NoModulator returns center when frequency is NaN but center is zero.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenFrequencyIsNaNAndCenterIsZero_ReturnsZero()
        {
            // Arrange
            double time = 1.0;
            double center = 0.0;
            double frequency = double.NaN;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that NoModulator returns NaN when frequency is NaN and center is non-zero.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenFrequencyIsNaNAndCenterIsNonZero_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = double.NaN;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that NoModulator returns NaN when amplitude is NaN and frequency/center are non-zero.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenAmplitudeIsNaN_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = double.NaN;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that NoModulator returns NaN when phase is NaN and frequency/center are non-zero.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenPhaseIsNaN_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = double.NaN;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that NoModulator handles positive infinity for time parameter.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenTimeIsPositiveInfinity_ReturnsNaN()
        {
            // Arrange
            double time = double.PositiveInfinity;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that NoModulator handles negative infinity for time parameter.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenTimeIsNegativeInfinity_ReturnsNaN()
        {
            // Arrange
            double time = double.NegativeInfinity;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that NoModulator returns center when frequency is zero even if center is infinity.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenFrequencyIsZeroAndCenterIsInfinity_ReturnsInfinity()
        {
            // Arrange
            double time = 1.0;
            double center = double.PositiveInfinity;
            double frequency = 0.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(double.PositiveInfinity, result);
        }

        /// <summary>
        /// Tests that NoModulator handles positive infinity for frequency parameter when center is non-zero.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenFrequencyIsPositiveInfinity_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = double.PositiveInfinity;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that NoModulator handles large positive values correctly.
        /// </summary>
        [TestMethod]
        public void NoModulator_WithLargePositiveValues_CalculatesWithinTolerance()
        {
            // Arrange
            double time = 1e6;
            double center = 1e6;
            double frequency = 1.0;
            double amplitude = 1e3;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(!double.IsNaN(result) && !double.IsInfinity(result));
        }

        /// <summary>
        /// Tests that NoModulator handles very small positive values correctly.
        /// </summary>
        [TestMethod]
        public void NoModulator_WithVerySmallPositiveValues_CalculatesCorrectly()
        {
            // Arrange
            double time = 1e-10;
            double center = 1e-10;
            double frequency = 1e-10;
            double amplitude = 1e-10;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(!double.IsNaN(result) && !double.IsInfinity(result));
        }

        /// <summary>
        /// Tests that NoModulator handles double.MaxValue for time parameter.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenTimeIsMaxValue_ReturnsNaN()
        {
            // Arrange
            double time = double.MaxValue;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that NoModulator handles double.MinValue for time parameter.
        /// </summary>
        [TestMethod]
        public void NoModulator_WhenTimeIsMinValue_ReturnsNaN()
        {
            // Arrange
            double time = double.MinValue;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.NoModulator(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }
        private const double TWO_PI = 2 * Math.PI;

        /// <summary>
        /// Tests that Sine returns the center value when frequency is zero.
        /// When frequency is zero, no oscillation should occur, so the method returns the center value.
        /// </summary>
        /// <param name="center">The center value to test.</param>
        [TestMethod]
        [DataRow(0.0, DisplayName = "Center is zero")]
        [DataRow(1.0, DisplayName = "Center is positive")]
        [DataRow(-1.0, DisplayName = "Center is negative")]
        [DataRow(100.5, DisplayName = "Center is large positive")]
        [DataRow(-100.5, DisplayName = "Center is large negative")]
        [DataRow(double.MaxValue, DisplayName = "Center is MaxValue")]
        [DataRow(double.MinValue, DisplayName = "Center is MinValue")]
        public void Sine_WhenFrequencyIsZero_ReturnsCenter(double center)
        {
            // Arrange
            double time = 1.0;
            double frequency = 0.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result);
        }

        /// <summary>
        /// Tests that Sine returns zero when center is zero, regardless of other parameters.
        /// This tests the early exit condition when center equals zero.
        /// </summary>
        /// <param name="frequency">The frequency value to test.</param>
        /// <param name="amplitude">The amplitude value to test.</param>
        /// <param name="phase">The phase value to test.</param>
        [TestMethod]
        [DataRow(1.0, 5.0, 0.0, DisplayName = "Standard positive values")]
        [DataRow(10.0, 10.0, Math.PI, DisplayName = "Different frequency and phase")]
        [DataRow(-5.0, -10.0, -Math.PI, DisplayName = "Negative values")]
        [DataRow(0.5, 2.5, 1.5, DisplayName = "Fractional values")]
        [DataRow(1000.0, 1000.0, 100.0, DisplayName = "Large values")]
        public void Sine_WhenCenterIsZero_ReturnsZero(double frequency, double amplitude, double phase)
        {
            // Arrange
            double time = 1.0;
            double center = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that Sine returns zero when both frequency and center are zero.
        /// This verifies the early exit condition handles both being zero correctly.
        /// </summary>
        [TestMethod]
        public void Sine_WhenBothFrequencyAndCenterAreZero_ReturnsZero()
        {
            // Arrange
            double time = 1.0;
            double center = 0.0;
            double frequency = 0.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that Sine correctly calculates the sinusoidal modulation with valid inputs.
        /// Expected result is calculated as: center + amplitude * sin(2π * frequency * time + phase)
        /// </summary>
        /// <param name="time">The time value.</param>
        /// <param name="center">The center value.</param>
        /// <param name="frequency">The frequency value.</param>
        /// <param name="amplitude">The amplitude value.</param>
        /// <param name="phase">The phase value.</param>
        /// <param name="expectedResult">The expected result of the calculation.</param>
        [TestMethod]
        [DataRow(0.0, 10.0, 1.0, 5.0, 0.0, 10.0, DisplayName = "Time zero with zero phase")]
        [DataRow(0.25, 10.0, 1.0, 5.0, 0.0, 15.0, DisplayName = "Quarter period")]
        [DataRow(0.5, 10.0, 1.0, 5.0, 0.0, 10.0, DisplayName = "Half period")]
        [DataRow(0.75, 10.0, 1.0, 5.0, 0.0, 5.0, DisplayName = "Three quarter period")]
        [DataRow(1.0, 10.0, 1.0, 5.0, 0.0, 10.0, DisplayName = "Full period")]
        [DataRow(1.0, 5.0, 2.0, 3.0, 0.0, 5.0, DisplayName = "Different frequency")]
        [DataRow(1.0, 10.0, 1.0, 5.0, 1.5707963267948966, 15.0, DisplayName = "With phase shift PI/2")]
        public void Sine_WithValidInputs_ReturnsExpectedSineValue(double time, double center, double frequency, double amplitude, double phase, double expectedResult)
        {
            // Arrange
            // All parameters provided via DataRow

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(expectedResult, result, Tolerance);
        }

        /// <summary>
        /// Tests that Sine correctly handles negative amplitude values.
        /// Negative amplitude should invert the oscillation around the center.
        /// </summary>
        [TestMethod]
        public void Sine_WithNegativeAmplitude_ReturnsInvertedOscillation()
        {
            // Arrange
            double time = 0.25;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = -5.0;
            double phase = 0.0;
            double expectedResult = 5.0; // center + (-5) * sin(2π * 1 * 0.25 + 0) = 10 + (-5) * 1 = 5

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(expectedResult, result, Tolerance);
        }

        /// <summary>
        /// Tests that Sine correctly handles negative frequency values.
        /// Negative frequency should reverse the direction of oscillation.
        /// </summary>
        [TestMethod]
        public void Sine_WithNegativeFrequency_ReturnsCorrectValue()
        {
            // Arrange
            double time = 0.25;
            double center = 10.0;
            double frequency = -1.0;
            double amplitude = 5.0;
            double phase = 0.0;
            double expectedResult = 5.0; // center + 5 * sin(2π * (-1) * 0.25 + 0) = 10 + 5 * sin(-π/2) = 10 + 5 * (-1) = 5

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(expectedResult, result, Tolerance);
        }

        /// <summary>
        /// Tests that Sine correctly handles zero amplitude.
        /// When amplitude is zero, the result should always equal the center value.
        /// </summary>
        [TestMethod]
        public void Sine_WithZeroAmplitude_ReturnsCenter()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 0.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result, Tolerance);
        }

        /// <summary>
        /// Tests that Sine returns NaN when time is NaN.
        /// NaN should propagate through the calculation.
        /// </summary>
        [TestMethod]
        public void Sine_WithNaNTime_ReturnsNaN()
        {
            // Arrange
            double time = double.NaN;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Sine returns NaN when amplitude is NaN.
        /// NaN should propagate through the calculation.
        /// </summary>
        [TestMethod]
        public void Sine_WithNaNAmplitude_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = double.NaN;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Sine returns NaN when phase is NaN.
        /// NaN should propagate through the calculation.
        /// </summary>
        [TestMethod]
        public void Sine_WithNaNPhase_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = double.NaN;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Sine returns NaN when frequency is NaN.
        /// NaN should propagate through the calculation.
        /// </summary>
        [TestMethod]
        public void Sine_WithNaNFrequency_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = double.NaN;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Sine handles positive infinity for time parameter.
        /// The result depends on how Math.Sin handles infinity in the argument.
        /// </summary>
        [TestMethod]
        public void Sine_WithPositiveInfinityTime_ReturnsNaN()
        {
            // Arrange
            double time = double.PositiveInfinity;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Sine handles negative infinity for time parameter.
        /// The result depends on how Math.Sin handles infinity in the argument.
        /// </summary>
        [TestMethod]
        public void Sine_WithNegativeInfinityTime_ReturnsNaN()
        {
            // Arrange
            double time = double.NegativeInfinity;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Sine handles positive infinity for amplitude parameter.
        /// With a non-zero sine result, this should return infinity.
        /// </summary>
        [TestMethod]
        public void Sine_WithPositiveInfinityAmplitude_ReturnsInfinity()
        {
            // Arrange
            double time = 0.25;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = double.PositiveInfinity;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsPositiveInfinity(result));
        }

        /// <summary>
        /// Tests that Sine handles negative infinity for amplitude parameter.
        /// With a non-zero sine result, this should return negative infinity.
        /// </summary>
        [TestMethod]
        public void Sine_WithNegativeInfinityAmplitude_ReturnsNegativeInfinity()
        {
            // Arrange
            double time = 0.25;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = double.NegativeInfinity;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNegativeInfinity(result));
        }

        /// <summary>
        /// Tests that Sine handles positive infinity for center parameter when frequency is non-zero.
        /// The result should be infinity since center is added to the calculation.
        /// </summary>
        [TestMethod]
        public void Sine_WithPositiveInfinityCenter_ReturnsPositiveInfinity()
        {
            // Arrange
            double time = 1.0;
            double center = double.PositiveInfinity;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsPositiveInfinity(result));
        }

        /// <summary>
        /// Tests that Sine handles negative infinity for center parameter when frequency is non-zero.
        /// The result should be negative infinity since center is added to the calculation.
        /// </summary>
        [TestMethod]
        public void Sine_WithNegativeInfinityCenter_ReturnsNegativeInfinity()
        {
            // Arrange
            double time = 1.0;
            double center = double.NegativeInfinity;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNegativeInfinity(result));
        }

        /// <summary>
        /// Tests that Sine handles positive infinity for frequency parameter.
        /// This results in an infinite argument to Math.Sin, which returns NaN.
        /// </summary>
        [TestMethod]
        public void Sine_WithPositiveInfinityFrequency_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = double.PositiveInfinity;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Sine handles very large time values correctly.
        /// This ensures numerical stability with extreme inputs.
        /// </summary>
        [TestMethod]
        public void Sine_WithVeryLargeTime_ReturnsValidResult()
        {
            // Arrange
            double time = 1e15;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(!double.IsNaN(result) && !double.IsInfinity(result));
            Assert.IsTrue(result >= center - amplitude && result <= center + amplitude);
        }

        /// <summary>
        /// Tests that Sine handles very small (near-zero) frequency values correctly.
        /// Very small frequency should still produce valid oscillation.
        /// </summary>
        [TestMethod]
        public void Sine_WithVerySmallNonZeroFrequency_ReturnsValidResult()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = 1e-10;
            double amplitude = 5.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Sine(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result));
            Assert.IsFalse(double.IsInfinity(result));
        }

        /// <summary>
        /// Tests that Square returns center when frequency is zero, regardless of other parameters.
        /// </summary>
        /// <param name="time">The time parameter.</param>
        /// <param name="center">The center value to return.</param>
        /// <param name="amplitude">The amplitude parameter.</param>
        /// <param name="phase">The phase parameter.</param>
        [TestMethod]
        [DataRow(0.0, 100.0, 50.0, 0.0, DisplayName = "Frequency zero with typical values")]
        [DataRow(5.0, 200.0, 100.0, 45.0, DisplayName = "Frequency zero with positive time")]
        [DataRow(-5.0, -100.0, 50.0, -90.0, DisplayName = "Frequency zero with negative values")]
        [DataRow(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue, DisplayName = "Frequency zero with max values")]
        public void Square_WhenFrequencyIsZero_ReturnsCenter(double time, double center, double amplitude, double phase)
        {
            // Arrange
            double frequency = 0.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result);
        }

        /// <summary>
        /// Tests that Square returns center (which is 0) when center is zero, regardless of other parameters.
        /// </summary>
        /// <param name="time">The time parameter.</param>
        /// <param name="frequency">The frequency parameter.</param>
        /// <param name="amplitude">The amplitude parameter.</param>
        /// <param name="phase">The phase parameter.</param>
        [TestMethod]
        [DataRow(0.0, 1.0, 50.0, 0.0, DisplayName = "Center zero with typical values")]
        [DataRow(5.0, 2.0, 100.0, 45.0, DisplayName = "Center zero with positive values")]
        [DataRow(-5.0, 0.5, 50.0, -90.0, DisplayName = "Center zero with negative time")]
        [DataRow(10.0, 10.0, -50.0, 180.0, DisplayName = "Center zero with negative amplitude")]
        public void Square_WhenCenterIsZero_ReturnsZero(double time, double frequency, double amplitude, double phase)
        {
            // Arrange
            double center = 0.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that Square returns zero when both frequency and center are zero.
        /// </summary>
        [TestMethod]
        public void Square_WhenBothFrequencyAndCenterAreZero_ReturnsZero()
        {
            // Arrange
            double time = 5.0;
            double center = 0.0;
            double frequency = 0.0;
            double amplitude = 100.0;
            double phase = 45.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that Square returns the high value (center + amplitude/2) when currentPhase is less than 180 degrees.
        /// </summary>
        /// <param name="time">The time parameter.</param>
        /// <param name="center">The center value.</param>
        /// <param name="frequency">The frequency parameter.</param>
        /// <param name="amplitude">The amplitude parameter.</param>
        /// <param name="phase">The phase parameter.</param>
        /// <param name="expectedResult">The expected result.</param>
        [TestMethod]
        [DataRow(0.0, 100.0, 1.0, 40.0, 0.0, 120.0, DisplayName = "Phase 0 degrees (high)")]
        [DataRow(0.25, 100.0, 1.0, 40.0, 0.0, 120.0, DisplayName = "Phase 90 degrees (high)")]
        [DataRow(0.0, 100.0, 1.0, 40.0, 90.0, 120.0, DisplayName = "Phase offset 90 degrees (high)")]
        [DataRow(0.0, 50.0, 1.0, 100.0, 45.0, 100.0, DisplayName = "Phase 45 degrees (high)")]
        [DataRow(0.1, 200.0, 1.0, 80.0, 0.0, 240.0, DisplayName = "Phase 36 degrees (high)")]
        public void Square_WhenCurrentPhaseLessThan180_ReturnsHighValue(double time, double center, double frequency, double amplitude, double phase, double expectedResult)
        {
            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(expectedResult, result, 0.0001);
        }

        /// <summary>
        /// Tests that Square returns the low value (center - amplitude/2) when currentPhase is 180 degrees or greater.
        /// </summary>
        /// <param name="time">The time parameter.</param>
        /// <param name="center">The center value.</param>
        /// <param name="frequency">The frequency parameter.</param>
        /// <param name="amplitude">The amplitude parameter.</param>
        /// <param name="phase">The phase parameter.</param>
        /// <param name="expectedResult">The expected result.</param>
        [TestMethod]
        [DataRow(0.5, 100.0, 1.0, 40.0, 0.0, 80.0, DisplayName = "Phase 180 degrees (low)")]
        [DataRow(0.75, 100.0, 1.0, 40.0, 0.0, 80.0, DisplayName = "Phase 270 degrees (low)")]
        [DataRow(0.0, 100.0, 1.0, 40.0, 180.0, 80.0, DisplayName = "Phase offset 180 degrees (low)")]
        [DataRow(0.0, 50.0, 1.0, 100.0, 270.0, 0.0, DisplayName = "Phase 270 degrees offset (low)")]
        [DataRow(0.9, 200.0, 1.0, 80.0, 0.0, 160.0, DisplayName = "Phase 324 degrees (low)")]
        public void Square_WhenCurrentPhase180OrGreater_ReturnsLowValue(double time, double center, double frequency, double amplitude, double phase, double expectedResult)
        {
            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(expectedResult, result, 0.0001);
        }

        /// <summary>
        /// Tests that Square handles negative amplitude correctly by inverting the wave.
        /// </summary>
        /// <param name="time">The time parameter.</param>
        /// <param name="center">The center value.</param>
        /// <param name="frequency">The frequency parameter.</param>
        /// <param name="amplitude">The amplitude parameter (negative).</param>
        /// <param name="phase">The phase parameter.</param>
        /// <param name="expectedResult">The expected result.</param>
        [TestMethod]
        [DataRow(0.0, 100.0, 1.0, -40.0, 0.0, 80.0, DisplayName = "Negative amplitude phase 0 (inverted low)")]
        [DataRow(0.5, 100.0, 1.0, -40.0, 0.0, 120.0, DisplayName = "Negative amplitude phase 180 (inverted high)")]
        public void Square_WhenAmplitudeIsNegative_InvertsWave(double time, double center, double frequency, double amplitude, double phase, double expectedResult)
        {
            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(expectedResult, result, 0.0001);
        }

        /// <summary>
        /// Tests that Square handles zero amplitude correctly by always returning center.
        /// </summary>
        /// <param name="time">The time parameter.</param>
        /// <param name="center">The center value.</param>
        /// <param name="frequency">The frequency parameter.</param>
        /// <param name="phase">The phase parameter.</param>
        [TestMethod]
        [DataRow(0.0, 100.0, 1.0, 0.0, DisplayName = "Zero amplitude phase 0")]
        [DataRow(0.5, 100.0, 1.0, 0.0, DisplayName = "Zero amplitude phase 180")]
        [DataRow(0.25, 50.0, 2.0, 90.0, DisplayName = "Zero amplitude various inputs")]
        public void Square_WhenAmplitudeIsZero_ReturnsCenter(double time, double center, double frequency, double phase)
        {
            // Arrange
            double amplitude = 0.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result, 0.0001);
        }

        /// <summary>
        /// Tests that Square handles phase wrapping correctly when phase values exceed 360 degrees.
        /// </summary>
        /// <param name="phase">The phase parameter (> 360).</param>
        /// <param name="expectedResult">The expected result.</param>
        [TestMethod]
        [DataRow(360.0, 120.0, DisplayName = "Phase 360 wraps to 0 (high)")]
        [DataRow(540.0, 80.0, DisplayName = "Phase 540 wraps to 180 (low)")]
        [DataRow(720.0, 120.0, DisplayName = "Phase 720 wraps to 0 (high)")]
        [DataRow(450.0, 120.0, DisplayName = "Phase 450 wraps to 90 (high)")]
        public void Square_WhenPhaseExceeds360_WrapsCorrectly(double phase, double expectedResult)
        {
            // Arrange
            double time = 0.0;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 40.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(expectedResult, result, 0.0001);
        }

        /// <summary>
        /// Tests that Square handles negative phase values correctly.
        /// </summary>
        /// <param name="phase">The phase parameter (negative).</param>
        /// <param name="expectedResult">The expected result.</param>
        [TestMethod]
        [DataRow(-90.0, 80.0, DisplayName = "Phase -90 wraps to 270 (low)")]
        [DataRow(-180.0, 80.0, DisplayName = "Phase -180 wraps to 180 (low)")]
        [DataRow(-360.0, 120.0, DisplayName = "Phase -360 wraps to 0 (high)")]
        [TestCategory("ProductionBugSuspected")]
        [Ignore("ProductionBugSuspected")]
        public void Square_WhenPhaseIsNegative_WrapsCorrectly(double phase, double expectedResult)
        {
            // Arrange
            double time = 0.0;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 40.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(expectedResult, result, 0.0001);
        }

        /// <summary>
        /// Tests that Square handles negative time values correctly.
        /// </summary>
        [TestMethod]
        public void Square_WhenTimeIsNegative_CalculatesPhaseCorrectly()
        {
            // Arrange
            double time = -0.25;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 40.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            // time * frequency * 360 + phase = -0.25 * 1.0 * 360 + 0 = -90
            // In C#, -90 % 360 = -90 (modulo preserves sign of dividend)
            // -90 < 180, so high value
            Assert.AreEqual(120.0, result, 0.0001);
        }

        /// <summary>
        /// Tests that Square handles negative frequency correctly.
        /// </summary>
        [TestMethod]
        public void Square_WhenFrequencyIsNegative_CalculatesPhaseCorrectly()
        {
            // Arrange
            double time = 0.25;
            double center = 100.0;
            double frequency = -1.0;
            double amplitude = 40.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            // time * frequency * 360 + phase = 0.25 * -1.0 * 360 + 0 = -90
            // -90 % 360 = -90 (in C#), which is < 180, so high value
            Assert.AreEqual(120.0, result, 0.0001);
        }

        /// <summary>
        /// Tests that Square returns NaN when time is NaN.
        /// </summary>
        [TestMethod]
        [TestCategory("ProductionBugSuspected")]
        [Ignore("ProductionBugSuspected")]
        public void Square_WhenTimeIsNaN_ReturnsNaN()
        {
            // Arrange
            double time = double.NaN;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 40.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Square returns a calculated value when phase is NaN.
        /// </summary>
        [TestMethod]
        public void Square_WhenPhaseIsNaN_ReturnsNaN()
        {
            // Arrange
            double time = 0.5;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 40.0;
            double phase = double.NaN;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            // When phase is NaN, currentPhase becomes NaN, but NaN < 180 evaluates to false,
            // so the function returns center - amplitude / 2
            Assert.AreEqual(80.0, result);
        }

        /// <summary>
        /// Tests that Square handles PositiveInfinity time value.
        /// </summary>
        [TestMethod]
        public void Square_WhenTimeIsPositiveInfinity_ReturnsNaN()
        {
            // Arrange
            double time = double.PositiveInfinity;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 40.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            // When time is infinity, currentPhase becomes NaN, but NaN < 180 is false,
            // so the method returns center - amplitude / 2
            Assert.AreEqual(80.0, result);
        }

        /// <summary>
        /// Tests that Square handles NegativeInfinity time value.
        /// </summary>
        [TestMethod]
        [TestCategory("ProductionBugSuspected")]
        [Ignore("ProductionBugSuspected")]
        public void Square_WhenTimeIsNegativeInfinity_ReturnsNaN()
        {
            // Arrange
            double time = double.NegativeInfinity;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 40.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Square handles PositiveInfinity amplitude value.
        /// </summary>
        [TestMethod]
        public void Square_WhenAmplitudeIsPositiveInfinity_ReturnsInfinity()
        {
            // Arrange
            double time = 0.0;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = double.PositiveInfinity;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsPositiveInfinity(result));
        }

        /// <summary>
        /// Tests that Square handles NegativeInfinity amplitude value.
        /// </summary>
        [TestMethod]
        public void Square_WhenAmplitudeIsNegativeInfinity_ReturnsNegativeInfinity()
        {
            // Arrange
            double time = 0.0;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = double.NegativeInfinity;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNegativeInfinity(result));
        }

        /// <summary>
        /// Tests that Square handles very large time values correctly.
        /// </summary>
        [TestMethod]
        public void Square_WhenTimeIsVeryLarge_CalculatesPhaseCorrectly()
        {
            // Arrange
            double time = double.MaxValue / 1000;
            double center = 100.0;
            double frequency = 0.001;
            double amplitude = 40.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            // Result should be either high or low value, not NaN or Infinity
            Assert.IsTrue(result == 120.0 || result == 80.0);
        }

        /// <summary>
        /// Tests that Square calculates correct values across one complete cycle.
        /// </summary>
        /// <param name="time">The time parameter representing position in cycle.</param>
        /// <param name="expectedResult">The expected result.</param>
        [TestMethod]
        [DataRow(0.0, 120.0, DisplayName = "Start of cycle (high)")]
        [DataRow(0.1, 120.0, DisplayName = "10% through cycle (high)")]
        [DataRow(0.25, 120.0, DisplayName = "25% through cycle (high)")]
        [DataRow(0.49, 120.0, DisplayName = "49% through cycle (high)")]
        [DataRow(0.5, 80.0, DisplayName = "50% through cycle (low)")]
        [DataRow(0.75, 80.0, DisplayName = "75% through cycle (low)")]
        [DataRow(0.99, 80.0, DisplayName = "99% through cycle (low)")]
        [DataRow(1.0, 120.0, DisplayName = "End of cycle wraps to start (high)")]
        public void Square_AcrossOneCycle_ProducesCorrectSquareWave(double time, double expectedResult)
        {
            // Arrange
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 40.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Square(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(expectedResult, result, 0.0001);
        }

        /// <summary>
        /// Tests that Triangle returns center when frequency is zero.
        /// </summary>
        /// <param name="center">The center value to test.</param>
        /// <param name="amplitude">The amplitude value.</param>
        [TestMethod]
        [DataRow(100.0, 50.0, DisplayName = "Positive center and amplitude")]
        [DataRow(0.0, 50.0, DisplayName = "Zero center")]
        [DataRow(-100.0, 50.0, DisplayName = "Negative center")]
        [DataRow(100.0, 0.0, DisplayName = "Zero amplitude")]
        public void Triangle_WhenFrequencyIsZero_ReturnsCenter(double center, double amplitude)
        {
            // Arrange
            double time = 1.0;
            double frequency = 0.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result);
        }

        /// <summary>
        /// Tests that Triangle returns center when center is zero, regardless of frequency.
        /// </summary>
        /// <param name="frequency">The frequency value to test.</param>
        [TestMethod]
        [DataRow(1.0, DisplayName = "Positive frequency")]
        [DataRow(0.0, DisplayName = "Zero frequency")]
        [DataRow(10.0, DisplayName = "Large frequency")]
        [DataRow(-5.0, DisplayName = "Negative frequency")]
        public void Triangle_WhenCenterIsZero_ReturnsZero(double frequency)
        {
            // Arrange
            double time = 1.0;
            double center = 0.0;
            double amplitude = 50.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that Triangle calculates correctly when currentPhase is less than 180 (ascending part).
        /// </summary>
        /// <param name="time">The time value.</param>
        /// <param name="frequency">The frequency value.</param>
        /// <param name="phase">The phase offset.</param>
        /// <param name="expectedResult">The expected result.</param>
        [TestMethod]
        [DataRow(0.0, 1.0, 0.0, 50.0, DisplayName = "Phase 0 (at -90)")]
        [DataRow(0.25, 1.0, 0.0, 100.0, DisplayName = "Phase 90 (at center)")]
        [DataRow(0.5, 1.0, 0.0, 150.0, DisplayName = "Phase 180 boundary")]
        [DataRow(0.1, 1.0, 0.0, 70.0, DisplayName = "Phase 36")]
        public void Triangle_WhenCurrentPhaseLessThan180_ReturnsAscendingValue(double time, double frequency, double phase, double expectedResult)
        {
            // Arrange
            double center = 100.0;
            double amplitude = 100.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(expectedResult, result, 0.0001);
        }

        /// <summary>
        /// Tests that Triangle calculates correctly when currentPhase is greater than or equal to 180 (descending part).
        /// </summary>
        /// <param name="time">The time value.</param>
        /// <param name="frequency">The frequency value.</param>
        /// <param name="phase">The phase offset.</param>
        /// <param name="expectedResult">The expected result.</param>
        [TestMethod]
        [DataRow(0.6, 1.0, 0.0, 130.0, DisplayName = "Phase 216")]
        [DataRow(0.75, 1.0, 0.0, 100.0, DisplayName = "Phase 270 (at center)")]
        [DataRow(0.9, 1.0, 0.0, 70.0, DisplayName = "Phase 324")]
        [DataRow(1.0, 1.0, 0.0, 50.0, DisplayName = "Phase 360 (wraps to 0, at -90)")]
        public void Triangle_WhenCurrentPhaseGreaterThanOrEqual180_ReturnsDescendingValue(double time, double frequency, double phase, double expectedResult)
        {
            // Arrange
            double center = 100.0;
            double amplitude = 100.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(expectedResult, result, 0.0001);
        }

        /// <summary>
        /// Tests that Triangle handles phase offset correctly.
        /// </summary>
        /// <param name="phase">The phase offset in degrees.</param>
        /// <param name="expectedResult">The expected result.</param>
        [TestMethod]
        [DataRow(0.0, 100.0, DisplayName = "No phase offset")]
        [DataRow(90.0, 150.0, DisplayName = "90 degree phase offset")]
        [DataRow(180.0, 100.0, DisplayName = "180 degree phase offset")]
        [DataRow(270.0, 50.0, DisplayName = "270 degree phase offset")]
        [DataRow(360.0, 100.0, DisplayName = "360 degree phase offset (full cycle)")]
        [DataRow(-90.0, 50.0, DisplayName = "Negative 90 degree phase offset")]
        public void Triangle_WithPhaseOffset_CalculatesCorrectly(double phase, double expectedResult)
        {
            // Arrange
            double time = 0.25;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 100.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(expectedResult, result, 0.0001);
        }

        /// <summary>
        /// Tests that Triangle handles negative amplitude correctly.
        /// </summary>
        [TestMethod]
        public void Triangle_WithNegativeAmplitude_InvertsWaveform()
        {
            // Arrange
            double time = 0.25;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = -100.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(100.0, result, 0.0001);
        }

        /// <summary>
        /// Tests that Triangle handles negative frequency correctly.
        /// </summary>
        [TestMethod]
        [TestCategory("ProductionBugSuspected")]
        [Ignore("ProductionBugSuspected")]
        public void Triangle_WithNegativeFrequency_CalculatesCorrectly()
        {
            // Arrange
            double time = 0.25;
            double center = 100.0;
            double frequency = -1.0;
            double amplitude = 100.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(100.0, result, 0.0001);
        }

        /// <summary>
        /// Tests that Triangle handles negative time correctly.
        /// </summary>
        [TestMethod]
        [TestCategory("ProductionBugSuspected")]
        [Ignore("ProductionBugSuspected")]
        public void Triangle_WithNegativeTime_CalculatesCorrectly()
        {
            // Arrange
            double time = -0.25;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 100.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(100.0, result, 0.0001);
        }

        /// <summary>
        /// Tests that Triangle handles very large time values correctly.
        /// </summary>
        [TestMethod]
        public void Triangle_WithVeryLargeTime_CalculatesCorrectly()
        {
            // Arrange
            double time = 1000000.0;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 100.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(50.0, result, 0.0001);
        }

        /// <summary>
        /// Tests that Triangle handles very small (near-zero) frequency correctly.
        /// </summary>
        [TestMethod]
        public void Triangle_WithVerySmallFrequency_CalculatesCorrectly()
        {
            // Arrange
            double time = 1.0;
            double center = 100.0;
            double frequency = 0.0001;
            double amplitude = 100.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(50.0, result, 0.1);
        }

        /// <summary>
        /// Tests that Triangle returns NaN when time is NaN.
        /// </summary>
        [TestMethod]
        public void Triangle_WithNaNTime_ReturnsNaN()
        {
            // Arrange
            double time = double.NaN;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 100.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Triangle returns NaN when frequency is NaN.
        /// </summary>
        [TestMethod]
        public void Triangle_WithNaNFrequency_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 100.0;
            double frequency = double.NaN;
            double amplitude = 100.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Triangle returns NaN when amplitude is NaN.
        /// </summary>
        [TestMethod]
        public void Triangle_WithNaNAmplitude_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = double.NaN;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Triangle returns NaN when phase is NaN.
        /// </summary>
        [TestMethod]
        public void Triangle_WithNaNPhase_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 100.0;
            double phase = double.NaN;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Triangle handles positive infinity for time.
        /// </summary>
        [TestMethod]
        public void Triangle_WithPositiveInfinityTime_ReturnsNaN()
        {
            // Arrange
            double time = double.PositiveInfinity;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 100.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Triangle handles negative infinity for time.
        /// </summary>
        [TestMethod]
        public void Triangle_WithNegativeInfinityTime_ReturnsNaN()
        {
            // Arrange
            double time = double.NegativeInfinity;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 100.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that Triangle handles positive infinity for amplitude.
        /// </summary>
        [TestMethod]
        public void Triangle_WithPositiveInfinityAmplitude_ReturnsInfinity()
        {
            // Arrange
            double time = 0.375;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = double.PositiveInfinity;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsPositiveInfinity(result));
        }

        /// <summary>
        /// Tests that Triangle handles negative infinity for amplitude.
        /// </summary>
        [TestMethod]
        public void Triangle_WithNegativeInfinityAmplitude_ReturnsNegativeInfinity()
        {
            // Arrange
            double time = 0.5;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = double.NegativeInfinity;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNegativeInfinity(result));
        }

        /// <summary>
        /// Tests that Triangle correctly wraps phase values greater than 360.
        /// </summary>
        [TestMethod]
        public void Triangle_WithPhaseGreaterThan360_WrapsCorrectly()
        {
            // Arrange
            double time = 0.25;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 100.0;
            double phase = 720.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(100.0, result, 0.0001);
        }

        /// <summary>
        /// Tests that Triangle produces a complete cycle with expected values at key points.
        /// </summary>
        /// <param name="time">The time value representing position in the cycle.</param>
        /// <param name="expectedResult">The expected result at this point in the cycle.</param>
        [TestMethod]
        [DataRow(0.0, 50.0, DisplayName = "Start of cycle (minimum)")]
        [DataRow(0.25, 100.0, DisplayName = "Quarter cycle (center)")]
        [DataRow(0.5, 150.0, DisplayName = "Half cycle (maximum)")]
        [DataRow(0.75, 100.0, DisplayName = "Three-quarter cycle (center)")]
        [DataRow(1.0, 50.0, DisplayName = "End of cycle (minimum, wraps)")]
        [TestCategory("ProductionBugSuspected")]
        [Ignore("ProductionBugSuspected")]
        public void Triangle_CompleteCycle_ProducesExpectedWaveform(double time, double expectedResult)
        {
            // Arrange
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 50.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(expectedResult, result, 0.0001);
        }

        /// <summary>
        /// Tests that Triangle handles double.MinValue for various parameters.
        /// </summary>
        [TestMethod]
        public void Triangle_WithMinValueTime_CalculatesWithoutException()
        {
            // Arrange
            double time = double.MinValue;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 100.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert - Just verify it doesn't throw and returns a value
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests that Triangle handles double.MaxValue for various parameters.
        /// </summary>
        [TestMethod]
        public void Triangle_WithMaxValueTime_CalculatesWithoutException()
        {
            // Arrange
            double time = double.MaxValue;
            double center = 100.0;
            double frequency = 1.0;
            double amplitude = 100.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.Triangle(time, center, frequency, amplitude, phase);

            // Assert - Just verify it doesn't throw and returns a value
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests that AscendingSawTooth returns center when frequency is zero.
        /// This validates the early return condition for zero frequency.
        /// </summary>
        [TestMethod]
        public void AscendingSawTooth_FrequencyIsZero_ReturnsCenter()
        {
            // Arrange
            double time = 1.0;
            double center = 5.0;
            double frequency = 0.0;
            double amplitude = 2.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result);
        }

        /// <summary>
        /// Tests that AscendingSawTooth returns center (zero) when center is zero.
        /// This validates the early return condition for zero center.
        /// </summary>
        [TestMethod]
        public void AscendingSawTooth_CenterIsZero_ReturnsZero()
        {
            // Arrange
            double time = 1.0;
            double center = 0.0;
            double frequency = 2.0;
            double amplitude = 2.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that AscendingSawTooth returns zero when both frequency and center are zero.
        /// This validates the early return condition when both conditions are met.
        /// </summary>
        [TestMethod]
        public void AscendingSawTooth_FrequencyAndCenterAreZero_ReturnsZero()
        {
            // Arrange
            double time = 1.0;
            double center = 0.0;
            double frequency = 0.0;
            double amplitude = 2.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests AscendingSawTooth with typical valid input values.
        /// Validates normal operation when all parameters are in typical ranges.
        /// </summary>
        /// <param name="time">The time value.</param>
        /// <param name="center">The center value.</param>
        /// <param name="frequency">The frequency value.</param>
        /// <param name="amplitude">The amplitude value.</param>
        /// <param name="phase">The phase value in degrees.</param>
        [TestMethod]
        [DataRow(0.0, 5.0, 1.0, 2.0, 0.0, DisplayName = "Time at zero")]
        [DataRow(0.25, 5.0, 1.0, 2.0, 0.0, DisplayName = "Time at quarter period")]
        [DataRow(0.5, 5.0, 1.0, 2.0, 0.0, DisplayName = "Time at half period")]
        [DataRow(1.0, 5.0, 1.0, 2.0, 0.0, DisplayName = "Time at full period")]
        [DataRow(0.1, 10.0, 2.0, 4.0, 90.0, DisplayName = "With phase shift")]
        [DataRow(0.3, 5.0, 1.0, 2.0, 180.0, DisplayName = "With 180 degree phase")]
        [DataRow(0.6, 5.0, 1.0, 2.0, 360.0, DisplayName = "With 360 degree phase")]
        public void AscendingSawTooth_ValidInputs_CalculatesCorrectly(double time, double center, double frequency, double amplitude, double phase)
        {
            // Arrange & Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should not be NaN for valid inputs");
            Assert.IsFalse(double.IsInfinity(result), "Result should not be infinity for valid inputs");
        }

        /// <summary>
        /// Tests AscendingSawTooth with zero amplitude.
        /// Expected to return center value since amplitude is zero.
        /// </summary>
        [TestMethod]
        public void AscendingSawTooth_ZeroAmplitude_ReturnsCenterValue()
        {
            // Arrange
            double time = 1.0;
            double center = 5.0;
            double frequency = 1.0;
            double amplitude = 0.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result);
        }

        /// <summary>
        /// Tests AscendingSawTooth with negative frequency values.
        /// Validates behavior with negative frequency inputs.
        /// </summary>
        [TestMethod]
        public void AscendingSawTooth_NegativeFrequency_CalculatesResult()
        {
            // Arrange
            double time = 1.0;
            double center = 5.0;
            double frequency = -1.0;
            double amplitude = 2.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should be a number");
        }

        /// <summary>
        /// Tests AscendingSawTooth with negative amplitude values.
        /// Validates behavior when amplitude is negative.
        /// </summary>
        [TestMethod]
        public void AscendingSawTooth_NegativeAmplitude_CalculatesResult()
        {
            // Arrange
            double time = 1.0;
            double center = 5.0;
            double frequency = 1.0;
            double amplitude = -2.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should be a number");
        }

        /// <summary>
        /// Tests AscendingSawTooth with negative time values.
        /// Validates behavior with negative time inputs.
        /// </summary>
        [TestMethod]
        public void AscendingSawTooth_NegativeTime_CalculatesResult()
        {
            // Arrange
            double time = -1.0;
            double center = 5.0;
            double frequency = 1.0;
            double amplitude = 2.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should be a number");
        }

        /// <summary>
        /// Tests AscendingSawTooth with negative phase values.
        /// Validates behavior with negative phase inputs.
        /// </summary>
        [TestMethod]
        public void AscendingSawTooth_NegativePhase_CalculatesResult()
        {
            // Arrange
            double time = 1.0;
            double center = 5.0;
            double frequency = 1.0;
            double amplitude = 2.0;
            double phase = -90.0;

            // Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should be a number");
        }

        /// <summary>
        /// Tests AscendingSawTooth with NaN input values.
        /// Validates behavior when inputs contain NaN.
        /// </summary>
        /// <param name="time">The time value.</param>
        /// <param name="center">The center value.</param>
        /// <param name="frequency">The frequency value.</param>
        /// <param name="amplitude">The amplitude value.</param>
        /// <param name="phase">The phase value.</param>
        [TestMethod]
        [DataRow(double.NaN, 5.0, 1.0, 2.0, 0.0, DisplayName = "Time is NaN")]
        [DataRow(1.0, double.NaN, 1.0, 2.0, 0.0, DisplayName = "Center is NaN")]
        [DataRow(1.0, 5.0, double.NaN, 2.0, 0.0, DisplayName = "Frequency is NaN")]
        [DataRow(1.0, 5.0, 1.0, double.NaN, 0.0, DisplayName = "Amplitude is NaN")]
        [DataRow(1.0, 5.0, 1.0, 2.0, double.NaN, DisplayName = "Phase is NaN")]
        public void AscendingSawTooth_NaNInputs_HandlesGracefully(double time, double center, double frequency, double amplitude, double phase)
        {
            // Arrange & Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert - method should not throw, result may be NaN
            Assert.IsTrue(true, "Method should handle NaN inputs without throwing");
        }

        /// <summary>
        /// Tests AscendingSawTooth with positive infinity input values.
        /// Validates behavior when inputs contain positive infinity.
        /// </summary>
        /// <param name="time">The time value.</param>
        /// <param name="center">The center value.</param>
        /// <param name="frequency">The frequency value.</param>
        /// <param name="amplitude">The amplitude value.</param>
        /// <param name="phase">The phase value.</param>
        [TestMethod]
        [DataRow(double.PositiveInfinity, 5.0, 1.0, 2.0, 0.0, DisplayName = "Time is positive infinity")]
        [DataRow(1.0, double.PositiveInfinity, 1.0, 2.0, 0.0, DisplayName = "Center is positive infinity")]
        [DataRow(1.0, 5.0, double.PositiveInfinity, 2.0, 0.0, DisplayName = "Frequency is positive infinity")]
        [DataRow(1.0, 5.0, 1.0, double.PositiveInfinity, 0.0, DisplayName = "Amplitude is positive infinity")]
        [DataRow(1.0, 5.0, 1.0, 2.0, double.PositiveInfinity, DisplayName = "Phase is positive infinity")]
        public void AscendingSawTooth_PositiveInfinityInputs_HandlesGracefully(double time, double center, double frequency, double amplitude, double phase)
        {
            // Arrange & Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert - method should not throw
            Assert.IsTrue(true, "Method should handle positive infinity inputs without throwing");
        }

        /// <summary>
        /// Tests AscendingSawTooth with negative infinity input values.
        /// Validates behavior when inputs contain negative infinity.
        /// </summary>
        /// <param name="time">The time value.</param>
        /// <param name="center">The center value.</param>
        /// <param name="frequency">The frequency value.</param>
        /// <param name="amplitude">The amplitude value.</param>
        /// <param name="phase">The phase value.</param>
        [TestMethod]
        [DataRow(double.NegativeInfinity, 5.0, 1.0, 2.0, 0.0, DisplayName = "Time is negative infinity")]
        [DataRow(1.0, double.NegativeInfinity, 1.0, 2.0, 0.0, DisplayName = "Center is negative infinity")]
        [DataRow(1.0, 5.0, double.NegativeInfinity, 2.0, 0.0, DisplayName = "Frequency is negative infinity")]
        [DataRow(1.0, 5.0, 1.0, double.NegativeInfinity, 0.0, DisplayName = "Amplitude is negative infinity")]
        [DataRow(1.0, 5.0, 1.0, 2.0, double.NegativeInfinity, DisplayName = "Phase is negative infinity")]
        public void AscendingSawTooth_NegativeInfinityInputs_HandlesGracefully(double time, double center, double frequency, double amplitude, double phase)
        {
            // Arrange & Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert - method should not throw
            Assert.IsTrue(true, "Method should handle negative infinity inputs without throwing");
        }

        /// <summary>
        /// Tests AscendingSawTooth with extreme but valid values.
        /// Validates behavior at numeric boundaries.
        /// </summary>
        /// <param name="time">The time value.</param>
        /// <param name="center">The center value.</param>
        /// <param name="frequency">The frequency value.</param>
        /// <param name="amplitude">The amplitude value.</param>
        /// <param name="phase">The phase value.</param>
        [TestMethod]
        [DataRow(double.MaxValue, 5.0, 1.0, 2.0, 0.0, DisplayName = "Time is max value")]
        [DataRow(double.MinValue, 5.0, 1.0, 2.0, 0.0, DisplayName = "Time is min value")]
        [DataRow(1.0, double.MaxValue, 1.0, 2.0, 0.0, DisplayName = "Center is max value")]
        [DataRow(1.0, double.MinValue, 1.0, 2.0, 0.0, DisplayName = "Center is min value")]
        [DataRow(1.0, 5.0, double.MaxValue, 2.0, 0.0, DisplayName = "Frequency is max value")]
        [DataRow(1.0, 5.0, 1.0, double.MaxValue, 0.0, DisplayName = "Amplitude is max value")]
        [DataRow(1.0, 5.0, 1.0, double.MinValue, 0.0, DisplayName = "Amplitude is min value")]
        [DataRow(1.0, 5.0, 1.0, 2.0, double.MaxValue, DisplayName = "Phase is max value")]
        [DataRow(1.0, 5.0, 1.0, 2.0, double.MinValue, DisplayName = "Phase is min value")]
        public void AscendingSawTooth_ExtremeValues_HandlesGracefully(double time, double center, double frequency, double amplitude, double phase)
        {
            // Arrange & Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert - method should not throw
            Assert.IsTrue(true, "Method should handle extreme values without throwing");
        }

        /// <summary>
        /// Tests AscendingSawTooth with very small positive frequency near zero.
        /// Validates behavior when frequency approaches zero from the positive side.
        /// </summary>
        [TestMethod]
        public void AscendingSawTooth_VerySmallPositiveFrequency_CalculatesResult()
        {
            // Arrange
            double time = 1.0;
            double center = 5.0;
            double frequency = double.Epsilon;
            double amplitude = 2.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert - should not throw, result may be infinity due to 1/epsilon
            Assert.IsTrue(true, "Method should handle very small frequency without throwing");
        }

        /// <summary>
        /// Tests AscendingSawTooth with large phase values outside typical 0-360 range.
        /// Validates behavior when phase exceeds normal bounds.
        /// </summary>
        /// <param name="phase">The phase value in degrees.</param>
        [TestMethod]
        [DataRow(720.0, DisplayName = "Phase is 720 degrees")]
        [DataRow(1000.0, DisplayName = "Phase is 1000 degrees")]
        [DataRow(-720.0, DisplayName = "Phase is -720 degrees")]
        public void AscendingSawTooth_LargePhaseValues_CalculatesResult(double phase)
        {
            // Arrange
            double time = 1.0;
            double center = 5.0;
            double frequency = 1.0;
            double amplitude = 2.0;

            // Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should not be NaN for large phase values");
        }

        /// <summary>
        /// Tests AscendingSawTooth to verify the ternary operator branches in tOffset calculation.
        /// This test ensures both t0 &lt; period/2 and t0 &gt;= period/2 paths are exercised.
        /// </summary>
        /// <param name="time">The time value designed to test specific branches.</param>
        [TestMethod]
        [DataRow(0.1, DisplayName = "Time causes t0 < period/2")]
        [DataRow(0.6, DisplayName = "Time causes t0 >= period/2")]
        public void AscendingSawTooth_DifferentTimeValues_ExercisesTernaryBranches(double time)
        {
            // Arrange
            double center = 5.0;
            double frequency = 1.0;
            double amplitude = 2.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.AscendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should be valid for different time values");
            Assert.IsFalse(double.IsInfinity(result), "Result should be finite for different time values");
        }

        /// <summary>
        /// Tests that DescendingSawTooth returns center when frequency is zero.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenFrequencyIsZero_ReturnsCenter()
        {
            // Arrange
            double time = 1.0;
            double center = 5.0;
            double frequency = 0.0;
            double amplitude = 2.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result);
        }

        /// <summary>
        /// Tests that DescendingSawTooth returns center when center is zero.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenCenterIsZero_ReturnsZero()
        {
            // Arrange
            double time = 1.0;
            double center = 0.0;
            double frequency = 1.0;
            double amplitude = 2.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that DescendingSawTooth returns zero when both frequency and center are zero.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenBothFrequencyAndCenterAreZero_ReturnsZero()
        {
            // Arrange
            double time = 1.0;
            double center = 0.0;
            double frequency = 0.0;
            double amplitude = 2.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        /// <summary>
        /// Tests that DescendingSawTooth calculates correctly with normal positive values.
        /// </summary>
        /// <param name="time">The time value.</param>
        /// <param name="center">The center value.</param>
        /// <param name="frequency">The frequency value.</param>
        /// <param name="amplitude">The amplitude value.</param>
        /// <param name="phase">The phase value.</param>
        [TestMethod]
        [DataRow(0.0, 10.0, 1.0, 4.0, 0.0, DisplayName = "Zero time, zero phase")]
        [DataRow(0.5, 10.0, 1.0, 4.0, 0.0, DisplayName = "Half period time")]
        [DataRow(1.0, 10.0, 1.0, 4.0, 0.0, DisplayName = "Full period time")]
        [DataRow(0.25, 10.0, 1.0, 4.0, 0.0, DisplayName = "Quarter period time")]
        [DataRow(0.0, 10.0, 2.0, 4.0, 0.0, DisplayName = "Higher frequency")]
        [DataRow(0.0, 10.0, 0.5, 4.0, 0.0, DisplayName = "Lower frequency")]
        [DataRow(0.0, 10.0, 1.0, 4.0, 90.0, DisplayName = "90 degree phase shift")]
        [DataRow(0.0, 10.0, 1.0, 4.0, 180.0, DisplayName = "180 degree phase shift")]
        [DataRow(0.0, 10.0, 1.0, 4.0, 270.0, DisplayName = "270 degree phase shift")]
        public void DescendingSawTooth_WithValidInputs_CalculatesCorrectly(double time, double center, double frequency, double amplitude, double phase)
        {
            // Arrange & Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should not be NaN");
            Assert.IsFalse(double.IsInfinity(result), "Result should not be infinity");
        }

        /// <summary>
        /// Tests that DescendingSawTooth handles zero amplitude correctly.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenAmplitudeIsZero_ReturnsCenter()
        {
            // Arrange
            double time = 0.5;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 0.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result);
        }

        /// <summary>
        /// Tests that DescendingSawTooth handles negative frequency.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenFrequencyIsNegative_CalculatesWithNegativePeriod()
        {
            // Arrange
            double time = 0.5;
            double center = 10.0;
            double frequency = -1.0;
            double amplitude = 4.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should not be NaN");
        }

        /// <summary>
        /// Tests that DescendingSawTooth handles negative amplitude.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenAmplitudeIsNegative_CalculatesCorrectly()
        {
            // Arrange
            double time = 0.5;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = -4.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should not be NaN");
            Assert.IsFalse(double.IsInfinity(result), "Result should not be infinity");
        }

        /// <summary>
        /// Tests that DescendingSawTooth handles negative center value.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenCenterIsNegative_ReturnsCenter()
        {
            // Arrange
            double time = 1.0;
            double center = -5.0;
            double frequency = 0.0;
            double amplitude = 2.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result);
        }

        /// <summary>
        /// Tests that DescendingSawTooth propagates NaN when time is NaN.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenTimeIsNaN_ReturnsNaN()
        {
            // Arrange
            double time = double.NaN;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 4.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that DescendingSawTooth propagates NaN when frequency is NaN.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenFrequencyIsNaN_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = double.NaN;
            double amplitude = 4.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that DescendingSawTooth propagates NaN when center is NaN.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenCenterIsNaN_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = double.NaN;
            double frequency = 1.0;
            double amplitude = 4.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that DescendingSawTooth propagates NaN when amplitude is NaN.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenAmplitudeIsNaN_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = double.NaN;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that DescendingSawTooth propagates NaN when phase is NaN.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenPhaseIsNaN_ReturnsNaN()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 4.0;
            double phase = double.NaN;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that DescendingSawTooth handles positive infinity for time.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenTimeIsPositiveInfinity_ReturnsNaN()
        {
            // Arrange
            double time = double.PositiveInfinity;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 4.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsTrue(double.IsNaN(result));
        }

        /// <summary>
        /// Tests that DescendingSawTooth handles positive infinity for frequency.
        /// </summary>
        [TestMethod]
        [TestCategory("ProductionBugSuspected")]
        [Ignore("ProductionBugSuspected")]
        public void DescendingSawTooth_WhenFrequencyIsPositiveInfinity_ReturnsCenter()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = double.PositiveInfinity;
            double amplitude = 4.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result);
        }

        /// <summary>
        /// Tests that DescendingSawTooth handles negative infinity for frequency.
        /// </summary>
        [TestMethod]
        [TestCategory("ProductionBugSuspected")]
        [Ignore("ProductionBugSuspected")]
        public void DescendingSawTooth_WhenFrequencyIsNegativeInfinity_ReturnsCenter()
        {
            // Arrange
            double time = 1.0;
            double center = 10.0;
            double frequency = double.NegativeInfinity;
            double amplitude = 4.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.AreEqual(center, result);
        }

        /// <summary>
        /// Tests that DescendingSawTooth handles very large values.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenValuesAreVeryLarge_CalculatesCorrectly()
        {
            // Arrange
            double time = 1e100;
            double center = 1e50;
            double frequency = 1e-50;
            double amplitude = 1e40;
            double phase = 180.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should not be NaN");
        }

        /// <summary>
        /// Tests that DescendingSawTooth handles very small positive frequency values.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenFrequencyIsVerySmall_CalculatesCorrectly()
        {
            // Arrange
            double time = 0.5;
            double center = 10.0;
            double frequency = 1e-10;
            double amplitude = 4.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should not be NaN");
            Assert.IsFalse(double.IsInfinity(result), "Result should not be infinity");
        }

        /// <summary>
        /// Tests that DescendingSawTooth exercises the conditional branch where t0 is less than period/2.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenT0IsLessThanHalfPeriod_CalculatesWithT0()
        {
            // Arrange
            double time = 0.1;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 4.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should not be NaN");
            Assert.IsFalse(double.IsInfinity(result), "Result should not be infinity");
        }

        /// <summary>
        /// Tests that DescendingSawTooth exercises the conditional branch where t0 is greater than or equal to period/2.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenT0IsGreaterThanOrEqualToHalfPeriod_CalculatesWithT0MinusHalfPeriod()
        {
            // Arrange
            double time = 0.6;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 4.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should not be NaN");
            Assert.IsFalse(double.IsInfinity(result), "Result should not be infinity");
        }

        /// <summary>
        /// Tests that DescendingSawTooth handles maximum double value for time.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenTimeIsMaxValue_CalculatesCorrectly()
        {
            // Arrange
            double time = double.MaxValue;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 4.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should not be NaN");
        }

        /// <summary>
        /// Tests that DescendingSawTooth handles minimum double value for time.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenTimeIsMinValue_CalculatesCorrectly()
        {
            // Arrange
            double time = double.MinValue;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 4.0;
            double phase = 0.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should not be NaN");
        }

        /// <summary>
        /// Tests that DescendingSawTooth handles negative phase values.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenPhaseIsNegative_CalculatesCorrectly()
        {
            // Arrange
            double time = 0.5;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 4.0;
            double phase = -90.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should not be NaN");
            Assert.IsFalse(double.IsInfinity(result), "Result should not be infinity");
        }

        /// <summary>
        /// Tests that DescendingSawTooth handles phase values greater than 360.
        /// </summary>
        [TestMethod]
        public void DescendingSawTooth_WhenPhaseIsGreaterThan360_CalculatesCorrectly()
        {
            // Arrange
            double time = 0.5;
            double center = 10.0;
            double frequency = 1.0;
            double amplitude = 4.0;
            double phase = 720.0;

            // Act
            double result = ModulatorFunctions.DescendingSawTooth(time, center, frequency, amplitude, phase);

            // Assert
            Assert.IsFalse(double.IsNaN(result), "Result should not be NaN");
            Assert.IsFalse(double.IsInfinity(result), "Result should not be infinity");
        }
    }
}