using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AdvancedCommercialServers
{
    public class ServerRackUtil
    {
        public static Dictionary<int, Graphic> PreGeneratedGraphics;

        private ServerRack parent;

        public ServerRackUtil(ServerRack parent)
        {
            this.parent = parent;
        }

        public static void EnsureGraphicsInitialized()
        {
            if (PreGeneratedGraphics == null)
            {
                PreGeneratedGraphics = new Dictionary<int, Graphic>();
                for (int i = 1; i <= 12; i++)
                {
                    string texturePath = $"Things/Building/ServerRack/ServerRack_fill_{i:D2}";
                    Graphic graphic = GraphicDatabase.Get<Graphic_Single>(
                        texturePath,
                        ShaderDatabase.Cutout,
                        new Vector2(2.6f, 2.6f),
                        Color.white
                    );
                    GraphicData graphicData = new GraphicData
                    {
                        drawSize = new Vector2(2.6f, 2.6f),
                        drawOffset = new Vector3(0f, 0f, 0.5f),
                        shadowData = new ShadowData
                        {
                            volume = new Vector3(0.3f, 0.5f, 0.3f),
                            offset = new Vector3(0f, 0f, -0.23f)
                        }
                    };
                    graphic.data = graphicData;
                    PreGeneratedGraphics[i] = graphic;
                }
            }
        }

        public void ValidateOrResetList()
        {
            bool fallback = false;

            if (parent.List == null || parent.List.Count == 0)
            {
                fallback = true;
            }
            else
            {
                foreach (var key in parent.List.Keys.ToList())
                {
                    if (key == null || DefDatabase<ThingDef>.GetNamedSilentFail(key.defName) == null)
                    {
                        fallback = true;
                        break;
                    }
                }
            }

            if (fallback)
            {
                Log.Warning("[ACS] Resource List is empty or invalid. Falling back to default list.");
                parent.List = ItemList.List.ToDictionary(entry => entry.Key, entry => entry.Value);
            }
        }

    }
}
