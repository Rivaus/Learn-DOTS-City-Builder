using quentin.tran.gameplay;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace quentin.tran.ui.popup
{
    public class PopupsManager : MonoBehaviour, ISingleton<PopupsManager>
    {
        public static PopupsManager Instance { get; private set; }

        [SerializeField]
        private UIDocument document;

        [SerializeField]
        private UniversalRendererData renderData;

        private VisualElement root;

        private StatisticsPopup statisticsPopup;
        private SettingsPopup settingsPopup;
        private InfoPopup infoPopup;

        private void Awake()
        {
            Instance = this;

            Debug.Assert(renderData != null);

            Debug.Assert(document != null);
            this.root = this.document.rootVisualElement.Q<VisualElement>("popups");
            Debug.Assert(root != null);

            this.statisticsPopup = new() { style = { position = Position.Absolute } };
            this.root.Add(this.statisticsPopup);
            this.statisticsPopup.Hide();

            this.settingsPopup = new(this.renderData) { style = { position = Position.Absolute } };
            this.root.Add(this.settingsPopup);
            this.settingsPopup.Hide();

            this.infoPopup = new() { style = { position = Position.Absolute } };
            this.root.Add(this.infoPopup);
            this.infoPopup.Hide();
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
                case PopupType.Info:
                    this.infoPopup.Show();
                    this.infoPopup.Update();
                    break;
                default:
                    break;
            }
        }

        public void Clear()
        {
        }

        public enum PopupType { Statistics, Settings, Info }
    }
}


