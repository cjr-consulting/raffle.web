using System.Collections.Generic;
using System.Linq;

namespace Raffle.Core
{
    public class StorageFile
    {
        public StorageFile(string name, byte[] data, string folder)
            : this(name, data, new[] { folder })
        {
        }

        public StorageFile(string name, byte[] data, IEnumerable<string> path)
        {
            Name = name;
            Data = data;
            Path = path as string[] ?? path.ToArray();
        }

        public string Name { get; }

        public byte[] Data { get; }

        public string[] Path { get; }
    }

}
