using System.Collections.Generic;
using ETModel;
using Google.Protobuf.Collections;
using System.Net;

using System.Threading.Tasks;
using System.Linq;

namespace ETHotfix
{
    public static class MapHelper
    {
        public static Session GetGateSession()
        {
            StartConfigComponent config = Game.Scene.GetComponent<StartConfigComponent>();
            IPEndPoint gateIPEndPoint = config.GateConfigs[0].GetComponent<InnerConfig>().IPEndPoint;
            //Log.Debug(gateIPEndPoint.ToString());
            Session gateSession = Game.Scene.GetComponent<NetInnerComponent>().Get(gateIPEndPoint);
            return gateSession;
        }

    }
}