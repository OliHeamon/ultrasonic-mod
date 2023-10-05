// Credit to Scalie for LocalizationHelper - https://github.com/ScalarVector1/DragonLens/blob/407a54e45d7a4828f660b46988feaf86092249b3/Helpers/LocalizationHelper.cs

using System.Text.RegularExpressions;
using Terraria.Localization;

namespace MP3Player.Localization
{
    public static class LocalizationHelper
    {
        /// <summary>
        /// Gets a localized text value of the mod.
        /// If no localization is found, the key itself is returned.
        /// </summary>
        /// <param name="key">the localization key</param>
        /// <param name="args">optional args that should be passed</param>
        /// <returns>the text should be displayed</returns>
        public static string GetText(string key, params object[] args)
        {
            return Language.Exists($"Mods.MP3Player.{key}") ? Language.GetTextValue($"Mods.MP3Player.{key}", args) : key;
        }

        public static string GetGUIText(string key, params object[] args)
        {
            return GetText($"UI.{key}", args);
        }

        public static bool IsCjkPunctuation(char a)
        {
            return Regex.IsMatch(a.ToString(), @"\p{IsCJKSymbolsandPunctuation}|\p{IsHalfwidthandFullwidthForms}");
        }

        public static bool IsCjkUnifiedIdeographs(char a)
        {
            return Regex.IsMatch(a.ToString(), @"\p{IsCJKUnifiedIdeographs}");
        }

        public static bool IsRightCloseCjkPunctuation(char a)
        {
            return a is '（' or '【' or '《' or '｛' or '｢' or '［' or '｟' or '“';
        }

        public static bool IsCjkCharacter(char a)
        {
            return IsCjkUnifiedIdeographs(a) || IsCjkPunctuation(a);
        }
    }
}
