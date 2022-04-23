using FileSorting.Logic;
using static FileSorting.Utils.FileUtils;

namespace FileSorting
{
    public class Program
    {
        const string DEFAULT_SOURCE_FILE = "source.txt";
        const int DEFAULT_NUMBERS_PER_FILE = 1;

        public static void Main(string[] args)
        {
            try
            {
                GetArguments(args, out string sourcePath, out int numbersPerSubfile, out bool debug);
                Metrics metrics;

                PrintSourceFile(sourcePath);
                metrics = new BalancedMerging(debug).Sort(sourcePath, numbersPerSubfile);
                PrintSortingResult("Balanced Merging", metrics);

                PrintSourceFile(sourcePath);
                metrics = new PolyphaseSorting(debug).Sort(sourcePath, numbersPerSubfile);
                PrintSortingResult("Polyphase Merging", metrics);

                PrintSourceFile(sourcePath);
                metrics = new OscillatedSorting(debug).Sort(sourcePath, numbersPerSubfile);
                PrintSortingResult("Oscillated Sorting", metrics);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error: " + exception.Message);
            }
        }

        private static void GetArguments(string[] args,
            out string sourcePath, out int numbersPerSubfile, out bool debug)
        {
            debug = false;
            if (args.Length >= 2)
            {
                FileInfo file = new FileInfo(args[0]);
                if (!file.Exists)
                {
                    throw new Exception($"Source file \"{file.FullName}\" not found!");
                }
                sourcePath = file.FullName;

                if (!int.TryParse(args[1], out numbersPerSubfile) ||
                    numbersPerSubfile < 1)
                {
                    throw new Exception($"The number of numbers per file must be more than one!");
                }

                if (args.Length > 2 && args[2] == "debug")
                {
                    debug = true;
                }

                return;
            }

            sourcePath = Path.Combine(GetAppDirectory(), DEFAULT_SOURCE_FILE);
            numbersPerSubfile = DEFAULT_NUMBERS_PER_FILE;

            string appNameArg = GetAppName() ?? "<app_name>";
            Console.WriteLine($"Usage: ./{appNameArg} <path/to/source/file> <numbers_per_subfile> <debug>");
            Console.WriteLine("* <debug> - is optional argument, type \"debug\" to display all the debug logs.");
            Console.WriteLine("\nThe program is started with the default arguments: ");
            Console.WriteLine($"{sourcePath}, {numbersPerSubfile}.\n");
        }

        private static void PrintSourceFile(string sourcePath)
        {
            Console.WriteLine("\nSource file:");
            Console.WriteLine(File.ReadAllText(sourcePath));
        }

        private static void PrintSortingResult(string sortingName, Metrics metrics)
        {
            Console.WriteLine($"\nThe result of {sortingName}:");
            Console.WriteLine(File.ReadAllText(metrics.DestinationPath));
            Console.WriteLine(metrics + "\n");
        }
    }
}
