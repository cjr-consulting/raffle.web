using System;
using System.IO;
using System.Reflection;

namespace Raffle.Core.Shared
{
    public class EmbeddedResourceReader
    {
        private readonly Assembly assembly = Assembly.GetExecutingAssembly();

        public EmbeddedResourceReader()
        {

        }

        public string GetContents(string resourceName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
