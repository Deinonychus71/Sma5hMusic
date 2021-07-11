using DynamicData;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using Sma5h.Mods.Music.Helpers;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sma5hMusic.GUI.ViewModels
{
    public class PlaylistPropertiesModalWindowViewModel : ModalBaseViewModel<PlaylistEntryViewModel>
    {
        private readonly ReadOnlyObservableCollection<PlaylistEntryViewModel> _playlists;
        private const string REGEX_REPLACE = @"[^a-zA-Z]";
        private readonly string REGEX_VALIDATION = $"^{MusicConstants.InternalIds.PLAYLIST_PREFIX}[a-z]+$";
        private readonly ILogger _logger;
        private readonly IGUIStateManager _guiStateManager;
        private readonly IViewModelManager _viewModelManager;

        [Reactive]
        public string PlaylistTitle { get; set; }
        [Reactive]
        public string PlaylistId { get; set; }

        [Reactive]
        public bool IsEdit { get; set; }

        public ReadOnlyObservableCollection<PlaylistEntryViewModel> Playlists { get { return _playlists; } }

        public PlaylistPropertiesModalWindowViewModel(ILogger<ModPropertiesModalWindowViewModel> logger, IViewModelManager viewModelManager,
            IGUIStateManager guiStateManager)
        {
            _logger = logger;
            _guiStateManager = guiStateManager;
            _viewModelManager = viewModelManager;

            this.WhenAnyValue(p => p.PlaylistTitle).Subscribe((o) => { FormatPlaylistId(o); });

            //Bind observables
            viewModelManager.ObservablePlaylistsEntries.Connect()
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
        }

        private void FormatPlaylistId(string playlistTitle)
        {
            if (!IsEdit)
            {
                if (string.IsNullOrEmpty(playlistTitle))
                {
                    PlaylistId = MusicConstants.InternalIds.PLAYLIST_PREFIX;
                }
                else
                {
                    PlaylistId = $"{MusicConstants.InternalIds.PLAYLIST_PREFIX}{Regex.Replace(playlistTitle, REGEX_REPLACE, string.Empty).ToLower()}";
                }
            }
        }

        protected override void LoadItem(PlaylistEntryViewModel item)
        {
            _logger.LogDebug("Load Item");

            if (item == null)
            {
                PlaylistId = string.Empty;
                PlaylistTitle = string.Empty;
                IsEdit = false;
            }
            else
            {
                IsEdit = true;
                PlaylistId = item.Id;
                PlaylistTitle = item.Title;
            }
        }

        protected override async Task<bool> SaveChanges()
        {
            _logger.LogDebug("Save Changes");

            if (!IsEdit)
            {
                await _guiStateManager.CreateNewPlaylist(new PlaylistEntry(PlaylistId, PlaylistTitle));
                _refSelectedItem = _viewModelManager.GetPlaylistViewModel(PlaylistId);
            }
            else
            {
                _refSelectedItem.Title = this.PlaylistTitle;
            }

            return true;
        }
    }
}
