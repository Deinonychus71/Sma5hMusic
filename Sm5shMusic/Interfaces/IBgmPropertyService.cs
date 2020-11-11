using Sm5shMusic.Models;
using System.Collections.Generic;

namespace Sm5shMusic.Interfaces
{
    public interface IMsbtService
    {
        bool GenerateNewEntries(List<MsbtNewEntryModel> newMsbtEntries, string inputMsbtFile, string outputMsbtFile);
    }
}
