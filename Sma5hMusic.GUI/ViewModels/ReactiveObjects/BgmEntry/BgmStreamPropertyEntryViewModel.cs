﻿using AutoMapper;
using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Interfaces;

namespace Sma5hMusic.GUI.ViewModels
{
    public class BgmStreamPropertyEntryViewModel : BgmBaseViewModel<BgmStreamPropertyEntry>
    {
        public string StreamId { get; }
        public string DataName0 { get; set; }
        public string DataName1 { get; set; }
        public string DataName2 { get; set; }
        public string DataName3 { get; set; }
        public string DataName4 { get; set; }
        public byte Loop { get; set; }
        [Reactive]
        public string EndPoint { get; set; }
        public ushort FadeOutFrame { get; set; }
        [Reactive]
        public string StartPointSuddenDeath { get; set; }
        [Reactive]
        public string StartPointTransition { get; set; }

        [Reactive]
        public string StartPoint0 { get; set; }
        [Reactive]
        public string StartPoint1 { get; set; }
        [Reactive]
        public string StartPoint2 { get; set; }
        [Reactive]
        public string StartPoint3 { get; set; }
        [Reactive]
        public string StartPoint4 { get; set; }

        //Getter Helpers
        public BgmPropertyEntryViewModel DataName0ViewModel { get { return _viewModelManager.GetBgmPropertyViewModel(DataName0); } }
        public BgmPropertyEntryViewModel DataName1ViewModel { get { return _viewModelManager.GetBgmPropertyViewModel(DataName1); } }
        public BgmPropertyEntryViewModel DataName2ViewModel { get { return _viewModelManager.GetBgmPropertyViewModel(DataName2); } }
        public BgmPropertyEntryViewModel DataName3ViewModel { get { return _viewModelManager.GetBgmPropertyViewModel(DataName3); } }
        public BgmPropertyEntryViewModel DataName4ViewModel { get { return _viewModelManager.GetBgmPropertyViewModel(DataName4); } }


        public BgmStreamPropertyEntryViewModel(IViewModelManager viewModelManager, IMapper mapper, BgmStreamPropertyEntry bgmStreamPropertyEntry)
            : base(viewModelManager, mapper, bgmStreamPropertyEntry)
        {
            StreamId = bgmStreamPropertyEntry.StreamId;
        }

        public override ReactiveObjectBaseViewModel GetCopy()
        {
            return _mapper.Map(this, new BgmStreamPropertyEntryViewModel(_viewModelManager, _mapper, GetReferenceEntity()));
        }

        public override ReactiveObjectBaseViewModel SaveChanges()
        {
            var original = _viewModelManager.GetBgmStreamPropertyViewModel(StreamId);
            _mapper.Map(this, original.GetReferenceEntity());
            _mapper.Map(this, original);
            return original;
        }
    }
}
