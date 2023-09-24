using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


            // Auto Shutdown Temperature Slider and Reset Button
            listing_Standard.Label($"Auto Shutdown Temperature (Celsius): {ServerModSettings.autoShutdownTemperatureCelsius}");
            listing_Standard.Gap(0f);
            ServerModSettings.autoShutdownTemperatureCelsius = (int)listing_Standard.Slider(ServerModSettings.autoShutdownTemperatureCelsius, 0, 1000);
            if (listing_Standard.ButtonText("Reset"))
            {
                ServerModSettings.autoShutdownTemperatureCelsius = 50;
            }
            listing_Standard.Gap(spacing);

            listing_Standard.End();

            settings.Write();
        }

        public override string SettingsCategory()
        {
            return "Advanced Commercial Servers";
        }
    }
}
