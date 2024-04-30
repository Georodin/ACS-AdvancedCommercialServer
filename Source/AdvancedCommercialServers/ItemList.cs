using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
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

        public static bool HaveSameKeys(Dictionary<ThingDef, bool> dict1, Dictionary<ThingDef, bool> dict2)
        {
            // Check if both dictionaries have the same count of keys
            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            // Check each key in dict1 to see if it is present in dict2
            foreach (var key in dict1.Keys)
            {
                if (!dict2.ContainsKey(key))
                {
                    return false;
                }
            }

            // If all keys match, return true
            return true;
        }
    }
}
