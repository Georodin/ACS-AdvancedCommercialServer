<<<<<<< HEAD
<<<<<<< Updated upstream
﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
=======
﻿using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // for Mathf
using System.Diagnostics; // StackTrace
>>>>>>> Stashed changes
=======
﻿using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // for Mathf
using System.Diagnostics; // StackTrace
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686

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

<<<<<<< HEAD
<<<<<<< Updated upstream
        public enum UninstallType
=======
        public static bool HasCopiedSettings => copiedItems != null;

        public ThingOwner innerContainer;

        private bool isInitialized = false;

        // cache to avoid repeated DefDatabase lookups
        private static StatDef _researchSpeedFactorStatDef;

        public ServerRack()
>>>>>>> Stashed changes
        {
            Basic,
            Advanced,
            Glitterworld
        }
=======
        public static bool HasCopiedSettings => copiedItems != null;
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686

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

<<<<<<< HEAD
        //updates

        public void UpdateResearchList()
        {
            if (ServerResearchAccumulated == null)
            {
                ServerResearchAccumulated = new Dictionary<ServerRack, float>();
            }

            float _thisResearchSpeed = 0f;

            if (!isServerDisabled)
            {
                _thisResearchSpeed = curr_ServerResearchSpeed * .015f * ServerModSettings.researchMultiplier;
            }
            if (ServerResearchAccumulated.ContainsKey(this))
                {
                    ServerResearchAccumulated[this] = _thisResearchSpeed;
                }
                else
                {
                    ServerResearchAccumulated.Add(this, _thisResearchSpeed);
                }
            }

        public void UpdateList()
        {
            activatedItems.Clear();

            if(!ItemList.HaveSameKeys(List, ItemList.List))
            {
                List = ItemList.List.ToDictionary(entry => entry.Key,entry => entry.Value);
            }

            // Populate the list with the activated items.
            foreach (var item in List)
            {
                if (item.Value)
                    activatedItems.Add(item.Key);
                //Log.Message($"item: {item.Key.label} is {item.Value}");
            }

            if (activatedItems.Count > 0)
            {
                currentResourceIndex = currentResourceIndex % activatedItems.Count;
            }
            else
            {
                currentResourceIndex = 0;
            }
        }

        void UpdateProgress()
        {
<<<<<<< Updated upstream
            if (activatedItems.Count == 0)
            {
=======
            if (isInitialized)
>>>>>>> Stashed changes
                return;

            ThingDef currentItem = activatedItems[currentResourceIndex];
            float valueComponent = DefDatabase<ThingDef>.GetNamed("ComponentSpacer").BaseMarketValue;
            float itemsToSpawn = Mathf.Max(1, Mathf.Floor(valueComponent / currentItem.BaseMarketValue));
            float totalProgressNeeded = currentItem.BaseMarketValue * itemsToSpawn;

<<<<<<< Updated upstream
            // Adjust progress increase rate based on the server speed and generation multiplier.
            currentProgress += (curr_ServerSpeed * .008f) * ServerModSettings.generationSpeedMultiplier;

            // Normalize current progress to a 0-100 scale based on total progress needed
            normalizedProgress = (currentProgress / totalProgressNeeded) * 100;

            if (normalizedProgress >= 100)
            {
                currentProgress = 0; // Reset progress for the next cycle
                ProcessActivatedItem();
            }
        }
=======
            if (Production == null)
                Production = new ServerRackProduction(this);
>>>>>>> Stashed changes

        void ProcessActivatedItem()
        {
            if (activatedItems.Count == 0)
                return; // No activated items to process.

<<<<<<< Updated upstream
            // Process the item at the current index.
            SpawnItem(activatedItems[currentResourceIndex]);

            // Update the index to point to the next item, wrapping around if necessary.
            currentResourceIndex = (currentResourceIndex + 1) % activatedItems.Count;
        }

        void SpawnItem(ThingDef item)
        {
            float valueComponent = DefDatabase<ThingDef>.GetNamed("ComponentSpacer").BaseMarketValue;
            float itemsToSpawn = Mathf.Max(1, Mathf.Floor(valueComponent / item.BaseMarketValue));

            Thing itemStack = ThingMaker.MakeThing(item);
            itemStack.stackCount = (int)itemsToSpawn;
            GenPlace.TryPlaceThing(itemStack, this.Position, Map, ThingPlaceMode.Near);
=======
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
>>>>>>> Stashed changes
        }

#if RW15
        public override void Tick()
        {
            base.Tick();
<<<<<<< Updated upstream

            if (powerTrader == null)
=======
            StateManager.HandleTick();

            if (this.IsHashIntervalTick(60))
>>>>>>> Stashed changes
            {
                powerTrader = this.GetComp<CompPowerTrader>();
            }

            if (this.IsHashIntervalTick(60)) // RimWorld has 60 ticks per in-game second
            {
                if (powerTrader.PowerOn)
                {
                    UpdateProgress(); // One second has passed in game time

                    if (ServerModSettings.generateHeat)
                    {
                        float energy = Math.Abs((this.GetComp<CompPowerTrader>().PowerOutput * multiplier) * ServerModSettings.generateHeatMultiplier);
                        GenTemperature.PushHeat(this.Position, this.Map, energy);
                    }
                }

                if (powerConsumption != ServerModSettings.powerConsumption)
                {
                    powerConsumption = ServerModSettings.powerConsumption;
                    UpdateServerRack();
                }
                this.CheckShutdownTemperature();
            }
        }

=======
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686
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

<<<<<<< HEAD
            if (curr_AdvancedServerCount <= 0)
            {
                var billsToRemove = billStack.Bills
                    .Where(bill => bill.Label.Equals("Uninstall advanced server"))
                    .ToList();
                foreach (var bill in billsToRemove)
                {
                    billStack.Bills.Remove(bill);
                }
            }

            if (curr_GlitterworldServerCount <= 0)
            {
                var billsToRemove = billStack.Bills
                    .Where(bill => bill.Label.Equals("Uninstall glitterworld server"))
                    .ToList();
                foreach (var bill in billsToRemove)
                {
                    billStack.Bills.Remove(bill);
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
=======
            HeatPusherPowered?.Tick();
        }
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686
#endif

        public override IEnumerable<Gizmo> GetGizmos() => UIHelper.GetGizmos(base.GetGizmos());

        public override string GetInspectString() => UIHelper.GetInspectString();

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            innerContainer?.TryDropAll(Position, Map, ThingPlaceMode.Near);
            base.Destroy(mode);
        }

<<<<<<< HEAD
<<<<<<< Updated upstream

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(
                outChildren,
                this.GetDirectlyHeldThings()
            );
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }
=======
        public ThingOwner GetDirectlyHeldThings() => innerContainer;
>>>>>>> Stashed changes

        public override IEnumerable<Gizmo> GetGizmos()
        {
            Command_Action setup_Action = new Command_Action();
            setup_Action.defaultLabel = "Select resources";
            setup_Action.icon = DefDatabase<ThingDef>.GetNamed("Silver").uiIcon;
            setup_Action.action = delegate
=======
        public ThingOwner GetDirectlyHeldThings() => innerContainer;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            if (Core != null)
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686
            {
                ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, innerContainer);
            }
        }

        public void CopySettings()
        {
            copiedItems = List.ToDictionary(entry => entry.Key, entry => entry.Value);
<<<<<<< HEAD
<<<<<<< Updated upstream
            Messages.Message("Copied settings", MessageTypeDefOf.TaskCompletion, historical: false);
=======
=======
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686
            Messages.Message(
                "ACS_CopiedSettings".Translate(),
                MessageTypeDefOf.TaskCompletion,
                historical: false
            );
<<<<<<< HEAD
>>>>>>> Stashed changes
=======
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686
        }

        public void PasteSettings()
        {
            if (copiedItems != null)
            {
                List = copiedItems.ToDictionary(entry => entry.Key, entry => entry.Value);
<<<<<<< HEAD
<<<<<<< Updated upstream
=======
                // keep Production in sync
                Production.UpdateActivatedItems(List);
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686
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
<<<<<<< HEAD
            Find.WindowStack.Add(new SetupDialog(this));
=======
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
=======
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686
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
<<<<<<< HEAD
>>>>>>> Stashed changes
=======
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686
        }
    }
}
