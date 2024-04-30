using System;
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
    }
}
