using static FileSorting.Utils.ByteUtils;

namespace FileSorting.Utils
{
    public static class FileUtils
    {
        /// <summary>
        /// Считывает <c>n</c> первых чисел из файла.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <param name="n">Количество чисел.</param>
        /// <returns>Список из n чисел, считанных с начала файла.</returns>
        public static List<int> ReadFirstNumbersFromFile(string path, int n)
        {
            if (n <= 0)
            {
                return new List<int>();
            }

            List<int> numbers = new List<int>(n);
            using (FileStream fstream = new FileStream(path, FileMode.Open))
            {
                while (numbers.Count < n &&
                      fstream.TryReadNextNumber(out int number))
                {
                    numbers.Add(number);
                }
            }

            return numbers;
        }
        public static bool TryReadNextNumber(this FileStream fstream, out int number)
        {
            number = 0;
            bool negative = false;

            int positionFromStart = 0;
            bool endOfNumber = false;
            do
            {
                int nextByte = fstream.ReadByte();
                if (nextByte < 0) // достигнут конец потока
                {
                    endOfNumber = true;
                }
                else
                {
                    byte symbol = (byte)nextByte;
                    if (symbol.IsMinus())
                    {
                        if (positionFromStart == 0)
                        {
                            negative = true;
                        }
                        else // встретили минус в середине числа
                        {
                            endOfNumber = true;
                        }
                    }
                    else
                    {
                        bool digitAdded = number.AddRightDigit(symbol, ref positionFromStart);
                        // Если уже начали считывать число и текущий символ - не цифра, число закончилось.
                        endOfNumber = positionFromStart != 0 && !digitAdded;
                    }
                }
            }
            while (!endOfNumber);

            if (positionFromStart > 0 && negative)
            {
                number *= -1;
            }

            return positionFromStart > 0; // в числе есть хотя бы одна цифра
        }

        /// <summary>
        /// Считывает <c>n</c> последних чисел из файла.
        /// </summary>
        /// <param name="path">Путь к файлу.</param>
        /// <param name="n">Количество чисел.</param>
        /// <returns>Список из n чисел, считанных с конца файла.</returns>
        public static List<int> ReadLastNumbersFromFile(string path, int n)
        {
            if (n <= 0)
            {
                return new List<int>();
            }

            List<int> numbers = new List<int>(n);
            using (FileStream fstream = new FileStream(path, FileMode.Open))
            {
                fstream.Seek(0, SeekOrigin.End);
                while (numbers.Count < n &&
                      fstream.TryReadPreviousNumber(out int number))
                {
                    numbers.Add(number);
                }
            }

            return numbers;
        }

        public static bool TryReadPreviousNumber(this FileStream fstream, out int number)
        {
            number = 0;

            int positionFromEnd = 0;
            bool endOfNumber = false;
            do
            {
                if (fstream.Position == 0) // достигнуто начало файла
                {
                    endOfNumber = true;
                }
                else
                {
                    // Перемещаемся на предыдущий символ и считываем его.
                    fstream.Seek(-1, SeekOrigin.Current);
                    byte symbol = (byte)fstream.ReadByte();
                    if (symbol.IsMinus())
                    {
                        if (positionFromEnd > 0)
                        {
                            number *= -1;
                            endOfNumber = positionFromEnd != 0;
                        }
                    }
                    else
                    {
                        bool digitAdded = number.AddLeftDigit(symbol, ref positionFromEnd);
                        // Если уже начали считывать число и текущий символ - не цифра, число закончилось.
                        endOfNumber = positionFromEnd != 0 && !digitAdded;
                    }

                    // Устанавливаем позицию перед только что прочитанным символом.
                    fstream.Seek(-1, SeekOrigin.Current);
                }
            }
            while (!endOfNumber);

            return positionFromEnd > 0; // в числе есть хотя бы одна цифра
        }
    }
}