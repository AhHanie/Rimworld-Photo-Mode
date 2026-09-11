using RimWorld;

namespace Photo_Mode
{
    public class MainButtonWorker_PhotoMode : MainButtonWorker
    {
        public override bool Disabled
        {
            get
            {
                if (base.Disabled)
                {
                    return true;
                }

                PhotoModeManager manager = PhotoModeManager.Current;
                if (manager == null)
                {
                    return true;
                }

                if (manager.IsTransitioning)
                {
                    return true;
                }

                return !manager.State.Active && !manager.CanEnter();
            }
        }

        public override bool Visible => base.Visible && ModSettings.ShowMainButton;

        public override void Activate()
        {
            PhotoModeManager manager = PhotoModeManager.Current;
            if (manager == null)
            {
                return;
            }

            if (manager.State.Active)
            {
                manager.RequestExit("MainButton");
                return;
            }

            if (!manager.CanEnter())
            {
                return;
            }

            manager.RequestEnter();
        }
    }
}
