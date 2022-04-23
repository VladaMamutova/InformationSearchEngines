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

        public Metrics Metrics { get; }
        protected bool DebugMode { get; }

        protected FileSorter(int deviceNumber,
                          string? rootDirectory = null,
                          string subfileExtension = SUBFILE_EXTENSION,
                          bool debugMode = false
        )
        {
            _rootDirectory = rootDirectory ?? GetAppDirectory();
            _subfileExtension = subfileExtension;
            Devices = DeviceBar.Generate(deviceNumber, ResultDirectoryPath);

            Metrics = new Metrics();
            DebugMode = debugMode;
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

            PrintDebugInfo("\nGenerate subfiles:\n");

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

                    Metrics.ReadCount += numbers.Count;
                    Metrics.WriteCount += numbers.Count;
                    PrintDebugFileInfo(path, numbers);
                }
                endOfFile = numbers.Count < numbersPerSubfile;
            }
            while (!endOfFile);

            return subfiles.ToArray();
        }

        protected string[] GenerateSubfiles(FileStream fileStream,
            int numbersPerSubfile, string[] subfilePaths)
        {
            if (subfilePaths.Length < 1)
            {
                return new string[0];
            }

            PrintDebugInfo("\nGenerate subfiles:\n");

            int subfileCount = 0;
            bool endOfFile = false;
            while (!endOfFile && subfileCount < subfilePaths.Length)
            {
                var numbers = fileStream.ReadNumbers(numbersPerSubfile);
                if (numbers.Count > 0)
                {
                    numbers.QuickSort();
                    WriteNumbersToFile(subfilePaths[subfileCount], numbers);

                    Metrics.ReadCount += numbers.Count;
                    Metrics.WriteCount += numbers.Count;
                    PrintDebugFileInfo(subfilePaths[subfileCount], numbers);

                    subfileCount++;
                }

                endOfFile = numbers.Count < numbersPerSubfile;
            }

            return subfilePaths.Take(subfileCount).ToArray();
        }

        protected void MergeSubfiles(List<string> subfilePaths, string mergedPath)
        {
            var paths = GetExistingPaths(subfilePaths);
            if (paths.Count < 1)
            {
                return;
            }
            else if (paths.Count == 1)
            {
                File.Move(paths[0], mergedPath);
                PrintDebugFileMoveInfo(paths[0], mergedPath);
                return;
            }

            PrintDebugMergingInfo(paths, mergedPath);

            var streams = paths.Select(path =>
                new FileStream(path, FileMode.Open)).ToList();
            var mergedStream = new FileStream(mergedPath, FileMode.Create);

            int[] numbers;
            HashSet<int> numbersIndexes = ReadNumbersFromStreams(streams, out numbers);
            while (numbersIndexes.Count > 0)
            {
                int indexMin = IndexOfMinNumber(numbers, numbersIndexes);
                mergedStream.WriteNumber(numbers[indexMin]);
                mergedStream.WriteWhitespace();
                PrintDebugInfo($"{numbers[indexMin]} ");
                ++Metrics.WriteCount;

                if (streams[indexMin].TryReadNextNumber(out int number))
                {
                    numbers[indexMin] = number;
                    ++Metrics.ReadCount;
                }
                else
                {
                    numbersIndexes.Remove(indexMin);
                }
            }

            PrintDebugInfo("\n");

            mergedStream.Close();
            streams.ForEach(stream => stream.Close());
            paths.ForEach(path => File.Delete(path));
        }

        private HashSet<int> ReadNumbersFromStreams(List<FileStream> streams, out int[] numbers)
        {
            HashSet<int> numbersIndexes = new HashSet<int>(streams.Count);
            numbers = new int[streams.Count];
            for (int i = 0; i < streams.Count; i++)
            {
                if (streams[i].TryReadNextNumber(out int number))
                {
                    numbers[i] = number;
                    numbersIndexes.Add(i);
                    ++Metrics.ReadCount;
                }
            }

            return numbersIndexes;
        }

        public int IndexOfMinNumber(int[] numbers, HashSet<int> indexes)
        {
            if (numbers.Length < 1 || indexes.Count < 1)
            {
                return -1;
            }

            int indexMin = indexes.First();
            for (int i = 0; i < numbers.Length; i++)
            {
                if (indexes.Contains(i))
                {
                    if (numbers[i] < numbers[indexMin])
                    {
                        indexMin = i;
                    }
                    ++Metrics.MergingComparisonCount;
                }
            }

            return indexMin;
        }

        protected void PrintDebugFileInfo(string filePath, List<int> numbers)
        {
            if (DebugMode)
            {
                string info = GetFileName(filePath) + " -> ";
                info += string.Join(' ', numbers) + "\n";
                PrintDebugInfo(info);
            }
        }

        protected void PrintDebugFileInfo(string filePath)
        {
            if (DebugMode)
            {
                string info = GetFileName(filePath) + " -> ";
                info += File.ReadAllText(filePath) + "\n";
                PrintDebugInfo(info);
            }
        }

        protected void PrintDebugMergingInfo(List<string> paths, string mergedPath)
        {
            if (DebugMode)
            {
                string info = string.Join(" + ", paths.Select(GetFileName));
                info += " = " + GetFileName(mergedPath) + " --> ";
                PrintDebugInfo(info);
            }
        }

        protected void PrintDebugFileMoveInfo(string sourcePath, string destinationPath)
        {
            PrintDebugInfo(GetFileName(sourcePath) + " -> " +
                GetFileName(destinationPath) + "\n");
        }

        protected void PrintDebugInfo(string value)
        {
            if (DebugMode)
            {
                Console.Write(value);
            }
        }

        protected string GetFileName(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public abstract Metrics Sort(string fileName, int numbersPerSubfile = NUMBERS_PER_SUBFILE);
    }
}