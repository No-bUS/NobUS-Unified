using NobUS.Frontend.MAUI.Presentation.Components;

namespace NobUS.Frontend.MAUI.Presentation.Pages;

internal class ToolsPage : Component
{
    public override VisualNode Render() => new ScrollView { new SportFacilityList() };
}
