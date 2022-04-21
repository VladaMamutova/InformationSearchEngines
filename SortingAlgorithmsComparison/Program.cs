using static SortingAlgorithmsComparison.Utils.ListUtils;
using SortingAlgorithmsComparison.Logic;

namespace SortingAlgorithmsComparison
{
    public class Program
    {
        const int DEFAULT_ARRAY_SIZE = 10;

        public static void Main(string[] args)
        {
            List<int> randomList = InitList(args);

            try
            {
                var sortedList = Comparator.SortAndCompare(randomList);
                Console.WriteLine("\n\n");

                sortedList = Comparator.SortAndCompare(sortedList);
                Console.WriteLine("\n\n");

                sortedList.ChangeSigns();
                sortedList = Comparator.SortAndCompare(sortedList);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error: " + exception.Message);
            }
        }

        private static List<int> InitList(string[] args)
        {
            int listSize;
            if (args.Length < 1 ||
               !Int32.TryParse(args[0], out listSize) ||
                listSize < 0)
            {
                listSize = DEFAULT_ARRAY_SIZE;
            }

            return new List<int>(GenerateValues(listSize, 10_000));
        }
    }
}
