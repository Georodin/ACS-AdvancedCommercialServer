using System.Collections.Generic;
using Verse;

namespace AdvancedCommercialServers
{
    internal class ItemList
    {
        // resources to generate (defaults)
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

<<<<<<< HEAD
<<<<<<< Updated upstream
        public static bool HaveSameKeys(Dictionary<ThingDef, bool> dict1, Dictionary<ThingDef, bool> dict2)
=======
        // NEW: call this after settings load or when settings change
        public static void ApplyCustomDefsFromSettings()
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686
        {
            if (ServerModSettings.customThingDefs == null) return;

            foreach (var defName in ServerModSettings.customThingDefs)
            {
                if (string.IsNullOrEmpty(defName)) continue;
                var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                if (def != null && !List.ContainsKey(def))
                {
                    // default to disabled; players toggle per rack
                    List.Add(def, false);
                }
            }
<<<<<<< HEAD

            // If all keys match, return true
            return true;
=======
        // NEW: call this after settings load or when settings change
        public static void ApplyCustomDefsFromSettings()
        {
            if (ServerModSettings.customThingDefs == null) return;

            foreach (var defName in ServerModSettings.customThingDefs)
            {
                if (string.IsNullOrEmpty(defName)) continue;
                var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                if (def != null && !List.ContainsKey(def))
                {
                    // default to disabled; players toggle per rack
                    List.Add(def, false);
                }
            }
>>>>>>> Stashed changes
=======
>>>>>>> 43be1abc7c52993afb6ee92914c1b3d011385686
        }
    }
}
