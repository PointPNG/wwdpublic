using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    /// Whether chairs will speak nearby lines.
    /// </summary>
    public static readonly CVarDef<bool> ChattyChairsEnabled =
        CVarDef.Create("fun.chatty_chairs_enabled", true, CVar.SERVER | CVar.ARCHIVE);
}
