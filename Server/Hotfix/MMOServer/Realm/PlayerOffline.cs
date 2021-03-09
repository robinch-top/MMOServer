﻿using ETModel;
using System;

namespace ETHotfix
{
    [MessageHandler(AppType.Realm)]
    public class PlayerOffline : AMHandler<PlayerOffline_G2R>
    {
        protected override async ETTask Run(Session session, PlayerOffline_G2R message)
        {
            //玩家下线
            Game.Scene.GetComponent<OnlineComponent>().Remove(message.UserID);
            Log.Info($"玩家{message.UserID}下线");

            await ETTask.CompletedTask;
        }
    }
}
