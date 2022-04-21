
using static SortingAlgorithmsComparison.Utils.ListUtils;

namespace SortingAlgorithmsComparison.Logic
{
    public class Comparator
    {
        public static List<int> SortAndCompare(List<int> source)
        {
            List<int> list1 = new List<int>(source);
            List<int> list2 = new List<int>(source);
            List<int> list3 = new List<int>(source);            

            Model.Result result1, result2, result3;
            try
            {
                result1 = list1.BubbleSort();
                result2 = list2.InsertionSort();
                result3 = list3.QuickSort();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error: " + exception.Message + Environment.NewLine);
                return source;
            }

            if (source.Count <= 1000)
            {
                Console.WriteLine($"Source array (n = {source.Count}):");
                source.Print();
            }

            PrintResultArray(list1, list2, list3);

            PrintResultInfo("Bubble Sort", result1);
            PrintResultInfo("Insertion Sort", result2);
            PrintResultInfo("Quick Sort", result3);

            return list1;
        }

        private static bool PrintResultArray(List<int> list1,
            List<int> list2, List<int> list3)
        {
            bool success = list1.SequenceEqual(list2) && list1.SequenceEqual(list3);
            Console.WriteLine("\nThe resulting arrays after sorting are{0} equal!\n", success ? "" : " not");

            if (list1.Count <= 1000)
            {
                if (success)
                {
                    Console.WriteLine($"Result array (n = {list1.Count}):");
                    list1.Print();
                }
                else
                {
                    Console.WriteLine($"List1: (n = {list1.Count}):");
                    list1.Print();

                    Console.WriteLine($"List2: (n = {list2.Count}):");
                    list2.Print();

                    Console.WriteLine($"List3: (n = {list3.Count}):");
                    list3.Print();
                }

            }

            return success;
        }

        private static void PrintResultInfo(string title, Model.Result result)
        {
            Console.WriteLine($"\n{title}:");
            Console.WriteLine(result);
        }
    }
}