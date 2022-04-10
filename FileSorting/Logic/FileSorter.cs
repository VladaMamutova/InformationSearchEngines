using FileSorting.Model;
using static FileSorting.Utils.FileUtils;
using static FileSorting.Utils.Sorting;

namespace FileSorting.Logic
{
    public abstract class FileSorter
    {
        protected const int NUMBERS_PER_SUBFILE = 10;
        protected const string SUBFILE_EXTENSION = ".txt";

        private string _rootDirectory;
        private string _subfileExtension;
        protected abstract string ResultDirectory { get; }

        protected DeviceBar Devices { get; }

        protected FileSorter(int deviceNumber,
                          string? rootDirectory = null,
                          string subfileExtension = SUBFILE_EXTENSION
        )
        {
            _rootDirectory = rootDirectory ?? GetAppDirectory();
            _subfileExtension = subfileExtension;
            Devices = DeviceBar.Generate(deviceNumber, ResultDirectoryPath);
        }

        protected string ResultDirectoryPath =>
            Path.Combine(_rootDirectory, ResultDirectory);
        protected string ResultFilePath =>
            Path.Combine(_rootDirectory, ResultDirectory + SubfileExtension);

        protected string SubfileExtension => _subfileExtension;

        protected void ReleaseResources()
        {
            Devices.Dispose();
            DirectoryInfo resultDirectory = new DirectoryInfo(ResultDirectoryPath);
            resultDirectory.Delete(true);
        }

        protected string GenerateInSubfilePath(int inDeviceIndex, int subfileIndex)
        {
            var device = Devices.GetInDevice(inDeviceIndex);
            return GenerateSubfilePath(device, subfileIndex);
        }

        protected string[] GenerateInSubfilePaths(int subfileIndex)
        {
            string[] subfilePaths = new string[Devices.InNumber];
            for (int i = 0; i < subfilePaths.Length; i++)
            {
                var device = Devices.GetInDevice(i);
                subfilePaths[i] = GenerateSubfilePath(device, subfileIndex);
            }

            return subfilePaths;
        }

        protected string GenerateOutSubfilePath(int outDeviceIndex, int subfileIndex)
        {
            var device = Devices.GetOutDevice(outDeviceIndex);
            return GenerateSubfilePath(device, subfileIndex);
        }

        protected string GenerateSubfilePath(Device device, int subfileIndex)
        {
            string subfileName = device.Name.ToLower() + (subfileIndex + 1);
            return device.GetFullPath(subfileName + SUBFILE_EXTENSION);
        }

        protected string[] SplitAndSort(string fileName, int numbersPerSubfile)
        {
            string[] subfiles;
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open))
            {
                subfiles = GenerateSubfiles(fileStream, numbersPerSubfile);
            }
            return subfiles;
        }

        protected string[] GenerateSubfiles(FileStream fileStream, int numbersPerSubfile)
        {
            if (numbersPerSubfile < 1)
            {
                return new string[0];
            }

            List<string> subfiles = new List<string>();
            bool endOfFile = false;
            do
            {
                var numbers = fileStream.ReadNumbers(numbersPerSubfile);
                if (numbers.Count > 0)
                {
                    numbers.QuickSort();
                    int index = subfiles.Count / Devices.InNumber;
                    string path = GenerateInSubfilePath(subfiles.Count, index);
                    WriteNumbersToFile(path, numbers);

                    subfiles.Add(path);
                }
                endOfFile = numbers.Count < numbersPerSubfile;
            }
            while (!endOfFile);

            return subfiles.ToArray();
        }

        protected string[] GenerateSubfiles(FileStream fileStream,
            int numbersPerSubfile, string[] subfilePaths)
        {
            int subfileCount = 0;
            int maxSubfileNumber = subfilePaths.Length;

            bool endOfFile = false;
            while (!endOfFile && subfileCount < maxSubfileNumber)
            {
                var numbers = fileStream.ReadNumbers(numbersPerSubfile);
                if (numbers.Count > 0)
                {
                    numbers.QuickSort();
                    WriteNumbersToFile(subfilePaths[subfileCount], numbers);
                    subfileCount++;
                }

                endOfFile = numbers.Count < numbersPerSubfile;
            }

            return subfilePaths.SkipLast(maxSubfileNumber - subfileCount).ToArray();
        }

        protected void MergeSubfiles(List<string> subfilePaths, string mergedPath)
        {
            var paths = GetExistingPaths(subfilePaths);
            if (paths.Count < 1)
            {
                return;
            }

            var streams = paths.Select(path =>
                new FileStream(path, FileMode.Open)).ToList();
            var mergedStream = new FileStream(mergedPath, FileMode.Create);

            while (TryReadMinNumberFromStreams(streams, out int number))
            {
                mergedStream.WriteNumber(number);
                mergedStream.WriteWhitespace();
            }

            mergedStream.Close();
            streams.ForEach(stream => stream.Close());
            paths.ForEach(path => File.Delete(path));
        }

        public abstract string Sort(string fileName, int numbersPerSubfile = NUMBERS_PER_SUBFILE);
    }
}