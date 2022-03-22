using static FileSorting.Utils.FileUtils;
using static FileSorting.Utils.ListUtils;

namespace FileSorting.Logic
{
    public static class BalancedMerging
    {
        const int DEFAULT_NUMBERS_PER_SUBFILE = 6;
        public static void Sort(string fileName,
            int numbersPerSubfile = DEFAULT_NUMBERS_PER_SUBFILE)
        {
            // TODO: размер подфайла должен рассчитываться по количеству свободной ОЗУ,
            // а не по количеству чисел в нём.

            List<int> numbers = ReadFirstNumbersFromFile(fileName, 30);
            numbers.Print();
        }
    }
}
