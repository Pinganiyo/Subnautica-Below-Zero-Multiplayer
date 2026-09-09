namespace Subnautica.Client.MonoBehaviours.Player
{
    using System.Collections.Generic;

    using UnityEngine;

    /**
     * Toggles a third-person follow camera for the local player.
     * Press F5 to enter / exit third-person view.
     *
     * In third-person the game's main camera is repositioned behind and
     * slightly above the player. All local-player renderers (including the
     * head) are force-enabled so you can see yourself fully.
     * Everything is restored on exit.
     */
    public class ThirdPersonCamera : MonoBehaviour
    {
        // -- tuneable constants ----------------------------------------------

        /// <summary>Distance the camera sits behind the player (metres).</summary>
        private const float Distance = 3.5f;

        /// <summary>Height offset above the player root (metres).</summary>
        private const float HeightOffset = 1.0f;

        /// <summary>How quickly the camera interpolates to its target position.</summary>
        private const float SmoothSpeed = 8f;

        /// <summary>Key used to toggle the view.</summary>
        private const KeyCode ToggleKey = KeyCode.F5;

        // -- state -----------------------------------------------------------

        private bool isThirdPerson = false;

        // Camera transform & component (SNCameraRoot or Camera.main).
        private Transform cameraRootTransform;
        private Camera    mainCamera;

        // Saved camera transform for first-person restore.
        private Vector3    savedCamLocalPos;
        private Quaternion savedCamLocalRot;

        // Saved culling mask.
        private int savedCullingMask;

        // Per-renderer enabled snapshots taken when entering 3rd person.
        private readonly Dictionary<Renderer, bool> rendererSnapshot
            = new Dictionary<Renderer, bool>();

        // -- lifecycle -------------------------------------------------------

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

            // -- 2. Include the Player layer in the culling mask --
            if (this.mainCamera != null)
            {
                this.savedCullingMask        = this.mainCamera.cullingMask;
                this.mainCamera.cullingMask |= 1 << LayerID.Player;
            }

            // -- 3. Force-enable every renderer on the local player hierarchy --
            //
            // Subnautica disables specific renderers (head, face, hair ...) for
            // first-person view. We snapshot their current state then enable all
            // of them so the full character body is visible in 3rd person.
            this.rendererSnapshot.Clear();
            foreach (var r in this.GetComponentsInChildren<Renderer>(true))
            {
                this.rendererSnapshot[r] = r.enabled;
                r.enabled = true;
            }

            PlayerSuitCustomizer.ApplyCustomization(this.gameObject, Subnautica.API.Features.ZeroPlayer.CurrentPlayer?.PlayerId ?? 0);
        }

        private void ExitThirdPerson()
        {
            // -- 1. Restore camera local transform --
            this.cameraRootTransform.localPosition = this.savedCamLocalPos;
            this.cameraRootTransform.localRotation = this.savedCamLocalRot;

            // -- 2. Restore culling mask --
            if (this.mainCamera != null)
            {
                this.mainCamera.cullingMask = this.savedCullingMask;
            }

            // -- 3. Restore per-renderer enabled state --
            foreach (var kvp in this.rendererSnapshot)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.enabled = kvp.Value;
                }
            }

            this.rendererSnapshot.Clear();
        }

        private void UpdateThirdPersonCamera()
        {
            // World-space target: behind and above the player.
            Vector3 playerPos     = this.transform.position;
            Vector3 playerForward = this.transform.forward;

            Vector3 targetPos = playerPos
                - playerForward * Distance
                + Vector3.up    * HeightOffset;

            // Smoothly move the camera toward the target (world space).
            Vector3 smoothedPos = Vector3.Lerp(
                this.cameraRootTransform.position,
                targetPos,
                Time.deltaTime * SmoothSpeed);

            this.cameraRootTransform.position = smoothedPos;

            // Always look at the player (slightly above feet for a nicer angle).
            Vector3 lookAt = playerPos + Vector3.up * (HeightOffset * 0.5f);
            this.cameraRootTransform.LookAt(lookAt);
        }

        private void OnDestroy()
        {
            // Ensure we leave the camera in first-person if the component is torn down.
            if (this.isThirdPerson && this.cameraRootTransform != null)
            {
                this.ExitThirdPerson();
            }
        }
    }
}
