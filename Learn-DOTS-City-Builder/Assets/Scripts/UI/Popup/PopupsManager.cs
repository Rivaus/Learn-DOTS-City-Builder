using quentin.tran.gameplay;
using UnityEngine;
using UnityEngine.UIElements;

namespace quentin.tran.ui.popup
{
    public class PopupsManager : MonoBehaviour, ISingleton<PopupsManager>
    {
        public static PopupsManager Instance { get; private set; }

        [SerializeField]
        private UIDocument document;

        private VisualElement root;

        private StatisticsPopup statisticsPopup;
        private SettingsPopup settingsPopup;

        private void Awake()
        {
            Instance = this;

            Debug.Assert(document != null);
            this.root = this.document.rootVisualElement.Q<VisualElement>("popups");
            Debug.Assert(root != null);

            this.statisticsPopup = new();
            this.root.Add(this.statisticsPopup);
            this.statisticsPopup.Hide();

            this.settingsPopup = new();
            this.root.Add(this.settingsPopup);
            this.settingsPopup.Hide();
        }

        public void OpenPopup(PopupType type)
        {
            switch (type)
            {
                case PopupType.Statistics:
                    this.statisticsPopup.Show();
                    break;
                case PopupType.Settings:
                    this.settingsPopup.Show();
                    break;
                default:
                    break;
            }
        }

        public void Clear()
        {
        }

        public enum PopupType { Statistics, Settings }
    }
}


