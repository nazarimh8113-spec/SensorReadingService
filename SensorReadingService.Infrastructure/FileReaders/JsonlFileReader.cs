using SensorReadingService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorReadingService.Infrastructure.FileReaders
{

    public class JsonlFileReader : IFileReader
    {
        public async IAsyncEnumerable<string> ReadLinesAsync(string path)
        {
            using var reader = new StreamReader(path);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (!string.IsNullOrWhiteSpace(line))
                    yield return line;
            }
        }
    }
}
