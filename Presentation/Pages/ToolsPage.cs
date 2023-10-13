using MauiReactor;
using ScrollView = MauiReactor.ScrollView;
using NobUS.Frontend.MAUI.Presentation.Components;

namespace NobUS.Frontend.MAUI.Presentation.Pages
{
    internal class ToolsPage : Component
    {
        public override VisualNode Render()
        {
            return new ScrollView { new SportFacilityList() };
        }
    }
}
