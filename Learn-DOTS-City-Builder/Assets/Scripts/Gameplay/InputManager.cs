using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace quentin.tran.gameplay
{
    /// <summary>
    /// Manages all inputs
    /// </summary>
    public class InputManager : ISingleton<InputManager>
    {
        public static InputManager Instance { get; private set; }

        private InputControls controls;

        public static Vector2 GetCameraMovement() => Instance.controls.CameraMap.Movement.ReadValue<Vector2>();

        public static float GetCameraZoom() => Instance.controls.CameraMap.Zoom.ReadValue<float>();

        public static event Action OnClick, OnClickRelease;

        public static event Action<Vector2> OnOrbitalRotate;

        public static event Action<float> OnZoom;

        public InputManager()
        {
            Instance = this;

            this.controls = new();
            this.controls.Enable();

            this.controls.CameraMap.Click.performed += Click;
            this.controls.CameraMap.Click.canceled += ClickRelease;
            this.controls.CameraMap.OrbitalView.performed += OrbitalRotate;
            this.controls.CameraMap.Zoom.performed += Zoom;
        }

        private void Click(InputAction.CallbackContext context) => OnClick?.Invoke();

        private void ClickRelease(InputAction.CallbackContext context) => OnClickRelease?.Invoke();

        private void OrbitalRotate(InputAction.CallbackContext context) => OnOrbitalRotate?.Invoke(context.ReadValue<Vector2>());

        private void Zoom(InputAction.CallbackContext context) => OnZoom?.Invoke(context.ReadValue<float>());

        public void Clear()
        {
            Instance = null;

            this.controls.CameraMap.Click.performed -= Click;
            this.controls.CameraMap.Click.canceled -= ClickRelease;
            this.controls.CameraMap.OrbitalView.performed -= OrbitalRotate;
            this.controls.CameraMap.Zoom.performed -= Zoom;
            this.controls.Dispose();
        }
    }
}

