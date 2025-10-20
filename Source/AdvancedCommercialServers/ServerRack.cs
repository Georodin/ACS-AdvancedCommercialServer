using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // for Mathf
using System.Diagnostics; // StackTrace

namespace AdvancedCommercialServers
{
    public class ServerRack : Building_WorkTable, IThingHolder, IExposable
    {
        public ServerRackCore Core;
        public ServerRackStateManager StateManager;
        public ServerRackProduction Production;
        public ServerRackUIHelper UIHelper;
        public ServerRackUtil Util;
        public ServerRackHeatPusherPowered HeatPusherPowered;

        public Dictionary<ThingDef, bool> List = new Dictionary<ThingDef, bool>();

        private static Dictionary<ThingDef, bool> copiedItems;

        public static bool HasCopiedSettings => copiedItems != null;

        public ThingOwner innerContainer;

        private bool isInitialized = false;

        // cache to avoid repeated DefDatabase lookups
        private static StatDef _researchSpeedFactorStatDef;

        public ServerRack()
        {
            Log.Warning("[ACS] ServerRack CONSTRUCTOR called.");
            innerContainer = new ThingOwner<Thing>();
            Initialize();
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref List, "items", LookMode.Def, LookMode.Value);
            Scribe_Deep.Look(ref innerContainer, false, "innerContainer", this);
            Scribe_Deep.Look(ref Production, false, "Production", this);

            base.ExposeData();
        }

        public override void PostMapInit()
        {
            Initialize();
            base.PostMapInit();
        }

        public void Initialize()
        {
            if (isInitialized)
                return;

            Core = new ServerRackCore(this);
            StateManager = new ServerRackStateManager(this);

            if (Production == null)
                Production = new ServerRackProduction(this);

            UIHelper = new ServerRackUIHelper(this);
            Util = new ServerRackUtil(this);
            HeatPusherPowered = new ServerRackHeatPusherPowered(this);

            if (Production.activatedItems == null)
                Production.activatedItems = new List<ThingDef>();

            if (innerContainer != null)
            {
                var toRemove = new List<Thing>();
                foreach (var thing in innerContainer)
                {
                    if (thing == null || thing.def == null)
                    {
                        Log.Warning("[ACS] Removing invalid thing from innerContainer.");
                        toRemove.Add(thing);
                    }
                }
                foreach (var thing in toRemove)
                    innerContainer.Remove(thing);
            }

            Production.UpdateActivatedItems(List);
            Core.UpdateServerRack();
            Util.ValidateItemList();

            // ensure bench sees the correct facility offset from the start
            RefreshResearchFacilityOffset();

            isInitialized = true;
        }

#if RW15
        public override void Tick()
        {
            base.Tick();
            StateManager.HandleTick();

            if (this.IsHashIntervalTick(60))
            {
                if (StateManager.IsOperational)
                {
                    Production.AdvanceProgress();
                }
            }

            // update linked bench facility bonus periodically (cheap)
            if (this.IsHashIntervalTick(250))
            {
                RefreshResearchFacilityOffset();
            }

            HeatPusherPowered?.Tick();
        }
#elif RW16
        protected override void Tick()
        {
            base.Tick();
            StateManager.HandleTick();

            if (this.IsHashIntervalTick(60))
            {
                if (StateManager.IsOperational)
                {
                    Production.AdvanceProgress();
                }
            }

            // update linked bench facility bonus periodically (cheap)
            if (this.IsHashIntervalTick(250))
            {
                RefreshResearchFacilityOffset();
            }

            HeatPusherPowered?.Tick();
        }
#endif

        public override IEnumerable<Gizmo> GetGizmos() => UIHelper.GetGizmos(base.GetGizmos());

        public override string GetInspectString() => UIHelper.GetInspectString();

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            innerContainer?.TryDropAll(Position, Map, ThingPlaceMode.Near);
            base.Destroy(mode);
        }

        public ThingOwner GetDirectlyHeldThings() => innerContainer;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            if (Core != null)
            {
                ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, innerContainer);
            }
        }

        public void CopySettings()
        {
            copiedItems = List.ToDictionary(entry => entry.Key, entry => entry.Value);
            Messages.Message(
                "ACS_CopiedSettings".Translate(),
                MessageTypeDefOf.TaskCompletion,
                historical: false
            );
        }

        public void PasteSettings()
        {
            if (copiedItems != null)
            {
                List = copiedItems.ToDictionary(entry => entry.Key, entry => entry.Value);
                // keep Production in sync
                Production.UpdateActivatedItems(List);
                Messages.Message(
                    "ACS_PastedSettings".Translate(),
                    MessageTypeDefOf.TaskCompletion,
                    historical: false
                );

                // server selection changed → refresh facility bonus
                RefreshResearchFacilityOffset();
            }
        }

        // --- SOUTH-only dynamic fill art selection using preferred Graphic override ---
        public override Graphic Graphic
        {
            get
            {
                if (!Spawned || (Find.DesignatorManager?.SelectedDesignator is Designator_Install))
                    return base.Graphic;

                ServerRackUtil.EnsureGraphicsInitialized();

                if (Rotation == Rot4.South)
                {
                    int sum = innerContainer?.Sum(t => t != null ? t.stackCount : 0) ?? 0;
                    if (sum > 0)
                    {
                        int idx = Mathf.Clamp(sum, 1, 12);
                        if (ServerRackUtil.PreGeneratedGraphics != null &&
                            ServerRackUtil.PreGeneratedGraphics.TryGetValue(idx, out var g) && g != null)
                        {
                            return g;
                        }
                    }
                }

                return base.Graphic;
            }
        }

        // --- NEW: push total map research speed into the facility stat offset (dynamic) ---
        private void RefreshResearchFacilityOffset()
        {
            if (Map == null)
                return;

            // Total (additive) research speed coming from all servers on this map.
            float totalMapSpeed = 0f;
            var mapComp = Map.GetComponent<MapComponent_ServerData>();
            if (mapComp != null)
            {
                totalMapSpeed = mapComp.TotalResearchSpeed;
            }

            if (_researchSpeedFactorStatDef == null)
            {
                _researchSpeedFactorStatDef = DefDatabase<StatDef>.GetNamedSilentFail(
                    "ResearchSpeedFactor"
                );
            }
            if (_researchSpeedFactorStatDef == null)
                return;

            // Find this rack's facility comp and update only its ResearchSpeedFactor offset.
            // Note: CompProperties objects are shared per def; in practice all ServerRacks on a map
            //       should expose the same value, so updating here is fine. We touch only the
            //       ResearchSpeedFactor entry.
            foreach (ThingComp comp in this.GetComps<ThingComp>())
            {
                CompProperties_Facility facProps =
                    comp != null ? comp.props as CompProperties_Facility : null;
                if (facProps == null || facProps.statOffsets == null)
                    continue;

                for (int i = 0; i < facProps.statOffsets.Count; i++)
                {
                    StatModifier mod = facProps.statOffsets[i];
                    if (mod != null && mod.stat == _researchSpeedFactorStatDef)
                    {
                        // Set to total additive bonus for this map (e.g., 0.45 => +45%)
                        mod.value = totalMapSpeed;
                    }
                }
            }
        }

        // Cache last caller so we only log when it changes
        private static string _acs_lastGraphicCaller;

        // Find the first stack frame that's *not* this class and not the getter itself
        private static string ACS_GetGraphicCaller()
        {
            var st = new StackTrace(1, false); // skip this method
            var frames = st.GetFrames();
            if (frames == null) return "unknown";

            foreach (var f in frames)
            {
                var m = f.GetMethod();
                var t = m?.DeclaringType;
                if (t == null) continue;

                // Skip our own class and the getter itself
                if (t == typeof(ServerRack)) continue;
                if (m.Name == "get_Graphic") continue;

                return $"{t.FullName}.{m.Name}";
            }
            return "unknown";
        }

        private static void ACS_LogGraphicCallerIfChanged(string extra = null)
        {
            string caller = ACS_GetGraphicCaller();
            if (!string.Equals(caller, _acs_lastGraphicCaller))
            {
                _acs_lastGraphicCaller = caller;
                if (!string.IsNullOrEmpty(extra))
                    Log.Message($"[ACS] Graphic getter caller: {caller} | {extra}");
                else
                    Log.Message($"[ACS] Graphic getter caller: {caller}");
            }
        }
    }
}
