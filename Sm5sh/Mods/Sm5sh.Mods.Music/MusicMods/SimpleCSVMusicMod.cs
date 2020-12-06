using CsvHelper;
using Microsoft.Extensions.Logging;
using Sm5sh.Helpers;
using Sm5sh.Mods.Music.Helpers;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.MusicMods.SimpleCSVMusicModModels;
using Sm5sh.Mods.Music.MusicMods.SimpleMusicModModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sm5sh.Mods.Music.MusicMods
{
    public class SimpleCSVMusicMod : SimpleMusicMod, IMusicMod
    {
        public SimpleCSVMusicMod(IAudioMetadataService audioMetadataService, ILogger<IMusicMod> logger, string musicModPath)
       : base(audioMetadataService, logger, musicModPath)
        {
        }

        protected override MusicModConfig LoadMusicModConfig()
        {
            //Check if disabled
            if (Path.GetFileName(ModPath).StartsWith("."))
            {
                _logger.LogDebug("{MusicModFile} is disabled.");
                return null;
            }

            var metadataCsvFile = Path.Combine(ModPath, MusicConstants.MusicModFiles.MUSIC_MOD_METADATA_CSV_FILE);
            if (File.Exists(metadataCsvFile))
            {
                _logger.LogDebug("Parsing {MusicModFile} CSV File", metadataCsvFile);
                using (var reader = new StreamReader(metadataCsvFile))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.PrepareHeaderForMatch = (header, index) => Regex.Replace(header, @"\s", string.Empty);
                        csv.Configuration.MissingFieldFound = null;
                        csv.Configuration.HeaderValidated = null;
                        List<MusicModCSVConfig> records = null;
                        try
                        {
                            records = csv.GetRecords<MusicModCSVConfig>().ToList();
                        }
                        catch (Exception e)
                        {
                            _logger.LogError("MusicModFile {MusicModFile} exists, but an error occured during parsing.", metadataCsvFile);
                            _logger.LogDebug("MusicModFile {MusicModFile} exists, but an error occured during parsing. {Exception} - {Stacktrace}", metadataCsvFile, e.Message, e.StackTrace);
                            return null;
                        }
                        _logger.LogDebug("Parsed {MusicModFile} CSV File", metadataCsvFile);
                        var output = FromCSV(ModPath, records);
                        _logger.LogDebug("Mapped {MusicModFile} CSV File to MusicModConfig model", metadataCsvFile);

                        //File backup, as it's an old version of mod
                        File.Copy(metadataCsvFile, $"{metadataCsvFile}.bak");

                        return output;
                    }
                }
            }
            else
            {
                //Cannot load music mod
                _logger.LogError("MusicModFile {MusicModFile} does not exist!", ModPath);
            }

            return null;
        }

        protected override bool SaveMusicModConfig()
        {
            throw new NotImplementedException();
        }

        public MusicModConfig FromCSV(string modPath, List<MusicModCSVConfig> csvEntries)
        {
            var output = new MusicModConfig()
            {
                Name = Path.GetFileName(modPath),
                Author = "CSV",
                Games = new List<Game>()
            };

            if (csvEntries != null)
            {
                foreach (var csvEntry in csvEntries)
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

                    foreach (var validLocale in LocaleHelper.ValidLocales)
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

                    for (int i = 1; i <= 9; i++)
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

        private T GetPropValue<T>(object src, string propName)
        {
            return (T)src.GetType().GetProperty(propName).GetValue(src, null);
        }
    }

    namespace SimpleCSVMusicModModels
    {
        public class MusicModCSVConfig
        {
            public string FileName { get; set; }

            //GameTitle
            public string GameId { get; set; }
            public string GameTitleEuDe { get; set; }
            public string GameTitleEuEn { get; set; }
            public string GameTitleEuEs { get; set; }
            public string GameTitleEuFr { get; set; }
            public string GameTitleEuIt { get; set; }
            public string GameTitleEuNl { get; set; }
            public string GameTitleEuRu { get; set; }
            public string GameTitleJpJa { get; set; }
            public string GameTitleKrKo { get; set; }
            public string GameTitleUsEn { get; set; }
            public string GameTitleUsEs { get; set; }
            public string GameTitleUsFr { get; set; }
            public string GameTitleZhCn { get; set; }
            public string GameTitleZhTw { get; set; }
            public string GameSeriesId { get; set; }

            //Song
            public string SongId { get; set; }
            public string SongRecordType { get; set; }
            public string PlaylistId1 { get; set; }
            public string PlaylistId2 { get; set; }
            public string PlaylistId3 { get; set; }
            public string PlaylistId4 { get; set; }
            public string PlaylistId5 { get; set; }
            public string PlaylistId6 { get; set; }
            public string PlaylistId7 { get; set; }
            public string PlaylistId8 { get; set; }
            public string PlaylistId9 { get; set; }
            public string SongTitleEuDe { get; set; }
            public string SongTitleEuEn { get; set; }
            public string SongTitleEuEs { get; set; }
            public string SongTitleEuFr { get; set; }
            public string SongTitleEuIt { get; set; }
            public string SongTitleEuNl { get; set; }
            public string SongTitleEuRu { get; set; }
            public string SongTitleJpJa { get; set; }
            public string SongTitleKrKo { get; set; }
            public string SongTitleUsEn { get; set; }
            public string SongTitleUsEs { get; set; }
            public string SongTitleUsFr { get; set; }
            public string SongTitleZhCn { get; set; }
            public string SongTitleZhTw { get; set; }

            public string SongAuthorEuDe { get; set; }
            public string SongAuthorEuEn { get; set; }
            public string SongAuthorEuEs { get; set; }
            public string SongAuthorEuFr { get; set; }
            public string SongAuthorEuIt { get; set; }
            public string SongAuthorEuNl { get; set; }
            public string SongAuthorEuRu { get; set; }
            public string SongAuthorJpJa { get; set; }
            public string SongAuthorKrKo { get; set; }
            public string SongAuthorUsEn { get; set; }
            public string SongAuthorUsEs { get; set; }
            public string SongAuthorUsFr { get; set; }
            public string SongAuthorZhCn { get; set; }
            public string SongAuthorZhTw { get; set; }

            public string SongCopyrightEuDe { get; set; }
            public string SongCopyrightEuEn { get; set; }
            public string SongCopyrightEuEs { get; set; }
            public string SongCopyrightEuFr { get; set; }
            public string SongCopyrightEuIt { get; set; }
            public string SongCopyrightEuNl { get; set; }
            public string SongCopyrightEuRu { get; set; }
            public string SongCopyrightJpJa { get; set; }
            public string SongCopyrightKrKo { get; set; }
            public string SongCopyrightUsEn { get; set; }
            public string SongCopyrightUsEs { get; set; }
            public string SongCopyrightUsFr { get; set; }
            public string SongCopyrightZhCn { get; set; }
            public string SongCopyrightZhTw { get; set; }
        }
    }
}
