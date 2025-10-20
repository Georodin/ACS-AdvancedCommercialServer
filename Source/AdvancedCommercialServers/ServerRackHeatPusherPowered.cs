using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace AdvancedCommercialServers
{
    // Helper, not a Comp
    public class ServerRackHeatPusherPowered
    {
        // Roughly: 1 kW -> ~4 heat/second (vanilla heater ~21 heat/sec), tweak as you like
        private const float HeatPerSecondPerKW = 2f;

<<<<<<< HEAD
<<<<<<< Updated upstream
        public override void CompTick()
=======
        private readonly ServerRack parent;

        public ServerRackHeatPusherPowered(ServerRack parent)
        {
            this.parent = parent; // Don't touch Label/Comps here
        }

        public void Tick()
>>>>>>> Stashed changes
        {
            if (parent == null || parent.Map == null) return;

<<<<<<< Updated upstream
            if (powerComp.PowerOn)
            {
                if (Time.frameCount % 100 == 0)
                {
                    Log.Message($"HEAT:{ServerModSettings.generateHeat}");
                }
                if (ServerModSettings.generateHeat)
                {
                    // Retrieve the CompPowerTrader component from the parent thing
                    var powerComp = this.parent.GetComp<CompPowerTrader>();
=======
        private readonly ServerRack parent;

        public ServerRackHeatPusherPowered(ServerRack parent)
        {
            this.parent = parent; // Don't touch Label/Comps here
        }

        public void Tick()
        {
            if (parent == null || parent.Map == null) return;
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686

            // keep your 10s auto-shutdown check
            if (parent.IsHashIntervalTick(600))
                parent.StateManager?.CheckAutoShutdownTemperature();

            if (!(parent.StateManager?.IsOperational ?? false)) return;

<<<<<<< HEAD
                if (Find.TickManager.TicksGame % 600 == 0)
                {
                    if (this.parent is ServerRack rack)
                    {
                        rack.CheckShutdownTemperature();
                    }
                }
            }
           
=======
            // keep your 10s auto-shutdown check
            if (parent.IsHashIntervalTick(600))
                parent.StateManager?.CheckAutoShutdownTemperature();

            if (!(parent.StateManager?.IsOperational ?? false)) return;

=======
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686
            var powerTrader = parent.GetComp<CompPowerTrader>();
            if (powerTrader == null || !powerTrader.PowerOn) return;
            if (!ServerModSettings.generateHeat) return;

            // match vanilla cadence: every 250 ticks (~4.17s); IsHashIntervalTick staggers per-thing
            if (!parent.IsHashIntervalTick(250)) return;

            // convert to kW; PowerOutput is negative when drawing
            float kw = Math.Abs(powerTrader.PowerOutput) / 1000f;

            float seconds = 250f / 60f; // ~4.1667 s per rare tick
            float heatAmount = kw * HeatPerSecondPerKW * ServerModSettings.generateHeatMultiplier * seconds;

            // optional taper to avoid runaway at high temps
            float ambient = parent.AmbientTemperature;
            float ambientFactor = 1f - Mathf.InverseLerp(30f, 120f, ambient);
            heatAmount *= Mathf.Clamp01(ambientFactor);

            if (heatAmount > 0.001f)
                GenTemperature.PushHeat(parent.Position, parent.Map, heatAmount);
<<<<<<< HEAD
>>>>>>> Stashed changes
=======
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686
        }
    }
}
