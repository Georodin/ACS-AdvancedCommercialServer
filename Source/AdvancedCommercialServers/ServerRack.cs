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

namespace AdvancedCommercialServers
{
    public class ServerRack : Building_WorkTable, IThingHolder, IExposable
    {
        const float multiplier = .005f;
        public static Dictionary<ServerRack, float> ServerResearchAccumulated;
        

        public Dictionary<ThingDef, bool> List = new Dictionary<ThingDef, bool>();

        CompPowerTrader powerTrader;

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

        private Dictionary<UninstallType, RecipeDef> recipeDefs = new Dictionary<UninstallType, RecipeDef>()
{
            { UninstallType.Basic, DefDatabase<RecipeDef>.GetNamed("Uninstall_ServerBasic") },
            { UninstallType.Advanced, DefDatabase<RecipeDef>.GetNamed("Uninstall_ServerAdvanced") },
            { UninstallType.Glitterworld, DefDatabase<RecipeDef>.GetNamed("Uninstall_ServerGlitterworld") }
        };

        //currently selected resources
        private List<ThingDef> activatedItems = new List<ThingDef>();

        //how many servers fit?
        const int maxServers = 12;

        //thingOwner to contain servers within rack
        public ThingOwner innerContainer = new ThingOwner<Thing>();
        public IThingHolder parent;

        //generation related fields
        bool isServerDisabled = false;
        float currentProgress = 0f;
        float normalizedProgress = 0f;
        private int currentResourceIndex = 0;

        //public getters
        public int curr_ServerCount = 0;
        public int curr_BasicServerCount = 0;
        public int curr_AdvancedServerCount = 0;
        public int curr_GlitterworldServerCount = 0;
        public float curr_ServerSpeed = 0f;
        public int curr_ServerPowerConsumption = 0;
        public float curr_ServerResearchSpeed = 0f;

        private int GetServerCount(Type serverType = null)
        {
            if (serverType == null)
            {
                return innerContainer.Sum(t => t.stackCount);
            }

            foreach (Thing item in innerContainer)
            {
                var server = item.def.GetModExtension<ServerBase>();
                if (server != null && server.GetType() == serverType)
                {
                    return item.stackCount;
                }
            }
            return 0;
        }

        private float GetServerSpeed()
        {
            float _accumulateOutput = 0;

            foreach (Thing item in innerContainer)
            {
                int stackCount = item.stackCount;

                var modExt = item.def.GetModExtension<ServerBase>();
                if (modExt != null)
                {
                    _accumulateOutput += (modExt.workingSpeed * stackCount) * ServerModSettings.generationSpeedMultiplier;
                }
            }

            return _accumulateOutput;
        }

        private int GetServerPowerConsumption()
        {
            int _accumulateOutput = 0;

            foreach (Thing item in innerContainer)
            {
                int stackCount = item.stackCount;

                var modExt = item.def.GetModExtension<ServerBase>();
                if (modExt != null)
                {
                    _accumulateOutput += (int)Math.Floor((modExt.powerConsumption * stackCount) * ServerModSettings.powerConsumption);
                }
            }

            return _accumulateOutput;
        }

        private float GetServerResearchSpeed()
        {
            return GetServerSpeed() * 1.5f * ServerModSettings.researchMultiplier;
        }

        //local backup, check for modSetting update
        private float powerConsumption = 1.0f;

        //copy resource settings
        static Dictionary<ThingDef, bool> copiedItems;

        //constructors
        public ServerRack()
        {
            UpdateList();
            this.innerContainer = new ThingOwner<Thing>(this);
        }

        public ServerRack(IThingHolder parent)
        {
            innerContainer = new ThingOwner<Thing>(this);
            this.parent = parent;
        }

        //Expose data to savegame
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref List, "items", LookMode.Def, LookMode.Value);
            Scribe_Deep.Look(ref innerContainer, false, "innerContainer", this);
            Scribe_Values.Look(ref currentProgress, "currentProgress", 0f);
            Scribe_Values.Look(ref currentResourceIndex, "currentIndex", 0);
            base.ExposeData();
        }

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

        public override void PostMapInit()
        {
            UpdateList();
            UpdateServerRack();
            base.PostMapInit();
        }

        public void InstallServer(Thing thing)
        {
            if (curr_ServerCount < maxServers)
            {
                int spaceLeft = maxServers - curr_ServerCount;
                int amountToAdd = Math.Min(spaceLeft, thing.stackCount);

                innerContainer.TryAdd(thing.SplitOff(amountToAdd));
                UpdateServerRack();
            }
        }

        public void UninstallServer(Pawn pawn, RecipeDef recipe)
        {

            foreach (Thing item in innerContainer)
            {

                if (recipe == recipeDefs[UninstallType.Basic] && item.def == DefDatabase<ThingDef>.GetNamed("ServerBasic"))
                {
                    Thing output;
                    if (
                        innerContainer.TryDrop(
                            item,
                            pawn.Position,
                            pawn.Map,
                            ThingPlaceMode.Near,
                            1,
                            out output
                        )
                    )
                    {
                        break;
                    }
                }

                if (recipe == recipeDefs[UninstallType.Advanced] && item.def == DefDatabase<ThingDef>.GetNamed("ServerAdvanced"))
                {
                    Thing output;
                    if (
                        innerContainer.TryDrop(
                            item,
                            pawn.Position,
                            pawn.Map,
                            ThingPlaceMode.Near,
                            1,
                            out output
                        )
                    )
                    {
                        break;
                    }
                }
                
                if (recipe == recipeDefs[UninstallType.Glitterworld] && item.def == DefDatabase<ThingDef>.GetNamed("ServerGlitterworld"))
                {
                    Thing output;
                    if (
                        innerContainer.TryDrop(
                            item,
                            pawn.Position,
                            pawn.Map,
                            ThingPlaceMode.Near,
                            1,
                            out output
                        )
                    )
                    {
                        break;
                    }
                }
            }
            UpdateServerRack();
        }

        public void UpdateServerRack()
        {
            curr_ServerCount = GetServerCount();
            curr_BasicServerCount = GetServerCount(typeof(ServerBasic));
            curr_AdvancedServerCount = GetServerCount(typeof(ServerAdvanced));
            curr_GlitterworldServerCount = GetServerCount(typeof(ServerGlitterworld));
            curr_ServerSpeed = GetServerSpeed();
            curr_ServerPowerConsumption = GetServerPowerConsumption();
            curr_ServerResearchSpeed = GetServerResearchSpeed();

            UpdateResearchList();

            if (!isServerDisabled)
            {
                foreach (ThingComp comp in this.GetComps<ThingComp>())
                {
                    if(comp.props is CompProperties_Facility facility)
                    {
                        foreach(StatModifier statModifier in facility.statOffsets)
                        {
                            var researchSpeedFactorStat = DefDatabase<StatDef>.GetNamed("ResearchSpeedFactor");
                            if (researchSpeedFactorStat != null & statModifier.stat == researchSpeedFactorStat)
                            {
                                statModifier.value = ServerResearchAccumulated.Values.Sum();
                            }
                        }
                    }
                }
                this.GetComp<CompPowerTrader>().PowerOutput = -curr_ServerPowerConsumption;
            }
            else
            {
                this.GetComp<CompPowerTrader>().PowerOutput = 0;
            }

            CheckBillsToRemove();
        }

        void CheckBillsToRemove()
        {
            // Remove install Bills if maxed out
            if (GetServerCount() >= maxServers) // Assuming maxServers is the maximum allowed stackCount
            {
                var billsToRemove = billStack.Bills
                    .Where(bill => !bill.Label.Contains("uninstall"))
                    .ToList();
                foreach (var bill in billsToRemove)
                {
                    billStack.Bills.Remove(bill);
                }
            }

            if (curr_BasicServerCount <= 0)
            {
                var billsToRemove = billStack.Bills
                    .Where(bill => bill.Label.Equals("Uninstall basic server"))
                    .ToList();
                foreach (var bill in billsToRemove)
                {
                    billStack.Bills.Remove(bill);
                }
            }

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
#endif

        public void CheckShutdownTemperature()
        {
            if (GetTemperature() >= ServerModSettings.autoShutdownTemperatureCelsius)
            {
                if (isServerDisabled == false)
                {
                    isServerDisabled = true;
                    UpdateServerRack();
                }
            }
            else
            {
                if (isServerDisabled == true)
                {
                    isServerDisabled = false;
                    UpdateServerRack();
                }
            }
        }

        public override Graphic Graphic
        {
            get
            {
                ServerRackUtil.EnsureGraphicsInitialized();  // Initialize graphics if not already done.
                int sum = innerContainer.Sum(t => t.stackCount);
                if (sum > 0 && ServerRackUtil.PreGeneratedGraphics.TryGetValue(sum, out Graphic graphic))
                {
                    if (Rotation == Rot4.South)
                    {
                        return graphic;
                    }
                }
                return base.Graphic;
            }
        }

        public bool IsUninstallAvailableNow(string serverName)
        {
            if (serverName.Contains("basic"))
            {
                if (curr_BasicServerCount > 0)
                {
                    return true;
                }
            }
            else if (serverName.Contains("advanced"))
            {
                if (curr_AdvancedServerCount > 0)
                {
                    return true;
                }
            }
            else if (serverName.Contains("glitterworld"))
            {
                if (curr_GlitterworldServerCount > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsInstallAvailableNow()
        {
            if (innerContainer.Sum(t => t.stackCount) >= 12)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override string GetInspectString()
        {
            CompPowerTrader trader = this.GetComp<CompPowerTrader>();
            PowerNet powerNet = trader.PowerNet;

            string additionalInfo = "";

            if (isServerDisabled)
            {
                additionalInfo = "Servers too hot. Auto shutdown due to temperature...\n";
            }

            if (activatedItems.Count != 0)
            {
                float valueComponent = DefDatabase<ThingDef>
                    .GetNamed("ComponentSpacer")
                    .BaseMarketValue;
                additionalInfo +=
                    "Awaiting payout: "
                    + activatedItems[currentResourceIndex]
                    + " x"
                    + Mathf.Max(1, Mathf.Floor(valueComponent / activatedItems[currentResourceIndex].BaseMarketValue))
                    + "\n";
            }
            else
            {
                additionalInfo += "No payout selected...\n";
            }

            additionalInfo += "Progress: " + normalizedProgress.ToString("F2") + "%\n";

            if (curr_BasicServerCount > 0)
            {
                additionalInfo += "Basic servers: x" + curr_BasicServerCount + "\n";
            }
            if (curr_AdvancedServerCount > 0)
            {
                additionalInfo += "Advanced servers: x" + curr_AdvancedServerCount + "\n";
            }
            if (curr_GlitterworldServerCount > 0)
            {
                additionalInfo += "Glitterworld servers: x" + curr_GlitterworldServerCount + "\n";
            }
            additionalInfo += "" + "Calculation performance: " + curr_ServerSpeed + " THz\n";

            additionalInfo += "Power needed: " + Math.Abs(this.GetComp<CompPowerTrader>().PowerOutput) + " W";

            if (powerNet != null)
            {
                additionalInfo +=
                    "\nGrid excess/stored "
                    + Math.Floor(powerNet.CurrentEnergyGainRate() * 60 * 1000)
                    + " W / "
                    + Math.Floor(powerNet.CurrentStoredEnergy())
                    + " Wd";
            }

            return additionalInfo;
        }

        public float GetTemperature()
        {
            // Getting the position of the server rack
            IntVec3 position = this.Position;

            // Getting the map where the server rack is placed
            Map map = this.Map;

            if (GenAdj.TryFindRandomAdjacentCell8WayWithRoom(this, out position))
            {
                Room room = position.GetRoom(map);
                return room.Temperature;
            }
            return map.mapTemperature.OutdoorTemp;
        }

        public IThingHolder ParentHolder
        {
            get { return this.parent; }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if(innerContainer != null)
            {
                innerContainer.TryDropAll(this.Position, this.Map, ThingPlaceMode.Near);
            }
            base.Destroy(mode);
        }

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
            {
                MessageSetup();
            };

            yield return setup_Action;

            Command_Action copy_Action = new Command_Action();
            copy_Action.defaultLabel = "Copy settings";
            copy_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings");
            copy_Action.action = delegate
            {
                CopySettings();
            };

            yield return copy_Action;

            if (copiedItems != null)
            {
                Command_Action paste_Action = new Command_Action();
                paste_Action.defaultLabel = "Paste settings";
                paste_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings");
                paste_Action.action = delegate
                {
                    PasteSettings();
                };

                yield return paste_Action;
            }

            foreach (var baseGizmo in base.GetGizmos())
            {
                yield return baseGizmo;
            }
        }

        public void CopySettings()
        {
            copiedItems = List.ToDictionary(entry => entry.Key, entry => entry.Value);
<<<<<<< Updated upstream
            Messages.Message("Copied settings", MessageTypeDefOf.TaskCompletion, historical: false);
=======
            Messages.Message(
                "ACS_CopiedSettings".Translate(),
                MessageTypeDefOf.TaskCompletion,
                historical: false
            );
>>>>>>> Stashed changes
        }

        public void PasteSettings()
        {
            if (copiedItems != null)
            {
                List = copiedItems.ToDictionary(entry => entry.Key, entry => entry.Value);
<<<<<<< Updated upstream
                Messages.Message(
                    "Pasted settings",
                    MessageTypeDefOf.TaskCompletion,
                    historical: false
                );
            }
        }

        public void MessageSetup()
        {
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
>>>>>>> Stashed changes
        }
    }
}
