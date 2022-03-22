namespace FileSorting.Utils
{
    public static class IntUtils
    {
        /// <summary>
        /// Добавляет левый (наибольший) разряд в число, если указанный символ является цифрой,
        /// и в случае успеха увеличивает счётчик цифр в числе.
        /// </summary>
        /// <param name="number">Число, которому необходимо добавить левый разряд.</param>
        /// <param name="symbol">ASCII-код символа.</param>
        /// <param name="digitCount">Текущее количество цифр в числе.</param>
        /// <returns>Возвращает true, если в число была добавлена первая цифра, false - иначе.</returns>
        public static bool AddLeftDigit(this ref int number, byte symbol, ref int digitCount)
        {
            if (symbol.IsDigit())
            {
                number += symbol.ToDigit() * (int)Math.Pow(10, digitCount);
                digitCount++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Добавляет правый (нулевой) разряд в число, если указанный символ является цифрой,
        /// и в случае успеха увеличивает счётчик цифр в числе.
        /// </summary>
        /// <param name="number">Число, которому необходимо добавить правый разряд.</param>
        /// <param name="symbol">ASCII-код символа.</param>
        /// <param name="digitCount">Текущее количество цифр в числе.</param>
        /// <returns>Возвращает true, если в число была добавлена последняя цифра, false - иначе.</returns>
        public static bool AddRightDigit(this ref int number, byte symbol, ref int digitCount)
        {
            if (symbol.IsDigit())
            {
                number = number * 10 + symbol.ToDigit();
                digitCount++;
                return true;
            }

            return false;
        }
    }
}
