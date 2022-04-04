using FileSorting.Model;
using static FileSorting.Utils.FileUtils;
using System.Text.RegularExpressions;

namespace FileSorting.Logic
{
    public class PolyphaseSorting : FileSorter
    {
        const int PLACEHOLDER_NUMBER = 0;
        const int DEVICE_NUMBER = 3;
        const string RESULT_DIRECTORY = "polyphase_sorting";

        private int _placeholderNumber = 0;

        public PolyphaseSorting() : base(DEVICE_NUMBER, resultDirectory: RESULT_DIRECTORY) { }

        protected override int SplitAndSort(string fileName, int numbersPerSubfile)
        {
            Devices.DefineInOutDevices(1, 0);
            int subfileNumber = base.SplitAndSort(fileName, numbersPerSubfile);
            Devices.DefineInOutDevices(2, 1);
            RedistributeSubfiles(subfileNumber);
            return subfileNumber;
        }

        protected override string[] PerformMerge(int subfileNumber)
        {
            List<string> mergedPathes = new List<string>();
            int mergedCount = 0;
            bool merged;
            do
            {
                merged = false;
                string firstPath = GenerateInSubfilePath(mergedCount, mergedCount);
                string secondPath = GenerateInSubfilePath(mergedCount + 1, mergedCount);
                if (File.Exists(firstPath) && File.Exists(secondPath))
                {
                    string mergedPath = GenerateOutSubfilePath(mergedCount, mergedCount);
                    MergeSubfiles(firstPath, secondPath, mergedPath);
                    merged = true;

                    mergedPathes.Add(mergedPath);
                    mergedCount++;
                }
            } while(merged);

            var nonMergedFiles = ArrangeSubfileNames(Devices.GetInDevice(0));
            mergedPathes.AddRange(nonMergedFiles);

            if (mergedPathes.Count == 1)
            {
                RemovePlaceholders(mergedPathes[0]);
            }

            return mergedPathes.ToArray();
        }

        private void RedistributeSubfiles(int subfilesNumber) 
        {
            if (subfilesNumber < 2)
            {
                return;
            }

            List<int> fibonacciNumbers = Find3NearestFibonacciNumbers(subfilesNumber);
            int resultSubfilesNumber = fibonacciNumbers[2];
            int secondDeviceSubfilesNumber = fibonacciNumbers[0];
            
            var firstDevice = Devices.GetInDevice(0);
            _placeholderNumber = resultSubfilesNumber - subfilesNumber;
            CreateSubfiles(firstDevice, _placeholderNumber, PLACEHOLDER_NUMBER.ToString());

            var secondDevice = Devices.GetInDevice(1);
            MoveSubfiles(secondDeviceSubfilesNumber, firstDevice, secondDevice);
            ArrangeSubfileNames(firstDevice);
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

        public void CreateSubfiles(Device device, int subfileNumber, string content)
        {
            int subfileIndex = device.Directory.GetFiles().Length;
            for (int i = 0; i < subfileNumber; i++)
            {
                CreateFile(GenerateSubfilePath(device, subfileIndex + i), content);
            }
        }

        public void RemovePlaceholders(string filePath)
        {
            if (_placeholderNumber == 0)
            {
                return;
            }

            int removedCount = 0;
            const int numbersPerRead = 1;

            string tempPath = Path.Combine(ResultDirectoryPath, "temp.txt");
            FileStream tempFileStream = new FileStream(tempPath, FileMode.Create);
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                List<int>numbers;
                do
                {
                    numbers = fileStream.ReadNextNumbers(numbersPerRead);
                    if (numbers.Count > 0)
                    {
                        if (numbers[0] == PLACEHOLDER_NUMBER &&
                            removedCount < _placeholderNumber)
                        {
                            removedCount++;
                        }
                        else
                        {
                            if (tempFileStream.Position > 0)
                            {
                                tempFileStream.WriteWhitespace();
                            }
                            tempFileStream.WriteNumber(numbers[0]);
                        }
                    }
                }
                while (numbers.Count == numbersPerRead);
            }
            tempFileStream.Close();

            File.Delete(filePath);
            File.Move(tempPath, filePath);
        }

        private void MoveSubfiles(int subfileNumber, Device source, Device destination)
        {
            var files = source.Directory.GetFiles();
            var newSubfileIndex = destination.Directory.GetFiles().Length;
            for (int i = 0; i < subfileNumber && i < files.Length; i++)
            {
                files[i].MoveTo(GenerateSubfilePath(destination, newSubfileIndex + i));
            }
        }

        private string[] ArrangeSubfileNames(Device device)
        {
            List<string> arrangedNames = new List<string>();

            var files = device.Directory.GetFiles();
            int maxSubfileNumber = files.Length;
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
                    } while (subfileNumber <= maxSubfileNumber);

                    fileToRename.MoveTo(resultName);
                }
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

        // private int SplitAndSortBySubfiles(string fileName, int numbersPerSubfile)
        // {
        //     int subfileIndex = 0;
        //     bool endOfFile = false;
        //     using (FileStream fstream = new FileStream(fileName, FileMode.Open))
        //     {
        //         while (!endOfFile)
        //         {
        //             List<int> numbers = fstream.ReadNextNumbers(numbersPerSubfile);
        //             if (numbers.Count > 0)
        //             {
        //                 numbers.QuickSort();

        //                 string path = Devices.GenerateSubfilePath(subfileIndex);
        //                 WriteNumbersToFile(path, numbers);

        //                 subfileIndex++;
        //             }
        //             endOfFile = numbers.Count < numbersPerSubfile;
        //         }
        //     }

        //     return subfileIndex;
        // }

        // protected override string MergeSubfiles(int mergedSubfileIndex)
        // {
        //     string firstPath = Devices.GenerateSubfilePath(mergedSubfileIndex * 2);
        //     string secondPath = Devices.GenerateSubfilePath(mergedSubfileIndex * 2 + 1);
        //     string mergedPath = Devices.GenerateSubfilePath(mergedSubfileIndex, true);

        //     FileInfo secondFile = new FileInfo(secondPath);
        //     if (!secondFile.Exists)
        //     {
        //         FileInfo mergedFile = new FileInfo(mergedPath);
        //         if (mergedFile.Exists)
        //         {
        //             mergedFile.Delete();
        //         }
        //         FileInfo firstFile = new FileInfo(firstPath);
        //         firstFile.MoveTo(mergedPath);
        //         return mergedPath;
        //     }

        //     MergeTwoFiles(firstPath, secondPath, mergedPath);
        //     return mergedPath;
        // }
    }
}
