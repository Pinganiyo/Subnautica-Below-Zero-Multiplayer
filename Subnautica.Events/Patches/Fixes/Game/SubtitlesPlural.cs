namespace Subnautica.Events.Patches.Fixes.Game
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    using HarmonyLib;

    using Subnautica.API.Features;

    [HarmonyPatch(typeof(global::Subtitles), nameof(global::Subtitles.AddRawLong))]
    public static class SubtitlesPlural
    {
        private static readonly (Regex Regex, string Replacement)[] PluralRules = new (Regex, string)[]
        {
            // Names
            (new Regex(@"\bRobin's\b"), "Robins'"),
            (new Regex(@"\brobin's\b"), "robins'"),
            (new Regex(@"\bRobin\b"), "Robins"),
            (new Regex(@"\brobin\b"), "robins"),

            // Contractions & Verbs
            (new Regex(@"\bI am\b"), "We are"),
            (new Regex(@"\bi am\b"), "we are"),
            (new Regex(@"\bI AM\b"), "WE ARE"),
            (new Regex(@"\bI was\b"), "We were"),
            (new Regex(@"\bi was\b"), "we were"),
            (new Regex(@"\bI WAS\b"), "WE WERE"),
            (new Regex(@"\bI'm\b"), "We're"),
            (new Regex(@"\bi'm\b"), "we're"),
            (new Regex(@"\bI'M\b"), "WE'RE"),
            (new Regex(@"\bI've\b"), "We've"),
            (new Regex(@"\bi've\b"), "we've"),
            (new Regex(@"\bI'll\b"), "We'll"),
            (new Regex(@"\bi'll\b"), "we'll"),
            (new Regex(@"\bI'd\b"), "We'd"),
            (new Regex(@"\bi'd\b"), "we'd"),

            // Pronouns
            (new Regex(@"\bMyself\b"), "Ourselves"),
            (new Regex(@"\bmyself\b"), "ourselves"),
            (new Regex(@"\bYourself\b"), "Yourselves"),
            (new Regex(@"\byourself\b"), "yourselves"),
            (new Regex(@"\bMine\b"), "Ours"),
            (new Regex(@"\bmine\b"), "ours"),
            (new Regex(@"\bMy\b"), "Our"),
            (new Regex(@"\bmy\b"), "our"),
            (new Regex(@"\bMe\b"), "Us"),
            (new Regex(@"\bme\b"), "us"),
            (new Regex(@"\bI\b"), "We"),
        };

        /**
         *
         * Intercepts and pluralizes all in-game dialogue subtitles when multiplayer is active.
         *
         */
        [HarmonyPrefix]
        private static void Prefix(ref StringBuilder text)
        {
            if (!Network.IsMultiplayerActive || text == null || text.Length == 0)
            {
                return;
            }

            try
            {
                string original = text.ToString();
                string result = original;

                for (int i = 0; i < PluralRules.Length; i++)
                {
                    result = PluralRules[i].Regex.Replace(result, PluralRules[i].Replacement);
                }

                if (result != original)
                {
                    text.Clear();
                    text.Append(result);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"SubtitlesPlural.Prefix: {ex}");
            }
        }
    }
}
