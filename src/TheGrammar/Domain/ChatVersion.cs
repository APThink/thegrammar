using System.ComponentModel;

namespace TheGrammar.Domain;

public enum ChatVersion
{
    [Description("3.5 Turbo")]
    Turbo35,
    [Description("4 Turbo")]
    Turbo4
}