using System;
using System.Collections.Generic;
using System.Text;

namespace CMGWpf.Utilities
{
    public static class EuclideanRhythm
    {
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
