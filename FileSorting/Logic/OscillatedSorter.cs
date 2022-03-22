using static FileSorting.Utils.FileUtils;
using static FileSorting.Utils.ListUtils;

namespace FileSorting.Logic
{
    public static class OscillatedSorter
    {
        public static void Sort(string fileName)
        {
            List<int> numbers = new List<int>(ReadLastNumbersFromFile(fileName, 30));
            numbers.Print();
        }
    }
}