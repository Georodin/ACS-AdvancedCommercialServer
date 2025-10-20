using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace AdvancedCommercialServers
{
    public class ServerRackUIHelper
    {
        private readonly ServerRack parent;

        public ServerRackUIHelper(ServerRack parent)
        {
            this.parent = parent;
        }

        public string GetInspectString()
        {
            if (parent.Core == null || parent.StateManager == null || parent.Production == null)
                return "Loading".Translate();

            StringBuilder sb = new StringBuilder();

            if (parent.StateManager.IsShutDownByHeat)
                sb.AppendLine("AutoShutdownHeatWarning".Translate());

            sb.AppendLine(parent.Production.GetProgressString());

            if (parent.Core.BasicServerCount > 0)
                sb.AppendLine("BasicServers".Translate(parent.Core.BasicServerCount));
            if (parent.Core.AdvancedServerCount > 0)
                sb.AppendLine("AdvancedServers".Translate(parent.Core.AdvancedServerCount));
            if (parent.Core.GlitterworldServerCount > 0)
                sb.AppendLine("GlitterworldServers".Translate(parent.Core.GlitterworldServerCount));

            sb.AppendLine("ServerSpeed".Translate(parent.Core.CurrentGenerationSpeed.ToString("F2")));

            float totalMapSpeed = parent.Map?.GetComponent<MapComponent_ServerData>()?.TotalResearchSpeed ?? 0f;
            sb.AppendLine("ResearchSpeed".Translate(
                (parent.Core.CurrentResearchSpeed * 100f).ToString("F0"),
                (totalMapSpeed * 100f).ToString("F0")));

            float powerUsage = Mathf.Abs(parent.GetComp<CompPowerTrader>().PowerOutput);
            sb.Append("PowerNeeded".Translate(powerUsage.ToString("F0")));

            PowerNet net = parent.GetComp<CompPowerTrader>()?.PowerNet;
            if (net != null)
            {
                sb.AppendLine();
                sb.Append("GridPowerStatus".Translate(
                    Mathf.Floor(net.CurrentEnergyGainRate() * 60 * 1000).ToString("F0"),
                    Mathf.Floor(net.CurrentStoredEnergy()).ToString("F0")));
            }

            return sb.ToString().TrimEnd();
        }


        public IEnumerable<Gizmo> GetGizmos(IEnumerable<Gizmo> baseGizmos)
        {
            foreach (var gizmo in baseGizmos)
                yield return gizmo;

            yield return new Command_Action
            {
                defaultLabel = "SelectResources".Translate(),
                defaultDesc = "SelectResourcesDesc".Translate(),
                icon = DefDatabase<ThingDef>.GetNamed("Silver").uiIcon,
                action = () => MessageSetup()
            };

            yield return new Command_Action
            {
                defaultLabel = "CopySettings".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings"),
                action = () => parent.CopySettings()
            };

            if (ServerRack.HasCopiedSettings)
            {
                yield return new Command_Action
                {
                    defaultLabel = "PasteSettings".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings"),
                    action = () => parent.PasteSettings()
                };
            }
        }


        public void MessageSetup()
        {
            // NEW: make sure rack.List has newly added global defs
            parent?.Util?.ValidateItemList();         // merge + fallback if needed
            ItemList.ApplyCustomDefsFromSettings();   // make sure global list includes persisted items

            Find.WindowStack.Add(new SetupDialog(parent));
        }
    }
}
