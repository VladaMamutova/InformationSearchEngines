using SortingAlgorithmsComparison.Model;
using System.Diagnostics;

namespace SortingAlgorithmsComparison.Logic
{
    public static class Sorting
    {
        public static Result BubbleSort(this List<int> list)
        {
            Result result = new Result();
            result.Timer.Start();

            for (int i = 0; i < list.Count; ++i)
            {
                for (int j = 0; j < list.Count - i - 1; ++j)
                {
                    if (list[j] > list[j + 1])
                    {
                        var temp = list[j];
                        list[j] = list[j + 1];
                        list[j + 1] = temp;

                        ++result.PermutationNumber;
                    }

                    ++result.ComparisonNumber;
                }
            }

            result.Timer.Stop();
            return result;
        }

        public static Result InsertionSort(this List<int> list)
        {
            Result result = new Result();
            result.Timer.Start();

            for (int i = 1; i < list.Count; ++i)
            {
                int current = list[i];
                int j = i;
                while (j > 0 && list[j - 1] > current)
                {
                    list[j] = list[j - 1];
                    --j;

                    ++result.ComparisonNumber;
                    ++result.PermutationNumber;
                }

                ++result.ComparisonNumber;

                if (j != i) {
                    list[j] = current;

                    ++result.PermutationNumber;
                }
            }

            result.Timer.Stop();
            return result;
        }

        public static Result QuickSort(this List<int> list)
        {
            if (list.Count < 1)
            {
                return new Result();
            }
            
            Stopwatch timer = new Stopwatch();
            timer.Start();
            
            var result = QuickSortFromLeftToRight(list, 0, list.Count -1);
            timer.Stop();
            result.Timer = timer;

            return result;
        }

        public static Result QuickSortFromLeftToRight(List<int> list, int left, int right)
        {
            Result result = new Result();

            if (left < right)
            {
                int pivotIndex = Partition(list, left, right, ref result);
                Result resultLeft = QuickSortFromLeftToRight(list, left, pivotIndex - 1);
                Result resultRight = QuickSortFromLeftToRight(list, pivotIndex + 1, right);

                result.ComparisonNumber += resultLeft.ComparisonNumber + resultRight.ComparisonNumber;
                result.PermutationNumber += resultLeft.PermutationNumber + resultRight.PermutationNumber;
            }

            return result;
        }

        private static int Partition(List<int> list, int left, int right,
            ref Result result)
        {
            int pivotIndex = (left + right) / 2; // опорный элемент - средний
            int pivot = list[pivotIndex];

            int temp = list[right];
            list[right] = list[pivotIndex];
            list[pivotIndex] = temp;
            ++result.PermutationNumber;

            var less = left;
            for (int i = left; i < right; ++i)
            {
                if (list[i] < pivot)
                {
                    temp = list[i];
                    list[i] = list[less];
                    list[less] = temp;
                    ++less;

                    ++result.PermutationNumber;
                }

                ++result.ComparisonNumber;
            }

            temp = list[right];
            list[right] = list[less];
            list[less] = temp;
            ++result.PermutationNumber;

            return less;
        }
    }
}