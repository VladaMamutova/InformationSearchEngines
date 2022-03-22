namespace FileSorting.Utils
{
    public static class ListUtils
    {
        const int MAX_GENERATE_VALUE = 100;

        public static int[] GenerateValues(int n, int maxValue = MAX_GENERATE_VALUE)
        {
            if (maxValue == MAX_GENERATE_VALUE)
            {
                maxValue = Math.Max(maxValue, n * 2);
            }

            int[] values = new int[n];
            var rand = new Random(0);
            for (int i = 0; i < n; i++)
            {
                values[i] = rand.Next() % maxValue;
            }

            return values;
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