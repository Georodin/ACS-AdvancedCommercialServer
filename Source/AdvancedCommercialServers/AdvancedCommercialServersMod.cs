using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AdvancedCommercialServers
{
    public class AdvancedCommercialServersMod : Mod
    {
        private ServerModSettings settings;

        public AdvancedCommercialServersMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<ServerModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);

            float spacing = 5f; // Adjust this value to increase/decrease the spacing between setting and Reset button.

            // Generate Heat Checkbox and Reset Button
            listing_Standard.CheckboxLabeled("Generate Heat", ref ServerModSettings.generateHeat, "Enable or Disable Heat Generation.");
            listing_Standard.Gap(0f);
            if (listing_Standard.ButtonText("Reset"))
            {
                ServerModSettings.generateHeat = true;
            }
            listing_Standard.Gap(spacing);


            // Generate Heat Multiplier Slider and Reset Button
            listing_Standard.Label($"Generate Heat Multiplier: {ServerModSettings.generateHeatMultiplier}");
            listing_Standard.Gap(0f);
            ServerModSettings.generateHeatMultiplier = listing_Standard.Slider(ServerModSettings.generateHeatMultiplier, 0f, 10f);
            if (listing_Standard.ButtonText("Reset"))
            {
                ServerModSettings.generateHeatMultiplier = 1.0f;
            }
            listing_Standard.Gap(spacing);


            // Generation Speed Multiplier Slider and Reset Button
            listing_Standard.Label($"Generation Speed Multiplier: {ServerModSettings.generationSpeedMultiplier}");
            listing_Standard.Gap(0f);
            ServerModSettings.generationSpeedMultiplier = listing_Standard.Slider(ServerModSettings.generationSpeedMultiplier, 0f, 20f);
            if (listing_Standard.ButtonText("Reset"))
            {
                ServerModSettings.generationSpeedMultiplier = 1.0f;
            }
            listing_Standard.Gap(spacing);


            // Power Consumption Slider and Reset Button
            listing_Standard.Label($"Power Consumption: {ServerModSettings.powerConsumption}");
            listing_Standard.Gap(0f);
            ServerModSettings.powerConsumption = listing_Standard.Slider(ServerModSettings.powerConsumption, 0f, 10f);
            if (listing_Standard.ButtonText("Reset"))
            {
                ServerModSettings.powerConsumption = 1.0f;
            }
            listing_Standard.Gap(spacing);


            // Power Consumption Slider and Reset Button
            listing_Standard.Label($"Research Multiplier: {ServerModSettings.researchMultiplier}");
            listing_Standard.Gap(0f);
            ServerModSettings.researchMultiplier = listing_Standard.Slider(ServerModSettings.researchMultiplier, 0f, 10f);
            if (listing_Standard.ButtonText("Reset"))
            {
                ServerModSettings.researchMultiplier = 1.0f;
            }
            listing_Standard.Gap(spacing);


            // Auto Shutdown Temperature Slider and Reset Button
            listing_Standard.Label($"Auto Shutdown Temperature (Celsius): {ServerModSettings.autoShutdownTemperatureCelsius}");
            listing_Standard.Gap(0f);
            ServerModSettings.autoShutdownTemperatureCelsius = (int)listing_Standard.Slider(ServerModSettings.autoShutdownTemperatureCelsius, 0, 1000);
            if (listing_Standard.ButtonText("Reset"))
            {
                ServerModSettings.autoShutdownTemperatureCelsius = 50;
            }
            listing_Standard.Gap(spacing);

            if (listing_Standard.ButtonText("Open ThingDef List"))
            {

                Find.WindowStack.Add(new DefNamesWindow()); // This opens the new window
            }

            listing_Standard.End();

            settings.Write();
        }

        public override void WriteSettings()
        {
            if (Find.Maps == null || !Find.Maps.Any())
            {
                base.WriteSettings();
                return;
            }

            foreach (Map map in Find.Maps)
            {
                if (map == null || !map.IsPlayerHome)
                    continue;

                foreach (Thing thing in map.listerThings.AllThings)
                {
                    if (thing is ServerRack rack)
                    {
                        rack.Production.UpdateActivatedItems(rack.List);
                    }
                }
            }

            base.WriteSettings();
        }


        public override string SettingsCategory()
        {
            return "Advanced Commercial Servers";
        }
    }

    public class DefNamesWindow : Window
    {
        public DefNamesWindow()
        {
            this.doCloseButton = true; // Adds a close button to the window
            this.closeOnClickedOutside = true; // Closes the window when clicked outside
        }

        private Vector2 leftScrollPosition;
        private Vector2 rightScrollPosition;
        private List<ThingDef> selectedThings = new List<ThingDef>();

        public override void DoWindowContents(Rect inRect)
        {
            List<ThingDef> allThings = DefDatabase<ThingDef>.AllDefsListForReading
                                            .Where(def => def.category == ThingCategory.Item)
                                            .Where(def => !typeof(MinifiedThing).IsAssignableFrom(def.thingClass))
                                            .OrderBy(def => def.label)
                                            .ToList();

            selectedThings = ItemList.List.Select(list => list.Key).OrderBy(def => def.label).ToList();

            // Split the window into two halves
            Rect leftRect = inRect;
            leftRect.width /= 2;
            Rect rightRect = leftRect;
            rightRect.x = leftRect.xMax;

            // Left List - Selected Items
            Widgets.BeginScrollView(leftRect, ref leftScrollPosition, new Rect(0, 0, leftRect.width - 16, selectedThings.Count * 28), true);
            var leftListing = new Listing_Standard();
            leftListing.Begin(new Rect(0, 0, leftRect.width - 16, selectedThings.Count * 28));
            foreach (ThingDef selectedDef in selectedThings.ToList().OrderBy(dev => dev.label)) // To avoid collection modified exception
            {
                Rect rowRect = leftListing.GetRect(28);
                DrawThingRow(rowRect, selectedDef, () => ItemList.List.Remove(selectedDef));
            }
            leftListing.End();
            Widgets.EndScrollView();

            // Right List - All Items
            Widgets.BeginScrollView(rightRect, ref rightScrollPosition, new Rect(0, 0, rightRect.width - 16, allThings.Count * 28), true);
            var rightListing = new Listing_Standard();
            rightListing.Begin(new Rect(0, 0, rightRect.width - 16, allThings.Count * 28));
            foreach (ThingDef def in allThings)
            {
                if (!selectedThings.Contains(def))
                {
                    Rect rowRect = rightListing.GetRect(28);
                    DrawThingRow(rowRect, def, () => ItemList.List.Add(def, false));
                }
            }
            rightListing.End();
            Widgets.EndScrollView();
        }

        private void DrawThingRow(Rect rowRect, ThingDef def, Action onClickAction)
        {
            float iconSize = 24f;
            float padding = 4f;
            float textHeight = 24f;

            Rect iconRect = new Rect(rowRect.x, rowRect.y, iconSize, iconSize);
            Rect textRect = new Rect(iconRect.xMax + padding, rowRect.y, rowRect.width - iconSize - padding, textHeight);

            Widgets.ThingIcon(iconRect, def);

            if (Widgets.ButtonText(textRect, def.label))
            {
                // Original behavior
                onClickAction?.Invoke();

                // NEW: persist to settings
                if (!ServerModSettings.customThingDefs.Contains(def.defName))
                {
                    ServerModSettings.customThingDefs.Add(def.defName);
                    // Also reflect immediately in global list
                    if (!ItemList.List.ContainsKey(def))
                        ItemList.List.Add(def, false);

                    // Let existing racks pick it up when opening the dialog or at next validation
                    var mod = LoadedModManager.GetMod<AdvancedCommercialServersMod>();
                    mod?.GetSettings<ServerModSettings>()?.Write();
                }
            }
        }
    }
}