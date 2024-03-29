namespace FileSorting.Model
{
    public class DeviceBar
    {
        private List<Device> devices;
        private int[] inIndexes;
        private int[] outIndexes;

        public int InNumber => inIndexes.Length;
        public int OutNumber => outIndexes.Length; 
        
        public DeviceBar(string[] names, string path)
        {
            devices = new List<Device>(names.Length);
            foreach(var name in names)
            {
                devices.Add(new Device(name, path));
            }

            inIndexes = new int[0];
            outIndexes = new int[0];

            Prepare();
        }

        public void DefineInOutDevices(int inNumber, int outNumber)
        {
            if (inNumber + outNumber > devices.Count)
            {
                throw new ArgumentException("Number of input and output devices " +
                    "exceeds the total number of devices.");
            }

            inIndexes = new int[inNumber];
            for(int i = 0; i < inIndexes.Length; i++)
            {
                inIndexes[i] = i;
            }

            outIndexes = new int[outNumber];
            for(int i = 0; i < outIndexes.Length; i++)
            {
                outIndexes[i] = inNumber + i;
            }
        }

        public void Prepare()
        {
            devices.ForEach(device => device.Prepare());
        }

        public void Dispose()
        {
            devices.ForEach(device => device.Dispose());
        }

        public Device GetInDevice(int inIndex)
        {
            return GetDevice(inIndexes[inIndex % inIndexes.Length]);
        }

        public Device GetOutDevice(int outIndex)
        {
            return GetDevice(outIndexes[outIndex % outIndexes.Length]);
        }

        public Device GetDevice(int index)
        {
            if (index < 0 || index >= devices.Count)
            {
                throw new Exception("Out of devices range");
            }

            return devices[index];
        }

        public void ShiftForward()
        {
            if (devices.Count < 1)
            {
                return;
            }

            int nextInIndex = inIndexes.Last() + 1;
            for(int i = 0; i < inIndexes.Length; i++)
            {
                inIndexes[i] = (nextInIndex + i) % devices.Count;
            }

            int nextOutIndex = inIndexes.Last() + 1;
            for(int i = 0; i < outIndexes.Length; i++)
            {
                outIndexes[i] = (nextOutIndex + i) % devices.Count;
            }
        }

        public static DeviceBar Generate(int size, string path)
        {
            if (size < 0)
            {
                throw new ArgumentException("Invalid size. It must be greater than zero.");
            }

            List<string> names = new List<string> { "A", "B", "C", "D", "E", 
                "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q",
                "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            if (size > names.Count)
            {
                throw new ArgumentException("Failed to generate Device Bar " +
                    $"of {size} elements. Max device number is {names.Count}");
            }

            return new DeviceBar(names.GetRange(0, size).ToArray(), path);
        }
    }
}
