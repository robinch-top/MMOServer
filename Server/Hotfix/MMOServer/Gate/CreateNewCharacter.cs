using System;
using ETModel;
using System.Collections.Generic;

namespace ETHotfix
{
    //客户端请求在当前账号下创建新的角色
    [MessageHandler(AppType.Gate)]
    public class CreateNewCharacter : AMRpcHandler<CreateNewCharacter_C2G, CreateNewCharacter_G2C>
    {
        protected override async ETTask Run(Session session, CreateNewCharacter_C2G request, CreateNewCharacter_G2C response, Action reply)
        {
            try
            {
                //验证Session
                if (!GateHelper.SignSession(session))
                {
                    response.Error = MMOErrorCode.ERR_UserNotOnline;
                    reply();
                    return;
                }

                // 限制最多可创建多少角色处理
                int max =  request.Max;
                if(max == 0)
                    max = 100;
                if(request.Index > max){
                    response.Error = MMOErrorCode.ERR_CannotCreateMoreCharacter;
                    reply();
                    return;
                }
                    
                DBProxyComponent dbProxy = Game.Scene.GetComponent<DBProxyComponent>();

                //获取玩家对象
                User user = session.GetComponent<SessionUserComponent>().User;
                
                //获取玩家所在大区编号
                UserInfo userInfo = await dbProxy.Query<UserInfo>(user.UserID);
                int GateAppId = RealmHelper.GetGateAppIdFromUserId(userInfo.Id);
                userInfo.LastPlay = request.Index;

                //检查角色名是否可用
                //会得到全部大区的同名角色 需遍历排除
                List<ComponentWithId> result = await dbProxy.Query<Character>($"{{Name:'{request.Name}'}}");
                foreach(var a in result)
                {
                    if(RealmHelper.GetGateAppIdFromUserId(((Character)a).UserID) == GateAppId)
                    {
                        //出现同名角色
                        response.Error = MMOErrorCode.ERR_CreateNewCharacter;
                        reply();
                        return;
                    }
                }

                //新建角色数据 
                Character character = ComponentFactory.Create<Character,string, long>(request.Name, userInfo.Id);
                character.Race = request.Race;
                character.Class = request.Class;
                character.Name = request.Name;
                character.Level = 1;
                character.Region = request.Region; 
                character.X = request.X; 
                character.Y = request.Y;
                character.Z = request.Z;
                character.Money = 0;
                character.Mail = 0;
                character.Index = request.Index;

                //构建同样的返回数据,减少前端再查询一次此角色数据
                response.Race = request.Race;
                response.Class = request.Class;
                response.CharaID = character.Id;
                response.Name = request.Name;
                response.Level = 1;
                response.Region = request.Region;
                response.X = request.X;
                response.Y = request.Y;
                response.Z = request.Z;
                response.Money = 0;
                response.Mail = 0;
                response.Index = request.Index;
                

                //新角色默认装备
                List<Component> equipInfo = await dbProxy.Query2<GlobalInfo>($"{{Type:'{request.Class}Equip'}}");
                foreach(GlobalInfo row in equipInfo)
                {
                    character.Equipments = row.Equipments;
                    response.Equipments = To.RepeatedField<EquipInfo>(row.Equipments);
                }

                //存储数据
                await dbProxy.Save(character);
                await dbProxy.Save(userInfo);

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