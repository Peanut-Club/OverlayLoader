using SmartOverlays;
using System;
using System.Collections.Generic;
using System.Linq;
using PluginAPI.Core;
using helpers;
using helpers.Time;
using Compendium.Mutes;
using Compendium.Voice.Profiles.Scp;
using Compendium.Voice;
using HarmonyLib;

namespace OverlayLoader {
    public class CompendiumHints {
        public static int ActiveChatVOffset = -14;
        public static Dictionary<uint, TempMessage> ActiveChatTmpMsgs = new Dictionary<uint, TempMessage>();

        public static int PlaybackVOffset = -13;
        public static Dictionary<uint, TempMessage> PlaybackTmpMsgs = new Dictionary<uint, TempMessage>();
        public static readonly Dictionary<uint, List<VoiceModifier>> ActiveModifiers = Traverse.CreateWithType(nameof(Compendium.Voice.VoiceChat)).Field<Dictionary<uint, List<VoiceModifier>>>("_activeModifiers").Value;
        public static string PlaybackMessage = $"<size=70%><b><color=#33FFA5>Playback je aktivní!</color></b></size>";

        public static void Load() {
            MuteManager.OnIssued += IssuedMute;
            MuteManager.OnExpired += ExpiredMute;
            OverlayManager.PreDisplayEvent.Register(UpdateHints);
        }

        public static void UnLoad() {
            MuteManager.OnIssued -= IssuedMute;
            MuteManager.OnExpired -= ExpiredMute;
            OverlayManager.PreDisplayEvent.Unregister(UpdateHints);
        }

        public static void Reload() {
            ActiveChatTmpMsgs.Clear();
            PlaybackTmpMsgs.Clear();
        }

        private static void ExpiredMute(Mute mute) {
            if (!mute.IsExpired() || !Player.TryGet(mute.TargetId, out Player target)) return;
            target.ReferenceHub.AddTempHint("\n\n\n\n\n\n\n\n\n\n\n<color=green>Již nejsi ztlumen!</color>");
        }

        private static void IssuedMute(Mute mute) {
            if (mute.IsExpired() || !Player.TryGet(mute.TargetId, out Player target)) return;
            TimeSpan length = new DateTime(mute.ExpiresAt) - DateTime.Now;
            target.ReferenceHub.AddTempHint($"\n\n\n\n\n\n\n\n\n\n\n<color=red>Jsi ztlumen na {length.UserFriendlySpan()}</color>");
        }

        private static void UpdateHints() {
            UpdatePlaybackHints();
            UpdateScpVoiceHints();
        }

        private static void UpdatePlaybackHints() {
            uint netId;
            ReferenceHub hub;
            foreach (var modifierPair in ActiveModifiers) {
                netId = modifierPair.Key;
                if (!modifierPair.Value.Contains(VoiceModifier.PlaybackEnabled)) continue;

                if (PlaybackTmpMsgs.ContainsKey(netId)) {
                    TempMessage tempMessage = PlaybackTmpMsgs[netId];
                    tempMessage.Duration = OverlayManager.refreshInterval;
                    tempMessage.SetMessages(PlaybackMessage, PlaybackVOffset);
                } else if (ReferenceHub.TryGetHubNetID(netId, out hub)) {
                    PlaybackTmpMsgs[netId] = hub.AddTempHint(PlaybackMessage, duration: OverlayManager.refreshInterval, voffset: PlaybackVOffset);
                }
            }

            foreach (var s in PlaybackTmpMsgs.Where(kv => kv.Value.Expired).ToList()) {
                PlaybackTmpMsgs.Remove(s.Key);
            }
        }

        private static void UpdateScpVoiceHints() {
            foreach (IVoiceProfile voiceProfile in Compendium.Voice.VoiceChat.Profiles) {
                UpdateScpVoiceHint(voiceProfile as ScpVoiceProfile);
            }

            foreach (var s in ActiveChatTmpMsgs.Where(kv => kv.Value.Expired)) {
                ActiveChatTmpMsgs.Remove(s.Key);
            }
        }

        private static void UpdateScpVoiceHint(ScpVoiceProfile scpVoiceProfile) {
            if (scpVoiceProfile == null || !scpVoiceProfile.IsEnabled && scpVoiceProfile.Owner == null) return;
            uint netId = scpVoiceProfile.Owner.netId;
            string message = $"<size=70%><b><color=#33FFA5>Aktivní kanál: {TypeAndColor(scpVoiceProfile.Flag)}</color></b></size>";
            if (ActiveChatTmpMsgs.ContainsKey(netId)) {
                TempMessage tempMessage = ActiveChatTmpMsgs[netId];
                tempMessage.Duration = OverlayManager.refreshInterval;
                tempMessage.SetMessages(message, ActiveChatVOffset);
            } else {
                ActiveChatTmpMsgs[netId] = scpVoiceProfile.Owner.AddTempHint(message, duration: OverlayManager.refreshInterval, voffset: ActiveChatVOffset);
            }
        }

        /*
        [Event]
        private static void d(PlayerChangeRoleEvent ev) {
            ScpVoiceProfile scpVoiceProfile = Compendium.Voice.VoiceChat.Profiles.First(profile => profile.Owner == ev.Player.ReferenceHub) as ScpVoiceProfile;
            if (ActiveChatTmpMsgs.ContainsKey(ev.Player.NetworkId)) {
                if (scpVoiceProfile == null) {
                    ActiveChatTmpMsgs.Remove(ev.Player.NetworkId);
                }
            } else {
                if (scpVoiceProfile != null) {
                    UpdateScpVoiceHint(scpVoiceProfile);
                }
            }
        }*/

        private static string TypeAndColor(ScpVoiceFlag flag) {
            switch (flag) {
                case ScpVoiceFlag.ScpChatOnly:
                    return "<color=#FF0000>SCP</color>";
                case ScpVoiceFlag.ProximityChatOnly:
                    return "<color=#90FF33>Proximity</color>";
                case ScpVoiceFlag.ProximityAndScpChat:
                    return "<color=#FF0000>SCP</color> a <color=#90FF33>Proximity</color>";
                default:
                    return "<color=#FF0000>UNKNOWN</color>";
            }
        }

    }
}
