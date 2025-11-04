using nadena.dev.ndmf.preview;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;

namespace Numeira;

[Overlay(typeof(SceneView), ID, Title, defaultDisplay = false, defaultLayout = Layout.Panel)]
internal sealed class PreviewOptionOverlay : Overlay, ITransientOverlay
{
    private const string Title = "ModEmo Preview Options";
    private const string ID = "numeira.mod-emo.preview-option";

    protected override Layout supportedLayouts => Layout.Panel;

    public bool visible {
        get
        {
            if (!NDMFPreviewPrefs.instance.EnablePreview)
                return false;

            if (Selection.activeGameObject == null || Selection.activeGameObject.GetComponentInParent<IModEmoExpression>() == null)
                return false;

            return true;
        } 
    }

    public override VisualElement CreatePanelContent()
    {
        var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath("d56573fba9a95d64cb263d85a8a1f659"));
        var root = uxml.CloneTree();
        var timeSlider = root.Q<Slider>("FrameTimeSlider");
        timeSlider?.SetValueWithoutNotify(ExpressionPreview.PreviewTime);
        timeSlider?.RegisterValueChangedCallback(x => ExpressionPreview.PreviewTime = x.newValue);

        var autoPlayToggle = root.Q<Toggle>("AutoPlayToggle");
        autoPlayToggle?.RegisterValueChangedCallback(x =>
        {
            timeSlider?.SetEnabled(!x.newValue);
            ExpressionPreview.AutoPlay = x.newValue;
        });
        autoPlayToggle!.value = ExpressionPreview.AutoPlay;

        return root;
    }
}