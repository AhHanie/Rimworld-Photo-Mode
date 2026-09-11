using System.Reflection;
using HarmonyLib;
using Verse;

namespace Photo_Mode
{
    public enum PhotoModePatchLifetime
    {
        Dynamic,
        AlwaysLoaded
    }

    public static class PhotoModePatchController
    {
        public const string RuntimeCategory = "PhotoMode.Runtime";

        private static Harmony harmony;
        private static bool categoryApplied;
        private static bool categoryRemovalPending;
        private static PhotoModePatchLifetime? pendingLifetime;

        public static PhotoModePatchLifetime CurrentLifetime { get; private set; } = PhotoModePatchLifetime.Dynamic;

        public static PhotoModePatchLifetime DisplayedLifetime => pendingLifetime ?? CurrentLifetime;

        public static bool CategoryApplied => categoryApplied;

        public static void Init(Harmony harmonyInstance)
        {
            harmony = harmonyInstance;
            CurrentLifetime = ModSettings.PatchLifetime;
        }

        public static void EnsureCategoryApplied()
        {
            if (categoryApplied)
            {
                categoryRemovalPending = false;
                return;
            }

            harmony.PatchCategory(Assembly.GetExecutingAssembly(), RuntimeCategory);
            categoryApplied = true;
            categoryRemovalPending = false;
        }

        public static void RemoveCategoryIfDynamic()
        {
            if (CurrentLifetime == PhotoModePatchLifetime.AlwaysLoaded)
            {
                return;
            }

            RemoveCategoryNow();
        }

        public static void MarkCategoryRemovalPendingIfDynamic()
        {
            if (CurrentLifetime == PhotoModePatchLifetime.AlwaysLoaded)
            {
                return;
            }

            categoryRemovalPending = true;
        }

        public static void OnGameInitialized()
        {
            if (CurrentLifetime == PhotoModePatchLifetime.AlwaysLoaded)
            {
                EnsureCategoryApplied();
            }

            TryApplyPendingLifetime();
        }

        public static void PumpMainThread()
        {
            if (categoryRemovalPending)
            {
                RemoveCategoryNow();
            }

            TryApplyPendingLifetime();
        }

        public static void RequestLifetimeChange(PhotoModePatchLifetime lifetime)
        {
            if (lifetime == CurrentLifetime)
            {
                pendingLifetime = null;
                return;
            }

            pendingLifetime = lifetime;
            TryApplyPendingLifetime();
        }

        public static bool IsSettingLocked()
        {
            if (pendingLifetime.HasValue)
            {
                return true;
            }

            PhotoModeManager manager = PhotoModeManager.Current;
            return manager != null && (manager.State.Active || manager.IsTransitioning);
        }

        private static void RemoveCategoryNow()
        {
            if (!categoryApplied)
            {
                categoryRemovalPending = false;
                return;
            }

            harmony.UnpatchCategory(Assembly.GetExecutingAssembly(), RuntimeCategory);
            categoryApplied = false;
            categoryRemovalPending = false;
        }

        private static bool CanApplyPendingLifetimeNow()
        {
            PhotoModeManager manager = PhotoModeManager.Current;
            if (manager != null && (manager.State.Active || manager.IsTransitioning))
            {
                return false;
            }

            return Current.Game != null;
        }

        private static void TryApplyPendingLifetime()
        {
            if (!pendingLifetime.HasValue || !CanApplyPendingLifetimeNow())
            {
                return;
            }

            PhotoModePatchLifetime target = pendingLifetime.Value;
            pendingLifetime = null;
            CurrentLifetime = target;
            ModSettings.PatchLifetime = target;
            ModSettings.Save();

            if (target == PhotoModePatchLifetime.AlwaysLoaded)
            {
                EnsureCategoryApplied();
            }
            else
            {
                RemoveCategoryNow();
            }
        }
    }
}
