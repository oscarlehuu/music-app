using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Interface
{
    public interface IJsonReader
    {
        T ReadJsonFile<T>(string filePath);
    }
}