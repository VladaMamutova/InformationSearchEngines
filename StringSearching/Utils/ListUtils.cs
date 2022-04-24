namespace StringSearching.Utils
{
    public static class ListUtils
    {
        public static void Print(this List<int> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Console.Write(list[i]);
                if (i != list.Count - 1)
                {
                    Console.Write(", ");
                }
            }
        }
    }
}