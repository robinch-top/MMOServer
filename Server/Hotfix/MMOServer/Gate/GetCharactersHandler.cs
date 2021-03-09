using System;
using ETModel;
using System.Collections.Generic;

namespace ETHotfix
{
    //客户端请求在当前账号下创建的角色
    [MessageHandler(AppType.Gate)]
    public class GetCharactersHandler : AMRpcHandler<GetCharacters_C2G, GetCharacters_G2C>
    {
        protected override async ETTask Run(Session session, GetCharacters_C2G request, GetCharacters_G2C response, Action reply)
        {
            try
            {
                // 验证Session
                if (!GateHelper.SignSession(session))
                {
                    response.Error = MMOErrorCode.ERR_UserNotOnline;
                    reply();
                    return;
                }

                // 获取用户对象
                User user = session.GetComponent<SessionUserComponent>().User;

                DBProxyComponent dbProxy = Game.Scene.GetComponent<DBProxyComponent>();
                List<Component> characters = await dbProxy.Query2<Character>($"{{UserID:{user.UserID}}}");
                foreach(Character row in characters)
                {
                    response.Characters.Add(new CharacterInfo(){
                        Name = row.Name,
                        Class = row.Class,
                        Level = row.Level,
                        Experience = row.Experience,
                        Money = row.Money,
                        Mail = row.Mail,
                        Title = row.Title,
                        Map = row.Map,
                        X = row.X,
                        Y = row.Y,
                        Z = row.Z,
                        Index = row.Index,
                        Equipments = To.RepeatedField<EquipInfo>(row.Equipments)
                    });
                }

                reply();
                await ETTask.CompletedTask;
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}