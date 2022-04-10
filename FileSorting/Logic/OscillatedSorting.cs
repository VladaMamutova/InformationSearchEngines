using static FileSorting.Utils.FileUtils;
using static FileSorting.Utils.Sorting;

namespace FileSorting.Logic
{
    public class OscillatedSorting : FileSorter
    {
        const int DEVICE_NUMBER = 4;

        protected override string ResultDirectory => "oscillated_sorting";

        public OscillatedSorting() : base(DEVICE_NUMBER) {}

        private string SplitAndMerge(string fileName, int numbersPerSubfile)
        {
            string mergedPath = "";
            int mergedCount = 0;
            bool endOfFile = false;
            bool merged = false;
            FileStream fileStream = new FileStream(fileName, FileMode.Open);
            do
            {
                // Генерируем путь к файлу слияния - путь к файлу
                // с индексом 0 на единственном выходном устройстве.
                mergedPath = GenerateOutSubfilePath(0, 0);

                if ((mergedCount + 1) % 4 == 0)
                {
                    // Каждая четвёртая по счёту операция - полное слияние 
                    // всех ранее слитых подфайлов (всех файлов с индексом 0)
                    // в один большой файл на выходном устройстве.
                    var inPaths = GenerateInSubfilePaths(0).ToList();
                    inPaths.Add(mergedPath);
                    string tempPath = Devices.GetOutDevice(0).GetFullPath("temp.txt");
                    MergeSubfiles(inPaths, tempPath);
                    File.Move(tempPath, mergedPath);
                    merged = endOfFile;
                }
                else if (!endOfFile)
                {
                    // На каждой операции на входных устройствах генерируеются
                    // по одному новому подфайлу (с индексом 1) из исходного файла,
                    // которые затем сливаются в один подфайл на выходном устройстве.
                    var inPaths = GenerateSubfiles(fileStream, numbersPerSubfile,
                        GenerateInSubfilePaths(1)).ToList();
                    MergeSubfiles(inPaths, mergedPath);
                    endOfFile = inPaths.Count < Devices.InNumber;
                }

                mergedCount++;
                Devices.ShiftForward();
            }
            while (!merged);

            fileStream.Close();
            return mergedPath;
        }

        public override string Sort(string fileName, int numbersPerSubfile = NUMBERS_PER_SUBFILE)
        {
            Devices.DefineInOutDevices(3, 1);

            string mergedPath = SplitAndMerge(fileName, numbersPerSubfile);
            GuaranteedMoveTo(mergedPath, ResultFilePath);

            ReleaseResources();
            return ResultFilePath;
        }
    }
}
