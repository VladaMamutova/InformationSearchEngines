using FileSorting.Logic;

namespace FileSorting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                OscillatedSorter.Sort(GetFileName(args));
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error: " + exception.Message);
            }
        }

        public static string GetFileName(string[] args)
        {
            string fileName;
            if (args.Length < 1)
            {
                string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "<app_name>";
                throw new Exception("Wrong number of arguments!" +
                    Environment.NewLine + $"Usage: ./{appName} <path/to/file>");
            }

            fileName = args[0];
            FileInfo file = new FileInfo(fileName);
            if (!file.Exists)
            {
                throw new Exception($"File \"{fileName}\" not found!");
            }

            return fileName;
        }
    }
}
