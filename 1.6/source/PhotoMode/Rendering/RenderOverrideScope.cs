using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public interface IRenderOverrideParticipant
    {
        void Apply(Camera camera);

        void Restore(Camera camera);
    }

    public class RenderOverrideScope
    {
        private readonly List<IRenderOverrideParticipant> participants = new List<IRenderOverrideParticipant>();
        private readonly List<IRenderOverrideParticipant> appliedParticipants = new List<IRenderOverrideParticipant>();

        private bool scopeActive;

        public bool ScopeActive => scopeActive;

        public void RegisterParticipant(IRenderOverrideParticipant participant)
        {
            if (participant == null || participants.Contains(participant))
            {
                return;
            }

            participants.Add(participant);
        }

        public void UnregisterParticipant(IRenderOverrideParticipant participant)
        {
            participants.Remove(participant);
            appliedParticipants.Remove(participant);
        }

        public void HandlePreCull(Camera camera)
        {
            if (camera == null || camera != Find.Camera)
            {
                return;
            }

            if (!WorldRendererUtility.DrawingMap)
            {
                return;
            }

            if (scopeActive)
            {
                Logger.Warning("RenderOverrideScope: previous scope was still open at the next pre-cull; forcing restore.");
                CloseScope(camera);
            }

            Begin(camera);
        }

        public void ForceCloseIfActive()
        {
            if (!scopeActive)
            {
                return;
            }

            Logger.Message("RenderOverrideScope: force-closing active scope.");
            CloseScope(Find.Camera);
        }

        private void Begin(Camera camera)
        {
            scopeActive = true;
            appliedParticipants.Clear();

            for (int i = 0; i < participants.Count; i++)
            {
                IRenderOverrideParticipant participant = participants[i];
                try
                {
                    participant.Apply(camera);
                }
                finally
                {
                    appliedParticipants.Add(participant);
                }
            }

            OnPostRenderHook.HookOnce(camera, () => CloseScope(camera));
        }

        private void CloseScope(Camera camera)
        {
            if (!scopeActive)
            {
                return;
            }

            try
            {
                for (int i = appliedParticipants.Count - 1; i >= 0; i--)
                {
                    appliedParticipants[i].Restore(camera);
                }
            }
            finally
            {
                appliedParticipants.Clear();
                scopeActive = false;
            }
        }
    }
}
