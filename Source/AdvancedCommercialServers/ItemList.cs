using System.Collections.Generic;
using Verse;

namespace AdvancedCommercialServers
{
    internal class ItemList
    {

        //resources to generate
        public static Dictionary<ThingDef, bool> List = new Dictionary<ThingDef, bool>
        {
            { DefDatabase<ThingDef>.GetNamed("Silver"), true },
            { DefDatabase<ThingDef>.GetNamed("Gold"), false },
            { DefDatabase<ThingDef>.GetNamed("Jade"), false },
            { DefDatabase<ThingDef>.GetNamed("Plasteel"), false },
            { DefDatabase<ThingDef>.GetNamed("Steel"), false },
            { DefDatabase<ThingDef>.GetNamed("Uranium"), false },
            { DefDatabase<ThingDef>.GetNamed("ComponentIndustrial"), false },
            { DefDatabase<ThingDef>.GetNamed("ComponentSpacer"), false },
            { DefDatabase<ThingDef>.GetNamed("Neutroamine"), false }
        };
    }
}
