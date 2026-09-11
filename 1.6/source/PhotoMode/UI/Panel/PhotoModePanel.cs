using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public partial class PhotoModePanel : Window
    {
        private const float PanelWidth = 320f;
        private const float EdgeMargin = 8f;
        private const float HeaderTopPadding = 14f;
        private const float HeaderRowHeight = 30f;
        private const float PreviewBypassRowHeight = 44f;
        private const float SectionHeaderHeight = 34f;
        private const float PlaceholderHeight = 24f;

        private readonly PhotoModeManager manager;
        private Vector2 scrollPosition;
        private float contentViewHeight = 300f;

        internal bool SuppressExitOnClose;

        public override Vector2 InitialSize => new Vector2(PanelWidth, UI.screenHeight - EdgeMargin * 2f);

        protected override float Margin => 12f;

        public PhotoModePanel(PhotoModeManager manager)
        {
            this.manager = manager;
            InitializePresetsCache();

            layer = WindowLayer.GameUI;
            preventCameraMotion = false;
            absorbInputAroundWindow = false;
            closeOnClickedOutside = false;
            doCloseX = true;
            drawInScreenshotMode = true;
            resizeable = false;
            draggable = false;
            onlyOneOfTypeAllowed = true;
            soundAppear = null;
            soundClose = null;
        }

        protected override void SetInitialSizeAndPosition()
        {
            Vector2 size = InitialSize;
            bool onRight = ModSettings.PanelSide == PhotoModePanelSide.Right;
            float x = onRight ? UI.screenWidth - size.x - EdgeMargin : EdgeMargin;
            windowRect = new Rect(x, EdgeMargin, size.x, size.y).Rounded();
        }

        public override void PreClose()
        {
            base.PreClose();

            if (SuppressExitOnClose)
            {
                return;
            }

            manager.RequestExitFromPanel();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect headerRect = new Rect(inRect.x, inRect.y + HeaderTopPadding, inRect.width, HeaderRowHeight);
            DrawHeader(headerRect);

            Rect previewBypassRect = new Rect(inRect.x, headerRect.yMax + 4f, inRect.width, PreviewBypassRowHeight);
            DrawPreviewBypassButton(previewBypassRect);

            float nextY = previewBypassRect.yMax + 4f;
            if (Prefs.DevMode)
            {
                Rect devToolbarRect = new Rect(inRect.x, nextY, inRect.width, HeaderRowHeight);
                DrawShowDevToolbarToggle(devToolbarRect);
                nextY = devToolbarRect.yMax + 4f;
            }

            Rect scrollOuterRect = new Rect(inRect.x, nextY, inRect.width, inRect.yMax - nextY);
            DrawSections(scrollOuterRect);
        }

        private void DrawSections(Rect outRect)
        {
            bool needsScrollbar = contentViewHeight > outRect.height;
            float viewWidth = outRect.width - (needsScrollbar ? 16f : 0f);
            Rect viewRect = new Rect(0f, 0f, viewWidth, contentViewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(new Rect(0f, 0f, viewWidth, 999999f));

            DrawCameraSection(listing, ref ModSettings.CameraSectionCollapsed);
            DrawOverlaySection(listing, ref ModSettings.OverlaySectionCollapsed);
            DrawSceneSection(listing, ref ModSettings.SceneSectionCollapsed);
            DrawPawnsSection(listing, ref ModSettings.PawnsSectionCollapsed);
            DrawPresetsSection(listing, ref ModSettings.PresetsSectionCollapsed);
            DrawImageSection(listing, ref ModSettings.ImageSectionCollapsed);
            DrawEffectsSection(listing, ref ModSettings.EffectsSectionCollapsed);
            DrawCaptureSection(listing, ref ModSettings.CaptureSectionCollapsed);

            contentViewHeight = listing.CurHeight;
            listing.End();

            Widgets.EndScrollView();
        }
    }
}
