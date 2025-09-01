using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace AdvancedCommercialServers
{
    public class ServerRackHeatPusherPowered : CompHeatPusherPowered
    {
        const float multiplier = .005f;

        private ServerRack parent;

        public ServerRackHeatPusherPowered(ServerRack parent)
        {
            this.parent = parent;
        }

        public override void CompTick()
        {

            if (powerComp.PowerOn)
            {
                if (parent.IsHashIntervalTick(600))
                {
                    parent.StateManager.CheckAutoShutdownTemperature();
                }

                if (parent.StateManager.IsOperational)
                {
                    PushHeat();
                }

                base.CompTick();
            }
        }

        private void PushHeat()
        {
            var powerTrader = this.parent.GetComp<CompPowerTrader>();
            if (powerTrader.PowerOn && ServerModSettings.generateHeat)
            {
                float heat = Math.Abs(powerTrader.PowerOutput) * multiplier * ServerModSettings.generateHeatMultiplier;
                GenTemperature.PushHeat(parent.Position, parent.Map, heat);
            }
        }

    }
}
