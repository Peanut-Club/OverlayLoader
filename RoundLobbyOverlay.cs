using Compendium;
using Compendium.Attributes;
using Compendium.Events;
using Compendium.Enums;
using helpers.Attributes;
using PluginAPI.Events;
using SmartOverlays;
using System;
using helpers.Configuration;
using InventorySystem.GUI.Descriptions;
using System.Globalization;
using System.Data.SqlTypes;
using helpers;
using Compendium.Features;

namespace OverlayLoader {

    public static class RoundLobbyOverlayConfig {
        public const string ExpireDateFormat = "dd-MM-yyyy HH:mm";

        [Config(Name = nameof(PartnersInfo), Description = "Partners Info on left site of the screen (empty string: \"\" to disable)")]
        public static string PartnersInfo { get; set; } = "<size=75%><color=#383><u>Partneři:</u></color></size>\\n<color=#0c0>BK Hosting Solutions:\\n<size=75%>- Cenově výhodný hosting s Anti-DDoS ochranou</color></size>\\n\\n<size=55%><color=#6f9>Více infa na discordu. (Odkaz je v Server Infu)</size>";


        [Config(Name = nameof(RecruitmentInfo), Description = "Recruitment Info on bottom of the screen (empty string: \"\" to disable)")]
        public static string RecruitmentInfo { get; set; } = "";


        [Config(Name = nameof(RecruitmentExpireTime), Description = $"DateTime ({ExpireDateFormat})")]
        public static string RecruitmentExpireTime { get; set; } = "";


    }

    public class RoundLobbyOverlay : OverlayManager.Overlay {
        private static RoundLobbyOverlay roundLobbyOverlay;
        private static bool isInLobby = false;

        //public readonly TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        public RoundLobbyOverlay() : base("Round Lobby Overlay"/*, 15*/) {
            var partnersInfo = RoundLobbyOverlayConfig.PartnersInfo;
            if (!string.IsNullOrEmpty(partnersInfo)) {
                var message = partnersInfo.Replace("\r\n", "\n").Replace("\\n", "\n").Replace("<br>", "\n");
                message = TempMessage.TrimStartCountNewLine(message, out _);
                var messages = TempMessage.SplitToMessages(message.TrimEnd(), OverlayManager.MaxCharsOnLine, MessageAlign.FullLeft);

                float messageLine = 5f;
                bool firstLine = true;
                foreach (var msg in messages) {
                    FLog.Info($"content: '{msg.Content}'; pixelSize: {msg.EmSize}");
                    if (!firstLine)
                        messageLine -= msg.EmSize;
                    firstLine = false;

                    AddMessage(msg, messageLine, msg.Align);
                }
            }

            if (TryGetRecruitInfo(out string recruitmentInfo)) {
                AddMessage(new Message(recruitmentInfo), -14, MessageAlign.Center);
            }

            string welcomeMessage = "<size=300%><color=#0d0>Vítejte na Peanut Clubu!</color></size>";
            AddMessage(new Message(welcomeMessage), 11.5f, MessageAlign.Center);
        }

        private static bool TryGetRecruitInfo(out string info) {
            info = RoundLobbyOverlayConfig.RecruitmentInfo;
            if (string.IsNullOrEmpty(info)) {
                return false;
            }

            var recruitmentExpireTime = RoundLobbyOverlayConfig.RecruitmentExpireTime;
            if (string.IsNullOrEmpty(recruitmentExpireTime)) {
                return true;
            }

            var expireFormat = RoundLobbyOverlayConfig.ExpireDateFormat;
            if (!DateTime.TryParseExact(recruitmentExpireTime, expireFormat, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out var expireTime)) {
                FLog.Error($"Invalid format of {RoundLobbyOverlayConfig.RecruitmentExpireTime}: '{recruitmentExpireTime}'; must be format: '{expireFormat}'");
                return false;
                //RoundLobbyOverlayConfig.RecruitmentExpireTime = "";
            }

            FLog.Info($"Recruitment info parsed time: {expireTime}");
            if (DateTime.Now >= expireTime) { //expired
                return false;
            }

            return true;
        }

        [Load]
        private static void OnLoad() {
            roundLobbyOverlay = new RoundLobbyOverlay();
        }

        [Event]
        private static void PlayerJoined(PlayerJoinedEvent ev) {
            if (!isInLobby || roundLobbyOverlay == null) return;
            var hub = ev.Player.ReferenceHub;
            Calls.NextFrame(() => hub.RegisterOverlay(roundLobbyOverlay));
        }

        [RoundStateChanged(RoundState.WaitingForPlayers)]
        private static void OnWaiting() {
            isInLobby = true;
        }

        [RoundStateChanged(RoundState.InProgress)]
        private static void OnGameStart() {
            isInLobby = false;
            ReferenceHub.AllHubs.ForEach(hub => hub.UnregisterOverlay(roundLobbyOverlay, false));
        }
    }
}
