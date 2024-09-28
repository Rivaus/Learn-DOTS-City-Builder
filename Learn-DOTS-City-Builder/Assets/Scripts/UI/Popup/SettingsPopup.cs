using quentin.tran.ui.manipulator;
using UnityEngine;
using UnityEngine.UIElements;

namespace quentin.tran.ui.popup
{
    [UxmlElement]
    public partial class SettingsPopup : VisualElement
    {
        private const string PLAYER_PREF_QUALITY_PRESET = "PlayerPref.Settings.QualitySettings";
        private const string PLAYER_PREF_VSYNC = "PlayerPref.Settings.VSync";

        private EnumField visualsModeDropdown;
        private Toggle vsyncToggle;

        public SettingsPopup()
        {
            VisualTreeAsset template = Resources.Load("settings-popup") as VisualTreeAsset;
            Debug.Assert(template is not null, "Popup template not found");
            this.Add(template.Instantiate());

            this.Q<Label>("header-label").AddManipulator(new DragAndDropManipulator(this));
            this.Q<Button>("close-button").clickable.clicked += () => this.Hide();

            BindVisualSettings();

            if (PlayerPrefs.HasKey(PLAYER_PREF_QUALITY_PRESET))
                SetQualitySettings((VisualQualityPresets) PlayerPrefs.GetInt(PLAYER_PREF_QUALITY_PRESET));

            if (PlayerPrefs.HasKey(PLAYER_PREF_VSYNC))
                SetVSync(PlayerPrefs.GetInt(PLAYER_PREF_QUALITY_PRESET) == 1);
        }

        private void BindVisualSettings()
        {
            this.visualsModeDropdown = this.Q<EnumField>("visuals-mode-dropdown");
            this.visualsModeDropdown.value = VisualQualityPresets.Low;
            this.visualsModeDropdown.RegisterValueChangedCallback(e =>
            {
                if (e.newValue is not VisualQualityPresets qualityPreset)
                    return;

                SetQualitySettings(qualityPreset);
            });

            this.vsyncToggle = this.Q<Toggle>("vsync-toggle");
            this.vsyncToggle.RegisterValueChangedCallback(e => SetVSync(e.newValue));
        }

        private void SetQualitySettings(VisualQualityPresets qualityPreset)
        {
            switch (qualityPreset)
            {
                case VisualQualityPresets.Low:
                    QualitySettings.SetQualityLevel(0);
                    break;
                default:
                    QualitySettings.SetQualityLevel(1);
                    break;
            }

            this.visualsModeDropdown.SetValueWithoutNotify(qualityPreset);

            PlayerPrefs.SetInt(PLAYER_PREF_QUALITY_PRESET, (int)qualityPreset);
        }

        private void SetVSync(bool vSync)
        {
            QualitySettings.vSyncCount = vSync ? 1 : 0;

            this.vsyncToggle.SetValueWithoutNotify(vSync);

            PlayerPrefs.SetInt(PLAYER_PREF_VSYNC, vSync ? 1 : 0);
        }
    }

    public enum VisualQualityPresets
    {
        Low,
        //Medium,
        //High,
        Ultra,
        //Custom
    }
}
