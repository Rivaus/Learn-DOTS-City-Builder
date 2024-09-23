using quentin.tran.ui.popup;
using UnityEngine.UIElements;

namespace quentin.tran.ui
{
    public class TopBarManager
    {
        public TopBarManager(VisualElement root)
        {
            Button settingsButton = root.Q<Button>("settings-button");
            settingsButton.clickable.clicked += () =>
                PopupsManager.Instance.OpenPopup(PopupsManager.PopupType.Settings);
        }
    }
}
