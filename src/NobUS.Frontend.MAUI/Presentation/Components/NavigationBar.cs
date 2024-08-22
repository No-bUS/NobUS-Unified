using static NobUS.Frontend.MAUI.Presentation.Styles;

namespace NobUS.Frontend.MAUI.Presentation.Components;

internal class NavigationBarState
{
    public NavigationBarItem SelectedItem { get; set; }
}

internal class NavigationBar : Component<NavigationBarState>
{
    private readonly List<NavigationBarItem> _items = new();

    public void Add(NavigationBarItem item) => _items.Add(item);

    public override VisualNode Render() =>
        new Grid("*,auto", "*")
        {
            new ContentView { State.SelectedItem?.Content() }
                .GridRow(0)
                .HCenter(),
            new Border
            {
                new CollectionView()
                    .ItemsLayout(new HorizontalLinearItemsLayout().ItemSpacing(8))
                    .ItemsSource(_items, RenderItem)
                    .ItemSizingStrategy(ItemSizingStrategy.MeasureAllItems)
                    .VCenter()
                    .HCenter()
            }
                .ToCard(0)
                .HeightRequest(80)
                .Background(Styler.Scheme.SurfaceContainer)
                .GridRow(1),
        }.Background(Styler.Scheme.Surface);

    private VisualNode RenderItem(NavigationBarItem item)
    {
        bool selected = item == State.SelectedItem;

        return new VerticalStackLayout
        {
            new Border
            {
                new Label()
                    .Text(char.ConvertFromUtf32((int)item.Icon))
                    .FontFamily("MIcon")
                    .TextColor(
                        selected ? Styler.Scheme.OnSecondaryContainer : Styler.Scheme.OnSurface
                    )
                    .FontSize(Sizes.Large * 1.2)
                    .HCenter()
            }
                .HeightRequest(32)
                .WidthRequest(64)
                .BackgroundColor(selected ? Styler.Scheme.SecondaryContainer : Colors.Transparent)
                .ToCard(32),
            new Label()
                .Text(item.Title)
                .FontSize(12)
                .FontFamily(selected ? "SemiBold" : "Regular")
                .TextColor(Styler.Scheme.OnSurface)
                .HCenter(),
        }
            .OnTapped(() =>
            {
                if (State.SelectedItem != item)
                {
                    SetState(s => s.SelectedItem = item);
                }
            })
            .MinimumWidthRequest(48)
            .Margin(0, 12, 0, 16)
            .Spacing(4)
            .HStart()
            .VCenter();
    }

    protected override void OnMounted()
    {
        State.SelectedItem = _items.First();
        base.OnMounted();
    }
}

internal record NavigationBarItem(string Title, MaterialIcons Icon, Func<Component> Content);
