using System.Text;
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
        public static List<int> ReadNextNumbers(this FileStream fileStream, int n)
        {
            List<int> numbers = new List<int>(n);
            while (numbers.Count < n &&
                fileStream.TryReadNextNumber(out int number))
            {
                numbers.Add(number);
            }

            return numbers;
        }
        public static bool TryReadNextNumber(this FileStream fileStream, out int number)
        {
            number = 0;
            bool negative = false;

            int positionFromStart = 0;
            bool endOfNumber = false;
            do
            {
                int nextByte = fileStream.ReadByte();
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
            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                fileStream.Seek(0, SeekOrigin.End);
                while (numbers.Count < n &&
                      fileStream.TryReadPreviousNumber(out int number))
                {
                    numbers.Add(number);
                }
            }

            return numbers;
        }

        public static bool TryReadPreviousNumber(this FileStream fileStream, out int number)
        {
            number = 0;

            int positionFromEnd = 0;
            bool endOfNumber = false;
            do
            {
                if (fileStream.Position == 0) // достигнуто начало файла
                {
                    endOfNumber = true;
                }
                else
                {
                    // Перемещаемся на предыдущий символ и считываем его.
                    fileStream.Seek(-1, SeekOrigin.Current);
                    byte symbol = (byte)fileStream.ReadByte();
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
                    fileStream.Seek(-1, SeekOrigin.Current);
                }
            }
            while (!endOfNumber);

            return positionFromEnd > 0; // в числе есть хотя бы одна цифра
        }

        public static void WriteNumbersToFile(string path, List<int> numbers)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                for (int i = 0; i < numbers.Count; i++)
                {
                    fileStream.WriteNumber(numbers[i]);
                    if (i < numbers.Count - 1)
                    {
                        fileStream.WriteWhitespace();
                    }
                }
            }
        }

        public static void WriteNumber(this FileStream fileStream, int number)
        {
            fileStream.Write(Encoding.UTF8.GetBytes(number.ToString()));
        }

        public static void WriteWhitespace(this FileStream fileStream)
        {
            fileStream.Write(Encoding.UTF8.GetBytes(" "));
        }

        static public FileStream Switch(this FileStream thisStream, FileStream firstStream,
            FileStream secondStream)
        {
            return thisStream.Name == secondStream.Name
                                ? firstStream
                                : secondStream;
        }

        public static void CreateFile(string path, string content)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                fileStream.Write(Encoding.UTF8.GetBytes(content));
            }
        }

        public static string GuaranteedMoveTo(this FileInfo sourceFile, string destinationPath)
        {
            if (!sourceFile.Exists)
            {
                return "";
            }
            
            FileInfo destinationFile = new FileInfo(destinationPath);
            if (destinationFile.Exists)
            {
                destinationFile.Delete();
            }
            
            sourceFile.MoveTo(destinationPath);
            return destinationPath;
        }

        public static string GetAppDirectory()
        {
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return System.IO.Path.GetDirectoryName(appPath) ?? "";
        }
    }
}