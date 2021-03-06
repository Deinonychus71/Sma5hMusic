using AutoMapper;
using ReactiveUI.Fody.Helpers;
using Sma5h.Mods.Music.Interfaces;
using Sma5h.Mods.Music.Models;
using Sma5hMusic.GUI.Interfaces;

namespace Sma5hMusic.GUI.ViewModels
{
    public class ModEntryViewModel : ReactiveObjectBaseViewModel
    {
        private readonly IViewModelManager _viewModelManager;
        private readonly IMapper _mapper;
        public bool DefaultFlag { get; set; }
        public IMusicMod MusicMod { get; }

        public string Id { get; }
        [Reactive]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Website { get; set; }
        public string ModPath { get { return MusicMod.ModPath; } }

        public ModEntryViewModel() { }

        public ModEntryViewModel(IViewModelManager viewModelManager, IMapper mapper, IMusicMod musicMod)
        {
            _viewModelManager = viewModelManager;
            _mapper = mapper;

            Id = musicMod.Mod.Id;
            MusicMod = musicMod;
            Name = musicMod?.Name;
            Description = musicMod?.Mod.Description;
            Author = musicMod?.Mod.Author;
            Website = musicMod?.Mod.Website;
        }

        public override ReactiveObjectBaseViewModel GetCopy()
        {
            return _mapper.Map(this, new ModEntryViewModel(_viewModelManager, _mapper, MusicMod));
        }

        public override ReactiveObjectBaseViewModel SaveChanges()
        {
            var original = _viewModelManager.GetModEntryViewModel(Id);
            _mapper.Map(this, original);
            return original;
        }

        public MusicModInformation GetMusicModInformation()
        {
            return new MusicModInformation()
            {
                Version = MusicMod.Mod.Version,
                Name = Name,
                Description = Description,
                Author = Author,
                Website = Website
            };
        }
    }
}
