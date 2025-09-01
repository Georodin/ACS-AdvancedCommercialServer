using Verse;
using System.Collections.Generic;

namespace AdvancedCommercialServers
{
    /// <summary>
    /// Map-level component that tracks total research speed from all operational ServerRacks.
    /// </summary>
    public class MapComponent_ServerData : MapComponent
    {
        /// <summary>
        /// Total research speed contributed by all server racks on the map.
        /// </summary>
        public float TotalResearchSpeed { get; private set; }

        public MapComponent_ServerData(Map map) : base(map)
        {
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            RecalculateTotalSpeed();
        }

        /// <summary>
        /// Recalculates the total research speed by summing all operational server racks.
        /// </summary>
        public void RecalculateTotalSpeed()
        {
            TotalResearchSpeed = 0f;

            List<Thing> allThings = map.listerThings.AllThings;

            for (int i = 0; i < allThings.Count; i++)
            {
                if (allThings[i] is ServerRack rack &&
                    rack.Core != null &&
                    rack.StateManager != null &&
                    rack.StateManager.IsOperational)
                {
                    TotalResearchSpeed += rack.Core.CurrentResearchSpeed;
                }
            }
        }
    }
}
