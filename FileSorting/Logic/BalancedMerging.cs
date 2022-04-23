using static FileSorting.Utils.FileUtils;
namespace FileSorting.Logic
{
    public class BalancedMerging : FileSorter
    {
        const int DEVICE_NUMBER = 4;
        protected override string ResultDirectory => "balanced_merging";

        public BalancedMerging(bool debugMode = false)
            : base(DEVICE_NUMBER, debugMode: debugMode) {}

        private string Merge(string[] subfilesPaths)
        {
            PrintDebugInfo("\nMerge Subfiles: \n");

            string mergedPath = "";
            while (subfilesPaths.Length > 1) {
                var mergedSubfiles = new List<string>();
                for (int i = 0; i < subfilesPaths.Length; i += 2)
                {
                    var paths = new List<string>();
                    paths.Add(subfilesPaths[i]);
                    if (i + 1 < subfilesPaths.Length)
                    {
                        paths.Add(subfilesPaths[i + 1]);
                    }

                    mergedPath = GenerateOutSubfilePath(i / 2, (i / 2) / Devices.OutNumber);
                    mergedSubfiles.Add(mergedPath);
                    MergeSubfiles(paths, mergedPath);
                }

                Devices.ShiftForward();
                subfilesPaths = mergedSubfiles.ToArray();
            }

            return mergedPath;
        }

        public override Metrics Sort(string fileName, int numbersPerSubfile = NUMBERS_PER_SUBFILE)
        {
            PrintDebugInfo("\n----- Balanced Merging -----\n");

            Metrics.Start();
            Devices.DefineInOutDevices(2, 2);

            string[] subfilePaths = SplitAndSort(fileName, numbersPerSubfile);
            string mergedPath = Merge(subfilePaths);
            GuaranteedMoveTo(mergedPath, ResultFilePath);

            ReleaseResources();
            Metrics.Stop(ResultFilePath);

            PrintDebugInfo("\nMove result file:\n");
            PrintDebugFileMoveInfo(mergedPath, ResultFilePath);

            return Metrics;
        }
    }
}
