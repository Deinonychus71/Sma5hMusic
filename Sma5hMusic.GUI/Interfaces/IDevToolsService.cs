using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sma5hMusic.GUI.Interfaces
{
    public interface IDevToolsService
    {
        Task<bool> ExportToCSV(string exportPath);
    }
}
