using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static RimWorld.ColonistBar;
using static UnityEngine.GraphicsBuffer;

namespace AdvancedCommercialServers
{
    public class ServerRack : Building_WorkTable, IThingHolder, IExposable
    {
        private Dictionary<ThingDef, bool> items = new Dictionary<ThingDef, bool>
        {
            { DefDatabase<ThingDef>.GetNamed("Silver"), true },
            { DefDatabase<ThingDef>.GetNamed("Gold"), false },
            { DefDatabase<ThingDef>.GetNamed("Plasteel"), false },
            { DefDatabase<ThingDef>.GetNamed("Steel"), false },
            { DefDatabase<ThingDef>.GetNamed("ComponentIndustrial"), false },
            { DefDatabase<ThingDef>.GetNamed("ComponentSpacer"), false },
            { DefDatabase<ThingDef>.GetNamed("Neutroamine"), false }
        };

        private float powerConsumption = 1.0f;

        // List to hold the activated items.
        private List<ThingDef> activatedItems = new List<ThingDef>();

        // Index to keep track of which item to process next.
        private int currentIndex = 0;

        const int maxServers = 12;
        public ThingOwner innerContainer = new ThingOwner<Thing>();
        public IThingHolder parent;

        static Dictionary<ThingDef, bool> copiedItems;
        bool isServerDisabled = false;

        float currentProgress = 0f;

        int sum_basic = 0;
        int sum_advanced = 0;
        int sum_glitterworld = 0;
        int sum_powerConsuption = 0;
        float sum_workingSpeed = 0;

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref innerContainer, true, "innerContainer", this);
            Scribe_Values.Look(ref currentProgress, "currentProgress", 0f);
            Scribe_Values.Look(ref currentIndex, "currentIndex", 0);
            base.ExposeData();
        }

        public void UpdateList()
        {
            activatedItems.Clear();

            // Populate the list with the activated items.
            foreach (var item in items)
            {
                if (item.Value)
                    activatedItems.Add(item.Key);
            }

            if (activatedItems.Count > 0)
            {
                currentIndex = currentIndex % activatedItems.Count;
            }
            else
            {
                currentIndex = 0;
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

        void UpdateProgress()
        {
            currentProgress +=
                (sum_workingSpeed * .008f) * ServerModSettings.generationSpeedMultiplier;

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

        void ProcessActivatedItem()
        {
            if (activatedItems.Count == 0)
                return; // No activated items to process.

            // Process the item at the current index.
            SpawnItem(activatedItems[currentIndex]);

            // Update the index to point to the next item, wrapping around if necessary.
            currentIndex = (currentIndex + 1) % activatedItems.Count;
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

        public ServerRack()
        {
            UpdateList();
            this.innerContainer = new ThingOwner<Thing>(this);
        }

        public override void PostMapInit()
        {
            UpdateServerRack();
            UpdateList();
            base.PostMapInit();
        }

        public ServerRack(IThingHolder parent)
        {
            innerContainer = new ThingOwner<Thing>(this);
            this.parent = parent;
        }

        public void InstallServer(Thing thing)
        {
            int totalCount = innerContainer.Sum(t => t.stackCount);
            if (totalCount < maxServers)
            {

                int spaceLeft = maxServers - totalCount;
                int amountToAdd = Math.Min(spaceLeft, thing.stackCount);

                innerContainer.TryAdd(thing.SplitOff(amountToAdd));
                UpdateServerRack();
            }
        }

        public void UninstallServer(Pawn pawn, string serverUnistallType)
        {
            // Lowercase serverType for case-insensitive comparison.
            string lowerUnistallType = serverUnistallType.ToLowerInvariant();
            string lowerJustServerType = "";

            if (lowerUnistallType.Contains("basic"))
            {
                lowerJustServerType = "basic";
            }
            else if (lowerUnistallType.Contains("advanced"))
            {
                lowerJustServerType = "advanced";
            }
            else if (lowerUnistallType.Contains("glitterworld"))
            {
                lowerJustServerType = "glitterworld";
            }

            foreach (Thing item in innerContainer)
            {
                if (item.Label.ToLowerInvariant().Contains(lowerJustServerType))
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
                        // Optionally, you could break out of the loop once the item has been dropped.
                        break;
                    }
                }
            }
            UpdateServerRack();
        }

        public void UpdateServerRack()
        {
            sum_basic = 0;
            sum_advanced = 0;
            sum_glitterworld = 0;
            sum_powerConsuption = 0;
            sum_workingSpeed = 0;

            foreach (Thing item in innerContainer)
            {
                int stackCount = item.stackCount;

                if (item.Label.Contains("Basic"))
                {
                    sum_basic += stackCount;
                }
                else if (item.Label.Contains("Advanced"))
                {
                    sum_advanced += stackCount;
                }
                else if (item.Label.Contains("Glitterworld"))
                {
                    sum_glitterworld += stackCount;
                }
            }

            if (!isServerDisabled)
            {
                if (activatedItems.Count != 0)
                {
                    foreach (Thing item in innerContainer)
                    {
                        int stackCount = item.stackCount;

                        var modExt = item.def.GetModExtension<ServerBase>();
                        if (modExt != null)
                        {
                            sum_powerConsuption += modExt.powerConsumption * stackCount;
                            sum_workingSpeed += modExt.workingSpeed * stackCount;
                        }
                    }
                }
                else
                {
                    if (activatedItems.Count == 0 && currentProgress != 100)
                    {
                        foreach (Thing item in innerContainer)
                        {
                            int stackCount = item.stackCount;

                            var modExt = item.def.GetModExtension<ServerBase>();
                            if (modExt != null)
                            {
                                sum_powerConsuption += modExt.powerConsumption * stackCount;
                                sum_workingSpeed += modExt.workingSpeed * stackCount;
                            }
                        }
                    }
                }
            }

            sum_powerConsuption = (int)Math.Floor(sum_powerConsuption * ServerModSettings.powerConsumption);

            this.GetComp<CompPowerTrader>().PowerOutput = -sum_powerConsuption;

            CheckBillsToRemove();
        }

        void CheckBillsToRemove()
        {
            // Remove install Bills if maxed out
            int totalCount = innerContainer.Sum(t => t.stackCount);
            if (totalCount >= maxServers) // Assuming maxServers is the maximum allowed stackCount
            {
                var billsToRemove = billStack.Bills
                    .Where(bill => !bill.Label.Contains("uninstall"))
                    .ToList();
                foreach (var bill in billsToRemove)
                {
                    billStack.Bills.Remove(bill);
                }
            }

            if (sum_basic <= 0)
            {
                var billsToRemove = billStack.Bills
                    .Where(bill => bill.Label.Equals("Uninstall basic server"))
                    .ToList();
                foreach (var bill in billsToRemove)
                {
                    billStack.Bills.Remove(bill);
                }
            }

            if (sum_advanced <= 0)
            {
                var billsToRemove = billStack.Bills
                    .Where(bill => bill.Label.Equals("Uninstall advanced server"))
                    .ToList();
                foreach (var bill in billsToRemove)
                {
                    billStack.Bills.Remove(bill);
                }
            }

            if (sum_glitterworld <= 0)
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
                if (sum_basic > 0)
                {
                    return true;
                }
            }
            else if (serverName.Contains("advanced"))
            {
                if (sum_advanced > 0)
                {
                    return true;
                }
            }
            else if (serverName.Contains("glitterworld"))
            {
                if (sum_glitterworld > 0)
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
                    + activatedItems[currentIndex]
                    + " x"
                    + Math.Floor(valueComponent / activatedItems[currentIndex].BaseMarketValue)
                    + "\n";
            }
            else
            {
                additionalInfo += "No payout selected...\n";
            }

            additionalInfo += "Progress: " + currentProgress.ToString("F2") + "%\n";

            if (sum_basic > 0)
            {
                additionalInfo += "Basic servers: x" + sum_basic + "\n";
            }
            if (sum_advanced > 0)
            {
                additionalInfo += "Advanced servers: x" + sum_advanced + "\n";
            }
            if (sum_glitterworld > 0)
            {
                additionalInfo += "Glitterworld servers: x" + sum_glitterworld + "\n";
            }
            additionalInfo += "" + "Calculation performance: " + sum_workingSpeed + " THz\n";

            additionalInfo += "Power needed: " + sum_powerConsuption + " W";

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
