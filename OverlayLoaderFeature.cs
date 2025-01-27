using Compendium.Features;
using Compendium.Events;
using PluginAPI.Events;
using SmartOverlays;

namespace OverlayLoader {
    public class OverlayLoaderFeature : ConfigFeatureBase {
        public override string Name => "OverlayLoader";
        public override bool IsPatch => false;
        public override bool CanBeShared => true;


        public override void Load() {
            base.Load();
            OverlayManager.ResetManager();
            CompendiumHints.Load();
            OverlayManager.RegisterEvents();
            Compendium.Messages.HintMessage.HintProxies += OverlayManager.SetPrimaryHint;
        }

        public override void Unload() {
            base.Unload();
            CompendiumHints.UnLoad();
            OverlayManager.UnregisterEvents();
            Compendium.Messages.HintMessage.HintProxies -= OverlayManager.SetPrimaryHint;
            OverlayManager.ResetManager();
        }

        public override void Reload() {
            base.Reload();
            OverlayManager.RegisterEvents();
            OverlayManager.ResetManager();
            CompendiumHints.Reload();
        }

        public override void Restart() {
            base.Restart();
            OverlayManager.ResetManager();
            CompendiumHints.Reload();
        }


        [Event]
        public static void PlayerLeft(PlayerLeftEvent ev) {
            var hub = ev.Player.ReferenceHub;
            hub.UnregisterAllOverlays();
        }
    }
}
