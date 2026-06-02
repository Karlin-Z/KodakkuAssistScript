using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using System.Windows.Forms;
using KodakkuAssist.Extensions;
using System.Threading.Tasks;
using System.Linq;
using Dalamud.Utility.Numerics;

namespace KarlinScriptNamespace;

/// <summary>
/// name and version affect the script name and version number displayed in the user interface.
/// territorys specifies the regions where this trigger is effective. If left empty, it will be effective in all regions.
/// Classes with the same GUID will be considered the same trigger. Please ensure your GUID is unique and does not conflict with others.
/// </summary>
[ScriptType(name: "绝凯夫卡", territorys: [1363],guid: "06cd8ccc-589d-46c4-8f50-36e3ca55919f", version:"0.0.0.1",author:"Karlin")]
public class 绝凯夫卡
{
    [UserSetting("危险颜色2")]
    public ScriptColor dangerColor { get; set; } = new();

    private int parse = 0;

    private bool trueFire = false;

    public void Init(ScriptAccessory accessory)
    {
        parse = 10;
    }

    
    [ScriptMethod(name: "恶狠狠毁荡",eventType: EventTypeEnum.StartCasting,eventCondition: ["ActionId:50179"])]
    public void 恶狠狠毁荡(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"恶狠狠毁荡 扇形死刑 一仇";
        dp.Scale = new(20);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId;
        dp.TargetOrderIndex=1;
        dp.TargetResolvePattern=PositionResolvePatternEnum.OwnerEnmityOrder;
        dp.Radian=MathF.PI/3*2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

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

    [ScriptMethod(name: "P1真假冰扇形", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47768|47774)$"])]
    public void P1真假冰扇形(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"真假冰扇形";
        dp.Scale = new(20);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId;
        dp.Radian = MathF.PI /2;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
    }

    [ScriptMethod(name: "P1真假雷直线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(47775|47777)$"])]
    public void P1真假雷直线(Event @event, ScriptAccessory accessory)
    {
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"P1真假雷直线";
        dp.Scale = new(10,40);
        dp.Color = accessory.Data.DefaultDangerColor;
        dp.Owner = @event.SourceId;
        dp.DestoryAt = 5000;
        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
    }
    [ScriptMethod(name: "P1真假火收集", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(02A1|02A2)$"],userControl:false)]
    public void P1真假火收集(Event @event, ScriptAccessory accessory)
    {
        trueFire = @event["Id"] == "02A2";
    }
    [ScriptMethod(name: "P1真假火分摊处理", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0080"])]
    public void P1真假火分摊处理(Event @event, ScriptAccessory accessory)
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

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
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
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
                }
                
            }
        });

    }
    [ScriptMethod(name: "P1真假火分散处理", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:007F"],suppress:1000)]
    public void P1真假火分散处理(Event @event, ScriptAccessory accessory)
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
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
                }
            }
            else
            {
                var targetindex = 0;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"假火分散0";
                dp.Scale = new(6);
                dp.Color =myindex<4? accessory.Data.DefaultSafeColor: accessory.Data.DefaultDangerColor;
                dp.Owner = accessory.Data.PartyList[0];
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

                dp.Name = $"假火分散4";
                dp.Color = myindex < 4 ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.PartyList[4];
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
            }
        });

    }
    [ScriptMethod(name: "P1真假火分散分摊TTS", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(0080|007F)$"],suppress:1000)]
    public void P1真假火分散分摊TTS(Event @event, ScriptAccessory accessory)
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

    //[ScriptMethod(name: "P1连线点名波动炮", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:47785"])]
    //public void P1连线点名波动炮(Event @event, ScriptAccessory accessory)
    //{
    //    var dp = accessory.Data.GetDefaultDrawProperties();
    //    dp.Name = $"P1连线点名波动炮";
    //    dp.Scale = new(6, 60);
    //    dp.Color = accessory.Data.DefaultDangerColor;
    //    dp.Position = @event.SourcePosition.WithY(0);
    //    dp.TargetObject = @event.TargetId;
    //    dp.DestoryAt = 7000;
    //    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
    //}
    [ScriptMethod(name: "P1单人塔", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:47784", "TargetIndex:1"])]
    public void P1单人塔(Event @event, ScriptAccessory accessory)
    {
        var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
        var targetindex = accessory.Data.PartyList.IndexOf((uint)@event.TargetId);
        var dp = accessory.Data.GetDefaultDrawProperties();
        dp.Name = $"P1单人塔";
        dp.Scale = new(4);
        dp.Color = Math.Abs(myindex - targetindex) == 4 ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
        dp.Position = @event.TargetPosition;
        dp.DestoryAt = 3500;
        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
    }

}

