namespace Subnautica.Client.MonoBehaviours.Player
{
    using System;
    using System.Linq;
    using Subnautica.API.Features;
    using Subnautica.Client.Core;
    using UnityEngine;
    using ServerModel = Subnautica.Network.Models.Server;

    public static class PlayerSuitCustomizer
    {
        public struct SuitColorOption
        {
            public byte Index;
            public string Name;
            public Color TintColor;
            public Color SpecColor;

            public SuitColorOption(byte index, string name, Color tintColor, Color specColor)
            {
                this.Index     = index;
                this.Name      = name;
                this.TintColor = tintColor;
                this.SpecColor = specColor;
            }
        }

        public struct HairColorOption
        {
            public byte Index;
            public string Name;
            public Color TintColor;
            public Color SpecColor;

            public HairColorOption(byte index, string name, Color tintColor, Color specColor)
            {
                this.Index     = index;
                this.Name      = name;
                this.TintColor = tintColor;
                this.SpecColor = specColor;
            }
        }

        /**
         * Available suit color palette.
         */
        public static readonly SuitColorOption[] Palette = new SuitColorOption[]
        {
            new SuitColorOption(1, "Default (Original)", Color.white, Color.white),
            new SuitColorOption(2, "Ocean Cyan",         new Color(0.15f, 1.25f, 1.6f, 1.0f), new Color(0.1f, 0.45f, 0.6f, 1.0f)),
            new SuitColorOption(3, "Solar Orange",       new Color(1.6f, 0.75f, 0.15f, 1.0f), new Color(0.5f, 0.25f, 0.08f, 1.0f)),
            new SuitColorOption(4, "Electric Purple",    new Color(1.35f, 0.25f, 1.55f, 1.0f), new Color(0.45f, 0.15f, 0.55f, 1.0f)),
            new SuitColorOption(5, "Acid Lime",          new Color(0.35f, 1.45f, 0.45f, 1.0f), new Color(0.15f, 0.5f, 0.2f, 1.0f)),
            new SuitColorOption(6, "Ruby Crimson",       new Color(1.5f, 0.18f, 0.22f, 1.0f), new Color(0.45f, 0.1f, 0.12f, 1.0f)),
            new SuitColorOption(7, "Hazard Gold",        new Color(1.55f, 1.1f, 0.12f, 1.0f), new Color(0.45f, 0.32f, 0.06f, 1.0f)),
            new SuitColorOption(8, "Arctic Ice",         new Color(1.15f, 1.3f, 1.45f, 1.0f), new Color(0.35f, 0.45f, 0.55f, 1.0f)),
            new SuitColorOption(9, "Cobalt Blue",        new Color(0.2f, 0.55f, 1.6f, 1.0f),  new Color(0.12f, 0.28f, 0.65f, 1.0f)),
            new SuitColorOption(10, "Coral Rose",        new Color(1.55f, 0.25f, 0.95f, 1.0f), new Color(0.55f, 0.15f, 0.35f, 1.0f)),
        };

        /**
         * Available hair color palette.
         */
        public static readonly HairColorOption[] HairPalette = new HairColorOption[]
        {
            new HairColorOption(1, "Default (Original)", Color.white, Color.white),
            new HairColorOption(2, "Golden Blonde",      new Color(3.5f, 2.7f, 0.6f, 1.0f),  new Color(1.0f, 0.85f, 0.3f, 1.0f)),
            new HairColorOption(3, "Auburn Copper",      new Color(3.4f, 1.2f, 0.2f, 1.0f),  new Color(1.0f, 0.45f, 0.15f, 1.0f)),
            new HairColorOption(4, "Raven Black",        new Color(0.04f, 0.04f, 0.05f, 1.0f), new Color(0.08f, 0.08f, 0.1f, 1.0f)),
            new HairColorOption(5, "Platinum Silver",    new Color(3.2f, 3.2f, 3.4f, 1.0f),  new Color(1.2f, 1.2f, 1.3f, 1.0f)),
            new HairColorOption(6, "Cyber Cyan",         new Color(0.2f, 3.2f, 3.5f, 1.0f),  new Color(0.2f, 1.1f, 1.3f, 1.0f)),
            new HairColorOption(7, "Crimson Ruby",       new Color(3.8f, 0.2f, 0.25f, 1.0f), new Color(1.2f, 0.15f, 0.2f, 1.0f)),
            new HairColorOption(8, "Amethyst Purple",    new Color(3.0f, 0.35f, 3.2f, 1.0f), new Color(1.1f, 0.2f, 1.2f, 1.0f)),
            new HairColorOption(9, "Emerald Green",      new Color(0.3f, 3.6f, 0.5f, 1.0f),  new Color(0.15f, 1.2f, 0.25f, 1.0f)),
            new HairColorOption(10, "Hot Pink",          new Color(3.8f, 0.35f, 2.2f, 1.0f), new Color(1.3f, 0.25f, 0.9f, 1.0f)),
        };

        /**
         * Resolves a suit color option by 1-based index.
         */
        public static SuitColorOption GetColorOption(byte index)
        {
            if (index >= 1 && index <= Palette.Length)
            {
                return Palette[index - 1];
            }

            return Palette[0];
        }

        /**
         * Resolves a hair color option by 1-based index.
         */
        public static HairColorOption GetHairColorOption(byte index)
        {
            if (index >= 1 && index <= HairPalette.Length)
            {
                return HairPalette[index - 1];
            }

            return HairPalette[0];
        }

        /**
         * Finds a suit color option by number or name query.
         */
        public static bool TryFindColorOption(string query, out SuitColorOption option)
        {
            option = Palette[0];
            if (string.IsNullOrEmpty(query))
            {
                return false;
            }

            query = query.Trim().ToLowerInvariant();

            if (byte.TryParse(query, out byte parsedIndex) && parsedIndex >= 1 && parsedIndex <= Palette.Length)
            {
                option = Palette[parsedIndex - 1];
                return true;
            }

            foreach (var item in Palette)
            {
                if (item.Name.ToLowerInvariant().Contains(query))
                {
                    option = item;
                    return true;
                }
            }

            return false;
        }

        /**
         * Finds a hair color option by number or name query.
         */
        public static bool TryFindHairColorOption(string query, out HairColorOption option)
        {
            option = HairPalette[0];
            if (string.IsNullOrEmpty(query))
            {
                return false;
            }

            query = query.Trim().ToLowerInvariant();

            if (byte.TryParse(query, out byte parsedIndex) && parsedIndex >= 1 && parsedIndex <= HairPalette.Length)
            {
                option = HairPalette[parsedIndex - 1];
                return true;
            }

            foreach (var item in HairPalette)
            {
                if (item.Name.ToLowerInvariant().Contains(query))
                {
                    option = item;
                    return true;
                }
            }

            return false;
        }

        /**
         * Gets the effective suit color index for a player.
         * Player 2 defaults to Ocean Cyan (index 2) until changed.
         */
        public static byte GetEffectiveSuitColor(byte playerId)
        {
            var player = (playerId > 0 && playerId == ZeroPlayer.CurrentPlayer?.PlayerId)
                ? ZeroPlayer.CurrentPlayer
                : (ZeroPlayer.GetPlayerById(playerId) ?? ZeroPlayer.CurrentPlayer);

            if (player != null && player.SuitColor >= 1 && player.SuitColor <= Palette.Length)
            {
                return player.SuitColor;
            }

            switch (playerId)
            {
                case 2:
                    return 2; // Player 2: Ocean Cyan by default
                case 3:
                    return 3; // Player 3: Solar Orange by default
                case 4:
                    return 4; // Player 4: Electric Purple
                case 5:
                    return 5; // Player 5: Acid Lime
                case 6:
                    return 6; // Player 6: Crimson Red
                case 7:
                    return 7; // Player 7: Bright Yellow
                case 8:
                    return 8; // Player 8: Arctic White
                default:
                    return 1; // Default (Original)
            }
        }

        /**
         * Gets the effective hair color index for a player.
         */
        public static byte GetEffectiveHairColor(byte playerId)
        {
            var player = (playerId > 0 && playerId == ZeroPlayer.CurrentPlayer?.PlayerId)
                ? ZeroPlayer.CurrentPlayer
                : (ZeroPlayer.GetPlayerById(playerId) ?? ZeroPlayer.CurrentPlayer);

            if (player != null && player.HairColor >= 1 && player.HairColor <= HairPalette.Length)
            {
                return player.HairColor;
            }

            return 1; // Default (Original)
        }

        /**
         * Suit color definition per player ID.
         */
        public static bool TryGetSuitColor(byte playerId, out Color tintColor, out Color specColor)
        {
            byte colorIndex = GetEffectiveSuitColor(playerId);
            var option = GetColorOption(colorIndex);
            tintColor = option.TintColor;
            specColor = option.SpecColor;
            return true;
        }

        /**
         * Hair color definition per player ID.
         */
        public static bool TryGetHairColor(byte playerId, out Color tintColor, out Color specColor)
        {
            byte colorIndex = GetEffectiveHairColor(playerId);
            var option = GetHairColorOption(colorIndex);
            tintColor = option.TintColor;
            specColor = option.SpecColor;
            return true;
        }

        /**
         * Sets the local player's suit color and broadcasts to other players.
         */
        public static void SetLocalSuitColor(byte colorIndex)
        {
            if (colorIndex < 1 || colorIndex > Palette.Length)
            {
                colorIndex = 1;
            }

            var option = GetColorOption(colorIndex);

            if (ZeroPlayer.CurrentPlayer != null)
            {
                ZeroPlayer.CurrentPlayer.SuitColor = colorIndex;
            }

            if (global::Player.main != null)
            {
                ApplySuitTint(global::Player.main.gameObject, ZeroPlayer.CurrentPlayer?.PlayerId ?? 0);
            }

            Settings.ModConfig.SaveSuitColor(colorIndex);

            if (Network.IsMultiplayerActive)
            {
                NetworkClient.SendPacket(new ServerModel.PlayerSuitColorArgs()
                {
                    SuitColor = colorIndex,
                    HairColor = ZeroPlayer.CurrentPlayer?.HairColor ?? 1,
                });
            }

            ErrorMessage.AddMessage($"Suit Color: {option.Name}");
        }

        /**
         * Sets the local player's hair color and broadcasts to other players.
         */
        public static void SetLocalHairColor(byte colorIndex)
        {
            if (colorIndex < 1 || colorIndex > HairPalette.Length)
            {
                colorIndex = 1;
            }

            var option = GetHairColorOption(colorIndex);

            if (ZeroPlayer.CurrentPlayer != null)
            {
                ZeroPlayer.CurrentPlayer.HairColor = colorIndex;
            }

            if (global::Player.main != null)
            {
                ApplyHairTint(global::Player.main.gameObject, ZeroPlayer.CurrentPlayer?.PlayerId ?? 0);
            }

            Settings.ModConfig.SaveHairColor(colorIndex);

            if (Network.IsMultiplayerActive)
            {
                NetworkClient.SendPacket(new ServerModel.PlayerSuitColorArgs()
                {
                    SuitColor = ZeroPlayer.CurrentPlayer?.SuitColor ?? 1,
                    HairColor = colorIndex,
                });
            }

            ErrorMessage.AddMessage($"Hair Color: {option.Name}");
        }

        /**
         * Cycles to the next available suit color.
         */
        public static void CycleNextSuitColor()
        {
            byte currentColor = GetEffectiveSuitColor(ZeroPlayer.CurrentPlayer?.PlayerId ?? 0);
            byte nextColor = (byte)(currentColor + 1);
            if (nextColor > Palette.Length)
            {
                nextColor = 1;
            }

            SetLocalSuitColor(nextColor);
        }

        /**
         * Cycles to the next available hair color.
         */
        public static void CycleNextHairColor()
        {
            byte currentColor = GetEffectiveHairColor(ZeroPlayer.CurrentPlayer?.PlayerId ?? 0);
            byte nextColor = (byte)(currentColor + 1);
            if (nextColor > HairPalette.Length)
            {
                nextColor = 1;
            }

            SetLocalHairColor(nextColor);
        }

        /**
         * Checks whether a renderer belongs to a suit part (suit, body, gloves, flippers),
         * strictly excluding head, face, hair, eyes, mouth, and bare hands.
         */
        public static bool IsSuitRenderer(Renderer renderer)
        {
            if (renderer == null || renderer.gameObject == null)
            {
                return false;
            }

            string name = renderer.gameObject.name.ToLowerInvariant();

            // Strict exclusions: Robin's head, face, hair, eyes, mouth, bare hands skin, tools, PDA
            if (name.Contains("head") || 
                name.Contains("face") || 
                name.Contains("hair") || 
                name.Contains("eye") || 
                name.Contains("mouth") || 
                name.Contains("teeth") || 
                name.Contains("pda") ||
                name.Contains("attach") ||
                name.Contains("flashlight") ||
                name.Contains("mask") ||
                name == "female_base_hand_geo" ||
                name.EndsWith("hand_geo")) // "female_base_hand_geo" is Robin's bare hand skin
            {
                return false;
            }

            // Inclusions: body, gloves, flipper/fins, suit parts
            if (name.Contains("body") || 
                name.Contains("gloves") || 
                name.Contains("flipper") || 
                name.Contains("fins") || 
                name.Contains("suit") || 
                name.Contains("hands_geo") || 
                name.Contains("arms") ||
                name.Contains("dive"))
            {
                return true;
            }

            return false;
        }

        /**
         * Checks whether a material belongs to Robin's hair (e.g. Robin_Facial_Hair_mtrl).
         */
        public static bool IsHairMaterial(Material mat)
        {
            if (mat == null)
            {
                return false;
            }

            string name = mat.name.ToLowerInvariant();
            if (name.Contains("hair"))
            {
                return true;
            }

            if (mat.mainTexture != null && mat.mainTexture.name.ToLowerInvariant().Contains("hair"))
            {
                return true;
            }

            return false;
        }

        /**
         * Checks whether a renderer belongs to the player's hair.
         */
        public static bool IsHairRenderer(Renderer renderer)
        {
            if (renderer == null || renderer.gameObject == null)
            {
                return false;
            }

            if (renderer.gameObject.name.ToLowerInvariant().Contains("hair"))
            {
                return true;
            }

            var sharedMats = renderer.sharedMaterials;
            if (sharedMats != null)
            {
                for (int i = 0; i < sharedMats.Length; i++)
                {
                    if (IsHairMaterial(sharedMats[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /**
         * Applies suit tinting to all suit renderers in the hierarchy.
         */
        public static void ApplySuitTint(GameObject root, byte playerId)
        {
            if (root == null || !TryGetSuitColor(playerId, out Color tintColor, out Color specColor))
            {
                return;
            }

            try
            {
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    if (!IsSuitRenderer(renderer))
                    {
                        continue;
                    }

                    var materials = renderer.materials;
                    if (materials == null || materials.Length == 0)
                    {
                        continue;
                    }

                    bool modified = false;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        var mat = materials[i];
                        if (mat != null && !IsHairMaterial(mat))
                        {
                            if (mat.HasProperty("_Color") && mat.GetColor("_Color") == tintColor)
                            {
                                continue;
                            }

                            ApplyMaterialTint(mat, tintColor, specColor);
                            modified = true;
                        }
                    }

                    if (modified)
                    {
                        renderer.materials = materials;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PlayerSuitCustomizer.ApplySuitTint: root: {root.name}, error: {ex}");
            }
        }

        /**
         * Applies hair tinting to all hair renderers and materials in the hierarchy.
         */
        public static void ApplyHairTint(GameObject root, byte playerId)
        {
            if (root == null || !TryGetHairColor(playerId, out Color tintColor, out Color specColor))
            {
                return;
            }

            try
            {
                var renderers = root.GetComponentsInChildren<Renderer>(true);
                foreach (var renderer in renderers)
                {
                    if (renderer == null || renderer.gameObject == null)
                    {
                        continue;
                    }

                    var materials = renderer.materials;
                    if (materials == null || materials.Length == 0)
                    {
                        continue;
                    }

                    bool modified = false;
                    bool isHairObj = renderer.gameObject.name.ToLowerInvariant().Contains("hair");

                    for (int i = 0; i < materials.Length; i++)
                    {
                        var mat = materials[i];
                        if (mat == null)
                        {
                            continue;
                        }

                        if (isHairObj || IsHairMaterial(mat))
                        {
                            if (mat.HasProperty("_Color") && mat.GetColor("_Color") == tintColor &&
                                mat.HasProperty("_ColorStrength") && Mathf.Approximately(mat.GetFloat("_ColorStrength"), (tintColor == Color.white ? 1.0f : 4.5f)))
                            {
                                continue;
                            }

                            ApplyHairMaterialTint(mat, tintColor, specColor);
                            modified = true;
                            Log.Info($"PlayerSuitCustomizer.ApplyHairTint: Tinted hair on '{renderer.gameObject.name}', mat: '{mat.name}', playerId: {playerId}");
                        }
                    }

                    if (modified)
                    {
                        renderer.materials = materials;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PlayerSuitCustomizer.ApplyHairTint: root: {root.name}, error: {ex}");
            }
        }

        /**
         * Applies both suit and hair customization to the target object hierarchy.
         */
        public static void ApplyCustomization(GameObject root, byte playerId)
        {
            ApplySuitTint(root, playerId);
            ApplyHairTint(root, playerId);
        }

        /**
         * Applies the tint colors to a suit material instance.
         */
        private static void ApplyMaterialTint(Material mat, Color tintColor, Color specColor)
        {
            if (mat == null)
            {
                return;
            }

            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", tintColor);
            }

            if (mat.HasProperty("_Color2"))
            {
                mat.SetColor("_Color2", tintColor);
            }

            if (mat.HasProperty("_Color3"))
            {
                mat.SetColor("_Color3", tintColor);
            }

            if (mat.HasProperty("_SpecColor"))
            {
                mat.SetColor("_SpecColor", specColor);
            }

            if (mat.HasProperty("_SpecColor2"))
            {
                mat.SetColor("_SpecColor2", specColor);
            }

            if (mat.HasProperty("_SpecColor3"))
            {
                mat.SetColor("_SpecColor3", specColor);
            }

            if (mat.HasProperty("_Tint"))
            {
                mat.SetColor("_Tint", tintColor);
            }

            if (mat.HasProperty("_TintColor"))
            {
                mat.SetColor("_TintColor", tintColor);
            }

            if (mat.HasProperty("_ColorStrength"))
            {
                mat.SetFloat("_ColorStrength", tintColor == Color.white ? 1.0f : 1.15f);
            }
        }

        /**
         * Applies the tint colors specifically to Robin's hair material.
         * Eliminates transparency and ocean blue specular glare while lifting dark albedo.
         */
        private static void ApplyHairMaterialTint(Material mat, Color tintColor, Color specColor)
        {
            if (mat == null)
            {
                return;
            }

            bool isDefault = (tintColor == Color.white);
            Color solidTint = new Color(tintColor.r, tintColor.g, tintColor.b, 1.0f);

            // 1. ELIMINATE TRANSPARENCY & GHOSTING:
            // Disable Marmoset additive glow and emission passes completely on hair.
            mat.DisableKeyword("MARMO_GLOW");
            mat.DisableKeyword("_EMISSION");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.EnableKeyword("_ALPHATEST_ON");

            if (mat.HasProperty("_EnableGlow"))
            {
                mat.SetFloat("_EnableGlow", 0.0f);
            }

            if (mat.HasProperty("_GlowColor"))
            {
                mat.SetColor("_GlowColor", Color.black);
            }

            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", Color.black);
            }

            if (mat.HasProperty("_GlowStrength"))
            {
                mat.SetFloat("_GlowStrength", 0.0f);
            }

            if (mat.HasProperty("_GlowStrengthNight"))
            {
                mat.SetFloat("_GlowStrengthNight", 0.0f);
            }

            // Force solid Cutout blend mode and depth writing
            if (mat.HasProperty("_Mode"))
            {
                mat.SetFloat("_Mode", 1.0f); // 1 = Cutout
            }

            if (mat.HasProperty("_SrcBlend"))
            {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            }

            if (mat.HasProperty("_DstBlend"))
            {
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            }

            if (mat.HasProperty("_ZWrite"))
            {
                mat.SetInt("_ZWrite", 1);
            }

            if (mat.renderQueue >= 3000)
            {
                mat.renderQueue = 2450; // Cutout queue
            }

            // 2. ELIMINATE BLUISH OCEAN SPECULAR GLARE:
            // Subnautica's Marmoset reflects the bright cyan/blue ambient water cubemap.
            // Keeping specular intensity low (<0.3) prevents the ocean reflection from turning hair blue.
            if (mat.HasProperty("_SpecIntensity"))
            {
                mat.SetFloat("_SpecIntensity", isDefault ? 1.0f : 0.25f);
            }

            if (mat.HasProperty("_SpecularScale"))
            {
                mat.SetFloat("_SpecularScale", isDefault ? 1.0f : 0.25f);
            }

            // Subtle dark specular tint so it does not reflect ambient cyan
            Color subtleSpec = isDefault
                ? Color.white
                : new Color(Mathf.Clamp01(specColor.r * 0.25f), Mathf.Clamp01(specColor.g * 0.25f), Mathf.Clamp01(specColor.b * 0.25f), 1.0f);

            if (mat.HasProperty("_SpecColor"))
            {
                mat.SetColor("_SpecColor", subtleSpec);
            }

            if (mat.HasProperty("_SpecColor2"))
            {
                mat.SetColor("_SpecColor2", subtleSpec);
            }

            if (mat.HasProperty("_SpecColor3"))
            {
                mat.SetColor("_SpecColor3", subtleSpec);
            }

            // 3. DIFFUSE / ALBEDO COLOR SETTINGS (Lifts dark/black hair to vivid colors):
            mat.color = solidTint;

            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", solidTint);
            }

            if (mat.HasProperty("_Color2"))
            {
                mat.SetColor("_Color2", solidTint);
            }

            if (mat.HasProperty("_Color3"))
            {
                mat.SetColor("_Color3", solidTint);
            }

            if (mat.HasProperty("_Tint"))
            {
                mat.SetColor("_Tint", solidTint);
            }

            if (mat.HasProperty("_TintColor"))
            {
                mat.SetColor("_TintColor", solidTint);
            }

            // Boost color strength cleanly so dark base hair texture reflects the vibrant dye
            if (mat.HasProperty("_ColorStrength"))
            {
                mat.SetFloat("_ColorStrength", isDefault ? 1.0f : 4.5f);
            }

            if (mat.HasProperty("_ColorStrengthAtNight"))
            {
                mat.SetFloat("_ColorStrengthAtNight", isDefault ? 1.0f : 4.5f);
            }

            if (mat.HasProperty("_ColorMultiplier"))
            {
                mat.SetFloat("_ColorMultiplier", isDefault ? 1.0f : 2.5f);
            }

            Log.Info($"PlayerSuitCustomizer.ApplyHairMaterialTint: Applied solid hair tint: {tintColor.ToString()}, isDefault: {isDefault}");
        }
    }

    /**
     * Helper component attached to the local player to maintain suit & hair tint and handle F6/F7 cycling.
     */
    public class LocalPlayerSuitTint : MonoBehaviour
    {
        private float nextCheckTime;

        public void Start()
        {
            var savedSuit = (byte)Settings.ModConfig.SuitColor.GetInt();
            if (savedSuit >= 1 && savedSuit <= PlayerSuitCustomizer.Palette.Length && ZeroPlayer.CurrentPlayer != null)
            {
                ZeroPlayer.CurrentPlayer.SuitColor = savedSuit;
            }

            var savedHair = (byte)Settings.ModConfig.HairColor.GetInt();
            if (savedHair >= 1 && savedHair <= PlayerSuitCustomizer.HairPalette.Length && ZeroPlayer.CurrentPlayer != null)
            {
                ZeroPlayer.CurrentPlayer.HairColor = savedHair;
            }

            this.ApplyTint();
        }

        public void Update()
        {
            if (GameInput.GetKeyDown(KeyCode.F6))
            {
                PlayerSuitCustomizer.CycleNextSuitColor();
            }

            if (GameInput.GetKeyDown(KeyCode.F7))
            {
                PlayerSuitCustomizer.CycleNextHairColor();
            }

            if (Time.time > this.nextCheckTime)
            {
                this.nextCheckTime = Time.time + 1.0f;
                this.ApplyTint();
            }
        }

        public void ApplyTint()
        {
            byte playerId = ZeroPlayer.CurrentPlayer?.PlayerId ?? 0;
            PlayerSuitCustomizer.ApplyCustomization(this.gameObject, playerId);
        }
    }
}
