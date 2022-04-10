using FileSorting.Model;
using static FileSorting.Utils.FileUtils;
using System.Text.RegularExpressions;

namespace FileSorting.Logic
{
    public class PolyphaseSorting : FileSorter
    {
        const int PLACEHOLDER = 0;
        const int DEVICE_NUMBER = 3;

        protected override string ResultDirectory => "polyphase_sorting";

        private int _placeholderNumber = 0;

        public PolyphaseSorting() : base(DEVICE_NUMBER) { }       

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
            _placeholderNumber = resultSubfilesNumber - subfiles.Length;
            CreateSubfiles(firstDevice, _placeholderNumber, PLACEHOLDER.ToString());

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

        private string[] MoveSubfiles(int subfileNumber, Device source, Device destination)
        {
            string[] subfilesPaths = new string[subfileNumber];

            var files = source.Directory.GetFiles();
            var newSubfileIndex = destination.Directory.GetFiles().Length;
            for (int i = 0; i < subfileNumber && i < files.Length; i++)
            {
                subfilesPaths[i] = GenerateSubfilePath(destination, newSubfileIndex + i);
                files[i].MoveTo(subfilesPaths[i]);
            }

            return subfilesPaths;
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

        public void RemovePlaceholders(string filePath)
        {
            if (_placeholderNumber == 0)
            {
                return;
            }

            int removedCount = 0;
            string tempPath = Path.Combine(ResultDirectoryPath, "temp.txt");
            FileStream tempFileStream = new FileStream(tempPath, FileMode.Create);
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                while(fileStream.TryReadNextNumber(out int number))
                {
                    if (number == PLACEHOLDER &&
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
                        tempFileStream.WriteNumber(number);
                    }
                    
                }
            }
            tempFileStream.Close();

            File.Delete(filePath);
            File.Move(tempPath, filePath);
        }

        private string Merge()
        {
            string mergedPath = GenerateInSubfilePath(0, 0);
            bool merged;
            do
            {
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

                var nonMergedFiles = ArrangeSubfileNames(Devices.GetInDevice(0));
                merged = nonMergedFiles.Length == 0;
                Devices.ShiftForward();
            } while(!merged);

            RemovePlaceholders(mergedPath);
            return mergedPath;
        }

        public override string Sort(string fileName, int numbersPerSubfile = NUMBERS_PER_SUBFILE)
        {
            Devices.DefineInOutDevices(1, 0);
            string[] subfilePaths = SplitAndSort(fileName, numbersPerSubfile);

            Devices.DefineInOutDevices(2, 1);
            RedistributeSubfiles(subfilePaths);

            string mergedPath = Merge();
            GuaranteedMoveTo(mergedPath, ResultFilePath);

            ReleaseResources();
            return ResultFilePath;
        }
    }
}
