using FileSorting.Model;
using static FileSorting.Utils.FileUtils;
using static FileSorting.Utils.Sorting;

namespace FileSorting.Logic
{
    public abstract class FileSorter
    {
        const int NUMBERS_PER_SUBFILE = 10;
        const string SUBFILE_EXTENSION = ".txt";

        private string _rootDirectory;
        private string _resultDirectory;
        private string _subfileExtension;

        protected DeviceBar Devices { get; }

        protected FileSorter(int deviceNumber,
                          string? rootDirectory = null,
                          string resultDirectory = "result",
                          string subfileExtension = SUBFILE_EXTENSION
        )
        {
            _rootDirectory = rootDirectory ?? GetAppDirectory();
            _resultDirectory = resultDirectory;
            _subfileExtension = subfileExtension;
            Devices = DeviceBar.Generate(deviceNumber, ResultDirectoryPath);
        }

        protected string ResultDirectoryPath =>
            Path.Combine(_rootDirectory, _resultDirectory);
        protected string ResultFilePath =>
            Path.Combine(_rootDirectory, _resultDirectory + SubfileExtension);
        
        protected string SubfileExtension => _subfileExtension;

        protected void ReleaseResources()
        {
            Devices.Dispose();
            DirectoryInfo resultDirectory = new DirectoryInfo(ResultDirectoryPath);
            resultDirectory.Delete(true);
        }

        protected string GenerateInSubfilePath(int deviceIndex, int subfileIndex)
        {
            var device = Devices.GetInDevice(deviceIndex);
            return GenerateSubfilePath(device, subfileIndex);
        }

        protected string GenerateOutSubfilePath(int deviceIndex, int subfileIndex)
        {
            var device = Devices.GetOutDevice(deviceIndex);
            return GenerateSubfilePath(device, subfileIndex);
        }

        protected string GenerateSubfilePath(Device device, int subfileIndex)
        {
            string subfileName = device.Name.ToLower() + (subfileIndex + 1);
            return device.GetFullPath(subfileName + SUBFILE_EXTENSION);
        }
        
        protected virtual int SplitAndSort(string fileName, int numbersPerSubfile)
        {
            int subfileCount = 0;
            using (FileStream fstream = new FileStream(fileName, FileMode.Open))
            {
                List<int> numbers;
                do
                {
                    numbers = fstream.ReadNextNumbers(numbersPerSubfile);
                    if (numbers.Count > 0)
                    {
                        numbers.QuickSort();
                        int index = subfileCount / Devices.InNumber;
                        string path = GenerateInSubfilePath(subfileCount, index);
                        WriteNumbersToFile(path, numbers);

                     subfileCount++;
                    }
                }
                while (numbers.Count == numbersPerSubfile);
            }

            return subfileCount;
        }

        protected string MergeAll(int subfileNumber)
        {
            string mergedPath = "";
            int needToMergeCount = subfileNumber;
            do
            {
                var mergedPaths = PerformMerge(needToMergeCount);
                Devices.ShiftForward();
                mergedPath = mergedPaths[0];
                needToMergeCount = mergedPaths.Length;
            }
            while(needToMergeCount != 1);

            new FileInfo(mergedPath).GuaranteedMoveTo(ResultFilePath);
            return mergedPath;
        }

        protected abstract string[] PerformMerge(int subfileNumber);

        protected string MergeSubfiles(string firstPath, string secondPath, string mergedPath)
        {
            if (!File.Exists(firstPath) && !File.Exists(secondPath))
            {
                throw new FileNotFoundException($"Nothing to merge! " +
                    "Files \"{firstPath}\" and \"{secondPath} don't exist!\"");
            }

            if (!File.Exists(firstPath))
            {
                File.Move(secondPath, mergedPath);
                return mergedPath;
            }

            if (!File.Exists(secondPath))
            {
                File.Move(firstPath, mergedPath);
                return mergedPath;
            }

            FileStream firstStream = new FileStream(firstPath, FileMode.Open);
            FileStream secondStream = new FileStream(secondPath, FileMode.Open);
            FileStream mergedStream = new FileStream(mergedPath, FileMode.Create);

            try
            {
                int number;
                firstStream.TryReadNextNumber(out number);
                FileStream stream = secondStream;

                int endOfFile = 0;
                while (endOfFile < 2)
                {
                    bool numberRead = stream.TryReadNextNumber(out int nextNumber);
                    if (!numberRead)
                    {
                        endOfFile++;
                        if (endOfFile == 2)
                        {
                            mergedStream.WriteNumber(number);
                        }
                        else
                        {
                            stream = stream.Switch(firstStream, secondStream);
                        }
                    }
                    else
                    {
                        mergedStream.WriteNumber(Math.Min(number, nextNumber));
                        mergedStream.WriteWhitespace();
                        if (nextNumber > number)
                        {
                            number = nextNumber;
                            if (endOfFile == 0)
                            {
                                stream = stream.Switch(firstStream, secondStream);
                            }
                        }
                    }
                }
            }
            finally
            {
                firstStream.Dispose();
                secondStream.Dispose();
                mergedStream.Dispose();

                new FileInfo(firstPath).Delete();
                new FileInfo(secondPath).Delete();
            }

            return mergedPath;
        }

        public string Sort(string fileName, int numbersPerSubfile = NUMBERS_PER_SUBFILE)
        {
            int subfileNumber = SplitAndSort(fileName, numbersPerSubfile);
            MergeAll(subfileNumber);
            ReleaseResources();
            return ResultFilePath;
        }
    }
}