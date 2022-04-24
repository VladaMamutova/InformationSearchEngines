namespace StringSearching.Logic
{
    public static class StringSearcher
    {
        public static int Find(string term, string text, int startIndex, ref int comparisonCount)
        {
            if (String.IsNullOrEmpty(term) || string.IsNullOrEmpty(text) ||
                startIndex < 0 || startIndex >= text.Length)
            {
                return -1;
            }

            int foundIndex = -1;
            int termIndex = 0;
            for (int i = startIndex; i < text.Length; i++)
            {
                comparisonCount++;
                if (term[termIndex] == text[i])
                {
                    if (termIndex == 0)
                    {
                        foundIndex = i;
                    }

                    termIndex++;

                    if (termIndex >= term.Length)
                    {
                        return foundIndex;
                    }
                }
                else
                {
                    termIndex = 0;
                    if (foundIndex >= 0)
                    {
                        i = foundIndex;
                        foundIndex = -1;
                    }
                }
            }

            return -1;
        }

        public static List<int> FindAll(string term, string text, out int comparisonCount)
        {
            comparisonCount = 0;
            if (String.IsNullOrEmpty(term) || string.IsNullOrEmpty(text))
            {
                return new List<int>();
            }

            List<int> foundIndexes = new List<int>();
            int index = 0;
            while (index > -1 && index < text.Length)
            {
                index = Find(term, text, index, ref comparisonCount);
                if (index > -1)
                {
                    foundIndexes.Add(index);
                    index++;
                }
            }

            return foundIndexes;
        }
    }
}