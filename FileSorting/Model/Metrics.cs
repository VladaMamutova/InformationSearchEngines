using System.Diagnostics;

namespace FileSorting.Logic
{
    public class Metrics
    {
        private Stopwatch timer;

        public int ReadCount { get; set; }
        public int WriteCount { get; set; }
        public int MergingComparisonCount { get; set; }

        public string DestinationPath { get; set; }

        public Metrics()
        {
            timer = new Stopwatch();
            DestinationPath = "";
        }

        public void Start()
        {
            timer.Start();
            ReadCount = 0;
            WriteCount = 0;
            MergingComparisonCount = 0;
        }

        public void Stop(string destinationPath)
        {
            timer.Stop();
            DestinationPath = destinationPath;
        }

        public override string ToString()
        {
            return $"File: {DestinationPath}\n" +
                   $"ReadCount: {ReadCount}, " +
                   $"WriteCount: {WriteCount}, " +
                   $"MergingComparisonCount: {MergingComparisonCount}, " +
                   $"Time: {timer.Elapsed.ToString("fffffff")} ms";
        }
    }
}