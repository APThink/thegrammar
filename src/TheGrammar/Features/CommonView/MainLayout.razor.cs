using MudBlazor;

namespace TheGrammar.Features.CommonView;

public partial class MainLayout
{
    private MudTheme _theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#5cb85c",
            PrimaryContrastText = "#ffffff",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#5cb85c",
            PrimaryContrastText = "#ffffff",
        }
    };
}