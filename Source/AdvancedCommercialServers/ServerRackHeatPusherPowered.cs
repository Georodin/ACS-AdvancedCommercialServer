using RimWorld;
using System;
using Verse;

namespace AdvancedCommercialServers
{
    public class ServerRackHeatPusherPowered : CompHeatPusherPowered
    {
        const float multiplier = .005f;

        public override void CompTick()
        {
            if (powerComp.PowerOn)
            {
                if (ServerModSettings.generateHeat)
                {
                    // Retrieve the CompPowerTrader component from the parent thing
                    var powerComp = this.parent.GetComp<CompPowerTrader>();

                    if (powerComp == null)
                    {
                        return;
                    }
                    this.Props.heatPerSecond = Math.Abs((powerComp.PowerOutput * multiplier)*ServerModSettings.generateHeatMultiplier);
                }
                // Adjust the heatPerSecond according to the absolute power consumption.
                

                if (Find.TickManager.TicksGame % 600 == 0)
                {
                    if (this.parent is ServerRack rack)
                    {
                        rack.CheckShutdownTemperature();
                    }
                }
            }
            base.CompTick();
        }
    }
}
