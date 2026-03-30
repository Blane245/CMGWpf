using CMGWpf.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMGWpf.PlayFunctions.Utilities
{
    public class RandomAlgorithm
    {
        public static void Initialize(Algorithm algorithm)
        {
            if (algorithm is Markovian markov)
            {
                markov.InitializeRandom();
            }
            else if (algorithm is Wiener wiener)
            {
                wiener.InitializeRandom();
            }
        }
    }
}
