using static FileSorting.Utils.FileUtils;
namespace FileSorting.Logic
{
    public class BalancedMerging : FileSorter
    {
        const int DEVICE_NUMBER = 4;
        protected override string ResultDirectory => "balanced_merging";

        public BalancedMerging() : base(DEVICE_NUMBER) {}

        private string Merge(string[] subfilesPaths)
        {
            string mergedPath = "";
            for (int i = 0; i < subfilesPaths.Length; i += 2)
            {
                var paths = new List<string>();
                paths.Add(subfilesPaths[i]);
                if (i + 1 < subfilesPaths.Length)
                {
                    paths.Add(subfilesPaths[i + 1]);
                }

                mergedPath = GenerateOutSubfilePath(i, i / Devices.OutNumber);
                MergeSubfiles(paths, mergedPath);

                Devices.ShiftForward();
            }

            return mergedPath;
        }

        public override string Sort(string fileName, int numbersPerSubfile = NUMBERS_PER_SUBFILE)
        {
            Devices.DefineInOutDevices(2, 2);

            string[] subfilePaths = SplitAndSort(fileName, numbersPerSubfile);

            string mergedPath = Merge(subfilePaths);
            GuaranteedMoveTo(mergedPath, ResultFilePath);

            ReleaseResources();
            return ResultFilePath;
        }
    }
}
