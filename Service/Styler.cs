using CommunityToolkit.Maui.Markup;
using MaterialColorUtilities.Maui;
using MaterialColorUtilities.Schemes;
using MauiReactor;
using MauiReactor.Shapes;
using Border = MauiReactor.Border;
using Label = MauiReactor.Label;

namespace NobUS.Frontend.MAUI.Service
{
    internal static class Styler
    {
        public static Scheme<Color> Scheme =>
            ((MaterialColorService)IMaterialColorService.Current).SchemeMaui;

        public static Scheme<Color> UseScheme(this Component _) => Scheme;

        public static Label BaseStyle(this Label label) => label.FontFamily("Regular").FontSize(16);

        public static Label Regular(this Label label) => label.FontFamily("Regular");

        public static Label SemiBold(this Label label) => label.FontFamily("SemiBold");

        public static Label Bold(this Label label) => label.FontFamily("Bold");

        public static Label ExtraBold(this Label label) => label.FontFamily("ExtraBold");

        public static Label Small(this Label label) => label.FontSize(Styles.Sizes.Small);

        public static Label Base(this Label label) => label.FontSize(Styles.Sizes.Base);

        public static Label Medium(this Label label) => label.FontSize(Styles.Sizes.Medium);

        public static Label Large(this Label label) => label.FontSize(Styles.Sizes.Large);

        public static Border ToCard(this Border border, int cornerRadius) =>
            border
                .StrokeShape(new RoundRectangle().CornerRadius(20))
                .StrokeThickness(0)
                .Stroke(Colors.Transparent)
                .Margin(-1);
    }

    internal static class Styles
    {
        internal static class Sizes
        {
            internal static int Small = 12;
            internal static int Base = 16;
            internal static int Medium = 20;
            internal static int Large = 24;
        }
    }
}
