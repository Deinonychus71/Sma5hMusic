using Sm5shMusic.Models;
using System.Collections.Generic;

namespace Sm5shMusic.Interfaces
{
    public interface IBgmPropertyService
    {
        bool GenerateBgmProperty(List<MusicModBgmEntry> bgmEntries);
    }
}
