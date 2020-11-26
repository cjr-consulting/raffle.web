using System;
using System.Threading.Tasks;

namespace Raffle.Core
{
    public interface IStorageService
    {
        Task<string> SaveFile(StorageFile file);
        string GetFullPath(string relativePath);
    }
}
