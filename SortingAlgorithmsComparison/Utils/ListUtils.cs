namespace SortingAlgorithmsComparison.Utils
{
    public static class ListUtils
    {
        public static int[] GenerateValues(int n, int maxValue)
        {
            int[] values = new int[n];
            var rand = new Random(0);
            for (int i = 0; i < n; i++)
            {
                values[i] = rand.Next() % maxValue;
            }

            return values;
        }

        public static void ChangeSigns(this List<int> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = -list[i];
            }
        }

        public static void Print(this List<int> list)
        {
            Console.Write("[");
            for (int i = 0; i < list.Count; i++)
            {
                Console.Write(list[i]);
                if (i != list.Count - 1)
                {
                    Console.Write(", ");
                }
            }

            Console.WriteLine("]");
        }
    }
}