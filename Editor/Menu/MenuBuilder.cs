using VRC.SDK3.Avatars.ScriptableObjects;

namespace Numeira;

internal sealed class MenuBuilder
{
    public string Name { get; set; } = "";

    public string ParameterName { get; set; } = "";

    public bool IsRadialPuppet { get; set; } = false;

    public List<MenuBuilder> Children { get; } = new();

    public VRCExpressionsMenu.Control Build()
    {
        var control = new VRCExpressionsMenu.Control()
        {
            name = Name,
            type = Children.Count != 0 ? VRCExpressionsMenu.Control.ControlType.SubMenu : IsRadialPuppet ? VRCExpressionsMenu.Control.ControlType.RadialPuppet : VRCExpressionsMenu.Control.ControlType.Toggle,
            
        };
        return control;
    }
}

internal interface IMenuSource
{
    public string Name { get; set; }
    public string ParameterName { get; set; }

}