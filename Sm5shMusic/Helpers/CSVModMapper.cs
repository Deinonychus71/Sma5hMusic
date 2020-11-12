using Sm5shMusic.Models;
using System.Collections.Generic;
using System.IO;

namespace Sm5shMusic.Helpers
{
    public static class CSVModMapper
    {
        public static MusicModConfig FromCSV(string modPath, List<MusicModCSVConfig> csvEntries)
        {
            var output = new MusicModConfig()
            {
                Name = Path.GetFileName(modPath),
                Author = "CSV",
                Songs = new List<Song>()
            };

            if(csvEntries != null)
            {
                foreach(var csvEntry in csvEntries)
                {
                    var newSong = new Song()
                    {
                        FileName = csvEntry.FileName,
                        GameTitle = new GameTitle()
                        {
                            Id = csvEntry.GameId,
                            SeriesId = csvEntry.GameSeriesId,
                            Title = new Dictionary<string, string>()
                        },
                        SongInfo = new SongInfo()
                        {
                            Id = csvEntry.SongId,
                            Rarity = csvEntry.SongRarity,
                            RecordType = csvEntry.SongRecordType,
                            Playlists = new List<PlaylistInfo>(),
                            Author = new Dictionary<string, string>(),
                            Copyright = new Dictionary<string, string>(),
                            Title = new Dictionary<string, string>()
                        }
                    };

                    foreach(var validLocale in LocaleHelper.ValidLocales)
                    {
                        var localePascal = LocaleHelper.GetPascalCaseLocale(validLocale);

                        var title = GetPropValue<string>(csvEntry, $"SongTitle{localePascal}");
                        if (!string.IsNullOrEmpty(title))
                            newSong.SongInfo.Title.Add(validLocale, title);

                        var author = GetPropValue<string>(csvEntry, $"SongAuthor{localePascal}");
                        if (!string.IsNullOrEmpty(author))
                            newSong.SongInfo.Author.Add(validLocale, author);

                        var copyright = GetPropValue<string>(csvEntry, $"SongCopyright{localePascal}");
                        if (!string.IsNullOrEmpty(copyright))
                            newSong.SongInfo.Copyright.Add(validLocale, copyright);

                        var gameTitle = GetPropValue<string>(csvEntry, $"GameTitle{localePascal}");
                        if (!string.IsNullOrEmpty(gameTitle))
                            newSong.GameTitle.Title.Add(validLocale, gameTitle);
                    }

                    output.Songs.Add(newSong);
                }
            }

            return output;
        }

        private static T GetPropValue<T>(object src, string propName)
        {
            return (T)src.GetType().GetProperty(propName).GetValue(src, null);
        }
    }
}
