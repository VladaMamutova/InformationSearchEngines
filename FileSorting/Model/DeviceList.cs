namespace FileSorting.Model
{
    public class DeviceList
    {
        public string[] Names { get; }
        public string[] Directories { get; }

        public int ActiveIndex { get; private set; }
        public int ActiveNumber { get; private set; }

        public DeviceList(string[] names, string rootPath, int activeNumber)
        {
            Names = names;
            Directories = new string[names.Length];
            for (int i = 0; i < Directories.Length; i++)
            {
                Directories[i] = Path.Combine(rootPath, GetDeviceName(i));
            }

            ActiveIndex = 0;
            ActiveNumber = Math.Min(Names.Length, activeNumber);
        }

        public void Prepare()
        {
            foreach (var directory in Directories)
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void Dispose()
        {
            foreach (var directory in Directories)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directory);
                foreach (FileInfo file in dirInfo.EnumerateFiles())
                {
                    file.Delete();
                }

                dirInfo.Delete();
            }
        }

        public void ActivateNextDevices()
        {
            ActiveIndex = (ActiveIndex + ActiveNumber) % Names.Length;
        }

        public void ActivatePreviousDevices()
        {
            ActiveIndex = Names.Length - Math.Abs(ActiveIndex - ActiveNumber);
        }

        public string GenerateSubfilePath(int subfileIndex, bool forNextActiveGroup = false)
        {
            if (forNextActiveGroup)
            {
                ActivateNextDevices();
            }

            string subfilePath = Path.Combine(GetActiveDeviceDirectory(subfileIndex),
                GenerateSubfileName(subfileIndex));

            if (forNextActiveGroup)
            {
                ActivatePreviousDevices();
            }

            return subfilePath;
        }

        private string GenerateSubfileName(int subfileIndex)
        {
            string number = (subfileIndex / ActiveNumber + 1).ToString();
            return GetActiveDeviceName(subfileIndex).ToLower() + number + ".txt";
        }

        private string GetDeviceName(int subfileIndex)
        {
            return Names[subfileIndex % Names.Length];
        }

        private string GetActiveDeviceName(int subfileIndex)
        {
            return Names[
               (ActiveIndex + subfileIndex % ActiveNumber) % Names.Length];
        }

        private string GetDeviceDirectory(int subfileIndex)
        {
            return Directories[ActiveIndex + subfileIndex % ActiveNumber];
        }

        private string GetActiveDeviceDirectory(int subfileIndex)
        {
            return Directories[
                (ActiveIndex + subfileIndex % ActiveNumber) % Names.Length];
        }
    }
}
