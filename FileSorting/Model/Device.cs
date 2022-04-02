namespace FileSorting.Model
{
    /// <summary>
    /// Запоминающее устройство.
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Имя устройства и имя директории, в которой
        /// находятся файлы устройства.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Полный путь к директории устройства.
        /// Имя директории является именем устройства.
        /// </summary>
        public string DirectoryPath { get; }

        /// <summary>
        /// Инициализирует новый объект запоминающего устройства,
        /// создаёт директорию устройства.
        /// </summary>
        /// <param name="name">Имя запоминающегося устройства.</param>
        /// <param name="parentPath">Родительская директория устройства.</param>
        public Device(string name, string parentPath)
        {
            Name = name;
            DirectoryPath = Path.Combine(parentPath, name);
        }

        /// <summary>
        /// Подготавливает устройство к работе, создавая его директорию.
        /// </summary>
        public void Prepare()
        {
            Directory.CreateDirectory(DirectoryPath);
        }

        /// <summary>
        /// Освобождает устройство, удаляя файлы в его директории и директорию.
        /// </summary>
        public void Dispose()
        {
            DirectoryInfo directory = new DirectoryInfo(DirectoryPath);
            foreach (FileInfo file in directory.EnumerateFiles())
            {
                file.Delete();
            }

            directory.Delete();
        }

        /// <summary>
        /// Генерирует полный путь к внутреннему объекту
        /// по его относительному пути.
        /// </summary>
        /// <param name="relativePath">Относительный путь объекта.</param>
        /// <returns>Возвращает полный путь к объекту.</returns>
        public string GetFullPath(string relativePath)
        {            
            return Path.Combine(DirectoryPath, relativePath);
        }

    }
}