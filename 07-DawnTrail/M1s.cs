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


namespace KarlinScriptNamespace
{
    [ScriptType(name: "M1s绘图", territorys: [1226], guid: "8010d865-7d6d-4c23-92e0-f4b0120e18ac", version: "0.0.0.6", author: "Karlin")]
    public class M1sDraw
    {
        [UserSetting("地板修复击退,Mt组安全半场")]
        public KnockBackMtPosition MtSafeFloor { get; set; }

        public enum KnockBackMtPosition
        {
            NorthHalf,
            SouthHalf,
            EastHalf,
            WestHalf
        }

        int? firstTargetIcon = null;
        List<int> FloorBrokeList = new ();
        uint copyCatTarget;
        uint parse;
        List<uint> P3TetherTarget = new();
        List<string> P3JumpSkill = new();
        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(@".*");
            //accessory.Method.MarkClear();

            firstTargetIcon = null;
            parse = 1;
            P3TetherTarget = new();
            P3JumpSkill = new();

        }
        [ScriptMethod(name: "分P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(38036|37963)$"],userControl:false)]
        public void 分P(Event @event, ScriptAccessory accessory)
        {
            parse++;
        }
        [ScriptMethod(name: "扇形引导", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37948"])]
        public void 扇形引导(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var Interval = 1000;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-1-1";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);



            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-1-2";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-1-3";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-1-4";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-2-1";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);



            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-2-2";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-2-3";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-2-4";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;

            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "扇形引导二段", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37952"])]
        public void 扇形引导二段(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导二段";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 1500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "左右刀", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3794[3467]$"])]
        public void 左右刀(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var idStr = @event["ActionId"];
            var isfast = (idStr == "37943" || idStr == "37947");
            var isleft = (idStr == "37947" || idStr == "37944");

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"左右刀-{(isleft ? "左" : "右")}{(isfast ? "快" : "慢")}";
            dp.Scale = new(40,20);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp.Owner = sid;
            dp.Rotation = isleft ? float.Pi / 2 : float.Pi / -2;
            dp.Delay = isfast ? 0 : 6000;
            dp.DestoryAt = isfast ? 6000 : 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "分身左右刀", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37989|3799[023])$"])]
        public void 分身左右刀(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var idStr = @event["ActionId"];
            var isfast = (idStr == "37989" || idStr == "37993");
            var isleft = (idStr == "37993" || idStr == "37990");

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"分身左右刀-{(isleft ? "左" : "右")}{(isfast ? "快" : "慢")}";
            dp.Scale = new(100);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp.Owner = sid;
            dp.Rotation = isleft ? float.Pi / 2 : float.Pi / -2;
            dp.Delay = isfast ? 0 : 6000;
            dp.DestoryAt = isfast ? 6000 : 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "跳跃左右刀", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^3796[5678]$"])]
        public void 跳跃左右刀(Event @event, ScriptAccessory accessory)
        {
            //37965左跳右刀
            //37966左跳左刀
            //37967右跳右刀
            //37968右跳左刀
            var actionId = @event["ActionId"];
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var leftJump = (actionId == "37965" || actionId == "37966");
            var leftFast= (actionId == "37966" || actionId == "37968");
            Vector3 dv;
            if (leftJump) dv = new(-10, 0, 0);
            else dv = new(10, 0, 0);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"跳跃左右刀-{(leftJump?"左":"右")}跳{(leftFast ? "左" : "右")}刀快";
            dp.Position=pos+dv;
            dp.Scale = new(60,30);
            dp.Rotation = leftFast? float.Pi / -2: float.Pi / 2;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3); ;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"跳跃左右刀-{(leftJump ? "左" : "右")}跳{(leftFast ? "右" : "左")}刀慢";
            dp.Position = pos + dv;
            dp.Scale = new(60,30);
            dp.Rotation = leftFast ? float.Pi / 2 : float.Pi / -2;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3); ;
            dp.Delay = 7000;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);


        }
        [ScriptMethod(name: "跳跃扇形引导", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(38959|37975)$"])]
        public void 跳跃扇形引导(Event @event, ScriptAccessory accessory)
        {
            //38959右扇
            //37975左扇
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            Vector3 dv = @event["ActionId"] == "37975" ? new(-10, 0, 0) : new(10, 0, 0);
            if (Math.Abs(pos.X-110)<1)
            {
                dv = @event["ActionId"] == "37975" ? new(0, 0, 10) : new(0, 0, -10);
            }
            if (Math.Abs(pos.X - 90) < 1)
            {
                dv = @event["ActionId"] == "37975" ? new(0, 0, -10) : new(0, 0, 10);
            }
            var Interval = 1000;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-1-1";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos+dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Delay = 0;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);



            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-1-2";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Delay = 0;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-1-3";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Delay = 0;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-1-4";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Delay = 0;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-2-1";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);



            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-2-2";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-2-3";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导-2-4";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = pos + dv;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Delay = 6000 + Interval;
            dp.DestoryAt = 3000 - Interval;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }

        [ScriptMethod(name: "跳跃扇形引导二段", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37980"])]
        public void 跳跃扇形引导二段(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "扇形引导二段";
            dp.Scale = new(100);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 1500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }


        [ScriptMethod(name: "P3连线收集", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0066"], userControl: false)]
        public void 本体连线收集(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            P3TetherTarget.Add(sid);
        }
        [ScriptMethod(name: "P3跳跃技能收集", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(38959|37975|3796[5678])$"])]
        public void P3跳跃技能收集(Event @event, ScriptAccessory accessory)
        {
            //38959 右扇
            //37975 左扇
            //37965 左跳右刀
            //37966 左跳左刀
            //37967 右跳右刀
            //37968 右跳左刀
            if (parse != 3) return;
            P3JumpSkill.Add(@event["ActionId"]);
        }

        [ScriptMethod(name: "P3分身连线技能", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0066"])]
        public void P3分身连线技能(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            Task.Delay(100).ContinueWith((t) =>
            {
                //38959 右扇
                //37975 左扇
                //37965 左跳右刀
                //37966 左跳左刀
                //37967 右跳右刀
                //37968 右跳左刀
                if (P3TetherTarget.Count < 3) return;
                var skillId= P3JumpSkill[P3TetherTarget.IndexOf(sid)];
                
                if(skillId == "38959" || skillId == "37975")
                {
                    var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                    var leftJump = (skillId == "37975");
                    var isNouthCopy = Math.Abs(pos.Z - 95) < 1;
                    Vector3 dv=new(0,0,0);
                    if (isNouthCopy)
                    {
                        dv = leftJump ? new(10, 0, 0) : new(-10, 0, 0);
                    }
                    else
                    {
                        dv = leftJump ? new(-10, 0, 0) : new(10, 0, 0);
                    }
                    
                    var Interval = 1000;

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分身跳跃扇形-{(isNouthCopy ? "北" : "南")}分身{(leftJump ? "左" : "右")}跳引导-1-1";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 1;
                    dp.Delay = 0;
                    dp.DestoryAt = 17000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);



                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分身跳跃扇形-{(isNouthCopy ? "北" : "南")}分身{(leftJump ? "左" : "右")}跳引导-1-2";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 2;
                    dp.Delay = 0;
                    dp.DestoryAt = 17000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分身跳跃扇形-{(isNouthCopy ? "北" : "南")}分身{(leftJump ? "左" : "右")}跳引导-1-3";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 3;
                    dp.Delay = 0;
                    dp.DestoryAt = 17000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分身跳跃扇形-{(isNouthCopy ? "北" : "南")}分身{(leftJump ? "左" : "右")}跳引导-1-4";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 4;
                    dp.Delay = 0;
                    dp.DestoryAt = 17000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分身跳跃扇形-{(isNouthCopy ? "北" : "南")}分身{(leftJump ? "左" : "右")}跳引导-2-1";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 1;
                    dp.Delay = 17000 + Interval;
                    dp.DestoryAt = 3000 - Interval;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);



                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分身跳跃扇形-{(isNouthCopy ? "北" : "南")}分身{(leftJump ? "左" : "右")}跳引导-2-2";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 2;
                    dp.Delay = 17000 + Interval;
                    dp.DestoryAt = 3000 - Interval;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分身跳跃扇形-{(isNouthCopy ? "北" : "南")}分身{(leftJump ? "左" : "右")}跳引导-2-3";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 3;
                    dp.Delay = 17000 + Interval;
                    dp.DestoryAt = 3000 - Interval;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分身跳跃扇形-{(isNouthCopy ? "北" : "南")}分身{(leftJump ? "左" : "右")}跳引导-2-4";
                    dp.Scale = new(100);
                    dp.Radian = float.Pi / 4;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Position = pos + dv;
                    dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    dp.TargetOrderIndex = 4;
                    dp.Delay = 17000 + Interval;
                    dp.DestoryAt = 3000 - Interval;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                }
                if(skillId == "37965" || skillId == "37966" || skillId == "37967" || skillId == "37968")
                {
                    var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                    var leftJump = (skillId == "37965" || skillId == "37966");
                    var leftFast = (skillId == "37966" || skillId == "37968");
                    var isNouthCopy = Math.Abs(pos.Z - 95) < 1;
                    Vector3 dv = new(0, 0, 0);
                    
                    if (isNouthCopy)
                    {
                        dv = leftJump ? new(10, 0, 0) : new(-10, 0, 0);
                    }else
                    {
                        dv = leftJump ? new(-10, 0, 0) : new(10, 0, 0);
                    }

                    var rotation= leftFast ? float.Pi / -2 : float.Pi / 2;
                    rotation += isNouthCopy ? float.Pi : 0;

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"跳跃左右刀-{(isNouthCopy ? "北" : "南")}分身{(leftJump ? "左" : "右")}跳{(leftFast ? "左" : "右")}刀快";
                    dp.Position = pos + dv;
                    dp.Scale = new(60,30);
                    dp.Rotation = rotation;
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(3); ;
                    dp.DestoryAt = 16000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"跳跃左右刀-{(isNouthCopy ? "北" : "南")}分身{(leftJump ? "左" : "右")}跳{(leftFast ? "左" : "右")}刀慢";
                    dp.Position = pos + dv;
                    dp.Scale = new(60, 30);
                    dp.Rotation = -rotation;
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(3); ;
                    dp.Delay = 16000;
                    dp.DestoryAt = 2000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }

            });
        }



        [ScriptMethod(name: "双人分摊", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37982|38016)$"])]
        public void 双人分摊(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var sid)) return;

            int[] stackGroup = [6, 5, 4, 7, 2, 1, 0, 3];
            var index= accessory.Data.PartyList.ToList().IndexOf(sid);
            var myIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            var isMyStack = (index == myIndex || myIndex == stackGroup[index]);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"双人分摊";
            dp.Scale = new(4);
            dp.Color = isMyStack?accessory.Data.DefaultSafeColor: accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "四人分摊", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37984|38018)$"])]
        public void 四人分摊(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var sid)) return;

            int[] h1Group = [0,2,4,6];
            var index = accessory.Data.PartyList.ToList().IndexOf(sid);
            var myIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);

            var isMyStack = (h1Group.Contains(index)== h1Group.Contains(myIndex));

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"双人分摊";
            dp.Scale = new(5);
            dp.Color = isMyStack ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "四人直线分摊", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:34722"])]
        public void 四人直线分摊(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;

            int[] h1Group = [0, 2, 4, 6];
            var index = accessory.Data.PartyList.ToList().IndexOf(tid);
            var myIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);

            var isMyStack = (h1Group.Contains(index) == h1Group.Contains(myIndex));

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"双人分摊";
            dp.Scale = new(6,40);
            dp.Color = isMyStack ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.TargetObject = tid;
            dp.DestoryAt = 5500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "七人直线分摊", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38039"])]
        public void 七人直线分摊(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"七人直线分摊";
            dp.Scale = new(5, 40);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = sid;
            dp.TargetObject = tid;
            dp.DestoryAt = 5500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "风圈分散", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38022"])]
        public void 风圈分散(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"风圈分散";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            if(sid==accessory.Data.Me) accessory.Method.TextInfo("风圈散开",8000,true);
        }
        [ScriptMethod(name: "职能分散提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38041"])]
        public void 职能分散提示(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TextInfo("职能分散",5500,false);
        }

        [ScriptMethod(name: "地板破坏安全区重置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37953"],userControl:false)]
        public void 地板破坏安全区重置(Event @event, ScriptAccessory accessory)
        {
            FloorBrokeList = new ();
        }
        [ScriptMethod(name: "地板破坏安全区", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(39276|37955)$"])]
        public void 地板破坏安全区(Event @event, ScriptAccessory accessory)
        {
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var centre = new Vector3(100, 0, 100);
            var dv = pos - centre;
            if (dv.Length() > 10) return;
            lock (FloorBrokeList)
            {
                var index = FloorToIndex(pos);
                FloorBrokeList.Add(index);
                
                if (FloorBrokeList.Count == 5)
                {
                    var during = 20000;
                    var nwSafe = (index == 0 || index == 2);
                    Vector3 endPos = default;
                    if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 0)
                    {
                        endPos = nwSafe ? new Vector3(95, 0, 95) : new Vector3(105, 0, 95);
                    }
                    else
                    {
                        endPos = nwSafe ? new Vector3(105, 0, 105) : new Vector3(95, 0, 105);
                    }
                    var safePosIndex1 = FloorToIndex(endPos);

                    var safePosBrokeIndex= FloorBrokeList.IndexOf(safePosIndex1);
                    if (safePosBrokeIndex == 0)
                    {
                        var startPos = Math.Abs(FloorBrokeList[3]- safePosIndex1)%4==1 ? IndexToFloor(FloorBrokeList[3]): IndexToFloor(FloorBrokeList[2]);
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"地板破坏安全区";
                        dp.Scale = new(2);
                        dp.Position = startPos;
                        dp.TargetPosition = endPos;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = during;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                    if (safePosBrokeIndex == 1)
                    {
                        if(Math.Abs(FloorBrokeList[3] - safePosIndex1) % 4 == 1)
                        {
                            var startPos = IndexToFloor(FloorBrokeList[3]);
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"地板破坏安全区";
                            dp.Scale = new(2);
                            dp.Position = startPos;
                            dp.TargetPosition = endPos;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                        else
                        {
                            var startPos = IndexToFloor(FloorBrokeList[3]);
                            var pos2= IndexToFloor(FloorBrokeList[0]);
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"地板破坏安全区";
                            dp.Scale = new(2);
                            dp.Position = startPos;
                            dp.TargetPosition = pos2;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                            dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"地板破坏安全区";
                            dp.Scale = new(2);
                            dp.Position = pos2;
                            dp.TargetPosition = endPos;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                        


                    }
                    if (safePosBrokeIndex == 2)
                    {
                        if (Math.Abs(FloorBrokeList[0] - safePosIndex1) % 4 == 1)
                        {
                            var pos2 = IndexToFloor(FloorBrokeList[0]);
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"地板破坏安全区";
                            dp.Scale = new(2);
                            dp.Position = endPos;
                            dp.TargetPosition = pos2;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                            dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"地板破坏安全区";
                            dp.Scale = new(2);
                            dp.Position = pos2;
                            dp.TargetPosition = new((endPos.X - 100) * 0.6f + 100, 0, (endPos.Z - 100) * 0.6f + 100);
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                        else
                        {
                            var pos1 = IndexToFloor(FloorBrokeList[3]);
                            var pos2 = IndexToFloor(FloorBrokeList[0]);
                            var pos3 = IndexToFloor(FloorBrokeList[1]);

                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"地板破坏安全区";
                            dp.Scale = new(2);
                            dp.Position = pos1;
                            dp.TargetPosition = pos2;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                            dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"地板破坏安全区";
                            dp.Scale = new(2);
                            dp.Position = pos2;
                            dp.TargetPosition = pos3;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                            dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"地板破坏安全区";
                            dp.Scale = new(2);
                            dp.Position = pos3;
                            dp.TargetPosition = endPos;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                    }
                    if (safePosBrokeIndex == 3)
                    {
                        if (Math.Abs(FloorBrokeList[0] - safePosIndex1) % 4 == 1)
                        {
                            var pos2 = IndexToFloor(FloorBrokeList[0]);
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"地板破坏安全区";
                            dp.Scale = new(2);
                            dp.Position = endPos;
                            dp.TargetPosition = pos2;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                            dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"地板破坏安全区";
                            dp.Scale = new(2);
                            dp.Position = pos2;
                            dp.TargetPosition = new((endPos.X - 100) * 0.6f + 100, 0, (endPos.Z - 100) * 0.6f + 100);
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                        else
                        {
                            var pos2 = IndexToFloor(FloorBrokeList[1]);
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"地板破坏安全区";
                            dp.Scale = new(2);
                            dp.Position = endPos;
                            dp.TargetPosition = pos2;
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                            dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"地板破坏安全区";
                            dp.Scale = new(2);
                            dp.Position = pos2;
                            dp.TargetPosition = new((endPos.X - 100) * 0.6f + 100, 0, (endPos.Z - 100) * 0.6f + 100);
                            dp.ScaleMode |= ScaleMode.YByDistance;
                            dp.Color = accessory.Data.DefaultSafeColor;
                            dp.DestoryAt = during;
                            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                        }
                    }




                    


                    
                    
                    
                    
                }
                

                

                

            }



        }
        
        

        [ScriptMethod(name: "分身猫爪点名", eventType: EventTypeEnum.TargetIcon,userControl:false)]
        public void 分身猫爪点名(Event @event, ScriptAccessory accessory)
        {
            if (ParsTargetIcon(@event["Id"]) != 320) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            copyCatTarget = tid;
        }
        [ScriptMethod(name: "分身砸地击飞", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37958"])]
        public void 分身砸地击飞(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(50).ContinueWith(t =>
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "分身砸地击飞";
                dp.Scale = new(1.5f, 10);
                dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
                dp.Owner = copyCatTarget;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
            });
        }

        [ScriptMethod(name: "分身砸地十字", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37960|37958)$"])]
        public void 分身砸地十字(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(50).ContinueWith(t =>
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "分身砸地十字";
                dp.Scale = new(1.5f, 80);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = copyCatTarget;
                dp.FixRotation = true;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "分身砸地十字";
                dp.Scale = new(1.5f, 80);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = copyCatTarget;
                dp.FixRotation = true;
                dp.Rotation = float.Pi / 2;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
            });
        }

        [ScriptMethod(name: "地板修复安全区", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00080004", "Index:regex:^(0000000[1247])$"])]
        public void 地板修复安全区(Event @event, ScriptAccessory accessory)
        {
            var myIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);

            if (MtSafeFloor == KnockBackMtPosition.SouthHalf || MtSafeFloor == KnockBackMtPosition.NorthHalf)
            {
                int[] northGroup = MtSafeFloor == KnockBackMtPosition.NorthHalf ? [0, 2, 4, 6] : [1, 3, 5, 7];
                var isNorthGroup = northGroup.Contains(myIndex);
                if (@event["Index"] == "00000001")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"地板修复安全区";
                    dp.Scale = new(20f, 10);
                    dp.Position = isNorthGroup ? new(90, 0, 85) : new(110, 0, 115);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
                if (@event["Index"] == "00000002")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"地板修复安全区";
                    dp.Scale = new(20f, 10);
                    dp.Position = isNorthGroup ? new(110, 0, 85) : new(90, 0, 115);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
                if (@event["Index"] == "00000004")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"地板修复安全区";
                    dp.Scale = new(10, 20);
                    dp.Position = isNorthGroup ? new(85, 0, 90) : new(115, 0, 110);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
                if (@event["Index"] == "00000007")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"地板修复安全区";
                    dp.Scale = new(10, 20);
                    dp.Position = isNorthGroup ? new(115, 0, 90) : new(85, 0, 110);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
            }
            else
            {
                int[] eastGroup = MtSafeFloor == KnockBackMtPosition.EastHalf ? [0, 2, 4, 6] : [1, 3, 5, 7];
                var isEastGroup = eastGroup.Contains(myIndex);
                if (@event["Index"] == "00000001")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"地板修复安全区";
                    dp.Scale = new(20f, 10);
                    dp.Position = isEastGroup ?  new(110, 0, 115): new(90, 0, 85);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
                if (@event["Index"] == "00000002")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"地板修复安全区";
                    dp.Scale = new(20f, 10);
                    dp.Position = isEastGroup ? new(110, 0, 85) : new(90, 0, 115);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
                if (@event["Index"] == "00000004")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"地板修复安全区";
                    dp.Scale = new(10, 20);
                    dp.Position = isEastGroup ? new(115, 0, 110) : new(85, 0, 90);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
                if (@event["Index"] == "00000007")
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"地板修复安全区";
                    dp.Scale = new(10, 20);
                    dp.Position = isEastGroup ? new(115, 0, 90) : new(85, 0, 110);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Straight, dp);
                }
            }


        }
        [ScriptMethod(name: "击退", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37964"])]
        public void 击退(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"击退";
            dp.Scale = new(1.5f,21);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.Rotation = float.Pi;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "远近分摊", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3961[12])$"])]
        public void 远近分摊(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            dur += 1300;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"远近分摊近";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = sid;
            dp.CentreResolvePattern=PositionResolvePatternEnum.PlayerNearestOrder;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"远近分摊远";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.DestoryAt = dur;
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
        private int ParsTargetIcon(string id)
        {
            firstTargetIcon??= int.Parse(id, System.Globalization.NumberStyles.HexNumber);
            return int.Parse(id, System.Globalization.NumberStyles.HexNumber) - (int)firstTargetIcon;
        }
        
        private int FloorToIndex(Vector3 pos)
        {
            var centre = new Vector3(100, 0, 100);
            var dv = pos - centre;
            var index = 0;
            if (dv.X > 0)
            {
                if (dv.Z > 0)
                {
                    index = 3;
                }
                else
                {
                    index = 0;
                }
            }
            else
            {
                if (dv.Z > 0)
                {
                    index = 2;
                }
                else
                {
                    index = 1;
                }
            }
            return index;
        }
        private Vector3 IndexToFloor(int index)
        {
            switch (index)
            {
                case 0: return new(105, 0, 95);
                case 1: return new(95, 0, 95);
                case 2: return new(95, 0, 105);
                case 3: return new(105, 0, 105);
            }
            return default;
        }
    }
}

