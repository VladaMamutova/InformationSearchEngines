namespace FileSorting.Logic
{
    public class BalancedMerging : FileSorter
    {
        const int DEVICE_NUMBER = 4;
        const string RESULT_DIRECTORY = "balanced_merging";

        public BalancedMerging() : base(DEVICE_NUMBER, resultDirectory: RESULT_DIRECTORY)
        {
            Devices.DefineInOutDevices(2, 2);
        }

        protected override string[] PerformMerge(int subfileNumber)
        {
            List<string> mergedPaths = new List<string>();
            int mergedCount = 0;
            do
            {
                string firstPath = GenerateInSubfilePath(mergedCount, mergedCount);
                string secondPath = GenerateInSubfilePath(mergedCount + 1, mergedCount);
                int mergedSubfileIndex = mergedCount / Devices.OutNumber;
                string mergedPath = GenerateOutSubfilePath(mergedCount, mergedSubfileIndex);
                MergeSubfiles(firstPath, secondPath, mergedPath);

                mergedPaths.Add(mergedPath);
                mergedCount++;
            } while (mergedCount * 2 < subfileNumber);

            return mergedPaths.ToArray();
        }
    }
}
