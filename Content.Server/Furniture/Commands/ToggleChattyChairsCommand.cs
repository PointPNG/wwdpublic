using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server.Furniture.Commands;

public sealed class ToggleChattyChairsCommand : IConsoleCommand
{
    public string Command => "togglechattychairs";
    public string Description => "Toggles talking chairs";
    public string Help => "Usage: togglechattychairs [true/false]";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var cfg = IoCManager.Resolve<IConfigurationManager>();
        if (args.Length > 1)
        {
            shell.WriteError($"Expected 0 or 1 arguments. {Help}");
            return;
        }

        bool enabled = cfg.GetCVar(CCVars.ChattyChairsEnabled);

        if (args.Length == 1)
        {
            if (!bool.TryParse(args[0], out enabled))
            {
                shell.WriteError("Argument must be boolean.");
                return;
            }
        }
        else
        {
            enabled = !enabled;
        }

        cfg.SetCVar(CCVars.ChattyChairsEnabled, enabled);
        shell.WriteLine(enabled ? "Chatty chairs enabled." : "Chatty chairs disabled.");
    }
}
