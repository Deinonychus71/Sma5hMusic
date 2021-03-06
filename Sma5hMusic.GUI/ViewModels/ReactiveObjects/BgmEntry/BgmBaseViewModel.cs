using AutoMapper;
using ReactiveUI;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Interfaces;

namespace Sma5hMusic.GUI.ViewModels
{
    public abstract class ReactiveObjectBaseViewModel : ReactiveObject
    {
        public abstract ReactiveObjectBaseViewModel GetCopy();

        public abstract ReactiveObjectBaseViewModel SaveChanges();
    }

    public abstract class BgmBaseViewModel<T> : ReactiveObjectBaseViewModel where T : BgmBase
    {
        private readonly T _refBgmBaseEntity;
        protected IMapper _mapper;
        protected IViewModelManager _viewModelManager;

        //Helper Getters
        public ModEntryViewModel MusicModViewModel { get { return _viewModelManager.GetModEntryViewModel(ModId); } }
        public IMusicMod MusicMod { get; }
        public EntrySource Source { get { return _refBgmBaseEntity.Source; } }
        public string ModId { get { return MusicMod?.Id; } }
        public string ModPath { get { return MusicMod?.ModPath; } }
        public bool IsMod { get { return Source == EntrySource.Mod; } }

        public BgmBaseViewModel(IViewModelManager viewModeManager, IMapper mapper, T bgmBaseEntity)
        {
            _viewModelManager = viewModeManager;
            _refBgmBaseEntity = bgmBaseEntity;
            _mapper = mapper;
            MusicMod = bgmBaseEntity?.MusicMod;
        }

        public T GetReferenceEntity()
        {
            return _refBgmBaseEntity;
        }
    }
}
