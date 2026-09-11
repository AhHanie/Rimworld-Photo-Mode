using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(Init, "PhotoMode.LoadingLabel", doAsynchronously: false, null);
        }

        private void Init()
        {
            GetSettings<ModSettings>();

            Harmony harmony = new Harmony("sk.photomode");
            PhotoModePatchController.Init(harmony);

            GameObject pumpObject = new GameObject("PhotoModePatchPump");
            UnityEngine.Object.DontDestroyOnLoad(pumpObject);
            pumpObject.AddComponent<PhotoModePatchPump>();
        }

        public override string SettingsCategory()
        {
            return "PhotoMode.SettingsTitle".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            ModSettingsWindow.Draw(inRect);
            base.DoSettingsWindowContents(inRect);
        }
    }
}
