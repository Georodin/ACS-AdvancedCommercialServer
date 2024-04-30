using Verse;

namespace AdvancedCommercialServers
{
    public class ServerModSettings : ModSettings
    {
        public static bool generateHeat = true;
        public static float generateHeatMultiplier = 1.0f;
        public static float generationSpeedMultiplier = 1.0f;
        public static float researchMultiplier = 1.0f;
        public static float powerConsumption = 1.0f;
        public static int autoShutdownTemperatureCelsius = 50;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref generateHeat, "generateHeat", true);
            Scribe_Values.Look(ref generateHeatMultiplier, "generateHeatMultiplier", 1.0f);
            Scribe_Values.Look(ref generationSpeedMultiplier, "generationSpeedMultiplier", 1.0f);
            Scribe_Values.Look(ref researchMultiplier, "researchMultiplier", 1.0f);
            Scribe_Values.Look(ref powerConsumption, "powerConsumption", 1.0f);
            Scribe_Values.Look(ref autoShutdownTemperatureCelsius, "autoShutdownTemperatureCelsius", 50);
        }
    }
}