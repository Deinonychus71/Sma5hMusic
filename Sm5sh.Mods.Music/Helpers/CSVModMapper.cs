using Sm5sh.Helpers;
using Sm5sh.Mods.Music.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sm5sh.Mods.Music.Helpers
{
    public static class CSVModMapper
    {
        public static MusicModConfig FromCSV(string modPath, List<MusicModCSVConfig> csvEntries)
        {
            var output = new MusicModConfig()
            {
                Name = Path.GetFileName(modPath),
                Author = "CSV",
                Games = new List<Game>()
            };

            if(csvEntries != null)
            {
                foreach(var csvEntry in csvEntries)
                {
                    var gameId = csvEntry.GameId;

                    var game = output.Games.FirstOrDefault(p => p.Id == gameId);
                    if (game == null)
                    {
                        game = new Game()
                        {
                            Id = csvEntry.GameId,
                            SeriesId = csvEntry.GameSeriesId,
                            Title = new Dictionary<string, string>(),
                            Songs = new List<Song>()
                        };

                        foreach (var validLocale in LocaleHelper.ValidLocales)
                        {
                            var localePascal = LocaleHelper.GetPascalCaseLocale(validLocale);
                            var gameTitle = GetPropValue<string>(csvEntry, $"GameTitle{localePascal}");
                            if (!string.IsNullOrEmpty(gameTitle))
                                game.Title.Add(validLocale, gameTitle);
                        }
                        output.Games.Add(game);
                    }

                    var newSong = new Song()
                    {
                        FileName = csvEntry.FileName,
                        Id = csvEntry.SongId,
                        RecordType = csvEntry.SongRecordType,
                        Playlists = new List<PlaylistInfo>(),
                        Author = new Dictionary<string, string>(),
                        Copyright = new Dictionary<string, string>(),
                        Title = new Dictionary<string, string>()
                    };

                    foreach(var validLocale in LocaleHelper.ValidLocales)
                    {
                        var localePascal = LocaleHelper.GetPascalCaseLocale(validLocale);

                        var title = GetPropValue<string>(csvEntry, $"SongTitle{localePascal}");
                        if (!string.IsNullOrEmpty(title))
                            newSong.Title.Add(validLocale, title);

                        var author = GetPropValue<string>(csvEntry, $"SongAuthor{localePascal}");
                        if (!string.IsNullOrEmpty(author))
                            newSong.Author.Add(validLocale, author);

                        var copyright = GetPropValue<string>(csvEntry, $"SongCopyright{localePascal}");
                        if (!string.IsNullOrEmpty(copyright))
                            newSong.Copyright.Add(validLocale, copyright);
                    }

                    for(int i = 1; i <= 9; i++)
                    {
                        var playlistId = GetPropValue<string>(csvEntry, $"PlaylistId{i}");
                        if (!string.IsNullOrEmpty(playlistId))
                            newSong.Playlists.Add(new PlaylistInfo()
                            {
                                Id = playlistId
                            });
                    }

                    game.Songs.Add(newSong);
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
