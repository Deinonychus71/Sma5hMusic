using System.Linq;

namespace Sm5sh.Helpers
{
    public class LocaleHelper
    {
        public static string[] ValidLocales = new string[]
        {
            "eu_de",
            "eu_en",
            "eu_es",
            "eu_fr",
            "eu_it",
            "eu_nl",
            "eu_ru",
            "jp_ja",
            "kr_ko",
            "us_en",
            "us_es",
            "us_fr",
            "zh_cn",
            "zh_tw"
        };

        public enum Locales
        {
            en_de,
            eu_en,
            eu_es,
            eu_fr,
            eu_it,
            eu_nl,
            eu_ru,
            jp_ja,
            kr_ko,
            us_en,
            us_es,
            us_fr,
            zh_cn,
            zh_tw
        }

        public static string GetPascalCaseLocale(string gameLocale)
        {
            if (!ValidLocales.Contains(gameLocale))
                return gameLocale;

            return $"{char.ToUpper(gameLocale[0])}{gameLocale[1]}{char.ToUpper(gameLocale[3])}{gameLocale[4]}";
        }
    }
}
