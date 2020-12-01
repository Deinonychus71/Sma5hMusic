using Avalonia.Controls;
using DynamicData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Sm5sh.GUI.Helpers;
using Sm5sh.Mods.Music;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sm5sh.GUI.ViewModels
{
    public class PlaylistPropertiesModalWindowViewModel : ReactiveValidationObject
    {
        private readonly IOptions<Sm5shMusicOptions> _config;
        private readonly ReadOnlyObservableCollection<PlaylistEntryViewModel> _playlists;
        private const string REGEX_REPLACE = @"[^a-zA-Z]";
        private string REGEX_VALIDATION = $"^{Constants.PLAYLIST_TITLE_PREFIX}[a-z]+$";
        private readonly ILogger _logger;

        [Reactive]
        public string PlaylistTitle { get; set; }
        [Reactive]
        public string PlaylistId { get; set; }

        [Reactive]
        public bool IsEdit { get; set; }

        public PlaylistEntryViewModel SelectedPlaylistEntry { get; private set; }

        public ReadOnlyObservableCollection<PlaylistEntryViewModel> Playlists { get { return _playlists; } }

        public ReactiveCommand<Window, Unit> ActionOK { get; }
        public ReactiveCommand<Window, Unit> ActionCancel { get; }

        public PlaylistPropertiesModalWindowViewModel(ILogger<ModPropertiesModalWindowViewModel> logger, IOptions<Sm5shMusicOptions> config, IObservable<IChangeSet<PlaylistEntryViewModel, string>> observablePlaylists)
        {
            _config = config;
            _logger = logger;

            this.WhenAnyValue(p => p.PlaylistTitle).Subscribe((o) => { FormatPlaylistId(o); });

            //Bind observables
            observablePlaylists
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _playlists)
                .DisposeMany()
                .Subscribe();

            this.ValidationRule(p => p.PlaylistId,
                p => !string.IsNullOrEmpty(p) && Regex.IsMatch(p, REGEX_VALIDATION),
                $"The playlist ID is invalid.");

            this.ValidationRule(p => p.PlaylistId,
                p => !string.IsNullOrEmpty(p) && ((!_playlists.Select(p => p.Id).Contains(p)) || IsEdit),
                $"The playlist ID already exists.");

            //Validation
            this.ValidationRule(p => p.PlaylistTitle,
                p => !string.IsNullOrEmpty(p),
                $"Please enter a Title.");

            var canExecute = this.WhenAnyValue(x => x.ValidationContext.IsValid);
            ActionOK = ReactiveCommand.Create<Window>(SubmitDialogOK, canExecute);
            ActionCancel = ReactiveCommand.Create<Window>(SubmitDialogCancel);
        }

        public void LoadPlaylist(PlaylistEntryViewModel vmPlaylist)
        {
            if (vmPlaylist == null)
            {
                SelectedPlaylistEntry = null;
                PlaylistId = string.Empty;
                PlaylistTitle = string.Empty;
                IsEdit = false;
            }
            else
            {
                SelectedPlaylistEntry = vmPlaylist;
                IsEdit = true;
                PlaylistId = vmPlaylist.Id;
                PlaylistTitle = vmPlaylist.Title;
            }
        }

        private void FormatPlaylistId(string playlistTitle)
        {
            if (!IsEdit)
            {
                if (string.IsNullOrEmpty(playlistTitle))
                {
                    PlaylistId = Constants.PLAYLIST_TITLE_PREFIX;
                }
                else
                {
                    PlaylistId = $"{Constants.PLAYLIST_TITLE_PREFIX}{Regex.Replace(playlistTitle, REGEX_REPLACE, string.Empty).ToLower()}";
                }
            }
        }

        public void SubmitDialogOK(Window window)
        {
            if (!IsEdit)
            {
                SelectedPlaylistEntry = new PlaylistEntryViewModel(new Mods.Music.Models.PlaylistEntry(PlaylistId, PlaylistTitle), null);
            }
            else
            {
                SelectedPlaylistEntry.Title = this.PlaylistTitle;
            }

            window.Close(window);
        }

        public void SubmitDialogCancel(Window window)
        {
            window.Close();
        }
    }
}
