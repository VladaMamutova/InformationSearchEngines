using System.Diagnostics;

namespace SortingAlgorithmsComparison.Model
{
    public struct Result
    {
        public int ComparisonNumber;
        public int PermutationNumber;
        public Stopwatch Timer;

        public Result()
        {
            ComparisonNumber = 0;
            PermutationNumber = 0;
            Timer = new Stopwatch();
        }

        public override string ToString()
        {
            return $"Comparisons: {ComparisonNumber}, " +
                   $"Permutations: {PermutationNumber}, " +
                   $"Time: {Timer.Elapsed.ToString("fffffff")} ms";
        }
    }
}