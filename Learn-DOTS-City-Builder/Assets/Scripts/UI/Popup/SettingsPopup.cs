using quentin.tran.ui.manipulator;
using UnityEngine;
using UnityEngine.UIElements;

namespace quentin.tran.ui.popup
{
    [UxmlElement]
    public partial class SettingsPopup : VisualElement
    {
        public SettingsPopup()
        {
            VisualTreeAsset template = Resources.Load("settings-popup") as VisualTreeAsset;
            Debug.Assert(template is not null, "Popup template not found");
            this.Add(template.Instantiate());

            this.Q<Label>("header-label").AddManipulator(new DragAndDropManipulator(this));
            this.Q<Button>("close-button").clickable.clicked += () => this.Hide();

            BindVisualSettings();
        }

        private void BindVisualSettings()
        {
            EnumField visualsModeDropdown = this.Q<EnumField>("visuals-mode-dropdown");
            visualsModeDropdown.value = VisualQualityPresets.Low;
            visualsModeDropdown.RegisterValueChangedCallback(e => {
                if (e.newValue is not VisualQualityPresets qualityPresets)
                    return;

                switch (qualityPresets)
                {
                    case VisualQualityPresets.Low :
                        QualitySettings.SetQualityLevel(0);
                        break;
                    default :
                        QualitySettings.SetQualityLevel(1);
                        break;                    
                }
            });
        }
    }

    public enum VisualQualityPresets
    {
        Low,
        Medium,
        High,
        Ultra,
        Custom
    }
}
