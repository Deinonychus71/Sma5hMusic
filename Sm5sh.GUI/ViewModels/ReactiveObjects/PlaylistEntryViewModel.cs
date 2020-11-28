using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Sm5sh.Mods.Music.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sm5sh.GUI.ViewModels
{
    public class PlaylistEntryViewModel : ReactiveObject
    {
        protected readonly PlaylistEntry _refPlaylistEntry;

        //To obtain reactive change for locale
        [Reactive]
        public string Title { get; set; }

        public Dictionary<short, ObservableCollection<PlaylistEntryValueViewModel>> Tracks { get; }

        public string Id { get { return _refPlaylistEntry.Id; } }

        public PlaylistEntryViewModel(PlaylistEntry playlistEntry, Dictionary<string, BgmEntryViewModel> refBgms)
        {
            _refPlaylistEntry = playlistEntry;
            Title = playlistEntry.Id;
            Tracks = GetPlaylistValueViewModelsByOrder(refBgms);
        }

        public PlaylistEntry GetPlaylistEntryReference()
        {
            return _refPlaylistEntry;
        }

        public Dictionary<short, ObservableCollection<PlaylistEntryValueViewModel>> GetPlaylistValueViewModelsByOrder(Dictionary<string, BgmEntryViewModel> refBgms)
        {
            var output = new Dictionary<short, List<PlaylistEntryValueViewModel>>();
            for(short i = 0; i < 16; i++)
                output.Add(i, new List<PlaylistEntryValueViewModel>());

            foreach (var track in _refPlaylistEntry.Tracks)
            {
                BgmEntryViewModel vmBgmEntry = null;
                if (refBgms.ContainsKey(track.UiBgmId))
                    vmBgmEntry = refBgms[track.UiBgmId];

                for (short i = 0; i < 16; i++)
                {
                    switch (i)
                    {
                        case 0:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order0, track.Incidence0, vmBgmEntry));
                            break;
                        case 1:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order1, track.Incidence1, vmBgmEntry));
                            break;
                        case 2:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order2, track.Incidence2, vmBgmEntry));
                            break;
                        case 3:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order3, track.Incidence3, vmBgmEntry));
                            break;
                        case 4:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order4, track.Incidence4, vmBgmEntry));
                            break;
                        case 5:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order5, track.Incidence5, vmBgmEntry));
                            break;
                        case 6:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order6, track.Incidence6, vmBgmEntry));
                            break;
                        case 7:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order7, track.Incidence7, vmBgmEntry));
                            break;
                        case 8:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order8, track.Incidence8, vmBgmEntry));
                            break;
                        case 9:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order9, track.Incidence9, vmBgmEntry));
                            break;
                        case 10:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order10, track.Incidence10, vmBgmEntry));
                            break;
                        case 11:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order11, track.Incidence11, vmBgmEntry));
                            break;
                        case 12:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order12, track.Incidence12, vmBgmEntry));
                            break;
                        case 13:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order13, track.Incidence13, vmBgmEntry));
                            break;
                        case 14:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order14, track.Incidence14, vmBgmEntry));
                            break;
                        case 15:
                            output[i].Add(new PlaylistEntryValueViewModel(track.UiBgmId, track.Order15, track.Incidence15, vmBgmEntry));
                            break;
                        default:
                            throw new Exception("Wrong Order Id");
                    }
                }
            }
            return output.ToDictionary(p => p.Key, p => new ObservableCollection<PlaylistEntryValueViewModel>(p.Value));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            PlaylistEntryViewModel p = obj as PlaylistEntryViewModel;
            if (p == null)
                return false;

            return p.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
