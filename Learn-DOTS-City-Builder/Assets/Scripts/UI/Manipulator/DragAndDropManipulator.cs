using UnityEngine;
using UnityEngine.UIElements;

namespace quentin.tran.ui.manipulator
{
    public class DragAndDropManipulator : MouseManipulator
    {
        private VisualElement root;
        private bool isActive;

        public DragAndDropManipulator(VisualElement root)
        {
            this.root = root;

            this.activators.Clear();
            this.activators.Add(new ManipulatorActivationFilter() { button = MouseButton.LeftMouse, clickCount = 1 });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(MouseDown);
            target.RegisterCallback<MouseMoveEvent>(MouseMove);
            target.RegisterCallback<MouseUpEvent>(MouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(MouseDown);
            target.UnregisterCallback<MouseMoveEvent>(MouseMove);
            target.UnregisterCallback<MouseUpEvent>(MouseUp);
        }

        private void MouseDown(MouseDownEvent e)
        {
            if (this.isActive || !CanStartManipulation(e) || target.HasMouseCapture())
                return;

            target.CaptureMouse();
            this.isActive = true;
        }

        private void MouseMove(MouseMoveEvent e)
        {
            if (!this.isActive)
                return;

            this.root.transform.position += (Vector3) e.mouseDelta;
        }

        private void MouseUp(MouseUpEvent _)
        {
            if (!this.isActive)
                return;

            this.isActive = false;
            target.ReleaseMouse();
        }
    }
}

