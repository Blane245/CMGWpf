using CMGWpf.Model.Generators;
using System.Diagnostics;

namespace CMGWpf.Utilities
{
    public static class StochasticUtilities
    {
        /// <summary>
        /// Create a composition for a stochastic generator based on the stochastic music theory of Iannis Xenakis. All that is needed from the generator is the number of time cells, the number of voices, and the average number of events per time cell. 
        /// </summary>
        /// <param name="generator">The stochastic generator to use for creating the composition.</param>
        /// <returns>A composition generated based on the stochastic music theory of Iannis Xenakis.</returns>
        public static Composition BuildComposition(Stochastic generator)
        {
            generator.InitializeComposition();
            FastRandom rN = generator.CompositionRn;
            generator.CompositionRn = rN;
            int nColumns = generator.Voices.Count;
            int nRows = generator.NumberOfTimeCells;
            double lambda = generator.Lambda;

            // develop the distribution of the frequency among the cells (N)
            (int[] N, _) = BuildCellDistribution(nColumns * nRows, lambda);
            DebugLog.Write($"BuildComposition: Cell Frequencies for {nRows * nColumns}, lambda = {lambda}:");
            foreach (var item in N)
            {
                Debug.Write($"{item}, ");
            }

            // initialize the composition with -1 counts showing that no cells have been populated yet
            Composition composition = new int[nRows][];
            for (int i = 0; i < nRows; i++)
            {
                composition[i] = new int[nColumns];
                for (int j = 0; j < nColumns; j++)
                {
                    composition[i][j] = -1;
                }
            }

            // construct an array that provides an urn to draw out random row numbers.
            int[] rowUrn = [];
            int rowPick = 0;
            // loop through the cell distribution table by occurrence (i) to determine the distribution of the cells in each row
            foreach (var (cellCount, i) in N.Select((v, i) => (v, i)))
            {
                if (i == 0 || cellCount == 0) { } // skip the zero count and a distribution point with no occurrences
                else
                {
                    // get the distribution within a row
                    DebugLog.Write($"BuildComposition: allocating {cellCount}, cells to row with {i} events");
                    (int[] Nc, _) = BuildCellDistribution(cellCount, (double)cellCount / (double)nRows);
                    DebugLog.Write($"BuildComposition: frequency distribution for cell count, lambda ({cellCount},{(double)cellCount / (double)nRows})"); foreach (var item in Nc) { Debug.Write($"{item},"); }
                    ;

                    // get a randomized list of rows to draw row from 
                    rowUrn = RandomizeIntegers(nRows, rN);
                    rowPick = 0;
                    DebugLog.Write($"New Row Urn:"); for (int iUrn = 0; iUrn < rowUrn.Length; iUrn++) { Debug.Write($"{rowUrn[iUrn]}, "); }
                    ; DebugLog.Write("");
                    foreach (var (rowCount, frequency) in Nc.Select((v, i) => (v, i)))
                    {
                        if (frequency == 0) continue;
                        DebugLog.Write($"BuildComposition: processing {rowCount} rows needing {frequency} cells");
                        for (int iRow = 0; iRow < rowCount && rowPick < rowUrn.Length; iRow++)
                        {
                            DebugLog.Write($"BuildComposition: processing row {rowPick} of {rowCount} rows with event frequency {frequency}");
                            // find a row that contains at least frequency cells available for assignment and but that list in an urn 
                            int[] cellUrn = [];
                            bool rowFound = false;
                            int nRow = -1;
                            while (!rowFound && rowPick < nRows)
                            {
                                nRow = rowUrn[rowPick];
                                int nCells = 0;
                                cellUrn = [];
                                foreach (var (value, iCell) in composition[nRow].Select((v, i) => (v, i)))
                                {
                                    if (value < 0)
                                    {
                                        nCells++;
                                        cellUrn = [.. cellUrn, iCell];
                                    }
                                }
                                if (nCells >= frequency) rowFound = true;
                                else rowPick++;
                            }
                            if (rowPick < nRows)
                            {
                                DebugLog.Write($"BuildComposition: found row {nRow} having {cellUrn.Length} available cells");
                                // randomize the cellUrn so we can pick the first available cell randomly
                                cellUrn = RandomizeIntegers(cellUrn, rN);
                                // there are at least frequency available cells, so place them in random columns by drawing from the randomized cellUrn
                                for (int k = 0; k < frequency; k++)
                                {
                                    int nColumn = cellUrn[k];
                                    DebugLog.Write($"BuildComposition: assigning {i} events to row, col ({nRow},{nColumn})");
                                    composition[nRow][nColumn] = i;
                                }
                                rowPick++;
                            } // else no row was found with enough cells. We are overpopulated. Just ignore this condition for now.
                              //if (rowPick >= nRows) throw new Exception($"Event density is too large for the ensemble and time cell counts.");
                              //}
                        }
                    }

                }
            }

            // finally place at least one cloud in any row that has none and mark all unallocated cells as having zero clouds. Making 1 cloud in each row prevents large silent periods
            for (int i = 0; i < nRows; i++)
            {
                bool foundRow = true;
                for (int j = 0; j < nColumns; j++)
                {
                    if (composition[i][j] > 0) { foundRow = false; }
                    else composition[i][j] = 0;
                }
                if (foundRow)
                {
                    //pick a random column and put a cloud there
                    int column = (int)Math.Floor(rN.NextDouble() * nColumns);
                    DebugLog.Write($"BuildComposition: putting a single cloud in row, column ({i}, {column})");
                    composition[i][column] = 1;
                }
            }
            return composition;

        }
        private static (int[], double[]) BuildCellDistribution(int cellCount, double lambda)
        {
            int[] N = [];
            double[] D = [];
            double thisProbability = Poisson(0, lambda);
            double thisCellCount = thisProbability * cellCount;
            N = [.. N, (int)Math.Round(thisCellCount)];
            D = [.. D, thisProbability];
            double sumP = thisProbability;
            int k = 0;
            while (sumP < 0.999)
            {
                k++;
                thisProbability = Poisson(k, lambda);
                thisCellCount = thisProbability * cellCount;
                sumP += thisProbability;
                N = [.. N, (int)Math.Round(thisCellCount)];
                D = [.. D, thisProbability];
            }
            return (N, D);
        }
        private static long Factorial(int k)
        {
            if (k <= 1) return 1;
            long result = 1;
            for (int i = 2; i <= k; i++)
            {
                result *= i;
            }
            return result;
        }
        private static double Poisson(int k, double lambda)
        {
            return Math.Exp(-lambda) * Math.Pow(lambda, k) / Factorial(k);
        }

        // create an array of the integers from 0 to n-1 and randomize it
        private static int[] RandomizeIntegers(int n, FastRandom rN)
        {
            int[] arr = new int[n]; for (int i = 0; i < n; i++) { arr[i] = i; }
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = (int)Math.Floor(rN.NextDouble() * (i + 1));
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            return arr;
        }

        // randomize an existing array of integers
        private static int[] RandomizeIntegers(int[] arr, FastRandom rN)
        {
            int[] result = (int[])arr.Clone();
            for (int i = result.Length - 1; i > 0; i--)
            {
                int j = (int)Math.Floor(rN.NextDouble() * (i + 1));
                (result[i], result[j]) = (result[j], result[i]);
            }
            return result;
        }

    }
}
