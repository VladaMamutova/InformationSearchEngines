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

            bool swap = true;
            for (int i = 0; i < list.Count && swap; ++i)
            {
                ++result.ComparisonNumber;

                swap = false;
                for (int right = 0; right < list.Count - i - 1; ++right)
                {
                    if (list[right] > list[right + 1])
                    {
                        var temp = list[right];
                        list[right] = list[right + 1];
                        list[right + 1] = temp;

                        swap = true;

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

            int minIndex;
            for (int i = 0; i < list.Count - 1; ++i)
            {
                minIndex = i;
                for (int j = i + 1; j < list.Count; ++j)
                {
                    if (list[j] < list[minIndex])
                    {
                        minIndex = j;
                    }

                    ++result.ComparisonNumber;
                }

                if (list[minIndex] != list[i])
                {
                    int temp = list[i];
                    list[i] = list[minIndex];
                    list[minIndex] = temp;

                    ++result.PermutationNumber;
                }

                ++result.ComparisonNumber;
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

            var result = QuickSortFromLeftToRight(list, 0, list.Count - 1);
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
            int pivot = (left + right) / 2;
            while (left < right)
            {
                while (left < pivot && (list[left] <= list[pivot]))
                {
                    left++;
                    ++result.ComparisonNumber;
                }

                while (right > pivot && (list[pivot] <= list[right]))
                {
                    right--;
                    ++result.ComparisonNumber;
                }
                
                if (left < right)
                {
                    int temp = list[left];
                    list[left] = list[right];
                    list[right] = temp;

                    if (left == pivot)
                    {
                        pivot = right;
                    }
                    else if (right == pivot)
                    {
                        pivot = left;
                    }

                    ++result.PermutationNumber;
                }
            }

            return pivot;
        }
    }
}