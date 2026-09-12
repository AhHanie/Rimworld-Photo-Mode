using RimWorld;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public class GameComponent_PhotoMode : GameComponent
    {
        public readonly PhotoModeManager Manager = new PhotoModeManager();

        public GameComponent_PhotoMode(Game game)
        {
            PhotoModeManager.SetCurrent(Manager);
        }

        public override void GameComponentUpdate()
        {
            base.GameComponentUpdate();

            if (!Manager.State.Active)
            {
                return;
            }

            Manager.State.PreviewBypassed = PhotoModeKeyBindingDefOf.PhotoMode_HoldPreviewBypass.IsDown || Manager.State.PreviewBypassMouseHeld;
            Manager.ValidatePawnState();
            Manager.ValidateEnvironmentState();
            Manager.ValidateSceneState();
        }

        public override void GameComponentOnGUI()
        {
            base.GameComponentOnGUI();

            PhotoModeOverlaySupplement.DrawColonistBarIfSuppressedByBaseline();

            if (Manager.State.Active && Manager.State.ShowDevToolbar && Prefs.DevMode && Find.UIRoot.screenshotMode.FiltersCurrentEvent)
            {
                Find.UIRoot.debugWindowOpener.DevToolStarterOnGUI();
            }

            if (Manager.State.Active && !Manager.State.PanelVisible && !ModSettings.HideHandleWhenPanelHidden)
            {
                PhotoModeHiddenPanelHandle.Draw(Manager);
            }

            if (Find.WindowStack.IsOpen<Dialog_DefineBinding>())
            {
                return;
            }

            if (Manager.IsTransitioning)
            {
                return;
            }

            if (Manager.State.Active && PhotoModeKeyBindingDefOf.PhotoMode_Capture.KeyDownEvent)
            {
                Event.current.Use();
                Manager.RequestCapture();
            }

            if (Manager.State.Active && PhotoModeKeyBindingDefOf.PhotoMode_TogglePanel.KeyDownEvent)
            {
                Event.current.Use();
                if (Manager.State.PanelVisible)
                {
                    Manager.HidePanel();
                }
                else
                {
                    Manager.ShowPanel();
                }
            }

            if (!PhotoModeKeyBindingDefOf.PhotoMode_ToggleActive.KeyDownEvent)
            {
                return;
            }

            if (Manager.State.Active)
            {
                Event.current.Use();
                Manager.RequestExit("KeyBindToggle");
                return;
            }

            if (!Manager.CanEnter())
            {
                return;
            }

            Event.current.Use();
            Manager.RequestEnter();
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            Manager.Reset();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            Manager.Reset();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Manager.Reset();
            PhotoModePatchController.OnGameInitialized();
            Logger.Message("Photo Mode game component initialized.");
        }

        public override void ExposeData()
        {
        }
    }
}
