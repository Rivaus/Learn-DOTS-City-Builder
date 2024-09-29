using quentin.tran.gameplay.buildingTool;
using UnityEngine;
using UnityEngine.UIElements;

namespace quentin.tran.ui
{
    public class GameplayUIManager : MonoBehaviour
    {
        public static bool CursorOnGameplay { get; private set; } = false;

        [SerializeField]
        private UIDocument document = null;

        private BottomBarManager bottomBarManager;

        private VisualElement gameplayContainer;

        private void Start()
        {
            VisualElement root = this.document.rootVisualElement;

            this.bottomBarManager = new BottomBarManager(root.Q<VisualElement>("bottom-bar"));
            TopBarManager topBarManager = new TopBarManager(root.Q<VisualElement>("top-bar"));

            this.gameplayContainer = root.Q<VisualElement>("gameplay");
            this.gameplayContainer.RegisterCallback<MouseEnterEvent>(MouseEnter);
            this.gameplayContainer.RegisterCallback<MouseLeaveEvent>(MouseLeave);
        }

        private void MouseEnter(MouseEnterEvent e)
        {
            BuilderController.Instance.Enable();
            CursorOnGameplay = true;
        }

        private void MouseLeave(MouseLeaveEvent e)
        {
            BuilderController.Instance.Disable();
            CursorOnGameplay = false;
        }

        private void OnDestroy()
        {
            this.bottomBarManager.Destroy();

            this.gameplayContainer.UnregisterCallback<MouseEnterEvent>(MouseEnter);
            this.gameplayContainer.UnregisterCallback<MouseLeaveEvent>(MouseLeave);
        }
    }
}

