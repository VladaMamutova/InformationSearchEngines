using StringSearching.Logic;
using StringSearching.Utils;

namespace StringSearching
{
    public class Program
    {
        const string DEFAULT_SOURCE_TEXT = "Мама. Привет, мир! Мама мыла раму. Папа, мама.";
        const string DEFAULT_TERM = "мама";

        public static void Main(string[] args)
        {
            try
            {
                GetArguments(args, out string text, out string term); 
                Console.WriteLine($"Source text (length = {text.Length}):\n{text}" + 
                    $"\n\nTerm (length = {term.Length}):\n{term}\n");
               
                List<int> indexes = StringSearcher.FindAll(term, text, out int comparisonCount);
                Console.WriteLine($"Result: {indexes.Count} occurrences found for {comparisonCount} comparisons.");
                Console.WriteLine($"Indexes of the term:");
                indexes.Print();
                Console.WriteLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error: " + exception.Message);
            }
        }

        private static void GetArguments(string[] args, out string text, out string term)
        {
            if (args.Length == 2)
            {
                FileInfo file = new FileInfo(args[0]);
                if (!file.Exists)
                {
                    throw new Exception($"Source file \"{file.FullName}\" not found!");
                }
                text = File.ReadAllText(file.FullName);

                term = args[1];
                return;
            }

            string appNameArg = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "<app_name>";
            Console.WriteLine($"Usage: ./{appNameArg} <path/to/source/file> <the_search_term_in_the_file>");

            Console.WriteLine($"The program is started with the default arguments.\n");
            text = DEFAULT_SOURCE_TEXT;
            term = DEFAULT_TERM;
        }
    }
}