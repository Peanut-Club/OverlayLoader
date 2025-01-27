using SmartOverlays;
using Compendium;
using PluginAPI.Core;
using PluginAPI.Events;
using Compendium.Events;
using helpers.Attributes;
using System;
using Compendium.Features;
using PlayerRoles;

namespace OverlayLoader {
    public class PeanutClubOverlay : OverlayManager.Overlay {
        private static readonly PeanutClubOverlay peanutClubOverlay = new PeanutClubOverlay();
        private Message _roundInfoMessage;
        private Message _timeMessage;
        //public readonly TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        public PeanutClubOverlay() : base("Peanut Club Overlay"/*, 15*/) {
            _roundInfoMessage = new Message("");
            AddMessage(_roundInfoMessage, 15.4f);

            _timeMessage = new Message("");
            AddMessage(_timeMessage, 15.4f, align: MessageAlign.Left);

            string altName = World.AlternativeServerName;
            string serverName = "<size=65%><color=#009fff>P</color><color=#1594ee>e</color><color=#2a8ade>a</color><color=#4080cd>n</color><color=#5576bd>u</color><color=#6b6cad>t</color> <color=#96578c>C</color><color=#ab4d7c>l</color><color=#c1436b>u</color><color=#d6395b>b</color>";
            string portName = (string.IsNullOrWhiteSpace(altName) || altName == "none") ? "</size>" : " <color=red>|</color></size> <size=50%><color=#00E4FF>" + altName + "</color></size>";
            //AddMessage(new Message("<size=60%><color=red>Probíhá nábor do AT! Více infa na našem discordu.</color></size>"), -14.45f);
            AddMessage(new Message($"{serverName}{portName}"), -15f);
        }

        public override void UpdateMessages() {
            int minutes = (int)Round.Duration.TotalSeconds / 60;
            int seconds = (int)Round.Duration.TotalSeconds % 60;
            _roundInfoMessage.Content = string.Format("<size=50%>Server TPS: {0:00} | Čas kola: {1:00}:{2:00}</size>", World.Ticks, minutes, seconds);
            _timeMessage.Content = $"<size=50%>{DateTime.Now.ToString("dd. MM. yyyy HH:mm:ss")}</size>";
        }

        [Load]
        public static void RegisterEvents() {
            OverlayManager.PreDisplayEvent.Register(peanutClubOverlay.UpdateMessages);
        }

        [Unload]
        public static void UnregisterEvents() {
            OverlayManager.PreDisplayEvent.Unregister(peanutClubOverlay.UpdateMessages);
        }


        [Event]
        public static void PlayerJoined(PlayerJoinedEvent ev) {
            var hub = ev.Player.ReferenceHub;
            Calls.NextFrame(() => hub.RegisterOverlay(peanutClubOverlay));
        }
    }
}
