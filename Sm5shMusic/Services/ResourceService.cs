using Microsoft.Extensions.Options;
using Sm5shMusic.Helpers;
using Sm5shMusic.Interfaces;
using System.IO;

namespace Sm5shMusic.Services
{
    public class ResourceService : IResourceService
    {
        private readonly Settings _settings;

        public ResourceService(IOptions<Settings> settings)
        {
            _settings = settings.Value;
        }

        //Executable Paths
        public string GetNus3AudioCLIExe()
        {
            return Path.Combine(_settings.LibsPath, Constants.ExternalTools.Nus3AudioPath, Constants.ExternalTools.Nus3AudioExe);
        }

        public string GetBgmPropertyExe()
        {
            return Path.Combine(_settings.LibsPath, Constants.ExternalTools.BgmPropertyPath, Constants.ExternalTools.BgmPropertyExe);
        }

        public string GetBgmPropertyHashes()
        {
            return Path.Combine(_settings.LibsPath, Constants.ExternalTools.BgmPropertyPath, Constants.ExternalTools.BgmPropertyHashes);
        }

        //Resources Paths
        public string GetBgmPropertyYmlResource()
        {
            return Path.Combine(_settings.ResourcesPath, Constants.ResourcesFiles.BgmPropertyYmlFile);
        }

        public string GetBgmDbLabelsCsvResource()
        {
            return Path.Combine(_settings.ResourcesPath, Constants.ResourcesFiles.UiBgmDbLabelsCsvFile);
        }

        public string GetNusBankIdsCsvResource()
        {
            return Path.Combine(_settings.ResourcesPath, Constants.ResourcesFiles.NusBankIdsCsvFile);
        }

        public string GetBgmPropertyResource()
        {
            return Path.Combine(_settings.ResourcesPath, Constants.ResourcesFiles.BgmPropertyFile);
        }

        public string GetBgmDbResource()
        {
            return Path.Combine(_settings.ResourcesPath, Constants.ResourcesFiles.ParamsPath, Constants.ResourcesFiles.UiBgmDbFile);
        }

        public string GetGameTitleDbResource()
        {
            return Path.Combine(_settings.ResourcesPath, Constants.ResourcesFiles.ParamsPath, Constants.ResourcesFiles.UiGameTitleDbFile);
        }

        public string GetNusBankTemplateResource()
        {
            return Path.Combine(_settings.ResourcesPath, Constants.ResourcesFiles.NusBankTemplatePath, Constants.ResourcesFiles.NusBankTemplateFile);
        }

        public string GetMsbtTitleResource(string locale)
        {
            return Path.Combine(_settings.ResourcesPath, Constants.ResourcesFiles.MsbtPath, string.Format(Constants.ResourcesFiles.MsbtTitleFile, locale));
        }

        public string GetMsbtBgmResource(string locale)
        {
            return Path.Combine(_settings.ResourcesPath, Constants.ResourcesFiles.MsbtPath, string.Format(Constants.ResourcesFiles.MsbtBgmFile, locale));
        }

        //Temp
        public string GetTemporaryAudioConversionFile()
        {
            return Path.Combine(_settings.TempPath, string.Format(Constants.ResourcesFiles.TemporaryAudioFile, _settings.AudioConversionFormat));
        }

        public string GetTemporaryBgmPropertiesYmlFile()
        {
            return Path.Combine(_settings.TempPath, Constants.ResourcesFiles.TemporaryBgmPropertyFile);
        }
    }
}
