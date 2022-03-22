namespace FileSorting.Utils
{
    public static class ByteUtils
    {
        public static bool IsDigit(this byte character)
        {
            return character - '0' >= 0 && character - '0' <= 9;
        }

        public static int ToDigit(this byte character)
        {
            return (int)(character - '0');

        }

        public static bool IsMinus(this byte character)
        {
            return character == '-';
        }
    }
}
