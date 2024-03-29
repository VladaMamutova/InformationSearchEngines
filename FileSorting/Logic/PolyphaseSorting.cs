using FileSorting.Model;
using static FileSorting.Utils.FileUtils;
using System.Text.RegularExpressions;

namespace FileSorting.Logic
{
    public class PolyphaseSorting : FileSorter
    {
        const int DEVICE_NUMBER = 3;

        protected override string ResultDirectory => "polyphase_sorting";

        public PolyphaseSorting(bool debugMode = false)
            : base(DEVICE_NUMBER, debugMode: debugMode) {}

        private void RedistributeSubfiles(string[] subfiles) 
        {
            if (subfiles.Length < 2)
            {
                return;
            }

            List<int> fibonacciNumbers = Find3NearestFibonacciNumbers(subfiles.Length);
            int resultSubfilesNumber = fibonacciNumbers[2];
            int secondDeviceSubfilesNumber = fibonacciNumbers[0];
            
            var firstDevice = Devices.GetInDevice(0);
            var secondDevice = Devices.GetInDevice(1);
            MoveSubfiles(secondDeviceSubfilesNumber, firstDevice, secondDevice);
            int newFiles = resultSubfilesNumber - subfiles.Length;
            CreateSubfiles(firstDevice, newFiles);
            ArrangeSubfiles(firstDevice);

            for (int i = 0; i < secondDeviceSubfilesNumber; i++)
            {
                PrintDebugFileInfo(GenerateSubfilePath(secondDevice, i));
            }
        }

        private List<int> Find3NearestFibonacciNumbers(int number)
        {
            if (number <= 0) {
                return new List<int>();
            }

            int first = 1;
            if (first == number)
            {
                return new List<int>() { first };
            }

            int second = 1;
            int third = first + second;
            while (third < number)
            {
                first = second;
                second = third;
                third = first + second;
            }

            return new List<int>() { first, second, third };
        }

        public void CreateSubfiles(Device device, int subfileNumber)
        {
            int subfileIndex = device.Directory.GetFiles().Length;
            for (int i = 0; i < subfileNumber; i++)
            {
                CreateFile(GenerateSubfilePath(device, subfileIndex + i));
            }
        }

        private string[] MoveSubfiles(int subfileNumber, Device source, Device destination)
        {
            string[] subfilesPaths = new string[subfileNumber];

            var sourceFileNumber = source.Directory.GetFiles().Length;
            var newSubfileIndex = destination.Directory.GetFiles().Length;
            for (int i = 0; i < subfileNumber && i < sourceFileNumber; i++)
            {
                subfilesPaths[i] = GenerateSubfilePath(destination, newSubfileIndex + i);
                string sourceFile = GenerateSubfilePath(source, sourceFileNumber - subfileNumber + i);
                File.Move(sourceFile, subfilesPaths[i]);
                PrintDebugFileInfo(subfilesPaths[i]);
            }

            return subfilesPaths;
        }

        private string[] ArrangeSubfiles(Device device)
        {
            List<string> arrangedNames = new List<string>();
            var files = device.Directory.GetFiles();
            if (files.Length < 1)
            {
                return new string[0];
            }

            PrintDebugInfo("\nArrange subfiles:\n");

            int arrangedIndex = 0;
            for (int i = 0; i < files.Length; i++)
            {
                string resultName = GenerateSubfilePath(device, i);
                if (!File.Exists(resultName))
                {
                    FileInfo fileToRename;
                    int subfileNumber;
                    do
                    {
                        fileToRename = files[arrangedIndex];
                        arrangedIndex++;
                        subfileNumber = ExtractSubfileNumber(fileToRename.Name);
                    } while (subfileNumber <= files.Length);

                    fileToRename.MoveTo(resultName);
                }

                PrintDebugFileInfo(resultName);
                arrangedNames.Add(resultName);
            }

            return arrangedNames.ToArray();
        }

        private int ExtractSubfileNumber(string subfileName)
        {
            MatchCollection matchCollection = Regex.Matches(subfileName, @"(\d)+");
            if (matchCollection.Count == 0)
            {
                throw new ArgumentException("Incorrect subfile name! " +
                    $"\"{subfileName}\" does not contain a number.");
            }

            return int.Parse(matchCollection[0].Value);
        }

        private string Merge()
        {
            string mergedPath = GenerateInSubfilePath(0, 0);
            bool merged;
            do
            {
                PrintDebugInfo("\nMerge Subfiles: \n");

                int i = 0;
                bool subfilesMerged;
                do
                {
                    subfilesMerged = false;
                    var inPaths = GenerateInSubfilePaths(i).ToList();
                    if (GetExistingPaths(inPaths).Count == Devices.InNumber)
                    {
                        mergedPath = GenerateOutSubfilePath(i, i);
                        MergeSubfiles(inPaths, mergedPath);
                        subfilesMerged = true;
                    }
                    i++;
                }
                while(subfilesMerged);

                var nonMergedFiles = ArrangeSubfiles(Devices.GetInDevice(0));
                merged = nonMergedFiles.Length == 0;
                Devices.ShiftForward();
            } while(!merged);

            return mergedPath;
        }

        public override Metrics Sort(string fileName, int numbersPerSubfile = NUMBERS_PER_SUBFILE)
        {
            PrintDebugInfo("\n----- Polyphase Sorting -----\n");

            Metrics.Start();
            Devices.DefineInOutDevices(1, 0);

            string[] subfilePaths = SplitAndSort(fileName, numbersPerSubfile);
            Devices.DefineInOutDevices(2, 1);
            RedistributeSubfiles(subfilePaths);
            string mergedPath = Merge();
            GuaranteedMoveTo(mergedPath, ResultFilePath);

            ReleaseResources();
            Metrics.Stop(ResultFilePath);

            PrintDebugInfo("\nMove result file:\n");
            PrintDebugFileMoveInfo(mergedPath, ResultFilePath);

            return Metrics;
        }
    }
}
