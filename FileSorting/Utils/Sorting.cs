namespace FileSorting.Utils
{
    public static class Sorting
    {
        public static void QuickSort(this List<int> list)
        {
            if (list.Count < 1)
            {
                return;
            }
            
            QuickSortFromLeftToRight(list, 0, list.Count -1);
        }

        public static void QuickSortFromLeftToRight(List<int> list, int left, int right)
        {
            if (left < right)
            {
                int pivotIndex = Partition(list, left, right);
                QuickSortFromLeftToRight(list, left, pivotIndex - 1);
                QuickSortFromLeftToRight(list, pivotIndex + 1, right);
            }
        }

        private static int Partition(List<int> list, int left, int right)
        {
            int pivotIndex = (left + right) / 2; // опорный элемент - средний
            int pivot = list[pivotIndex];

            int temp = list[right];
            list[right] = list[pivotIndex];
            list[pivotIndex] = temp;

            var less = left;
            for (int i = left; i < right; ++i)
            {
                if (list[i] < pivot)
                {
                    temp = list[i];
                    list[i] = list[less];
                    list[less] = temp;
                    ++less;
                }
            }

            temp = list[right];
            list[right] = list[less];
            list[less] = temp;

            return less;
        }
    }
}