using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AdvancedCommercialServers
{
    public class ServerRack : Building_WorkTable, IThingHolder, IExposable
    {

        public static Dictionary<ServerRack, float> ServerResearchAccumulated;

        //resources to generate
        private Dictionary<ThingDef, bool> items = new Dictionary<ThingDef, bool>
        {
            { DefDatabase<ThingDef>.GetNamed("Silver"), true },
            { DefDatabase<ThingDef>.GetNamed("Gold"), false },
            { DefDatabase<ThingDef>.GetNamed("Jade"), false },
            { DefDatabase<ThingDef>.GetNamed("Plasteel"), false },
            { DefDatabase<ThingDef>.GetNamed("Steel"), false },
            { DefDatabase<ThingDef>.GetNamed("ComponentIndustrial"), false },
            { DefDatabase<ThingDef>.GetNamed("ComponentSpacer"), false },
            { DefDatabase<ThingDef>.GetNamed("Neutroamine"), false }
        };

        public enum UninstallType
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
            Scribe_Collections.Look(ref items, "items", LookMode.Def, LookMode.Value);
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

            if (items == null || items.Count != 8)
            {
                items = new Dictionary<ThingDef, bool>
                {
                    { DefDatabase<ThingDef>.GetNamed("Silver"), true },
                    { DefDatabase<ThingDef>.GetNamed("Gold"), false },
                    { DefDatabase<ThingDef>.GetNamed("Jade"), false },
                    { DefDatabase<ThingDef>.GetNamed("Plasteel"), false },
                    { DefDatabase<ThingDef>.GetNamed("Steel"), false },
                    { DefDatabase<ThingDef>.GetNamed("ComponentIndustrial"), false },
                    { DefDatabase<ThingDef>.GetNamed("ComponentSpacer"), false },
                    { DefDatabase<ThingDef>.GetNamed("Neutroamine"), false }
                };
            }

            // Populate the list with the activated items.
            foreach (var item in items)
            {
                if (item.Value)
                    activatedItems.Add(item.Key);
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
            currentProgress +=
                (curr_ServerSpeed * .008f) * ServerModSettings.generationSpeedMultiplier;

            if (currentProgress >= 100)
            {
                if (activatedItems.Count == 0)
                {
                    currentProgress = 100;
                }
                else
                {
                    currentProgress = 0;
                    ProcessActivatedItem();
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (this.GetComp<CompPowerTrader>().PowerOn)
            {
                if (this.IsHashIntervalTick(30)) // RimWorld has 60 ticks per in-game second
                {
                    UpdateProgress(); // One second has passed in game time
                }
            }

            //lazy check for mod setting change
            if (this.IsHashIntervalTick(600))
            {
                if (powerConsumption != ServerModSettings.powerConsumption)
                {
                    powerConsumption = ServerModSettings.powerConsumption;
                    UpdateServerRack();
                }
            }
        }

        void ProcessActivatedItem()
        {
            if (activatedItems.Count == 0)
                return; // No activated items to process.

            // Process the item at the current index.
            SpawnItem(activatedItems[currentResourceIndex]);

            // Update the index to point to the next item, wrapping around if necessary.
            currentResourceIndex = (currentResourceIndex + 1) % activatedItems.Count;
        }

        void SpawnItem(ThingDef item)
        {
            float valueComponent = DefDatabase<ThingDef>
                .GetNamed("ComponentSpacer")
                .BaseMarketValue;

            Thing itemStack = ThingMaker.MakeThing(item);
            itemStack.stackCount = (int)Math.Floor(valueComponent / item.BaseMarketValue);
            GenPlace.TryPlaceThing(itemStack, this.Position, Map, ThingPlaceMode.Near);
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
        }

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
                int sum = innerContainer.Sum(t => t.stackCount);
                if (sum > 0)
                {
                    string texturePath = $"Things/Building/ServerRack/ServerRack_fill_{sum:D2}";
                    if (Rotation == Rot4.South)
                    {
                        Graphic _graphic = GraphicDatabase.Get<Graphic_Single>(
                            texturePath,
                            ShaderDatabase.Cutout,
                            new UnityEngine.Vector2(2.6f, 2.6f),
                            UnityEngine.Color.white
                        );
                        GraphicData graphicData = new GraphicData();
                        graphicData.drawSize = new UnityEngine.Vector2(2.6f, 2.6f);
                        graphicData.drawOffset = new UnityEngine.Vector3(0f, 0f, 0.5f);
                        graphicData.shadowData = new ShadowData
                        {
                            volume = new UnityEngine.Vector3(0.3f, 0.5f, 0.3f),
                            offset = new UnityEngine.Vector3(0f, 0f, -0.23f)
                        };

                        _graphic.data = graphicData;

                        return _graphic;
                    }
                    else
                    {
                        return base.Graphic;
                    }
                }
                else
                {
                    return base.Graphic;
                }
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
                    + Math.Floor(valueComponent / activatedItems[currentResourceIndex].BaseMarketValue)
                    + "\n";
            }
            else
            {
                additionalInfo += "No payout selected...\n";
            }

            additionalInfo += "Progress: " + currentProgress.ToString("F2") + "%\n";

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
            copiedItems = items;
            Messages.Message("Copied settings", MessageTypeDefOf.TaskCompletion, historical: false);
        }

        public void PasteSettings()
        {
            if (copiedItems != null)
            {
                items = copiedItems;
                Messages.Message(
                    "Pasted settings",
                    MessageTypeDefOf.TaskCompletion,
                    historical: false
                );
            }
        }

        public void MessageSetup()
        {
            Find.WindowStack.Add(new SetupDialog(items, this));
        }
    }
}
