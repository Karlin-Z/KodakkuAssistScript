using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.Draw;
using Dalamud.Utility.Numerics;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using Dalamud.Memory.Exceptions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommons;
using System.Linq;
using ImGuiNET;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Module.Draw.Manager;
using System.Security.Cryptography;


namespace KarlinScriptNamespace
{
    [ScriptType(name: "M5s绘图", territorys: [1257], guid: "ecf3365e-8d21-5daa-0329-8aa33ee30778", version: "0.0.0.1", author: "Karlin", note: noteStr, updateInfo: updateInfoStr)]
    public class M5sDraw
    {
        const string noteStr =
        """
        
        """;
        const string updateInfoStr =
        """
        
        """;

        private ulong BossId;
        
        public void Init(ScriptAccessory accessory)
        {
           

        }
        [ScriptMethod(name: "BossId记录", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:41767", "TargetIndex:1"],userControl:false)]
        public void BossId记录(Event @event, ScriptAccessory accessory)
        {
            BossId = @event.SourceId;
        }

        [ScriptMethod(name: "死刑", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:01D7"])]
        public void 死刑(Event @event, ScriptAccessory accessory)
        {
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M1s 死刑";
            dp.Scale = new(25);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = BossId;
            dp.TargetObject = @event.TargetId;
            dp.DestoryAt = 5700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
    }
}

