using Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedCommercialServers
{
    public class ServerRackCore
    {
        private const int MaxServers = 12;
        private readonly ServerRack parent;

        public int ServerCount => parent.innerContainer.Sum(t => t.stackCount);
        public int BasicServerCount => GetServerCount(typeof(ServerBasic));
        public int AdvancedServerCount => GetServerCount(typeof(ServerAdvanced));
        public int GlitterworldServerCount => GetServerCount(typeof(ServerGlitterworld));

        public float CurrentGenerationSpeed => CalculateServerSpeed();
        public int CurrentPowerDraw => CalculatePowerConsumption();
        public float CurrentResearchSpeed =>
            parent.StateManager.IsOperational
                ? CurrentGenerationSpeed * 0.015f * ServerModSettings.researchMultiplier
                : 0f;

        public ServerRackCore(ServerRack parent)
        {
            this.parent = parent;
        }

        public void InstallServer(Thing thing)
        {
            if (ServerCount >= MaxServers) return;

            int spaceLeft = MaxServers - ServerCount;
            int amountToAdd = Math.Min(spaceLeft, thing.stackCount);
            parent.innerContainer.TryAdd(thing.SplitOff(amountToAdd));

            Log.Warning("[ACS] innerContainer contents after install:");
            foreach (Thing item in parent.innerContainer)
            {
                Log.Warning($"  - {item.LabelCap} | def: {item.def.defName} | count: {item.stackCount}");
            }

            parent.Core.UpdateServerRack();
        }

        public void UninstallServer(Pawn pawn, ThingDef serverDef)
        {
            Thing item = parent.innerContainer.FirstOrDefault(t => t.def == serverDef);
            if (item != null)
            {
                parent.innerContainer.TryDrop(item, pawn.Position, pawn.Map, ThingPlaceMode.Near, 1, out _);
                UpdateServerRack();
            }
        }

        public void UpdateServerRack()
        {
            if (parent == null)
            {
                Log.Warning("[ACS] UpdateServerRack: parent is null.");
                return;
            }

            if (parent.StateManager == null)
            {
                Log.Warning("[ACS] UpdateServerRack: parent.StateManager is null.");
                return;
            }

            var powerComp = parent.GetComp<CompPowerTrader>();
            if (powerComp == null)
            {
                Log.Warning("[ACS] UpdateServerRack: CompPowerTrader is null.");
            }
            else
            {
                powerComp.PowerOutput = parent.StateManager.IsOperational ? -CurrentPowerDraw : 0f;
                Log.Message($"[ACS] PowerOutput set to: {powerComp.PowerOutput}");
            }

            if (parent.Map == null)
            {
                Log.Warning("[ACS] UpdateServerRack: parent.Map is null.");
                return;
            }

            var serverData = parent.Map.GetComponent<MapComponent_ServerData>();
            if (serverData == null)
            {
                Log.Warning("[ACS] UpdateServerRack: MapComponent_ServerData is null.");
                return;
            }

            Log.Message("[ACS] Recalculating total speed...");
            serverData.RecalculateTotalSpeed();
        }


        private int GetServerCount(Type serverType)
        {
            foreach (Thing item in parent.innerContainer)
            {
                var ext = item.def.GetModExtension<ServerBase>();
                if (ext != null && ext.GetType() == serverType)
                    return item.stackCount;
            }

            return 0;
        }

        private float CalculateServerSpeed()
        {
            float total = 0f;

            foreach (Thing item in parent.innerContainer)
            {
                int count = item.stackCount;
                var ext = item.def.GetModExtension<ServerBase>();
                if (ext != null)
                {
                    total += (ext.workingSpeed * count) * ServerModSettings.generationSpeedMultiplier;
                }
            }

            return total;
        }

        private int CalculatePowerConsumption()
        {
            int total = 0;

            foreach (Thing item in parent.innerContainer)
            {
                int count = item.stackCount;
                var ext = item.def.GetModExtension<ServerBase>();
                if (ext != null)
                {
                    total += (int)Math.Floor(ext.powerConsumption * count * ServerModSettings.powerConsumption);
                }
            }

            return total;
        }

        public bool IsInstallAvailableNow()
        {
            return ServerCount < MaxServers;
        }

        public bool IsUninstallAvailable(ThingDef def)
        {
            return parent.innerContainer.Any(t => t.def == def && t.stackCount > 0);
        }
    }
}
