using quentin.tran.ui.manipulator;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace quentin.tran.ui.popup
{
    [UxmlElement]
    public partial class SettingsPopup : VisualElement
    {
        private const string PLAYER_PREF_QUALITY_PRESET = "PlayerPref.Settings.QualitySettings";
        private const string PLAYER_PREF_VSYNC = "PlayerPref.Settings.VSync";

        private const string PLAYER_PREF_SSAO = "PlayerPref.Settings.SSAO";

        private EnumField visualsModeDropdown;
        private Toggle vsyncToggle, ssaoToggle;

        private UniversalRendererData renderData;


        public SettingsPopup() : this(null)
        {

        }

        public SettingsPopup(UniversalRendererData renderData)
        {
            Application.targetFrameRate = -1;

            this.renderData = renderData;

            VisualTreeAsset template = Resources.Load("settings-popup") as VisualTreeAsset;
            Debug.Assert(template is not null, "Popup template not found");
            this.Add(template.Instantiate());

            this.Q<Label>("header-label").AddManipulator(new DragAndDropManipulator(this));
            this.Q<Button>("close-button").clickable.clicked += () => this.Hide();

            BindVisualSettings();

            if (PlayerPrefs.HasKey(PLAYER_PREF_QUALITY_PRESET))
                SetQualitySettings((VisualQualityPresets)PlayerPrefs.GetInt(PLAYER_PREF_QUALITY_PRESET));
            else
                SetQualitySettings(VisualQualityPresets.High);

            if (PlayerPrefs.HasKey(PLAYER_PREF_VSYNC))
                SetVSync(PlayerPrefs.GetInt(PLAYER_PREF_VSYNC) == 1);

            if (PlayerPrefs.HasKey(PLAYER_PREF_SSAO))
                SetSSAO(PlayerPrefs.GetInt(PLAYER_PREF_SSAO) == 1);
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

            this.ssaoToggle = this.Q<Toggle>("ssao-toggle");
            this.ssaoToggle.RegisterValueChangedCallback(e => SetSSAO(e.newValue));
        }

        private void SetQualitySettings(VisualQualityPresets qualityPreset)
        {
            switch (qualityPreset)
            {
                case VisualQualityPresets.Low:
                    QualitySettings.SetQualityLevel(0);
                    break;
                case VisualQualityPresets.Medium:
                    QualitySettings.SetQualityLevel(1);
                    break;
                case VisualQualityPresets.High:
                    QualitySettings.SetQualityLevel(2);
                    break;
                case VisualQualityPresets.Ultra:
                    QualitySettings.SetQualityLevel(3);
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

        private void SetSSAO(bool enableSSAO)
        {
            if (renderData.TryGetRendererFeature(out ScreenSpaceAmbientOcclusion ssaoPass))
            {
                ssaoPass.SetActive(enableSSAO);
            }

            this.ssaoToggle.SetValueWithoutNotify(enableSSAO);
            PlayerPrefs.SetInt(PLAYER_PREF_SSAO, enableSSAO ? 1 : 0);
        }
    }

    public enum VisualQualityPresets
    {
        Low,
        Medium,
        High,
        Ultra,
        //Custom
    }
}
