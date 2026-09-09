namespace Subnautica.Client.MonoBehaviours.Player
{
    using System.Collections.Generic;

    using UnityEngine;

    /**
     * Toggles a third-person follow camera for the local player.
     * Press F5 to enter / exit third-person view.
     *
     * Supports dynamic zoom / distance adjustment (1.0m to 3.5m) with adaptive framing.
     */
    public class ThirdPersonCamera : MonoBehaviour
    {
        // -- tuneable constants ----------------------------------------------

        /// <summary>Minimum distance (metres) for close-up view.</summary>
        public const float MinDistance = 1.0f;

        /// <summary>Maximum distance (metres), strictly capped at original distance.</summary>
        public const float MaxDistance = 3.5f;

        /// <summary>Default starting distance.</summary>
        public const float DefaultDistance = 2.8f;

        /// <summary>How quickly distance and camera interpolate.</summary>
        public const float SmoothSpeed = 8f;

        /// <summary>Key used to toggle third-person view.</summary>
        private const KeyCode ToggleKey = KeyCode.F5;

        // -- singleton -------------------------------------------------------

        public static ThirdPersonCamera Instance { get; private set; }

        public static float CurrentDistance => Instance != null ? Instance.targetDistance : DefaultDistance;

        public static float UserHeightOffset
        {
            get => Instance != null ? Instance.userHeightOffset : 0.0f;
            set
            {
                if (Instance != null)
                {
                    Instance.userHeightOffset = Mathf.Clamp(value, -1.5f, 1.5f);
                }
            }
        }

        public static void SetDistance(float dist)
        {
            if (Instance != null)
            {
                Instance.targetDistance = Mathf.Clamp(dist, MinDistance, MaxDistance);
            }
        }

        // -- state -----------------------------------------------------------

        private bool isThirdPerson = false;
        private float targetDistance = DefaultDistance;
        private float currentDistance = DefaultDistance;
        private float userHeightOffset = 0.0f;

        // Camera transform & component (SNCameraRoot or Camera.main).
        private Transform cameraRootTransform;
        private Camera    mainCamera;

        // Saved camera transform for first-person restore.
        private Vector3    savedCamLocalPos;
        private Quaternion savedCamLocalRot;
        private Vector3    savedMainCamLocalPos;
        private Quaternion savedMainCamLocalRot;

        // Saved culling mask.
        private int savedCullingMask;

        // Per-renderer enabled snapshots taken when entering 3rd person.
        private readonly Dictionary<Renderer, bool> rendererSnapshot
            = new Dictionary<Renderer, bool>();

        // -- lifecycle -------------------------------------------------------

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            this.CacheCameraRoot();
        }

        private void Update()
        {
            if (GameInput.GetKeyDown(ToggleKey))
            {
                this.Toggle();
            }

            if (this.isThirdPerson)
            {
                this.HandleZoomInput();
            }
        }

        private void LateUpdate()
        {
            if (this.isThirdPerson)
            {
                this.UpdateThirdPersonCamera();
            }
        }

        // -- private helpers -------------------------------------------------

        private void CacheCameraRoot()
        {
            if (SNCameraRoot.main != null)
            {
                this.cameraRootTransform = SNCameraRoot.main.transform;
                this.mainCamera          = SNCameraRoot.main.GetComponentInChildren<Camera>();
            }
            else if (Camera.main != null)
            {
                this.cameraRootTransform = Camera.main.transform;
                this.mainCamera          = Camera.main;
            }
        }

        private void Toggle()
        {
            if (this.cameraRootTransform == null)
            {
                this.CacheCameraRoot();
                if (this.cameraRootTransform == null)
                {
                    return;
                }
            }

            this.isThirdPerson = !this.isThirdPerson;

            if (this.isThirdPerson)
            {
                this.EnterThirdPerson();
            }
            else
            {
                this.ExitThirdPerson();
            }
        }

        private void EnterThirdPerson()
        {
            // -- 1. Save camera local transform --
            this.savedCamLocalPos = this.cameraRootTransform.localPosition;
            this.savedCamLocalRot = this.cameraRootTransform.localRotation;

            if (this.mainCamera != null && this.mainCamera.transform != this.cameraRootTransform)
            {
                this.savedMainCamLocalPos = this.mainCamera.transform.localPosition;
                this.savedMainCamLocalRot = this.mainCamera.transform.localRotation;
            }

            // -- 2. Include the Player layer in the culling mask --
            if (this.mainCamera != null)
            {
                this.savedCullingMask        = this.mainCamera.cullingMask;
                this.mainCamera.cullingMask |= 1 << LayerID.Player;
            }

            // -- 3. Make head visible in game logic --
            if (global::Player.main != null)
            {
                global::Player.main.SetHeadVisible(true, false);
            }

            // -- 4. Force-enable every renderer on the local player hierarchy --
            this.rendererSnapshot.Clear();
            foreach (var r in this.GetComponentsInChildren<Renderer>(true))
            {
                this.rendererSnapshot[r] = r.enabled;
                r.enabled = true;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            PlayerSuitCustomizer.ApplyCustomization(this.gameObject, Subnautica.API.Features.ZeroPlayer.CurrentPlayer?.PlayerId ?? 0);

            // Snap camera immediately on entry
            this.currentDistance = this.targetDistance;
            float t = Mathf.InverseLerp(MinDistance, MaxDistance, this.currentDistance);
            float heightOffset = Mathf.Lerp(0.35f, 0.15f, t) + this.userHeightOffset;
            float lookAtOffset = Mathf.Lerp(0.15f, -0.05f, t) + this.userHeightOffset;

            Vector3 playerPos     = this.transform.position;
            Vector3 playerForward = this.transform.forward;

            Vector3 targetPos = playerPos
                - playerForward * this.currentDistance
                + Vector3.up    * heightOffset;

            this.cameraRootTransform.position = targetPos;
            Vector3 lookAt = playerPos + Vector3.up * lookAtOffset;
            this.cameraRootTransform.LookAt(lookAt);

            if (this.mainCamera != null && this.mainCamera.transform != this.cameraRootTransform)
            {
                this.mainCamera.transform.localPosition = Vector3.zero;
                this.mainCamera.transform.localRotation = Quaternion.identity;
            }
        }

        private void ExitThirdPerson()
        {
            // -- 1. Restore camera local transform --
            this.cameraRootTransform.localPosition = this.savedCamLocalPos;
            this.cameraRootTransform.localRotation = this.savedCamLocalRot;

            if (this.mainCamera != null && this.mainCamera.transform != this.cameraRootTransform)
            {
                this.mainCamera.transform.localPosition = this.savedMainCamLocalPos;
                this.mainCamera.transform.localRotation = this.savedMainCamLocalRot;
            }

            // -- 2. Restore culling mask --
            if (this.mainCamera != null)
            {
                this.mainCamera.cullingMask = this.savedCullingMask;
            }

            // -- 3. Hide head in first-person --
            if (global::Player.main != null)
            {
                global::Player.main.SetHeadVisible(false, false);
            }

            // -- 4. Restore per-renderer enabled state --
            foreach (var kvp in this.rendererSnapshot)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.enabled = kvp.Value;
                }
            }

            this.rendererSnapshot.Clear();
        }

        private void HandleZoomInput()
        {
            // 1. Alt + Mouse Scroll Wheel (or Ctrl)
            bool isModifierHeld = GameInput.GetKey(KeyCode.LeftAlt) || GameInput.GetKey(KeyCode.RightAlt) ||
                                  GameInput.GetKey(KeyCode.LeftControl) || GameInput.GetKey(KeyCode.RightControl);
            if (isModifierHeld)
            {
                float scroll = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.0001f)
                {
                    this.targetDistance = Mathf.Clamp(this.targetDistance - scroll * 2.5f, MinDistance, MaxDistance);
                }
            }

            // 2. [ and ] keys (tap or hold)
            if (GameInput.GetKey(KeyCode.LeftBracket))
            {
                this.targetDistance = Mathf.Clamp(this.targetDistance - 2.0f * Time.deltaTime, MinDistance, MaxDistance);
            }
            if (GameInput.GetKey(KeyCode.RightBracket))
            {
                this.targetDistance = Mathf.Clamp(this.targetDistance + 2.0f * Time.deltaTime, MinDistance, MaxDistance);
            }

            // 3. PageUp and PageDown keys
            if (GameInput.GetKey(KeyCode.PageUp))
            {
                this.targetDistance = Mathf.Clamp(this.targetDistance - 2.0f * Time.deltaTime, MinDistance, MaxDistance);
            }
            if (GameInput.GetKey(KeyCode.PageDown))
            {
                this.targetDistance = Mathf.Clamp(this.targetDistance + 2.0f * Time.deltaTime, MinDistance, MaxDistance);
            }

            // 4. Keypad + and Keypad - keys
            if (GameInput.GetKey(KeyCode.KeypadPlus))
            {
                this.targetDistance = Mathf.Clamp(this.targetDistance - 2.0f * Time.deltaTime, MinDistance, MaxDistance);
            }
            if (GameInput.GetKey(KeyCode.KeypadMinus))
            {
                this.targetDistance = Mathf.Clamp(this.targetDistance + 2.0f * Time.deltaTime, MinDistance, MaxDistance);
            }

            // 5. Up and Down Arrow keys to adjust camera height
            if (GameInput.GetKey(KeyCode.UpArrow))
            {
                this.userHeightOffset = Mathf.Clamp(this.userHeightOffset + 1.0f * Time.deltaTime, -1.5f, 1.5f);
            }
            if (GameInput.GetKey(KeyCode.DownArrow))
            {
                this.userHeightOffset = Mathf.Clamp(this.userHeightOffset - 1.0f * Time.deltaTime, -1.5f, 1.5f);
            }
        }

        private void UpdateThirdPersonCamera()
        {
            if (global::Player.main != null && global::Player.main.riggedHead != null && !global::Player.main.riggedHead.activeSelf)
            {
                global::Player.main.SetHeadVisible(true, false);
            }

            // Smoothly interpolate current distance toward target distance
            this.currentDistance = Mathf.Lerp(this.currentDistance, this.targetDistance, Time.deltaTime * SmoothSpeed);

            // Adaptive framing based on distance:
            // Point directly to Robin's body/torso
            float t = Mathf.InverseLerp(MinDistance, MaxDistance, this.currentDistance);
            float heightOffset = Mathf.Lerp(0.35f, 0.15f, t) + this.userHeightOffset;
            float lookAtOffset = Mathf.Lerp(0.15f, -0.05f, t) + this.userHeightOffset;

            Vector3 playerPos     = this.transform.position;
            Vector3 playerForward = this.transform.forward;

            Vector3 targetPos = playerPos
                - playerForward * this.currentDistance
                + Vector3.up    * heightOffset;

            // Smoothly move the camera toward target position (world space)
            Vector3 smoothedPos = Vector3.Lerp(
                this.cameraRootTransform.position,
                targetPos,
                Time.deltaTime * SmoothSpeed);

            this.cameraRootTransform.position = smoothedPos;

            // Frame the character properly so the camera looks directly at Robin's body/head
            Vector3 lookAt = playerPos + Vector3.up * lookAtOffset;
            this.cameraRootTransform.LookAt(lookAt);

            // Zero out relative local offset on child camera so it aligns cleanly
            if (this.mainCamera != null && this.mainCamera.transform != this.cameraRootTransform)
            {
                this.mainCamera.transform.localPosition = Vector3.zero;
                this.mainCamera.transform.localRotation = Quaternion.identity;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            // Ensure we leave the camera in first-person if the component is torn down.
            if (this.isThirdPerson && this.cameraRootTransform != null)
            {
                this.ExitThirdPerson();
            }
        }
    }
}
