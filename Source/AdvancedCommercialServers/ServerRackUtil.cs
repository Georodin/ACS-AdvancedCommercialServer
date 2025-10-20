<<<<<<< Updated upstream
﻿using System;
=======
﻿using RimWorld;
>>>>>>> Stashed changes
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AdvancedCommercialServers
{
    internal class ServerRackUtil
    {
        public static Dictionary<int, Graphic> PreGeneratedGraphics;

<<<<<<< Updated upstream
=======
        private static bool initQueued;
        private static bool initialized;

        private ServerRack parent;

        public ServerRackUtil(ServerRack parent)
        {
            this.parent = parent;
            EnsureGraphicsInitialized();
        }

>>>>>>> Stashed changes
        public static void EnsureGraphicsInitialized()
        {
            if (initialized && PreGeneratedGraphics != null)
                return;

            if (!UnityData.IsInMainThread)
            {
                if (!initQueued)
                {
                    initQueued = true;
                    LongEventHandler.ExecuteWhenFinished(() =>
                    {
                        try
                        {
                            BuildGraphics();
                            initialized = true;
                        }
                        finally
                        {
                            initQueued = false;
                        }
                    });
                }
                return;
            }

            BuildGraphics();
            initialized = true;
        }

        private static void BuildGraphics()
        {
            if (PreGeneratedGraphics == null)
            {
                PreGeneratedGraphics = new Dictionary<int, Graphic>(capacity: 12);

                for (int i = 1; i <= 12; i++)
                {
                    string texturePath = $"Things/Building/ServerRack/ServerRack_fill_{i:D2}";
                    var data = new GraphicData
                    {
                        graphicClass = typeof(Graphic_Single),
                        texPath = texturePath,
                        shaderType = ShaderTypeDefOf.Cutout,
                        drawSize = new Vector2(2.6f, 2.6f),
                        drawOffset = new Vector3(0f, 0f, 0.5f),
                        shadowData = new ShadowData
                        {
                            volume = new Vector3(0.3f, 0.5f, 0.3f),
                            offset = new Vector3(0f, 0f, -0.23f)
                        }
                    };

                    PreGeneratedGraphics[i] = data.Graphic;
                }
            }
        }
<<<<<<< Updated upstream
=======

        public void ValidateItemList()
        {
            // Make sure the global defaults have the saved custom defs
            ItemList.ApplyCustomDefsFromSettings();

            bool fallback = false;

            if (parent.List == null || parent.List.Count == 0)
            {
                fallback = true;
            }
            else
            {
                // Remove null/invalid keys and detect any corruption
                var toRemove = new List<ThingDef>();
                foreach (var kv in parent.List)
                {
                    if (kv.Key == null)
                    {
                        toRemove.Add(kv.Key);
                        fallback = true;
                    }
                }
                foreach (var k in toRemove) parent.List.Remove(k);

                // Merge any new items from the global list into the rack list (default to false)
                foreach (var kv in ItemList.List)
                {
                    if (kv.Key != null && !parent.List.ContainsKey(kv.Key))
                        parent.List.Add(kv.Key, false);
                }
            }

            if (fallback)
            {
                Log.Warning("[ACS] Resource List is empty or invalid. Falling back to default list.");
                parent.List = ItemList.List.ToDictionary(entry => entry.Key, entry => entry.Value);
            }
        }
>>>>>>> Stashed changes
    }
}
