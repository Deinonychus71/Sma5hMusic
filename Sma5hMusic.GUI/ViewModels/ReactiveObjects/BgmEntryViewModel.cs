﻿using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmEntryViewModel : ReactiveObjectBaseViewModel
    {
        public EntrySource Source { get { return DbRootViewModel.Source; } }

        public IMusicMod MusicMod { get; private set; }
        public BgmDbRootEntryViewModel DbRootViewModel { get; private set; }
        public BgmStreamSetEntryViewModel StreamSetViewModel { get; private set; }
        public BgmAssignedInfoEntryViewModel AssignedInfoViewModel { get; private set; }
        public BgmStreamPropertyEntryViewModel StreamPropertyViewModel { get; private set; }
        public BgmPropertyEntryViewModel BgmPropertyViewModel { get; private set; }

        private BgmEntryViewModel()
        {

        }

        public BgmEntryViewModel(BgmDbRootEntryViewModel bgmDbRootEntryView)
        {
            MusicMod = bgmDbRootEntryView.MusicMod;
            DbRootViewModel = bgmDbRootEntryView;
            StreamSetViewModel = bgmDbRootEntryView.StreamSetViewModel;
            AssignedInfoViewModel = StreamSetViewModel.Info0ViewModel;
            StreamPropertyViewModel = AssignedInfoViewModel.StreamPropertyViewModel;
            BgmPropertyViewModel = StreamPropertyViewModel.DataName0ViewModel;
        }

        public override ReactiveObjectBaseViewModel GetCopy()
        {
            return new BgmEntryViewModel()
            {
                MusicMod = MusicMod,
                DbRootViewModel = (BgmDbRootEntryViewModel)DbRootViewModel.GetCopy(),
                StreamSetViewModel = (BgmStreamSetEntryViewModel)StreamSetViewModel.GetCopy(),
                AssignedInfoViewModel = (BgmAssignedInfoEntryViewModel)AssignedInfoViewModel.GetCopy(),
                StreamPropertyViewModel = (BgmStreamPropertyEntryViewModel)StreamPropertyViewModel.GetCopy(),
                BgmPropertyViewModel = (BgmPropertyEntryViewModel)BgmPropertyViewModel.GetCopy()
            };
        }

        public override ReactiveObjectBaseViewModel SaveChanges()
        {
            return new BgmEntryViewModel()
            {
                MusicMod = MusicMod,
                DbRootViewModel = (BgmDbRootEntryViewModel)DbRootViewModel.SaveChanges(),
                StreamSetViewModel = (BgmStreamSetEntryViewModel)StreamSetViewModel.SaveChanges(),
                AssignedInfoViewModel = (BgmAssignedInfoEntryViewModel)AssignedInfoViewModel.SaveChanges(),
                StreamPropertyViewModel = (BgmStreamPropertyEntryViewModel)StreamPropertyViewModel.SaveChanges(),
                BgmPropertyViewModel = (BgmPropertyEntryViewModel)BgmPropertyViewModel.SaveChanges()
            };
        }
        public MusicModEntries GetMusicModEntries()
        {
            var output = new MusicModEntries();
            output.BgmDbRootEntries.Add(DbRootViewModel.GetReferenceEntity());
            output.BgmStreamSetEntries.Add(StreamSetViewModel.GetReferenceEntity());
            output.BgmAssignedInfoEntries.Add(AssignedInfoViewModel.GetReferenceEntity());
            output.BgmStreamPropertyEntries.Add(StreamPropertyViewModel.GetReferenceEntity());
            output.BgmPropertyEntries.Add(BgmPropertyViewModel.GetReferenceEntity());
            output.GameTitleEntries.Add(DbRootViewModel.GameTitleViewModel.GetReferenceEntity());
            output.SeriesEntries.Add(DbRootViewModel.SeriesViewModel.GetReferenceEntity());
            return output;
        }

        public MusicModDeleteEntries GetMusicModDeleteEntries()
        {
            var output = new MusicModDeleteEntries();
            output.BgmDbRootEntries.Add(DbRootViewModel.GetReferenceEntity().UiBgmId);
            output.BgmStreamSetEntries.Add(StreamSetViewModel.GetReferenceEntity().StreamSetId);
            output.BgmAssignedInfoEntries.Add(AssignedInfoViewModel.GetReferenceEntity().InfoId);
            output.BgmStreamPropertyEntries.Add(StreamPropertyViewModel.GetReferenceEntity().StreamId);
            output.BgmPropertyEntries.Add(BgmPropertyViewModel.GetReferenceEntity().NameId);
            return output;
        }
    }
}
