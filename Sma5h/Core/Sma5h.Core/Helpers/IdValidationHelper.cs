using System.Text.RegularExpressions;

namespace Sma5h.Helpers
{
    public static class IdValidationHelper
    {
        private static readonly Regex _idValidatorRegex = new Regex(@"^[a-z0-9_\s,]*$");

        public static bool IsLegalId(string idToCheck)
        {
            return _idValidatorRegex.IsMatch(idToCheck);
        }
    }
}
