using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSupport.Interfaces
{
    public interface IGenesysInterface
    {
        Task<(bool Success, string Message)> LoadGenesysDB();
        int? GetGPoint(ulong id);
    }
}
