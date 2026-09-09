namespace Subnautica.API.Features.Helper
{
    using System;
    using System.IO;

    using Newtonsoft.Json;

    public class ModConfigFormat
    {
        /**
         *
         * Sunucuya bağlanma zaman aşımı süresi.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public ModConfigFormatItem ConnectionTimeout { get; set; } = new ModConfigFormatItem(120, "Connection timeout period. (Type: Number/Second, Default: 120, Min: 60, Max: 300)");

        /**
         *
         * Player suit color option.
         *
         */
        public ModConfigFormatItem SuitColor { get; set; } = new ModConfigFormatItem(0, "Player suit color (0: Default/Auto, 1: Default/Original, 2: Ocean Cyan, 3: Solar Orange, 4: Electric Purple, 5: Acid Lime, 6: Ruby Crimson, 7: Hazard Gold, 8: Arctic Ice, 9: Cobalt Blue, 10: Coral Rose)");

        /**
         *
         * Player hair color option.
         *
         */
        public ModConfigFormatItem HairColor { get; set; } = new ModConfigFormatItem(0, "Player hair color (0: Default, 1: Default/Original, 2: Golden Blonde, 3: Auburn Copper, 4: Raven Black, 5: Platinum Silver, 6: Cyber Cyan, 7: Crimson Ruby, 8: Amethyst Purple, 9: Emerald Green, 10: Hot Pink)");

        /**
         *
         * Sınıf ayarlamalarını yapar.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public void Initialize()
        {
            var filePath = Paths.GetLauncherGameCorePath("Config.json");
            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }

            try
            {
                var config = JsonConvert.DeserializeObject<ModConfigFormat>(File.ReadAllText(filePath));
                if (config.ConnectionTimeout != null && config.ConnectionTimeout.GetInt() >= 60 && config.ConnectionTimeout.GetInt() <= 300)
                {
                    this.ConnectionTimeout.SetValue(config.ConnectionTimeout.GetInt());
                }

                if (config.SuitColor != null && config.SuitColor.GetInt() >= 0 && config.SuitColor.GetInt() <= 10)
                {
                    this.SuitColor.SetValue(config.SuitColor.GetInt());
                }

                if (config.HairColor != null && config.HairColor.GetInt() >= 0 && config.HairColor.GetInt() <= 10)
                {
                    this.HairColor.SetValue(config.HairColor.GetInt());
                }
            }
            catch (Exception ex)
            {
                Log.Error($"ModConfigFormat.Initialize - Exception: {ex}");
            }
        }

        /**
         *
         * Saves suit color selection to Config.json.
         *
         */
        public void SaveSuitColor(byte colorIndex)
        {
            try
            {
                this.SuitColor.SetValue(colorIndex);
                var filePath = Paths.GetLauncherGameCorePath("Config.json");
                File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Log.Error($"ModConfigFormat.SaveSuitColor - Exception: {ex}");
            }
        }

        /**
         *
         * Saves hair color selection to Config.json.
         *
         */
        public void SaveHairColor(byte colorIndex)
        {
            try
            {
                this.HairColor.SetValue(colorIndex);
                var filePath = Paths.GetLauncherGameCorePath("Config.json");
                File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Log.Error($"ModConfigFormat.SaveHairColor - Exception: {ex}");
            }
        }
    }

    public class ModConfigFormatItem
    {
        /**
         *
         * Açıklamayı barındırır.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public string Description { get; set; }

        /**
         *
         * Değeri barındırır.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public object Value { get; set; }

        /**
         *
         * Sınıf ayarlamalarını yapar.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public ModConfigFormatItem(object value, string description)
        {
            this.Value       = value;
            this.Description = description;
        }

        /**
         *
         * Değeri gğnceller
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public void SetValue(object value)
        {
            this.Value = value;
        }

        /**
         *
         * Int Değeri döner.
         *
         * @author Ismail <ismaiil_0234@hotmail.com>
         *
         */
        public int GetInt(int defaultValue = -1)
        {
            try
            {
                return Convert.ToInt32(this.Value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
