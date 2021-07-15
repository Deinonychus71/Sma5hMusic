using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music.Models;
using Sma5h.Mods.Music.Models.PlaylistEntryModels;
using Sma5hMusic.GUI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sma5hMusic.GUI.ViewModels
{
    public class PlaylistEntryViewModel : ReactiveObjectBaseViewModel
    {
        protected readonly PlaylistEntry _refPlaylistEntry;
        private List<string> _allTracks;
        private bool _cachedTracks = false;
        private bool _cachedModTracks = false;

        //To obtain reactive change for locale
        [Reactive]
        public string Title { get; set; }

        public Dictionary<short, ObservableCollection<PlaylistEntryValueViewModel>> Tracks { get; }

        public string Id { get { return _refPlaylistEntry.Id; } }

        public List<string> AllTracks { get { return GetAllTracks(); } }

        public List<string> AllModTracks { get { return GetAllModTracks(); } }

        public PlaylistEntryViewModel(PlaylistEntry playlistEntry, Dictionary<string, BgmDbRootEntryViewModel> refBgms = null)
        {
            _refPlaylistEntry = playlistEntry;
            if (string.IsNullOrEmpty(playlistEntry.Title))
                Title = Constants.GetBgmPlaylistName(playlistEntry.Id);
            else
                Title = playlistEntry.Title;
            Tracks = ToPlaylistValueViewModelsByOrder(refBgms);
            _cachedTracks = false;
            _cachedModTracks = false;
            for (short i = 0; i < 16; i++)
                ReorderSongs(i);
        }

        public PlaylistEntry GetPlaylistEntryReference()
        {
            return _refPlaylistEntry;
        }

        public List<string> GetAllTracks()
        {
            if (!_cachedTracks || _allTracks == null)
            {
                _allTracks = Tracks.SelectMany(p => p.Value).Select(p => p.UiBgmId).Distinct().ToList();
                _cachedTracks = true;
            }
            return _allTracks;
        }

        public List<string> GetAllModTracks()
        {
            if (!_cachedModTracks || _allTracks == null)
            {
                _allTracks = Tracks.SelectMany(p => p.Value).Where(p => p.IsBgmMod).Select(p => p.UiBgmId).Distinct().ToList();
                _cachedModTracks = true;
            }
            return _allTracks;
        }

        public void ReorderSongs(short orderId)
        {
            short i = 0;
            var listPlaylistTracks = Tracks[orderId].Where(p => !p.Hidden).OrderBy(p => p.Order);
            foreach (var track in listPlaylistTracks)
            {
                track.Order = i;
                i++;
            }
        }

        public void ReorderAndUnhideSong(short orderId, PlaylistEntryValueViewModel vmPlaylistTracksToReorder)
        {
            short i = 1;
            var listPlaylistTracks = Tracks[orderId].Where(p => !p.Hidden).OrderBy(p => p.Order);
            foreach (var track in listPlaylistTracks)
            {
                track.Order = i;
                i++;
            }
            vmPlaylistTracksToReorder.Order = 0;
        }

        public void ReorderSongs(short orderId, IEnumerable<PlaylistEntryValueViewModel> vmPlaylistTracksToReorder, short newPosition)
        {
            var minAffected = newPosition;
            var maxAffected = newPosition;

            var listSelectedVmPlaylistTracks = new List<PlaylistEntryValueViewModel>();
            var orderValues = new List<short>();
            foreach (var vmPlaylistTrack in Tracks[orderId].Where(p => !p.Hidden && vmPlaylistTracksToReorder.Contains(p)).OrderBy(p => p.Order))
            {
                listSelectedVmPlaylistTracks.Add(vmPlaylistTrack);
                if (vmPlaylistTrack.Order < minAffected)
                    minAffected = vmPlaylistTrack.Order;
                else if (vmPlaylistTrack.Order > maxAffected)
                    maxAffected = vmPlaylistTrack.Order;
                orderValues.Add(vmPlaylistTrack.Order);
            }

            var listUnselectedButAffected = new List<PlaylistEntryValueViewModel>();
            foreach (var vmPlaylistTrack in Tracks[orderId]
                .Where(p => !p.Hidden && p.Order >= minAffected && p.Order <= maxAffected && !vmPlaylistTracksToReorder.Contains(p))
                .OrderBy(p => p.Order))
            {
                listUnselectedButAffected.Add(vmPlaylistTrack);
                orderValues.Add(vmPlaylistTrack.Order);
            }

            orderValues = orderValues.OrderBy(p => p).ToList();

            for (short i = minAffected; i <= maxAffected; i++)
            {
                if (listUnselectedButAffected.Count > 0 && listUnselectedButAffected.First().Order < newPosition)
                {
                    listUnselectedButAffected[0].Order = orderValues[0];
                    orderValues.RemoveAt(0);
                    listUnselectedButAffected.RemoveAt(0);
                }
                else
                {
                    if (listSelectedVmPlaylistTracks.Count > 0)
                    {
                        listSelectedVmPlaylistTracks[0].Order = orderValues[0];
                        orderValues.RemoveAt(0);
                        listSelectedVmPlaylistTracks.RemoveAt(0);
                    }
                    else if (listUnselectedButAffected.Count > 0)
                    {
                        listUnselectedButAffected[0].Order = orderValues[0];
                        orderValues.RemoveAt(0);
                        listUnselectedButAffected.RemoveAt(0);
                    }
                }
            }
        }

        public List<PlaylistEntryValueViewModel> AddSongs(List<BgmDbRootEntryViewModel> sourceObjs, short orderId, short destinationIndex, ushort incidence)
        {
            var output = new List<PlaylistEntryValueViewModel>();

            for (short i = 0; i < 16; i++)
            {
                var tracks = Tracks[i].Where(p => !p.Hidden).OrderBy(p => p.Order).ToList();
                foreach (var track in tracks.Where(p => p.Order >= destinationIndex))
                    track.Order += (short)sourceObjs.Count;

                short k = destinationIndex;
                foreach (var sourceObj in sourceObjs)
                {
                    var newValue = new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, k, incidence, sourceObj);
                    Tracks[i].Add(newValue);
                    if (i == orderId)
                        output.Add(newValue);
                    k++;
                }
                ReorderSongs(i);
            }

            _cachedTracks = false;
            _cachedModTracks = false;

            return output;
        }

        public void AddSong(BgmDbRootEntryViewModel sourceObj, PlaylistValueEntry playlistValueEntry)
        {
            Tracks[0].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order0, playlistValueEntry.Incidence0, sourceObj));
            Tracks[1].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order1, playlistValueEntry.Incidence1, sourceObj));
            Tracks[2].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order2, playlistValueEntry.Incidence2, sourceObj));
            Tracks[3].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order3, playlistValueEntry.Incidence3, sourceObj));
            Tracks[4].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order4, playlistValueEntry.Incidence4, sourceObj));
            Tracks[5].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order5, playlistValueEntry.Incidence5, sourceObj));
            Tracks[6].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order6, playlistValueEntry.Incidence6, sourceObj));
            Tracks[7].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order7, playlistValueEntry.Incidence7, sourceObj));
            Tracks[8].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order8, playlistValueEntry.Incidence8, sourceObj));
            Tracks[9].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order9, playlistValueEntry.Incidence9, sourceObj));
            Tracks[10].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order10, playlistValueEntry.Incidence10, sourceObj));
            Tracks[11].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order11, playlistValueEntry.Incidence11, sourceObj));
            Tracks[12].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order12, playlistValueEntry.Incidence12, sourceObj));
            Tracks[13].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order13, playlistValueEntry.Incidence13, sourceObj));
            Tracks[14].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order14, playlistValueEntry.Incidence14, sourceObj));
            Tracks[15].Add(new PlaylistEntryValueViewModel(this, sourceObj.UiBgmId, playlistValueEntry.Order15, playlistValueEntry.Incidence15, sourceObj));

            for (short i = 0; i < 16; i++)
                ReorderSongs(i);

            _cachedTracks = false;
            _cachedModTracks = false;
        }

        public void RemoveSong(string bgmId)
        {
            for (short i = 0; i < 16; i++)
            {
                var refValue = Tracks[i].FirstOrDefault(p => p.UiBgmId == bgmId);
                if (refValue != null)
                    Tracks[i].Remove(refValue);
                ReorderSongs(i);
            }
            _cachedTracks = false;
            _cachedModTracks = false;
        }

        public PlaylistEntry ToPlaylistEntry()
        {
            var output = new PlaylistEntry(this.Id, Title);

            var nbrItems = Tracks[0].Count;
            for (int i = 0; i < nbrItems; i++)
            {
                var newTrack = new PlaylistValueEntry() { UiBgmId = Tracks[0][i].UiBgmId };
                output.Tracks.Add(newTrack);

                newTrack.Order0 = Tracks[0][i].Order;
                newTrack.Order1 = Tracks[1][i].Order;
                newTrack.Order2 = Tracks[2][i].Order;
                newTrack.Order3 = Tracks[3][i].Order;
                newTrack.Order4 = Tracks[4][i].Order;
                newTrack.Order5 = Tracks[5][i].Order;
                newTrack.Order6 = Tracks[6][i].Order;
                newTrack.Order7 = Tracks[7][i].Order;
                newTrack.Order8 = Tracks[8][i].Order;
                newTrack.Order9 = Tracks[9][i].Order;
                newTrack.Order10 = Tracks[10][i].Order;
                newTrack.Order11 = Tracks[11][i].Order;
                newTrack.Order12 = Tracks[12][i].Order;
                newTrack.Order13 = Tracks[13][i].Order;
                newTrack.Order14 = Tracks[14][i].Order;
                newTrack.Order15 = Tracks[15][i].Order;
                newTrack.Incidence0 = Tracks[0][i].Incidence;
                newTrack.Incidence1 = Tracks[1][i].Incidence;
                newTrack.Incidence2 = Tracks[2][i].Incidence;
                newTrack.Incidence3 = Tracks[3][i].Incidence;
                newTrack.Incidence4 = Tracks[4][i].Incidence;
                newTrack.Incidence5 = Tracks[5][i].Incidence;
                newTrack.Incidence6 = Tracks[6][i].Incidence;
                newTrack.Incidence7 = Tracks[7][i].Incidence;
                newTrack.Incidence8 = Tracks[8][i].Incidence;
                newTrack.Incidence9 = Tracks[9][i].Incidence;
                newTrack.Incidence10 = Tracks[10][i].Incidence;
                newTrack.Incidence11 = Tracks[11][i].Incidence;
                newTrack.Incidence12 = Tracks[12][i].Incidence;
                newTrack.Incidence13 = Tracks[13][i].Incidence;
                newTrack.Incidence14 = Tracks[14][i].Incidence;
                newTrack.Incidence15 = Tracks[15][i].Incidence;
            }
            return output;
        }

        public Dictionary<short, ObservableCollection<PlaylistEntryValueViewModel>> ToPlaylistValueViewModelsByOrder(Dictionary<string, BgmDbRootEntryViewModel> refBgms)
        {
            var output = new Dictionary<short, List<PlaylistEntryValueViewModel>>();
            for (short i = 0; i < 16; i++)
                output.Add(i, new List<PlaylistEntryValueViewModel>());

            foreach (var track in _refPlaylistEntry.Tracks)
            {
                BgmDbRootEntryViewModel vmBgmEntry = null;
                if (refBgms != null && refBgms.ContainsKey(track.UiBgmId))
                    vmBgmEntry = refBgms[track.UiBgmId];

                for (short i = 0; i < 16; i++)
                {
                    switch (i)
                    {
                        case 0:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order0, track.Incidence0, vmBgmEntry));
                            break;
                        case 1:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order1, track.Incidence1, vmBgmEntry));
                            break;
                        case 2:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order2, track.Incidence2, vmBgmEntry));
                            break;
                        case 3:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order3, track.Incidence3, vmBgmEntry));
                            break;
                        case 4:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order4, track.Incidence4, vmBgmEntry));
                            break;
                        case 5:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order5, track.Incidence5, vmBgmEntry));
                            break;
                        case 6:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order6, track.Incidence6, vmBgmEntry));
                            break;
                        case 7:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order7, track.Incidence7, vmBgmEntry));
                            break;
                        case 8:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order8, track.Incidence8, vmBgmEntry));
                            break;
                        case 9:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order9, track.Incidence9, vmBgmEntry));
                            break;
                        case 10:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order10, track.Incidence10, vmBgmEntry));
                            break;
                        case 11:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order11, track.Incidence11, vmBgmEntry));
                            break;
                        case 12:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order12, track.Incidence12, vmBgmEntry));
                            break;
                        case 13:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order13, track.Incidence13, vmBgmEntry));
                            break;
                        case 14:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order14, track.Incidence14, vmBgmEntry));
                            break;
                        case 15:
                            output[i].Add(new PlaylistEntryValueViewModel(this, track.UiBgmId, track.Order15, track.Incidence15, vmBgmEntry));
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

            if (!(obj is PlaylistEntryViewModel p))
                return false;

            return p.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override ReactiveObjectBaseViewModel GetCopy()
        {
            return this; //Not needed for this object
        }

        public override ReactiveObjectBaseViewModel SaveChanges()
        {
            return this; //Not needed for this object
        }
    }
}
