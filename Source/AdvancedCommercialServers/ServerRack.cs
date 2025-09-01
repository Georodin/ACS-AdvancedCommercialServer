using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

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
            {
                return;
            }

            Core = new ServerRackCore(this);
            StateManager = new ServerRackStateManager(this);

            if (Production == null)
            {
                Production = new ServerRackProduction(this);
            }

            UIHelper = new ServerRackUIHelper(this);
            Util = new ServerRackUtil(this);
            HeatPusherPowered = new ServerRackHeatPusherPowered(this);

            if (Production.activatedItems == null)
            {
                Production.activatedItems = new List<ThingDef>();
            }

            if (innerContainer != null)
            {
                var toRemove = new List<Thing>();
                foreach (var thing in innerContainer)
                {
                    if (thing == null || thing.def == null)
                    {
                        Log.Warning($"[ACS] Removing invalid thing from innerContainer.");
                        toRemove.Add(thing);
                    }
                }

                foreach (var thing in toRemove)
                {
                    innerContainer.Remove(thing);
                }
            }

            Util.ValidateOrResetList();
            Production.UpdateActivatedItems(List);
            Core.UpdateServerRack();
            isInitialized = true;
        }

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
        }

        public override IEnumerable<Gizmo> GetGizmos() => UIHelper.GetGizmos(base.GetGizmos());

        public override string GetInspectString() => UIHelper.GetInspectString();

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            innerContainer?.TryDropAll(Position, Map, ThingPlaceMode.Near);
            base.Destroy(mode);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

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
            Messages.Message("ACS_CopiedSettings".Translate(), MessageTypeDefOf.TaskCompletion, historical: false);
        }

        public void PasteSettings()
        {
            if (copiedItems != null)
            {
                List = copiedItems.ToDictionary(entry => entry.Key, entry => entry.Value);
                Messages.Message("ACS_PastedSettings".Translate(), MessageTypeDefOf.TaskCompletion, historical: false);
            }
        }

    }
}