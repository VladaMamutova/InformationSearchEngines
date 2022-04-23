using System.Text;
using static System.IO.Path;
using static System.Reflection.Assembly;

namespace FileSorting.Utils
{
    public static class FileUtils
    {
        /// <summary>
        /// Считывает следующие <c>n</c> чисел из файлового потока.
        /// </summary>
        /// <param name="fileStream">Файловый поток.</param>
        /// <param name="n">Количество чисел.</param>
        /// <returns>Список из следующих n чисел, считанных с текущей позиции 
        /// файлового потока.</returns>
        public static List<int> ReadNumbers(this FileStream fileStream, int n)
        {
            List<int> numbers = new List<int>(n);
            while (numbers.Count < n &&
                fileStream.TryReadNextNumber(out int number))
            {
                numbers.Add(number);
            }

            return numbers;
        }

        /// <summary>
        /// Считывает следующее найденное число из файлового потока.
        /// </summary>
        /// <param name="fileStream">Файловый поток.</param>
        /// <param name="number">Число, необходимое для инициализации.
        /// Если операция считывания числа из файлового потока завершилась успешно,
        /// то <c>number</c> будет равно этому числу, иначе будет равно 0.</param>
        /// <returns>Возвращает true, если операция считывания числа из файла
        /// завершилась успешно, false - иначе.</returns>
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
        /// Считывает предыдующие <c>n</c> чисел из файлового потока.
        /// </summary>
        /// <param name="fileStream">Файловый поток.</param>
        /// <param name="n">Количество чисел.</param>
        /// <returns>Список из n предыдующих чисел, считанных с текущей позиции файлового потока.</returns>
        public static List<int> ReadPreviousNumbers(this FileStream fileStream, int n)
        {
            List<int> numbers = new List<int>(n);
            while (numbers.Count < n &&
                fileStream.TryReadPreviousNumber(out int number))
            {
                numbers.Add(number);
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
        
        public static List<string> GetExistingPaths(List<string> paths)
        {
            return paths.FindAll(path => File.Exists(path));
        }

        public static void WriteNumbersToFile(string path, List<int> numbers)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                fileStream.WriteNumbers(numbers);
            }
        }

        public static void WriteNumbers(this FileStream fileStream, List<int> numbers)
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

        public static void WriteNumber(this FileStream fileStream, int number)
        {
            fileStream.Write(Encoding.UTF8.GetBytes(number.ToString()));
        }

        public static void WriteWhitespace(this FileStream fileStream)
        {
            fileStream.Write(Encoding.UTF8.GetBytes(" "));
        }

        public static void CreateFile(string path)
        {
            var stream = File.Create(path);
            stream.Close();
        }

        public static string GuaranteedMoveTo(string sourcePath, string destinationPath)
        {
            if (!File.Exists(sourcePath))
            {
                return "";
            }

            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            File.Move(sourcePath, destinationPath);
            return destinationPath;
        }

        public static string GetAppDirectory()
        {
            return GetDirectoryName(GetExecutingAssembly().Location) ?? "";
        }

        public static string GetAppName()
        {
            return GetExecutingAssembly().GetName().Name ?? "";
        }
    }
}