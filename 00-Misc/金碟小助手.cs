using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using System.Windows.Forms;
using Dalamud.Game.ClientState.Objects.Types;

namespace MyScriptNamespace
{
    [ScriptType(name: "金碟小助手", territorys: [144],guid: "9eed3085-cb11-738a-7e06-5ba0aa67363c", version:"0.0.0.1",author:"Karlin")]
    public class 金碟小助手
    {
        
        


        DateTime lasthealth=DateTime.Now;
        DateTime lastdot= DateTime.Now;

        public void Init(ScriptAccessory accessory)
        {
        }


        [ScriptMethod(name: "快刀一闪竹子范围", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add", "DataId:regex:^(201077[789])$"])]
        public void 快刀一闪竹子范围(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dataId = @event["DataId"];
            var dur = 11500;
            if (dataId== "2010777")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "快刀一闪竹子_单边";
                dp.Scale = new(5,28);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Rotation = float.Pi / 2;
                dp.Owner = sid;
                dp.DestoryAt = dur;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            if (dataId == "2010778")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "快刀一闪竹子_双边";
                dp.Scale = new(5, 56);
                dp.Rotation = float.Pi / 2;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = sid;
                dp.DestoryAt = dur;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
            }
            if (dataId == "2010779")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "快刀一闪竹子_钢铁";
                dp.Scale = new(11);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = sid;
                dp.DestoryAt = dur;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }

        }

        [ScriptMethod(name: "快刀一闪_狗测试", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["DataId:9644"])]
        public void 狗测试(Event @event, ScriptAccessory accessory)
        {

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "快刀一闪_狗";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 30000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);


        }



        private static bool ParseObjectId(string? idStr, out uint id)
        {
            id = 0;
            if (string.IsNullOrEmpty(idStr)) return false;
            try
            {
                var idStr2 = idStr.Replace("0x", "");
                id = uint.Parse(idStr2, System.Globalization.NumberStyles.HexNumber);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

