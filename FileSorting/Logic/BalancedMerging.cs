using FileSorting.Model;
using static FileSorting.Utils.FileUtils;
using static FileSorting.Utils.Sorting;

namespace FileSorting.Logic
{
    public static class BalancedMerging
    {
        const int DEFAULT_NUMBERS_PER_SUBFILE = 1;
        const int DEVICE_NUMBER = 4;
        const string RESULT_DIRECTORY = "balanced_merging";
        const string SUBFILE_EXTENSION = ".txt";

        static readonly string ResultPath;

        static readonly DeviceBar Devices;

        static BalancedMerging()
        {
            ResultPath = Path.Combine(GetAppDirectory(), RESULT_DIRECTORY);
            Devices = DeviceBar.Generate(DEVICE_NUMBER, ResultPath);
            Devices.DefineInOutDevices(2, 2);
            Devices.Prepare();
        }

        public static void Sort(string fileName,
            int numbersPerSubfile = DEFAULT_NUMBERS_PER_SUBFILE)
        {
            int subfilesNumber = SplitAndSortBySubfiles(fileName, numbersPerSubfile);

            bool merged = false;
            string mergedPath = "";
            while (!merged)
            {
                int mergedSubfileIndex = 0;
                while (mergedSubfileIndex * 2 < subfilesNumber)
                {
                    mergedPath = MergeSubfiles(mergedSubfileIndex);
                    mergedSubfileIndex++;
                }

                Devices.ShiftForward();

                subfilesNumber = mergedSubfileIndex;
                merged = subfilesNumber == 1;
                if (merged)
                {
                    string resultPath = Path.Combine(GetAppDirectory(), RESULT_DIRECTORY + SUBFILE_EXTENSION);
                    FileInfo resultFile = new FileInfo(resultPath);
                    if (resultFile.Exists)
                    {
                        resultFile.Delete();
                    }
                    FileInfo mergedFile = new FileInfo(mergedPath);
                    mergedFile.CopyTo(resultPath);
                }
            }

            ReleaseResources();
        }

        private static int SplitAndSortBySubfiles(string fileName, int numbersPerSubfile)
        {
            int subfileIndex = 0;
            bool endOfFile = false;
            using (FileStream fstream = new FileStream(fileName, FileMode.Open))
            {
                while (!endOfFile)
                {
                    List<int> numbers = fstream.ReadNextNumbers(numbersPerSubfile);
                    if (numbers.Count > 0)
                    {
                        numbers.QuickSort();
                        string path = GenerateInSubfilePath(subfileIndex);
                        WriteNumbersToFile(path, numbers);

                        subfileIndex++;
                    }
                    endOfFile = numbers.Count < numbersPerSubfile;
                }
            }

            return subfileIndex;
        }

        private static string MergeSubfiles(int index)
        {
            var firstDevice = Devices.GetInDevice(index);
            string firstPath = GenerateSubfilePath(firstDevice, index);
            var secondDevice = Devices.GetInDevice(index + 1);
            string secondPath = GenerateSubfilePath(secondDevice, index);

            string mergedPath = GenerateOutSubfilePath(index);

            FileInfo firstFile = new FileInfo(firstPath);
            FileInfo secondFile = new FileInfo(secondPath);

            if (!firstFile.Exists)
            {
                return secondFile.GuaranteedMoveTo(mergedPath);
            }

            if (!secondFile.Exists)
            {
                return firstFile.GuaranteedMoveTo(mergedPath);
            }

            return MergeTwoFiles(firstPath, secondPath, mergedPath);
        }

        private static string GenerateInSubfilePath(int subfileIndex)
        {
            var device = Devices.GetInDevice(subfileIndex);
            return GenerateSubfilePath(device, subfileIndex / Devices.InNumber);
        }

        private static string GenerateOutSubfilePath(int subfileIndex)
        {
            var device = Devices.GetOutDevice(subfileIndex);
            return GenerateSubfilePath(device, subfileIndex / Devices.OutNumber);
        }

        private static string GenerateSubfilePath(Device device, int subfileIndex)
        {
            string subfileName = device.Name.ToLower() + (subfileIndex + 1);
            return device.GetFullPath(subfileName + SUBFILE_EXTENSION);
        }

        private static string MergeTwoFiles(string firstPath, string secondPath, string mergedPath)
        {
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

        static public void ReleaseResources()
        {
            Devices.Dispose();

            DirectoryInfo resultDirectory = new DirectoryInfo(ResultPath);
            resultDirectory.Delete();
        }
    }
}
