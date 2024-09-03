using UnityEngine;
using UnityEngine.UIElements;

namespace quentin.tran.ui.popup
{
    public class PopupsManager : MonoBehaviour
    {
        public static PopupsManager Instance { get; private set; }

        [SerializeField]
        private UIDocument document;

        private VisualElement root;

        private StatisticsPopup statisticsPopup;
        private object swit;

        private void Awake()
        {
            Instance = this;

            Debug.Assert(document != null);
            this.root = this.document.rootVisualElement.Q<VisualElement>("popups");
            Debug.Assert(root != null);

            this.statisticsPopup = new();
            this.root.Add(this.statisticsPopup);
            this.statisticsPopup.Hide();
        }

        public void OpenPopup(PopupType type)
        {
            switch (type)
            {
                case PopupType.Statistics:
                    this.statisticsPopup.Show();
                    break;
                default:
                    break;
            }
        }

        public enum PopupType { Statistics }
    }
}


