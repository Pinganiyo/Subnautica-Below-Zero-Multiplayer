namespace Subnautica.Client.MonoBehaviours.Player
{
    using UnityEngine;

    /**
     * Toggles a third-person follow camera for the local player.
     * Press F5 to enter / exit third-person view.
     *
     * In third-person the game's main camera is repositioned behind and
     * slightly above the player. The first-person state is restored on exit.
     */
    public class ThirdPersonCamera : MonoBehaviour
    {
        // ── tuneable constants ──────────────────────────────────────────────

        /// <summary>Distance the camera sits behind the player (metres).</summary>
        private const float Distance = 3.5f;

        /// <summary>Height offset above the player root (metres).</summary>
        private const float HeightOffset = 1.0f;

        /// <summary>How quickly the camera interpolates to its target position.</summary>
        private const float SmoothSpeed = 8f;

        /// <summary>Key used to toggle the view.</summary>
        private const KeyCode ToggleKey = KeyCode.F5;

        // ── state ───────────────────────────────────────────────────────────

        private bool isThirdPerson = false;

        // The game camera's original local position and rotation are stored so
        // we can restore them when switching back to first-person.
        private Vector3    savedCamLocalPos;
        private Quaternion savedCamLocalRot;

        // SNCameraRoot is the child GameObject that holds the main Camera in
        // Subnautica Below Zero. We cache it to avoid repeated Find() calls.
        private Transform cameraRootTransform;

        // The actual Camera component, used to manipulate the culling mask.
        private Camera mainCamera;

        // The original culling mask so we can restore it on exit.
        private int savedCullingMask;

        // ── lifecycle ───────────────────────────────────────────────────────

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

        // ── private helpers ─────────────────────────────────────────────────

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
            // Save the first-person local transform so we can restore it later.
            this.savedCamLocalPos = this.cameraRootTransform.localPosition;
            this.savedCamLocalRot = this.cameraRootTransform.localRotation;

            // Make the local player's head/body visible by including the Player layer.
            if (this.mainCamera != null)
            {
                this.savedCullingMask      = this.mainCamera.cullingMask;
                this.mainCamera.cullingMask |= 1 << LayerID.Player;
            }
        }

        private void ExitThirdPerson()
        {
            // Snap back to the stored first-person transform.
            this.cameraRootTransform.localPosition = this.savedCamLocalPos;
            this.cameraRootTransform.localRotation = this.savedCamLocalRot;

            // Restore the original culling mask (hides local player head again).
            if (this.mainCamera != null)
            {
                this.mainCamera.cullingMask = this.savedCullingMask;
            }
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
