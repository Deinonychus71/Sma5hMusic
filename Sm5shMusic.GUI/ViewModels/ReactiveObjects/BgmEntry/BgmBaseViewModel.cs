using AutoMapper;
using ReactiveUI;
using Sm5sh.Mods.Music.Interfaces;
using Sm5sh.Mods.Music.Models;
using Sm5shMusic.GUI.Interfaces;

namespace Sm5shMusic.GUI.ViewModels
{
    public abstract class ReactiveObjectBaseViewModel : ReactiveObject
    {
        public abstract ReactiveObjectBaseViewModel GetCopy();

        public abstract ReactiveObjectBaseViewModel SaveChanges();
    }

    public abstract class BgmBaseViewModel<T> : ReactiveObjectBaseViewModel where T : BgmBase
    {
        private T _refBgmBaseEntity;
        protected IMapper _mapper;
        protected IViewModelManager _audioStateManager;

        //Helper Getters
        public IMusicMod MusicMod { get; }
        public EntrySource Source { get { return MusicMod != null ? EntrySource.Mod : EntrySource.Core; } }
        public string ModName { get { return MusicMod?.Name; } }
        public string ModAuthor { get { return MusicMod?.Mod.Author; } }
        public string ModWebsite { get { return MusicMod?.Mod.Website; } }
        public string ModId { get { return MusicMod?.Id; } }
        public string ModPath { get { return MusicMod?.ModPath; } }
        public bool IsMod { get { return Source == EntrySource.Mod; } }

        public BgmBaseViewModel(IViewModelManager audioStateManager, IMapper mapper, T bgmBaseEntity)
        {
            _audioStateManager = audioStateManager;
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
