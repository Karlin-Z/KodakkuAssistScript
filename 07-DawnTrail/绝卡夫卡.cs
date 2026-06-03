using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dalamud.Utility.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using KodakkuAssist.Extensions;
using KodakkuAssist.Module.Draw;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Script;
using KodakkuAssist.Data;
using System.Numerics;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace KarlinScriptNamespace;

/// <summary>
/// name and version affect the script name and version number displayed in the user interface.
/// territorys specifies the regions where this trigger is effective. If left empty, it will be effective in all regions.
/// Classes with the same GUID will be considered the same trigger. Please ensure your GUID is unique and does not conflict with others.
/// </summary>
[ScriptType(name: "绝凯夫卡", territorys: [1363],guid: "06cd8ccc-589d-46c4-8f50-36e3ca55919f", version:"0.0.0.4",author:"Karlin", updateInfo: updateInfoStr)]
public class 绝凯夫卡
{
    const string updateInfoStr =
        """
        精修  P1
        增加分摊击退指示
        增加连线岩石范围
        """;
    [UserSetting("危险颜色2")]
    public ScriptColor dangerColor { get; set; } = new();

    [UserSetting("P1火分散延迟显示时间")]
    public int p1FireDelay { get; set; } = 2000;

    private int parse = 0;

    private bool trueFire = false;

    public void Init(ScriptAccessory accessory)
    {
        parse = 10;
    }

    #region P1

    private ulong p1BossId = 0;
    [ScriptMethod(eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:48370"],userControl:false)]
    public void P1_阶段转换(Event @event, ScriptAccessory accessory)
    {
        parse++;
        p1Queue = [3, 2, 1, 0, 4, 5, 6, 7];
        lightHit = [];
        towerPos = [];
        p1BossId = @event.SourceId;
        p1TeleportList = [0,0,0,0,0,0,0,0];
        p1stoneCount = 0;
        accessory.Log.Debug($"Parse: {parse}");
    }

    [ScriptMethod(eventType: EventTypeEnum.StartCasting,eventCondition: ["ActionId:50179"])]
    public void P1_恶狠狠毁荡(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"恶狠狠毁荡 扇形死刑 一仇";
        dp.Scale = new(40);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
        dp.Owner = @event.SourceId;
        dp.TargetObject=@event.TargetId;
        dp.Radian=MathF.PI/3*2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        dp.TargetObject=0xE0000000;
        dp.TargetResolvePattern=PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.TargetOrderIndex = 2;
        dp.Color = dangerColor.V4;
        dp.Name = $"恶狠狠毁荡 扇形死刑 二仇 预兆";
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Name = $"恶狠狠毁荡 扇形死刑 二仇";
        dp.Delay = 4600;
        dp.DestoryAt = 3000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod( eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47768|47774)$"])]
    public void P1_真假冰扇形(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"真假冰扇形";
        dp.Scale = new(20);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId;
        dp.Radian = MathF.PI /2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47775|47777)$"])]
    public void P1_真假雷直线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"P1真假雷直线";
        dp.Scale = new(10,40);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
    }
    [ScriptMethod(eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(02A1|02A2)$"],userControl:false)]
    public void P1_真假火收集(Event @event, ScriptAccessory accessory)
    {
        trueFire = @event["Id"] == "02A2";
    }
    [ScriptMethod(eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0080"])]
    public void P1_真假火分摊处理(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(50).ContinueWith(o =>
        {
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var targetindex = accessory.Data.PartyList.IndexOf((uint)@event.TargetId);
            if (trueFire)
            {
                

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"真火分摊{@event["TargetId"]}";
                dp.Scale = new(6);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = @event.TargetId;
                dp.DestoryAt = 6000;

                if((targetindex < 4) == (myindex < 4))
                {
                    dp.Color = accessory.Data.DefaultSafeColor;
                }

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else if((targetindex < 4) == (myindex < 4))
            {
                for (int i = 0; i < 8; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"假火分摊{i}";
                    dp.Scale = new(5);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Delay = p1FireDelay;
                    dp.DestoryAt = 6000 - p1FireDelay;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                
            }
        });

    }
    [ScriptMethod(eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:007F"],suppress:1000)]
    public void P1_真假火分散处理(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(50).ContinueWith(o =>
        {
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (trueFire)
            {
                for (int i = 0; i < 8; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"真火分散{i}";
                    dp.Scale = new(5);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Delay= p1FireDelay;
                    dp.DestoryAt = 6000- p1FireDelay;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"假火分摊0";
                dp.Scale = new(6);
                dp.Color =myindex<4? accessory.Data.DefaultSafeColor: accessory.Data.DefaultDangerColor;
                dp.Owner = accessory.Data.PartyList[0];
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                dp.Name = $"假火分摊4";
                dp.Color = myindex < 4 ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.PartyList[4];
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        });

    }
    [ScriptMethod(  eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(0080|007F)$"])]
    public void P1_真假火分散分摊TTS(Event @event, ScriptAccessory accessory)
    {
        Task.Delay(50).ContinueWith(o =>
        {

            var fireStack = @event["Id"]=="0080";

            if (fireStack == trueFire)
            {
                accessory.Method.TTS("分摊");
                accessory.Method.TextInfo("分摊", 6000);
            }
            else
            {
                accessory.Method.TTS("分散");
                accessory.Method.TextInfo("分散", 6000);
            }

        });
    }

    [ScriptMethod(eventType: EventTypeEnum.StartCasting,eventCondition: ["ActionId:regex:^(47776|47771)$"])]
    public void P1_屏蔽假雷假冰(Event @event, ScriptAccessory accessory)
    {
        var obj = accessory.Data.Objects.SearchById((uint)@event.SourceId);
        if (obj == null || !obj.IsValid()) return;
        WriteVisible(accessory, obj, false, 5000);
    }
    [ScriptMethod(eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47775|47768)$"])]
    public void P1_屏蔽真雷冰(Event @event, ScriptAccessory accessory)
    {
        var obj = accessory.Data.Objects.SearchById((uint)@event.SourceId);
        if (obj == null || !obj.IsValid()) return;
        WriteVisible(accessory, obj, false, 5000);
    }

    [ScriptMethod(eventType: EventTypeEnum.Tether, eventCondition: ["Id:002D"])]
    public void P1_连线点名击退(Event @event, ScriptAccessory accessory)
    {
        if (parse != 11) return;
        if (@event.TargetId != accessory.Data.Me) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"P1连线点名击退";
        dp.Scale = new(1.5f, 13);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
        dp.Owner = @event.TargetId;
        dp.TargetObject= @event.SourceId;
        dp.Rotation = MathF.PI;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    private List<int> lightHit = [];
    private List<int> p1Queue = [];
    private List<Vector3> towerPos = [];
    object treadLock = new();
    [ScriptMethod(eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:47784", "TargetIndex:1"])]
    public void P1_单人塔(Event @event, ScriptAccessory accessory)
    {
        var targetindex = accessory.Data.PartyList.IndexOf((uint)@event.TargetId);
        var pos = @event.TargetPosition;
        lock (treadLock)
        {
            lightHit.Add(targetindex);
            towerPos.Add(pos);
        }
        if (towerPos.Count == 4)
        {
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            towerPos.Sort((a, b) => a.X.CompareTo(b.X));
            var towerPeople = p1Queue.Where(x => !lightHit.Contains(x)).ToList();
            if (lightHit.Contains(myindex))
            {
                for (int i = 0; i < 4; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"单人塔";
                    dp.Scale = new(4);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = towerPos[i];
                    dp.DestoryAt = 3500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                
            }
            else
            {
                var towerIndex = towerPeople.IndexOf(myindex);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"单人塔";
                dp.Scale = new(4);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Position = towerPos[towerIndex];
                dp.DestoryAt = 3500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = $"单人塔指路";
                dp2.Scale= new(2);
                dp2.ScaleMode= ScaleMode.YByDistance;
                dp2.Color = accessory.Data.DefaultSafeColor;
                dp2.Owner=accessory.Data.Me;
                dp2.TargetPosition = towerPos[towerIndex];
                dp2.DestoryAt = 3500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
            }
            
        }
    }

    [ScriptMethod(eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:5078"])]
    public void P1_陷阱点名(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"P1_陷阱点名";
        dp.Scale = new(6);
        dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
        dp.Owner = @event.TargetId;
        dp.Delay=int.Parse(@event["DurationMilliseconds"])-5000;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }
    [ScriptMethod(eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:5078"])]
    public void P1_陷阱点名击退指示(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"P1_陷阱点名击退指示";
        dp.Scale = new(2,14);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner=accessory.Data.Me;
        dp.TargetObject = @event.TargetId;
        dp.Rotation = MathF.PI;
        dp.FadeCentreObject = @event.TargetId;
        dp.FadeDistance = 6;
        dp.Delay = int.Parse(@event["DurationMilliseconds"]) - 5000;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
    }

    [ScriptMethod(eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:50722"],suppress:1000)]
    public void P1_超驱动死刑(Event @event, ScriptAccessory accessory)
    {
        if (parse==11|| parse==12)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P1_超驱动死刑";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = @event.SourceId;
            dp.CentreResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
            dp.CentreOrderIndex = 1;
            dp.DestoryAt = 7500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        
    }

    private int p1stoneCount = 0;
    [ScriptMethod(eventType: EventTypeEnum.Tether, eventCondition: ["Id:002D"])]
    public void P1_连线岩石弹(Event @event, ScriptAccessory accessory)
    {
        if (parse != 12) return;
        lock (treadLock)
        {
            p1stoneCount++;
        }
        var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        if (pos.Y > 10) return;
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"P1_连线岩石弹";
        dp.Scale = new(5);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.TargetId;
        dp.Delay= p1stoneCount > 4 ? 8500 : 6500;
        dp.DestoryAt = 4000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

    [ScriptMethod(eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:64","Id2:128"])]
    public void P1_左右刀(Event @event, ScriptAccessory accessory)
    {
        if (parse == 12)
        {
            var source = accessory.Data.Objects.SearchById(@event.SourceId);
            if (source.DataId == 2015164)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P1_左右刀";
                dp.Scale = new(20);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Position = new(100, 0, 100);
                dp.Radian = MathF.PI;
                dp.Rotation = -MathF.PI / 2;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
            if (source.DataId == 2015165)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P1_左右刀";
                dp.Scale = new(20);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Position = new(100, 0, 100);
                dp.Radian = MathF.PI;
                dp.Rotation = MathF.PI / 2;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
        }

    }

    private List<int> p1TeleportList = [];
    [ScriptMethod(eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(487[6789]|5079|508[012])$"],userControl:false)]
    public void P1_传送buff记录(Event @event, ScriptAccessory accessory)
    {
        //上 4876 5079
        //下 4877 5080
        //右 4878 5081 
        //左 4879 5082 
        var dur = int.Parse(@event["DurationMilliseconds"]);
        var tIndex=accessory.Data.PartyList.IndexOf((uint)@event.TargetId);
        var dir4=0;
        var statusID = @event["StatusID"];
        if (statusID == "4876" || statusID == "5079") dir4 = 1;
        else if (statusID == "4877" || statusID == "5080") dir4 = 2;
        else if (statusID == "4878" || statusID == "5081") dir4 = 3;
        else if (statusID == "4879" || statusID == "5082") dir4 = 4;
        lock (p1TeleportList)
        {
            p1TeleportList[tIndex] += dur < 8000 ? dir4 * 10 : dir4;
        }
        //传送距离6m
    }

    [ScriptMethod(eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(487[6789]|5079|508[012])$"],suppress:1000)]
    public void P1_传送buff放置_指路(Event @event, ScriptAccessory accessory)
    {
        //13 31 上右
        //14 41 上左
        //23 32 下右
        //24 42 下左
        Task.Delay(100).ContinueWith(t => {
        
            var  myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var tp = p1TeleportList[myindex];
            Vector3 pos1 =new();
            Vector3 pos2 = new();
            if (tp == 0) return;
            //(100,0,91)上中点 (106,0,91)上右点
            switch (tp)
            {
                case 13:
                    pos1 = new(106, 0, 91);
                    pos2 = new(100, 0, 91);
                    break;
                case 31:
                    pos1 = new(100, 0, 91);
                    pos2 = new(106, 0, 91);
                    break;
                case 14:
                    pos1 = new(91, 0, 100);
                    pos2 = new(91, 0, 94);
                    break;
                case 41:
                    pos1 = new(91, 0, 94);
                    pos2 = new(91, 0, 100);
                    break;
                case 23:
                    pos1 = new(109, 0, 100);
                    pos2 = new(109, 0, 106);
                    break;
                case 32:
                    pos1 = new(109, 0, 106);
                    pos2 = new(109, 0, 100);
                    break;
                case 24:
                    pos1 = new(94, 0, 109);
                    pos2 = new(100, 0, 109);
                    break;
                case 42:
                    pos1 = new(100, 0, 109);
                    pos2 = new(94, 0, 109);
                    break;
                case 11:
                    pos1 = new(115, 0, 100);
                    pos2 = new(115, 0, 106);
                    break;
                case 22:
                    pos1 = new(85, 0, 100);
                    pos2 = new(85, 0, 94);
                    break;
                case 33:
                    pos1 = new(100, 0, 115);
                    pos2 = new(94, 0, 115);
                    break;
                case 44:
                    pos1 = new(100, 0, 85);
                    pos2 = new(106, 0, 85);
                    break;
                default:
                    break;
            }
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"传送指路1";
            dp.Scale = new(2);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner= accessory.Data.Me;
            dp.TargetPosition = pos1;
            dp.ScaleMode = ScaleMode.YByDistance;
            dp.DestoryAt=7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = $"传送指路1-2";
            dp2.Scale = new(2);
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.Position = pos1;
            dp2.TargetPosition = pos2;
            dp2.ScaleMode = ScaleMode.YByDistance;
            dp2.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);

            var dp3 = accessory.Data.GetDefaultDrawProperties();
            dp3.Name = $"传送指路2";
            dp3.Scale = new(2);
            dp3.Color = accessory.Data.DefaultSafeColor;
            dp3.Owner= accessory.Data.Me;
            dp3.TargetPosition = pos2;
            dp3.ScaleMode = ScaleMode.YByDistance;
            dp3.Delay = 7000;
            dp3.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);

        });
    }


    #endregion

    #region 工具方法
    public static unsafe void WriteVisible(ScriptAccessory sa, IGameObject? actor, bool visible,int recoverInterval=0)
    {
        if (actor == null || !actor.IsValid()) return;

        try
        {
            var gameObject = (GameObject*)actor.Address;
            var oldFlags = gameObject->RenderFlags;
            gameObject->RenderFlags = visible
                ? VisibilityFlags.None
                : VisibilityFlags.Model;
            if (recoverInterval<=0)
            {
                return;
            }
            Task.Delay(recoverInterval).ContinueWith(_ =>
            {
                if (actor == null || !actor.IsValid()) return;

                try
                {
                    var gameObject = (GameObject*)actor.Address;
                    gameObject->RenderFlags = oldFlags;
                }
                catch (Exception e)
                {
                    sa.Log.Error(e.ToString());
                }
            });
        }
        catch (Exception e)
        {
            sa.Log.Error(e.ToString());
        }
    }
    private Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
    {

        Vector2 v2 = new(point.X - centre.X, point.Z - centre.Z);

        var rot = (MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian);
        var lenth = v2.Length();
        return new(centre.X + MathF.Sin(rot) * lenth, centre.Y, centre.Z - MathF.Cos(rot) * lenth);
    }
    #endregion

}

