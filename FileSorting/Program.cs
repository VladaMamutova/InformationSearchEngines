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
                GetArguments(args, out string sourcePath, out int numbersPerSubfile);
                string destinationPath = "";

                destinationPath = new BalancedMerging().Sort(sourcePath, numbersPerSubfile);
                Console.WriteLine($"The result of Balanced Merging is saved to \"{destinationPath}\"");

                destinationPath = new PolyphaseSorting().Sort(sourcePath, numbersPerSubfile);
                Console.WriteLine($"The result of Polyphase Sorting is saved to \"{destinationPath}\"");

                destinationPath = new OscillatedSorting().Sort(sourcePath, numbersPerSubfile);
                Console.WriteLine($"The result of Oscillated Sorting is saved to \"{destinationPath}\"");
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error: " + exception.Message);
            }
        }

        public static void GetArguments(string[] args,
            out string sourcePath, out int numbersPerSubfile)
        {
            if (args.Length == 2)
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
                return;
            }

            string sourceName = DEFAULT_SOURCE_FILE;
            sourcePath = Path.Combine(GetAppDirectory(), sourceName);
            numbersPerSubfile = DEFAULT_NUMBERS_PER_FILE;

            string appNameArg = GetAppName() ?? "<app_name>";
            Console.WriteLine($"Usage: ./{appNameArg} <path/to/source/file> <numbers_per_subfile>");
            Console.Write("The program is started with default arguments: ");
            Console.WriteLine($"{sourceName}, {numbersPerSubfile}.\n");
        }
    }
}
