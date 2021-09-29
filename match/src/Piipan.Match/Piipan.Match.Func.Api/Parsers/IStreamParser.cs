using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Piipan.Match.Func.Api.Parsers
{
    public interface IStreamParser<T>
    {
        Task<T> Parse(Stream stream);
    }
}