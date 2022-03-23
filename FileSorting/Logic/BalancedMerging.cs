using FileSorting.Model;
using static FileSorting.Utils.FileUtils;
using static FileSorting.Utils.Sorting;

namespace FileSorting.Logic
{
    public static class BalancedMerging
    {
        const int DEFAULT_NUMBERS_PER_SUBFILE = 1;
        const string RESULT_DIRECTORY = "balanced_merging";

        static readonly string ResultPath;
        static readonly DeviceList Devices;

        static BalancedMerging()
        {
            ResultPath = Path.Combine(GetAppDirectory(), RESULT_DIRECTORY);
            Devices = new DeviceList(new string[] { "A", "B", "C", "D" }, ResultPath, 2);

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

                Devices.ActivateNextDevices();
                subfilesNumber = mergedSubfileIndex;
                merged = subfilesNumber == 1;
                if (merged)
                {
                    string resultPath = Path.Combine(GetAppDirectory(), RESULT_DIRECTORY + ".txt");
                    FileInfo resultFile = new FileInfo(resultPath);
                    if (resultFile.Exists)
                    {
                        resultFile.Delete();
                    }
                    FileInfo mergedFile = new FileInfo(mergedPath);
                    mergedFile.CopyTo(resultPath);
                }
            }
            
            // ReleaseResources();
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

                        string path = Devices.GenerateSubfilePath(subfileIndex);
                        WriteNumbersToFile(path, numbers);

                        subfileIndex++;
                    }
                    endOfFile = numbers.Count < numbersPerSubfile;
                }
            }

            return subfileIndex;
        }

        private static string MergeSubfiles(int mergedSubfileIndex)
        {
            string firstPath = Devices.GenerateSubfilePath(mergedSubfileIndex * 2);
            string secondPath = Devices.GenerateSubfilePath(mergedSubfileIndex * 2 + 1);
            string mergedPath = Devices.GenerateSubfilePath(mergedSubfileIndex, true);

            FileInfo secondFile = new FileInfo(secondPath);
            if (!secondFile.Exists)
            {
                FileInfo mergedFile = new FileInfo(mergedPath);
                if (mergedFile.Exists)
                {
                    mergedFile.Delete();
                }
                FileInfo firstFile = new FileInfo(firstPath);
                firstFile.MoveTo(mergedPath);
                return mergedPath;
            }

            MergeTwoFiles(firstPath, secondPath, mergedPath);
            return mergedPath;
        }

        private static void MergeTwoFiles(string firstPath, string secondPath, string mergedPath)
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
            }
        }

        static public void ReleaseResources()
        {
            Devices.Dispose();

            DirectoryInfo resultDirectory = new DirectoryInfo(ResultPath);
            resultDirectory.Delete();
        }
    }
}
