using Verse;
using RimWorld;
using System;

namespace AdvancedCommercialServers
{
    public class ServerRackStateManager
    {
        private readonly ServerRack parent;
        private CompPowerTrader powerTrader;

        public bool IsShutDownByHeat { get; private set; }
        public bool IsShutDownByPowerLoss { get; private set; }

        public bool IsOperational => !IsShutDownByHeat && !IsShutDownByPowerLoss && parent.Spawned;

        private bool lastPowerState = false;

        public ServerRackStateManager(ServerRack parent)
        {
            this.parent = parent;
        }

        public void HandleTick()
        {
            if (powerTrader == null)
                powerTrader = parent.GetComp<CompPowerTrader>();

            if (lastPowerState != powerTrader.PowerOn)
            {
                IsShutDownByPowerLoss = !powerTrader.PowerOn;
                lastPowerState = powerTrader.PowerOn;
                parent.Core.UpdateServerRack();
            }
        }

        public void CheckAutoShutdownTemperature()
        {
            float temperature = GetAmbientTemperature();
            bool shouldShutDown = temperature >= ServerModSettings.autoShutdownTemperatureCelsius;

            if (IsShutDownByHeat != shouldShutDown)
            {
                IsShutDownByHeat = shouldShutDown;
                parent.Core.UpdateServerRack();
            }
        }

        private float GetAmbientTemperature()
        {
            Map map = parent.Map;

            if (GenAdj.TryFindRandomAdjacentCell8WayWithRoom(parent, out IntVec3 pos))
            {
                Room room = pos.GetRoom(map);
                return room?.Temperature ?? map.mapTemperature.OutdoorTemp;
            }

            return map.mapTemperature.OutdoorTemp;
        }
    }
}
