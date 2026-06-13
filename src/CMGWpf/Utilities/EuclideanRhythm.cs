namespace CMGWpf.Utilities
{
    public static class EuclideanRhythm
    {
        /// <summary>
        /// `Get` generates a Euclidean rhythm pattern based on the specified parameters. It creates a sequence of 1s (representing "on" notes) and 0s (representing "off" notes) that are distributed as evenly as possible across the total number of notes. The `offset` parameter allows for rotating the resulting pattern to start at a different point in the sequence.
        /// </summary>
        /// <param name="onNotes">The number of "on" notes in the rhythm.</param>
        /// <param name="totalNotes">The total number of notes in the rhythm.</param>
        /// <param name="offset">The number of positions to rotate the resulting pattern.</param>
        /// <returns>An array representing the Euclidean rhythm pattern.</returns>
        public static int[] Get(int onNotes, int totalNotes, int offset)
        {
            List<List<int>> groups = [];
            for (int i = 0; i < totalNotes; i++)
            {
                groups.Add([i < onNotes ? 1 : 0]);
            }

            int l;
            while ((l = groups.Count - 1) > 0)
            {
                int start = 0;
                List<int> first = groups[0];
                while (start < l && first == groups[start])
                    start++;
                if (start == l) break;

                int end = l;
                List<int> last = groups[l];
                while (end > 0 && last == groups[end])
                    end--;
                if (end == 0) break;

                int count = Math.Min(start, l - end);

                List<List<int>> newGroups = [];
                for (int i = 0; i < count; i++)
                {
                    List<int> combined = [.. groups[i]];
                    combined.AddRange(groups[l - i]);
                    newGroups.Add(combined);
                }
                for (int i = count; i < groups.Count - count; i++)
                {
                    newGroups.Add(groups[i]);
                }
                groups = newGroups;
            }

            List<int> flattened = [.. groups.SelectMany(g => g)];
            int[] sequence = RotateArray([.. flattened], offset);
            return sequence;
        }

        private static int[] RotateArray(int[] arr, int num)
        {
            if (arr.Length == 0) return arr;

            num = num % arr.Length;
            if (num < 0) num += arr.Length;

            int[] rotated = new int[arr.Length];
            Array.Copy(arr, arr.Length - num, rotated, 0, num);
            Array.Copy(arr, 0, rotated, num, arr.Length - num);
            return rotated;
        }
    }
}
