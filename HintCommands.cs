using BetterCommands.Permissions;
using BetterCommands;
using Hints;
using PluginAPI.Core;
using SmartOverlays;

namespace OverlayLoader {
    public static class HintCommands {
        [Command("hint", CommandType.RemoteAdmin)]
        [Description("Show hint")]
        [Permission(PermissionLevel.Lowest)]
        public static string HintCmd(Player sender, string message) {
            sender.ReferenceHub.hints.Show(new TextHint(message, new HintParameter[] { new StringHintParameter(message) }, null, 30));
            return $"Send to {sender.Nickname}";
        }

        [Command("tempmessage", CommandType.RemoteAdmin)]
        [CommandAliases("tmes", "thint")]
        [Description("Show temporary hint message")]
        [Permission(PermissionLevel.Lowest)]
        public static string TempMessageCmd(Player sender, float duration, string message) {
            TempMessage tm = sender.ReferenceHub.AddTempHint(message, duration);
            return $"Temporary message showed for {duration} seconds!";
        }

        [Command("primarymessage", CommandType.RemoteAdmin)]
        [CommandAliases("pmes", "phint")]
        [Description("Set primary hint message")]
        [Permission(PermissionLevel.Lowest)]
        public static string PrimaryHintMessageCmd(Player sender, float duration, string message) {
            sender.ReferenceHub.SetPrimaryHint(message, duration);
            return $"Primary message scheduled for {duration} seconds!";
        }


        [Command("odebug", CommandType.GameConsole)]
        [Description("Enable debug info")]
        [Permission(PermissionLevel.Lowest)]
        public static string CmdDebug(Player sender) {
            OverlayManager.debugInfo = !OverlayManager.debugInfo;
            return $"Debug Enabled: {OverlayManager.debugInfo}";
        }
    }
}
