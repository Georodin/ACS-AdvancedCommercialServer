﻿using Verse;

namespace AdvancedCommercialServers
{
    public class ServerBase : DefModExtension
    {
        public string name = "BaseServer";
        public int powerConsumption = 0;
        public float workingSpeed = 0f;

        public ServerBase() { }
    }

    public class ServerBasic : ServerBase
    {

    }

    public class ServerAdvanced : ServerBase
    {

    }

    public class ServerGlitterworld : ServerBase
    {

    }
}
