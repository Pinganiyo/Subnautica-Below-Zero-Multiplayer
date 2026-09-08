namespace Subnautica.Client.MonoBehaviours.Player
{
    using System;
    using Subnautica.API.Features;
    using UnityEngine;

    public static class PlayerSuitCustomizer
    {
        /**
         * Suit color definition per player ID.
         * Player 1 is default Robin suit (unmodified).
         * Player 2 is Arctic White / Silver suit.
         * Player 3 is Ocean Cyan suit.
         * Player 4 is Solar Orange suit.
         * Other players have distinct identifiable colors.
         */
        public static bool TryGetSuitColor(byte playerId, out Color tintColor, out Color specColor)
        {
            switch (playerId)
            {
                case 2:
                    // Player 2: Arctic White / Silver suit with brilliant specular sheen
                    tintColor = new Color(2.4f, 2.4f, 2.5f, 1.0f);
                    specColor = new Color(1.8f, 1.8f, 1.8f, 1.0f);
                    return true;

                case 3:
                    // Player 3: Ocean Cyan
                    tintColor = new Color(0.2f, 2.0f, 2.5f, 1.0f);
                    specColor = new Color(0.3f, 1.5f, 2.0f, 1.0f);
                    return true;

                case 4:
                    // Player 4: Solar Orange
                    tintColor = new Color(2.5f, 1.2f, 0.2f, 1.0f);
                    specColor = new Color(2.0f, 1.0f, 0.3f, 1.0f);
                    return true;

                case 5:
                    // Player 5: Electric Purple
                    tintColor = new Color(2.0f, 0.4f, 2.4f, 1.0f);
                    specColor = new Color(1.5f, 0.5f, 2.0f, 1.0f);
                    return true;

                case 6:
                    // Player 6: Acid Lime
                    tintColor = new Color(0.3f, 2.4f, 0.6f, 1.0f);
                    specColor = new Color(0.4f, 2.0f, 0.6f, 1.0f);
                    return true;

                case 7:
                    // Player 7: Crimson Red
                    tintColor = new Color(2.5f, 0.3f, 0.3f, 1.0f);
                    specColor = new Color(2.0f, 0.4f, 0.4f, 1.0f);
                    return true;

                case 8:
                    // Player 8: Bright Yellow
                    tintColor = new Color(2.4f, 2.2f, 0.2f, 1.0f);
                    specColor = new Color(2.0f, 1.8f, 0.3f, 1.0f);
                    return true;

                default:
                    // Player 1 (Host) and unassigned players remain default
                    tintColor = Color.white;
                    specColor = Color.white;
                    return false;
            }
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

                    // Check if already tinted with this color
                    if (materials[0] != null && materials[0].HasProperty("_Color") && materials[0].GetColor("_Color") == tintColor)
                    {
                        continue;
                    }

                    for (int i = 0; i < materials.Length; i++)
                    {
                        ApplyMaterialTint(materials[i], tintColor, specColor);
                    }

                    renderer.materials = materials;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PlayerSuitCustomizer.ApplySuitTint: root: {root.name}, error: {ex}");
            }
        }

        /**
         * Applies the tint colors to a material instance.
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
                mat.SetFloat("_ColorStrength", 1.5f);
            }
        }
    }

    /**
     * Helper component attached to the local player to maintain suit tint in first-person view.
     */
    public class LocalPlayerSuitTint : MonoBehaviour
    {
        private float nextCheckTime;

        public void Start()
        {
            this.ApplyTint();
        }

        public void Update()
        {
            if (Time.time > this.nextCheckTime)
            {
                this.nextCheckTime = Time.time + 3.0f;
                this.ApplyTint();
            }
        }

        public void ApplyTint()
        {
            if (Network.IsMultiplayerActive && ZeroPlayer.CurrentPlayer != null && ZeroPlayer.CurrentPlayer.PlayerId > 1)
            {
                PlayerSuitCustomizer.ApplySuitTint(this.gameObject, ZeroPlayer.CurrentPlayer.PlayerId);
            }
        }
    }
}
