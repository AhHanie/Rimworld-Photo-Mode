using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Photo_Mode
{
    public partial class PhotoModePanel
    {
        private string newPresetNameBuffer = string.Empty;
        private string editingPresetId;
        private string editingNameBuffer = string.Empty;
        private string presetStatusMessage;
        private bool presetStatusIsError;
        private List<PhotoPresetInfo> presetsCache;

        private void InitializePresetsCache()
        {
            presetsCache = manager.ListPresets();
        }

        private void DrawPresetsSection(Listing_Standard listing, ref bool collapsed)
        {
            collapsed = DrawSectionHeader(listing, collapsed, "PhotoMode.Section.Presets");

            if (!collapsed)
            {
                DrawSaveNewPresetRow(listing);

                listing.Gap(6f);

                if (presetsCache.Count == 0)
                {
                    Rect noneRect = listing.GetRect(PlaceholderHeight);
                    GUI.color = Color.gray;
                    Widgets.Label(noneRect, "PhotoMode.Presets.None".Translate());
                    GUI.color = Color.white;
                }
                else
                {
                    for (int i = 0; i < presetsCache.Count; i++)
                    {
                        DrawPresetRow(listing, presetsCache[i]);
                        listing.Gap(4f);
                    }
                }

                listing.Gap(4f);

                Rect resetRect = listing.GetRect(PlaceholderHeight);
                if (Widgets.ButtonText(resetRect, "PhotoMode.Presets.ResetToDefault".Translate()))
                {
                    manager.ResetVisualPreset();
                    SetPresetStatus("PhotoMode.Presets.Status.ResetDone".Translate(), false);
                }

                if (!string.IsNullOrEmpty(presetStatusMessage))
                {
                    Rect statusRect = listing.GetRect(PlaceholderHeight);
                    GUI.color = presetStatusIsError ? Color.red : Color.green;
                    Widgets.Label(statusRect, presetStatusMessage);
                    GUI.color = Color.white;
                }
            }

            listing.Gap(4f);
        }

        private void RefreshPresets()
        {
            presetsCache = manager.ListPresets();
        }

        private void SetPresetStatus(string message, bool isError)
        {
            presetStatusMessage = message;
            presetStatusIsError = isError;
        }

        private void DrawSaveNewPresetRow(Listing_Standard listing)
        {
            Rect rowRect = listing.GetRect(PlaceholderHeight);
            Rect fieldRect = rowRect.LeftPart(0.65f).ContractedBy(2f, 0f);
            Rect buttonRect = rowRect.RightPart(0.32f).ContractedBy(2f, 0f);

            newPresetNameBuffer = Widgets.TextField(fieldRect, newPresetNameBuffer);

            if (Widgets.ButtonText(buttonRect, "PhotoMode.Presets.Save".Translate()))
            {
                PhotoPresetOperationResult result = manager.SavePreset(newPresetNameBuffer);
                if (result.Status == PhotoPresetOperationStatus.InvalidName)
                {
                    SetPresetStatus("PhotoMode.Presets.Status.NameRequired".Translate(), true);
                }
                else if (result.Success)
                {
                    newPresetNameBuffer = string.Empty;
                    RefreshPresets();
                    SetPresetStatus("PhotoMode.Presets.Status.Saved".Translate(result.Preset.RenamableLabel), false);
                }
                else
                {
                    SetPresetStatus("PhotoMode.Presets.Status.SaveFailed".Translate(), true);
                }
            }
        }

        private void DrawPresetRow(Listing_Standard listing, PhotoPresetInfo preset)
        {
            if (editingPresetId == preset.Id)
            {
                Rect editRowRect = listing.GetRect(PlaceholderHeight);
                Rect fieldRect = editRowRect.LeftPart(0.6f).ContractedBy(2f, 0f);
                Rect confirmRect = editRowRect.RightPart(0.37f).LeftHalf().ContractedBy(2f, 0f);
                Rect cancelRect = editRowRect.RightPart(0.37f).RightHalf().ContractedBy(2f, 0f);

                editingNameBuffer = Widgets.TextField(fieldRect, editingNameBuffer);

                if (Widgets.ButtonText(confirmRect, "PhotoMode.Presets.Confirm".Translate()))
                {
                    PhotoPresetOperationResult result = manager.RenamePreset(preset.Id, editingNameBuffer);
                    if (result.Status == PhotoPresetOperationStatus.InvalidName)
                    {
                        SetPresetStatus("PhotoMode.Presets.Status.NameRequired".Translate(), true);
                    }
                    else
                    {
                        editingPresetId = null;
                        if (result.Success)
                        {
                            RefreshPresets();
                            SetPresetStatus("PhotoMode.Presets.Status.Renamed".Translate(result.Preset.RenamableLabel), false);
                        }
                        else
                        {
                            SetPresetStatus("PhotoMode.Presets.Status.RenameFailed".Translate(), true);
                        }
                    }
                }

                if (Widgets.ButtonText(cancelRect, "PhotoMode.Presets.Cancel".Translate()))
                {
                    editingPresetId = null;
                }

                return;
            }

            Rect labelRowRect = listing.GetRect(PlaceholderHeight);
            Rect labelRect = labelRowRect.LeftPart(0.62f);
            Rect loadRect = labelRowRect.RightPart(0.35f);

            Widgets.Label(labelRect, preset.RenamableLabel);
            if (Widgets.ButtonText(loadRect, "PhotoMode.Presets.Load".Translate()))
            {
                PhotoPresetOperationResult result = manager.LoadPreset(preset.Id);
                if (result.Status == PhotoPresetOperationStatus.Success)
                {
                    SetPresetStatus("PhotoMode.Presets.Status.Loaded".Translate(preset.RenamableLabel), false);
                }
                else if (result.Status == PhotoPresetOperationStatus.WeatherDefMissing)
                {
                    SetPresetStatus("PhotoMode.Presets.Status.WeatherMissing".Translate(preset.RenamableLabel), true);
                }
                else if (result.Status == PhotoPresetOperationStatus.LutMissing)
                {
                    SetPresetStatus("PhotoMode.Presets.Status.LutMissing".Translate(preset.RenamableLabel), true);
                }
                else
                {
                    SetPresetStatus("PhotoMode.Presets.Status.LoadFailed".Translate(), true);
                }
            }

            Rect actionsRowRect = listing.GetRect(PlaceholderHeight);
            float columnWidth = actionsRowRect.width / 3f;

            Rect renameRect = new Rect(actionsRowRect.x, actionsRowRect.y, columnWidth, actionsRowRect.height).ContractedBy(2f, 0f);
            if (Widgets.ButtonText(renameRect, "PhotoMode.Presets.Rename".Translate()))
            {
                editingPresetId = preset.Id;
                editingNameBuffer = preset.RenamableLabel;
            }

            Rect duplicateRect = new Rect(actionsRowRect.x + columnWidth, actionsRowRect.y, columnWidth, actionsRowRect.height).ContractedBy(2f, 0f);
            if (Widgets.ButtonText(duplicateRect, "PhotoMode.Presets.Duplicate".Translate()))
            {
                PhotoPresetOperationResult result = manager.DuplicatePreset(preset.Id);
                if (result.Success)
                {
                    RefreshPresets();
                    SetPresetStatus("PhotoMode.Presets.Status.Duplicated".Translate(result.Preset.RenamableLabel), false);
                }
                else
                {
                    SetPresetStatus("PhotoMode.Presets.Status.DuplicateFailed".Translate(), true);
                }
            }

            Rect deleteRect = new Rect(actionsRowRect.x + columnWidth * 2f, actionsRowRect.y, columnWidth, actionsRowRect.height).ContractedBy(2f, 0f);
            if (Widgets.ButtonText(deleteRect, "PhotoMode.Presets.Delete".Translate()))
            {
                string label = preset.RenamableLabel;
                if (manager.DeletePreset(preset.Id))
                {
                    if (editingPresetId == preset.Id)
                    {
                        editingPresetId = null;
                    }

                    RefreshPresets();
                    SetPresetStatus("PhotoMode.Presets.Status.Deleted".Translate(label), false);
                }
                else
                {
                    SetPresetStatus("PhotoMode.Presets.Status.DeleteFailed".Translate(), true);
                }
            }
        }
    }
}
