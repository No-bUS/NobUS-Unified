using NobUS.Frontend.MAUI.Service;

namespace NobUS.Frontend.MAUI.Presentation.Pages
{
    internal class HomePage : Component
    {
        public override VisualNode Render() =>
            new Grid
            {
                new VerticalStackLayout
                {
                    new VerticalStackLayout
                    {
                        new Label()
                            .Text(
                                DateTime.Now.Hour switch
                                {
                                    < 6 => "🌙Sleep tight",
                                    < 9 => "☕Good morning",
                                    < 12 => "🌼Have a wonderful day",
                                    < 15 => "🍽Good afternoon",
                                    < 18 => "💪Keep up the good work",
                                    < 21 => "🌆Good evening",
                                    _ => "😴Sweet dreams"
                                }
                            )
                            .Large()
                            .Bold(),
                    },
                    new VerticalStackLayout
                    {
                        new Label().Text("📌Pinned stations").Medium().SemiBold(),
                    },
                    new VerticalStackLayout
                    {
                        new Label().Text("🌟Embarking on a new journey?").Medium().SemiBold(),
                        new Grid("auto,auto", "auto,*")
                        {
                            new Entry()
                                .Placeholder("Leaving...")
                                .GridColumn(1)
                                .GridRow(0)
                                .ClearButtonVisibility(ClearButtonVisibility.WhileEditing)
                                .IsSpellCheckEnabled(false)
                                .VerticalOptions(LayoutOptions.Center),
                            new Entry()
                                .Placeholder("Going to...")
                                .GridColumn(1)
                                .GridRow(1)
                                .ClearButtonVisibility(ClearButtonVisibility.WhileEditing)
                                .IsSpellCheckEnabled(false)
                                .VerticalOptions(LayoutOptions.Center),
                        }.RowSpacing(10)
                    },
                }.Spacing(40)
            };
    }
}
