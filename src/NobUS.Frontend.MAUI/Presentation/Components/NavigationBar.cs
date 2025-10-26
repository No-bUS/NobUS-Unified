using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using static NobUS.Frontend.MAUI.Presentation.Styles;

namespace NobUS.Frontend.MAUI.Presentation.Components;

internal class NavigationBarState
{
    public NavigationBarItem? SelectedItem { get; set; }
}

internal interface INavigationAware
{
    void OnNavigatedTo();

    void OnNavigatedFrom();
}

internal class NavigationBar : Component<NavigationBarState>
{
    private readonly List<NavigationBarItem> _items = new();

    public void Add(NavigationBarItem item) => _items.Add(item);

    public override VisualNode Render()
    {
        NavigationBarItem? selectedItem = State.SelectedItem ?? _items.FirstOrDefault();

        if (selectedItem is null)
        {
            return new Grid();
        }

        return new Grid("*,auto", "*")
        {
            new Grid
            {
                _items.Select(item =>
                    new ContentView { item.GetContent() }
                        .IsVisible(item == selectedItem)
                        .Opacity(item == selectedItem ? 1 : 0)
                        .InputTransparent(item != selectedItem)
                        .Margin(0, 0, 0, 12)
                ),
            }
                .Padding(0, 0, 0, 12)
                .GridRow(0),
            new Border
            {
                new CollectionView()
                    .ItemsLayout(new HorizontalLinearItemsLayout().ItemSpacing(12))
                    .ItemsSource(_items, RenderItem)
                    .ItemSizingStrategy(ItemSizingStrategy.MeasureAllItems)
                    .VCenter()
                    .HCenter(),
            }
                .StrokeThickness(0)
                .Stroke(Colors.Transparent)
                .HeightRequest(68)
                .Background(
                    new Microsoft.Maui.Controls.LinearGradientBrush
                    {
                        GradientStops =
                        {
                            new Microsoft.Maui.Controls.GradientStop(
                                Styler.Scheme.SurfaceContainerHigh,
                                0.0f
                            ),
                            new Microsoft.Maui.Controls.GradientStop(
                                Styler.Scheme.SurfaceContainer,
                                1.0f
                            ),
                        },
                        EndPoint = new Point(1, 1),
                    }
                )
                .GridRow(1)
                .Margin(16, 0, 16, 18),
        }.Background(
            new Microsoft.Maui.Controls.LinearGradientBrush
            {
                GradientStops =
                {
                    new Microsoft.Maui.Controls.GradientStop(Styler.Scheme.Surface, 0f),
                    new Microsoft.Maui.Controls.GradientStop(Styler.Scheme.SurfaceContainer, 1f),
                },
                EndPoint = new Point(0.5, 1),
            }
        );
    }

    private VisualNode RenderItem(NavigationBarItem item)
    {
        bool selected = item == State.SelectedItem;

        return new VerticalStackLayout
        {
            new Border
            {
                new Label()
                    .Text(char.ConvertFromUtf32((int)item.Icon))
                    .FontFamily("MIcon-Regular")
                    .TextColor(
                        selected
                            ? Styler.Scheme.OnSecondaryContainer
                            : Styler.Scheme.OnSurfaceVariant
                    )
                    .FontSize(Sizes.Medium * 1.25)
                    .HCenter(),
            }
                .HeightRequest(32)
                .WidthRequest(60)
                .Background(
                    new Microsoft.Maui.Controls.LinearGradientBrush
                    {
                        GradientStops =
                        {
                            new Microsoft.Maui.Controls.GradientStop(
                                selected ? Styler.Scheme.SecondaryContainer : Colors.Transparent,
                                0f
                            ),
                            new Microsoft.Maui.Controls.GradientStop(
                                selected ? Styler.Scheme.Secondary : Colors.Transparent,
                                1f
                            ),
                        },
                        EndPoint = new Point(1, 1),
                    }
                )
                .ToCard(28)
                .Padding(0, 4),
            new Label()
                .Text(item.Title)
                .FontSize(12)
                .FontFamily(selected ? "SemiBold" : "Regular")
                .TextColor(selected ? Styler.Scheme.OnSurface : Styler.Scheme.OnSurfaceVariant)
                .HCenter(),
        }
            .OnTapped(() =>
            {
                if (State.SelectedItem == item)
                {
                    item.NotifyReactivated();
                    return;
                }

                var previous = State.SelectedItem;
                SetState(s => s.SelectedItem = item);
                previous?.NotifyDeactivated();
                item.NotifyActivated();
            })
            .MinimumWidthRequest(52)
            .Margin(0, 12, 0, 16)
            .Spacing(4)
            .HStart()
            .VCenter();
    }

    protected override void OnMounted()
    {
        if (_items.Any() && State.SelectedItem == null)
        {
            var first = _items.First();
            SetState(s => s.SelectedItem = first);
            first.NotifyActivated();
        }

        base.OnMounted();
    }
}

internal class NavigationBarItem
{
    private readonly Func<Component> _contentFactory;
    private Component? _content;

    public NavigationBarItem(string title, MaterialIcons icon, Func<Component> contentFactory)
    {
        Title = title;
        Icon = icon;
        _contentFactory = contentFactory;
    }

    public string Title { get; }

    public MaterialIcons Icon { get; }

    public Component GetContent()
    {
        _content ??= _contentFactory();
        return _content;
    }

    public void NotifyActivated()
    {
        if (GetContent() is INavigationAware aware)
        {
            aware.OnNavigatedTo();
        }
    }

    public void NotifyReactivated()
    {
        if (_content is INavigationAware aware)
        {
            aware.OnNavigatedTo();
        }
    }

    public void NotifyDeactivated()
    {
        if (_content is INavigationAware aware)
        {
            aware.OnNavigatedFrom();
        }
    }
}
