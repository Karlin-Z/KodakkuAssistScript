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
using System.Linq;
using Dalamud.Bindings.ImGui;
using KodakkuAssist.Module.GameOperate;
using KodakkuAssist.Module.Draw.Manager;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading;
using KodakkuAssist.Extensions;


namespace KarlinScriptNamespace
{
    [ScriptType(name: "M5s绘图", territorys: [1257], guid: "ecf3365e-8d21-5daa-0329-8aa33ee30778", version: "0.0.0.2", author: "Karlin", note: noteStr, updateInfo: updateInfoStr)]
    public class M5sDraw
    {
        const string noteStr =
        """
        
        """;
        const string updateInfoStr =
        """
        1.增加九字切地火绘图
        2.增加俄罗斯方块安全区绘图
        """;

        [UserSetting("左右刀分摊延迟显示时间")]
        public uint StackDelay { get; set; } = 5000;
        [UserSetting("分身左右刀间隔")]
        public int filpDuring { get; set; } = 2450;

        private int parse;
        private ulong BossId;
        private bool isHealerStack;
        private int fireSafePoint = 0;
        private int fireCount = 0;
        private bool spotLightClockwise;
        private List<uint> flipDir=[];
        private List<DateTime> wavelengthBuffEndTime = [default, default, default, default, default, default, default, default];
        private ManualResetEvent SpotlightResetEvent = new(false);
        private ManualResetEvent BurnBuffResetEvent = new(false);
        private bool spotLightN;
        private bool longBurnBuff;
        private int spotLightRound;
        [Flags]
        private enum FlipDangerArea
        {
            None = 0,
            Point1 = 1,
            Point2 = 2,
            Point3 = 4,
            Point4 = 8,
        }

        public void Init(ScriptAccessory accessory)
        {
            parse = 0;

        }
        [ScriptMethod(name: "BossId记录", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:41767", "TargetIndex:1"],userControl:false)]
        public void BossId记录(Event @event, ScriptAccessory accessory)
        {
            BossId = @event.SourceId;
        }

        [ScriptMethod(name: "流血死刑", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:01D7"])]
        public void 死刑(Event @event, ScriptAccessory accessory)
        {
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 流血死刑";
            dp.Scale = new(25);
            dp.Radian = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = BossId;
            dp.TargetObject = @event.TargetId;
            dp.DestoryAt = 5700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        [ScriptMethod(name: "分摊方式记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42880|42881)$"], userControl: false)]
        public void 分摊方式记录(Event @event, ScriptAccessory accessory)
        {
            isHealerStack = @event.ActionId == 42881;
        }
        [ScriptMethod(name: "左右刀", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4279[234567]|4280[0123459]|4281[01234]|4220[345678])$"])]
        public void 左右刀(Event @event, ScriptAccessory accessory)
        {
            var actionId = @event.ActionId;
            var during=7000;
            if(     actionId ==42792 || actionId==42793 
                || actionId == 42794 || actionId == 42795 
                || actionId == 42796 || actionId == 42797
                || actionId == 42203 || actionId == 42794
                )
            {
                during = 6500;
            }
            var roatation = float.Pi / 2;
            if (    actionId == 42792 || actionId == 42793 || actionId == 42794 
                ||  actionId == 42800 || actionId == 42801 || actionId == 42802
                ||  actionId == 42809 || actionId == 42810 || actionId == 42811 
                ||  actionId == 42203 || actionId == 42205 || actionId == 42207)
            {
                roatation = -roatation;
            }
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 左右刀_前";
            dp.Owner = @event.SourceId;
            dp.Scale = new(40, 20);
            dp.Rotation = roatation;
            dp.DestoryAt = during;
            dp.Color= accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 左右刀_后";
            dp.Owner = @event.SourceId;
            dp.Scale = new(40, 20);
            dp.Rotation = -roatation;
            dp.Delay = during;
            dp.DestoryAt = 8500-during;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "左右刀对穿提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4279[234567]|4280[0123459]|4281[01234]|4220[345678])$"])]
        public void 左右刀对穿提示(Event @event, ScriptAccessory accessory)
        {
            var actionId = @event.ActionId;
            var during = 7000;
            if (actionId == 42792 || actionId == 42793
                || actionId == 42794 || actionId == 42795
                || actionId == 42796 || actionId == 42797
                || actionId == 42203 || actionId == 42794
                )
            {
                during = 6500;
            }
            Task.Delay(during).ContinueWith(t =>
            {
                accessory.Method.TextInfo("穿", 1000);
                accessory.Method.TTS("穿");
            });

        }
        [ScriptMethod(name: "左右刀分组分摊", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4279[234567]|4280[0123459]|4281[01234]|4220[345678])$"])]
        public void 左右刀分组分摊(Event @event, ScriptAccessory accessory)
        {
            if (isHealerStack)
            {
                var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var h1Group = myindex == 0 || myindex == 2 || myindex == 4 || myindex == 6;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 左右刀_H1分摊";
                dp.Owner = @event.SourceId;
                dp.TargetObject = accessory.Data.PartyList[2];
                dp.Scale = new(8, 50);
                dp.Delay = StackDelay;
                dp.DestoryAt = 10500 - StackDelay;
                dp.Color = h1Group ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 左右刀_H2分摊";
                dp.Owner = @event.SourceId;
                dp.TargetObject = accessory.Data.PartyList[3];
                dp.Scale = new(8, 50);
                dp.Delay = StackDelay;
                dp.DestoryAt = 10500 - StackDelay;
                dp.Color = h1Group ? accessory.Data.DefaultDangerColor : accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            else
            {
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var tGroup = myindex == 0 || myindex == 1;
                var hGroup = myindex == 2 || myindex == 3;
                var dGroup = myindex == 4 || myindex == 5 || myindex == 6 || myindex == 7;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 左右刀_T分摊";
                dp.Owner = @event.SourceId;
                dp.TargetObject = myindex == 0? accessory.Data.PartyList[1]: accessory.Data.PartyList[0];
                dp.Scale = new(40);
                dp.Radian = float.Pi / 4;
                dp.Delay = StackDelay;
                dp.DestoryAt = 10500 - StackDelay;
                dp.Color = tGroup ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 左右刀_H分摊";
                dp.Owner = @event.SourceId;
                dp.TargetObject = myindex == 2 ? accessory.Data.PartyList[3] : accessory.Data.PartyList[2];
                dp.Scale = new(40);
                dp.Radian = float.Pi / 4;
                dp.Delay = StackDelay;
                dp.DestoryAt = 10500 - StackDelay;
                dp.Color = hGroup ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 左右刀_D分摊";
                dp.Owner = @event.SourceId;
                dp.TargetObject = myindex == 4 ? accessory.Data.PartyList[5]: accessory.Data.PartyList[4];
                dp.Scale = new(40);
                dp.Radian = float.Pi / 4;
                dp.Delay = StackDelay;
                dp.DestoryAt = 10500 - StackDelay;
                dp.Color = dGroup ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
        }

        [ScriptMethod(name: "舞会阶段重置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42838"], userControl: false)]
        public void 舞会阶段重置(Event @event, ScriptAccessory accessory)
        {
            fireSafePoint = 0;
            fireCount = 0;
            parse ++;
            SpotlightResetEvent = new(false);
            BurnBuffResetEvent = new(false);
            spotLightRound = 0;
        }
        [ScriptMethod(name: "内圈聚光灯顺逆记录", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4572", "SourceDataId:18363"], userControl: false)]
        public void 内圈聚光灯顺逆记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            var pos = @event.SourcePosition;
            if (MathF.Abs(pos.X - 97.5f) < 1 && MathF.Abs(pos.Z - 92.5f) < 1) spotLightClockwise = false;
            if (MathF.Abs(pos.X - 102.5f) < 1 && MathF.Abs(pos.Z - 92.5f) < 1) spotLightClockwise = true;
        }
        [ScriptMethod(name: "地火情况记录", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:3", "Flag:regex:^(2|32)$"], userControl: false)]
        public void 地火情况记录(Event @event, ScriptAccessory accessory)
        {
            if (fireSafePoint != 0) return;
            fireSafePoint = @event["Flag"] == "2" ? 2 : 1;
        }
        [ScriptMethod(name: "地火绘图", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:3", "Flag:regex:^(8|128)$"])]
        public void 地火绘图(Event @event, ScriptAccessory accessory)
        {
            fireCount ++;
            accessory.Log.Debug($"{parse} {fireCount}");
            if (parse == 1 && fireCount == 10) return;
            if (parse == 4 && fireCount == 8) return;
            var wn = @event["Flag"]=="8";
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var xOffset = wn == (i % 2 == 0) ? 5 : 0;
                    Vector3 pos = new(85.0f + j * 10 + xOffset, 0, 82.5f + i * 5);
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"M5s 地火绘图 第{fireCount+1}轮 {j}{i}";
                    dp.Position = pos;
                    dp.Scale = new(5);
                    dp.Rotation = -float.Pi / 2;
                    dp.DestoryAt = 4000;
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
            }
        }
        [ScriptMethod(name: "第一次燃起来Buff处理位置", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4461"])]
        public void 第一次燃起来Buff处理位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (@event.TargetId != accessory.Data.Me) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            Task.Delay(dur - 8000).ContinueWith(t => {
                if (parse != 1) return;
                var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var first = dur < 25000;
                Vector3 dealpos = default;
                if (myindex == 2 || myindex == 6)
                {
                    dealpos = fireSafePoint ==2 ?  new(87.5f, 0, 87.5f) : dealpos = new(87.5f, 0, 112.5f);
                }
                if (myindex == 3 || myindex == 7)
                {
                    dealpos = fireSafePoint == 2 ? new(112.5f, 0, 112.5f) : dealpos = new(112.5f, 0, 87.5f);
                }
                if (myindex == 0 || myindex == 4)
                {
                    if (first)
                    {
                        if (fireSafePoint == 1)
                        {
                            dealpos= spotLightClockwise ? new(97.5f, 0, 92.5f) : new(92.5f, 0, 97.5f);
                        }
                        if(fireSafePoint == 2)
                        {
                            dealpos = spotLightClockwise ? new(92.5f, 0, 102.5f) : new(97.5f, 0, 107.5f);
                        }
                    }
                    else
                    {
                        if (fireSafePoint == 1)
                        {
                            dealpos = spotLightClockwise ? new(92.5f, 0, 97.5f) : new(97.5f, 0, 92.5f);
                        }
                        if (fireSafePoint == 2)
                        {
                            dealpos = spotLightClockwise ? new(97.5f, 0, 107.5f) : new(92.5f, 0, 102.5f);
                        }
                    }
                }
                if (myindex == 1 || myindex == 5)
                {
                    if (first)
                    {
                        if (fireSafePoint == 1)
                        {
                            dealpos = spotLightClockwise ? new(102.5f, 0, 107.5f) : new(107.5f, 0, 102.5f);
                        }
                        if (fireSafePoint == 2)
                        {
                            dealpos = spotLightClockwise ? new(107.5f, 0, 97.5f) : new(102.5f, 0, 92.5f);
                        }
                    }
                    else
                    {
                        if (fireSafePoint == 1)
                        {
                            dealpos = spotLightClockwise ? new(107.5f, 0, 102.5f) : new(102.5f, 0, 107.5f);
                        }
                        if (fireSafePoint == 2)
                        {
                            dealpos = spotLightClockwise ? new(102.5f, 0, 92.5f) : new(107.5f, 0, 97.5f);
                        }
                    }
                }


                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 燃起来Buff处理位置";
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition= dealpos;
                dp.Scale = new(3);
                dp.ScaleMode|=ScaleMode.YByDistance;
                dp.DestoryAt = 10000;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 燃起来Buff处理位置";
                dp.Position = dealpos;
                dp.Scale = new(2.5f);
                dp.DestoryAt = 10000;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

            });
        }
        [ScriptMethod(name: "二次燃起来聚光灯记录", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4572", "SourceDataId:18363"], userControl: false)]
        public void 二次燃起来聚光灯记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5) return;
            var pos = @event.SourcePosition;
            if (MathF.Abs(pos.X - 85f) < 1 && MathF.Abs(pos.Z - 100f) < 1) 
            { 
                spotLightN = true;
                SpotlightResetEvent.Set();
            }
            if (MathF.Abs(pos.X - 85f) < 1 && MathF.Abs(pos.Z - 85f) < 1)
            {
                spotLightN = false;
                SpotlightResetEvent.Set();
            }
        }
        [ScriptMethod(name: "第二次燃起来Buff处理位置", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4461"])]
        public void 第二次燃起来Buff处理位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5) return;
            if (@event.TargetId != accessory.Data.Me) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            Task.Delay(dur - 9500).ContinueWith(t =>
            {
                if (parse != 5) return;
                SpotlightResetEvent.WaitOne();
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var first = dur < 11000;
                Vector3 dealpos = default;
                //正点灯 （100,0,85） 斜点灯（85,0,85）
                if (myindex == 0 || myindex == 6)
                {
                    if (spotLightN) 
                        dealpos = first ? new(85, 0, 85) : new(100, 0, 85);
                    else 
                        dealpos = first ? new(100, 0, 85) : new(85, 0, 85);
                }
                if (myindex == 1 || myindex == 5)
                {
                    if (spotLightN)
                        dealpos = first ? new(115, 0, 115) : new(100, 0, 115);
                    else
                        dealpos = first ? new(100, 0, 115) : new(115, 0, 115);
                }
                if (myindex == 2 || myindex == 4)
                {
                    if (spotLightN)
                        dealpos = first ? new(85, 0, 115) : new(85, 0, 100);
                    else
                        dealpos = first ? new(85, 0, 100) : new(85, 0, 115);
                }
                if (myindex == 3 || myindex == 7)
                {
                    if (spotLightN)
                        dealpos = first ? new(115, 0, 85) : new(115, 0, 100);
                    else
                        dealpos = first ? new(115, 0, 100) : new(115, 0, 85);
                }

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 燃起来Buff处理位置";
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.Scale = new(3);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = first ? 9500 : 10000;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 燃起来Buff处理位置";
                dp.Position = dealpos;
                dp.Scale = new(2.5f);
                dp.Delay = first ? 0 : 1000;
                dp.DestoryAt = first ? 9500 : 9000;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

            });
            



        }
        [ScriptMethod(name: "第二次燃起来Buff记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4461"],userControl:false)]
        public void 第二次燃起来Buff记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5) return;
            if (@event.TargetId != accessory.Data.Me) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            longBurnBuff = dur > 11000;
            BurnBuffResetEvent.Set();
        }
        [ScriptMethod(name: "二次燃起来引导位置", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4561", "SourceDataId:18362"], suppress:1000)]
        public void 二次燃起来引导位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5) return;
            spotLightRound++;
            var pos = @event.SourcePosition;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            SpotlightResetEvent.WaitOne();
            BurnBuffResetEvent.WaitOne();
            var danceN = (MathF.Abs(pos.X - 100f) < 1 || MathF.Abs(pos.Z - 100f) < 1);
            var first = spotLightRound == 1;
            if (!longBurnBuff && first) return;
            if (longBurnBuff && !first) return;
            var nowSpotlightN =spotLightN != first;
            Vector3 dealpos = default;
            if (myindex == 0 || myindex == 6)
            {
                if (nowSpotlightN)
                    dealpos = danceN ? new(98, 0, 93) : new(93, 0, 93);
                else
                    dealpos = danceN ? new(100, 0, 91) : new(95, 0, 93);
            }
            if (myindex == 1 || myindex == 5)
            {
                if (nowSpotlightN)
                    dealpos = danceN ? new(102, 0, 107) : new(107, 0, 107);
                else
                    dealpos = danceN ? new(100, 0, 109) : new(105, 0, 107);
            }
            if (myindex == 2 || myindex == 4)
            {
                if (nowSpotlightN)
                    dealpos = danceN ? new(93, 0, 102) : new(93, 0, 107);
                else
                    dealpos = danceN ? new(91, 0, 100) : new(93, 0, 105);
            }
            if (myindex == 3 || myindex == 7)
            {
                if (nowSpotlightN)
                    dealpos = danceN ? new(107, 0, 98) : new(107, 0, 93);
                else
                    dealpos = danceN ? new(109, 0, 100) : new(105, 0, 93);
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 二次燃起来引导位置";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 10700;
            dp.Color = accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "钢铁月环", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42876|42878)$"])]
        public void 钢铁月环(Event @event, ScriptAccessory accessory)
        {
            var circle = @event.ActionId == 42876;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 钢铁月环_钢铁";
            dp.Owner = @event.SourceId;
            dp.Scale = new(7);
            dp.Delay= circle ? 0 : 5000;
            dp.DestoryAt = circle ? 5000 : 2500;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 钢铁月环_月环";
            dp.Owner = @event.SourceId;
            dp.Scale = new(40);
            dp.InnerScale = new(5);
            dp.Radian = float.Pi * 2;
            dp.Delay = circle ? 5000 : 0;
            dp.DestoryAt = circle ? 2500 : 5000;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

        }

        [ScriptMethod(name: "水波二段", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42853)$"])]
        public void 水波二段(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 钢铁月环_钢铁";
            dp.Owner = @event.SourceId;
            dp.Scale = new(40);
            dp.Radian = float.Pi / 4;
            dp.DestoryAt = 2500;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "分身阶段重置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39474"], userControl: false)]
        public void 分身阶段重置(Event @event, ScriptAccessory accessory)
        {
            flipDir = [];
            parse ++;
        }
        [ScriptMethod(name: "分身左右刀记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4276[2345])$"],userControl:false)]
        public void 分身左右刀记录(Event @event, ScriptAccessory accessory)
        {
            flipDir.Add(@event.ActionId-42761);
            accessory.Log.Debug($"{@event.ActionId - 42761}");
        }
        [ScriptMethod(name: "Wavelenth记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(4463|4462)$"], userControl: false)]
        public void Wavelenth记录(Event @event, ScriptAccessory accessory)
        {
            var index = accessory.Data.PartyList.IndexOf((uint)@event.TargetId);
            if (index == -1) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            lock (wavelengthBuffEndTime)
            {
                wavelengthBuffEndTime[index] = DateTime.Now.AddMilliseconds(dur);
            }
        }
        [ScriptMethod(name: "Wavelenth搭档", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(4463|4462)$"])]
        public void Wavelenth搭档(Event @event, ScriptAccessory accessory)
        {
            if (accessory.Data.Me != @event.TargetId) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            Task.Delay(dur - 4000).ContinueWith(t =>
            {
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var myEnd=wavelengthBuffEndTime[myindex];
                for (int i = 0; i < accessory.Data.PartyList.Count; i++)
                {
                    if (i != myindex && Math.Abs((wavelengthBuffEndTime[i] - myEnd).TotalSeconds) < 2)
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"M5s Wavelenth搭档";
                        dp.Owner = accessory.Data.PartyList[i];
                        dp.Scale = new(2);
                        dp.DestoryAt = 4000;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    }
                }
            });
            
        }
        [ScriptMethod(name: "Wavelenth分摊提醒", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(4463|4462)$"])]
        public void Wavelenth分摊提醒(Event @event, ScriptAccessory accessory)
        {
            if (accessory.Data.Me != @event.TargetId) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            Task.Delay(dur - 4000).ContinueWith(t =>
            {
                accessory.Method.TextInfo("分摊", 4000);
                accessory.Method.TTS("分摊");
            });

        }
        [ScriptMethod(name: "舞浪全开拉怪TTS", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42836)$"])]
        public void 舞浪全开拉怪TTS(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.TTS("拉怪南");
            accessory.Method.TextInfo("拉怪南", 5000);
        }
        [ScriptMethod(name: "分身左右刀", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42858)$"])]
        public void 分身左右刀(Event @event, ScriptAccessory accessory)
        {
            if (parse!=2) return;
            var dur1 = 5800;
            for (int i = 1; i < flipDir.Count; i++)
            {
                if (flipDir[i] != flipDir[i-1])
                {
                    break;
                }
                dur1 += filpDuring;
            }
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 分身左右刀1";
            dp.Owner = BossId;
            dp.Scale = new(40, 20);
            dp.Rotation = flipDir[0] == 3 ? float.Pi / 2 : -float.Pi / 2;
            dp.DestoryAt = dur1;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            for (int i = 1; i < flipDir.Count; i++)
            {
                if (flipDir[i] == flipDir[i - 1]) continue;
                var dur = filpDuring;
                for (int j = i+1; j < flipDir.Count; j++)
                {
                    if (flipDir[j] != flipDir[j - 1])
                    {
                        break;
                    }
                    dur += filpDuring;
                }
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 分身左右刀{i+1}";
                dp.Owner = BossId;
                dp.Scale = new(40, 20);
                dp.Rotation = flipDir[i] == 3 ? float.Pi / 2 : -float.Pi / 2;
                dp.Delay = 5800 + (i - 1) * filpDuring;
                dp.DestoryAt = dur;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }
        [ScriptMethod(name: "分身前后左右刀范围预告", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(41872)$"])]
        public void 分身前后左右刀范围预告(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6) return;

            FlipDangerArea danger1Area = FlipDangerArea.None;
            FlipDangerArea danger2Area = FlipDangerArea.None;
            FlipDangerArea danger3Area = FlipDangerArea.None;
            FlipDangerArea danger4Area = FlipDangerArea.None;

            danger1Area |= flipDir[0] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            for (int i = 0; i < flipDir.Count; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 前后左右刀范围预告{1}";
                dp.Owner = BossId;
                dp.Scale = new(40, 20);
                dp.Rotation = flipDir[i] switch
                {
                    1 => 0,
                    2 => float.Pi,
                    3 => float.Pi / 2,
                    _ => -float.Pi / 2,
                };
                dp.Delay = i  * 750;
                dp.DestoryAt = 750;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }

        }
        [ScriptMethod(name: "分身前后左右刀对穿方式", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(41872)$"])]
        public void 分身前后左右刀对穿方式(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6) return;

            FlipDangerArea danger1Area = FlipDangerArea.None;
            FlipDangerArea danger2Area = FlipDangerArea.None;
            FlipDangerArea danger3Area = FlipDangerArea.None;
            FlipDangerArea danger4Area = FlipDangerArea.None;

            danger1Area |= flipDir[0] switch
            {
                1 => FlipDangerArea.Point2| FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger1Area |= flipDir[1] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger2Area |= flipDir[2] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger2Area |= flipDir[3] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger3Area |= flipDir[4] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger3Area |= flipDir[5] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger4Area |= flipDir[6] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            danger4Area |= flipDir[7] switch
            {
                1 => FlipDangerArea.Point2 | FlipDangerArea.Point3,
                2 => FlipDangerArea.Point1 | FlipDangerArea.Point4,
                3 => FlipDangerArea.Point1 | FlipDangerArea.Point2,
                _ => FlipDangerArea.Point3 | FlipDangerArea.Point4,
            };
            Vector3 dealpos1 = default;
            if (!danger1Area.HasFlag(FlipDangerArea.Point1)) dealpos1 = new(101, 0, 99);
            if (!danger1Area.HasFlag(FlipDangerArea.Point2)) dealpos1 = new(101, 0, 101);
            if (!danger1Area.HasFlag(FlipDangerArea.Point3)) dealpos1 = new(99, 0, 101);
            if (!danger1Area.HasFlag(FlipDangerArea.Point4)) dealpos1 = new(99, 0, 99);
            Vector3 dealpos2 = default;
            if (!danger2Area.HasFlag(FlipDangerArea.Point1)) dealpos2 = new(101, 0, 99);
            if (!danger2Area.HasFlag(FlipDangerArea.Point2)) dealpos2 = new(101, 0, 101);
            if (!danger2Area.HasFlag(FlipDangerArea.Point3)) dealpos2 = new(99, 0, 101);
            if (!danger2Area.HasFlag(FlipDangerArea.Point4)) dealpos2 = new(99, 0, 99);
            Vector3 dealpos3 = default;
            if (!danger3Area.HasFlag(FlipDangerArea.Point1)) dealpos3 = new(101, 0, 99);
            if (!danger3Area.HasFlag(FlipDangerArea.Point2)) dealpos3 = new(101, 0, 101);
            if (!danger3Area.HasFlag(FlipDangerArea.Point3)) dealpos3 = new(99, 0, 101);
            if (!danger3Area.HasFlag(FlipDangerArea.Point4)) dealpos3 = new(99, 0, 99);
            Vector3 dealpos4 = default;
            if (!danger4Area.HasFlag(FlipDangerArea.Point1)) dealpos4 = new(101, 0, 99);
            if (!danger4Area.HasFlag(FlipDangerArea.Point2)) dealpos4 = new(101, 0, 101);
            if (!danger4Area.HasFlag(FlipDangerArea.Point3)) dealpos4 = new(99, 0, 101);
            if (!danger4Area.HasFlag(FlipDangerArea.Point4)) dealpos4 = new(99, 0, 99);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 分身前后左右刀处理位置1";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos1;
            dp.Scale = new(3);
            dp.ScaleMode|= ScaleMode.YByDistance;
            dp.DestoryAt = 8300;
            dp.Color = accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 分身前后左右刀处理位置2_1";
            dp.Position = dealpos1;
            dp.TargetPosition = dealpos2;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 8300;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 分身前后左右刀处理位置2_2";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos2;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = 8300;
            dp.DestoryAt = 3000;
            dp.Color = accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 分身前后左右刀处理位置3_1";
            dp.Position = dealpos2;
            dp.TargetPosition = dealpos3;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = 8300;
            dp.DestoryAt = 3000;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 分身前后左右刀处理位置3_2";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos3;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = 11300;
            dp.DestoryAt = 3000;
            dp.Color = accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 分身前后左右刀处理位置4_1";
            dp.Position = dealpos3;
            dp.TargetPosition = dealpos4;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = 11300;
            dp.DestoryAt = 3000;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 分身前后左右刀处理位置4_2";
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos4;
            dp.Scale = new(3);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = 14300;
            dp.DestoryAt = 3000;
            dp.Color = accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


        }
        [ScriptMethod(name: "分身左右刀对穿提示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42858)$"])]
        public void 分身左右刀对穿提示(Event @event, ScriptAccessory accessory)
        {
            for (int i = 1; i < flipDir.Count; i++)
            {
                if (flipDir[i] != flipDir[i - 1])
                {
                    Task.Delay(5800 + (i - 1) * filpDuring).ContinueWith(t =>
                    {
                        if (parse!=2) return;
                        accessory.Method.TextInfo("穿", 1000);
                        accessory.Method.TTS("穿");
                    });
                }
                
            }
        }
        [ScriptMethod(name: "箭头分身左右刀", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42869|42870|42788|42789)$"])]
        public void 箭头分身左右刀(Event @event, ScriptAccessory accessory)
        {
            var left = @event.ActionId == 42869 || @event.ActionId == 42788;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 箭头分身左右刀";
            dp.Owner = @event.SourceId;
            dp.Scale = new(90, 30);
            dp.Rotation = left ? -float.Pi / 2 : float.Pi / 2;
            dp.DestoryAt = 5000;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);


        }
        
        [ScriptMethod(name: "连续钢铁月环", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39908"])]
        public void 连续钢铁月环(Event @event, ScriptAccessory accessory)
        {
            for (int i = 0; i < 3; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 连续钢铁月环_钢铁";
                dp.Owner = @event.SourceId;
                dp.Scale = new(7);
                dp.Delay = 8000 + i * 2 * filpDuring;
                dp.DestoryAt = filpDuring;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }

            for (int i = 0; i < 4; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 连续钢铁月环_月环";
                dp.Owner = @event.SourceId;
                dp.Scale = new(40);
                dp.InnerScale = new(5);
                dp.Radian = float.Pi * 2;
                dp.Delay = i == 0 ? 5200 : 5500 + i * 2 * filpDuring;
                dp.DestoryAt = i == 0 ? 2800 : filpDuring;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
            }
            
        }
        [ScriptMethod(name: "俄罗斯方块重置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42836"], userControl: false)]
        public void 俄罗斯方块重置(Event @event, ScriptAccessory accessory)
        {
            parse = 3;

        }
        [ScriptMethod(name: "俄罗斯方块 分摊分散", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42844|42846)$"])]
        public void 俄罗斯方块_分摊分散(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            if (@event["ActionId"] =="42844")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 俄罗斯方块_分摊";
                dp.Owner = @event.TargetId;
                dp.Scale = new(4);
                dp.DestoryAt = 5000;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 俄罗斯方块_分散";
                dp.Owner = @event.TargetId;
                dp.Scale = new(5);
                dp.DestoryAt = 5000;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }

        }
        [ScriptMethod(name: "俄罗斯方块地火安全区绘图", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:4", "Flag:regex:^(128|512)$"])]
        public void 俄罗斯方块地火安全区绘图(Event @event, ScriptAccessory accessory)
        {
            var dur = 2058;
            var left = @event["Flag"] == "128";
            for (int i = 0; i < 15; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 俄罗斯方块地火安全区绘图 第{i}轮 1";
                dp.Position = new(left ? 87.5f : 112.5f, 0, 60.0f + i * 5);
                dp.Scale = new(5,25);
                dp.Delay = i == 0 ? 0 : 3000 + (i - 1) * dur;
                dp.DestoryAt = i == 0 ? 3000 : dur;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 俄罗斯方块地火安全区绘图 第{i}轮 2";
                dp.Position = new(92.5f, 0, 45.0f + i * 5);
                dp.Scale = new(5, 20);
                dp.Delay = i == 0 ? 0 : 3000 + (i - 1) * dur;
                dp.DestoryAt = i == 0 ? 3000 : dur;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 俄罗斯方块地火安全区绘图 第{i}轮 3";
                dp.Position = new(left ? 102.5f : 97.5f, 0, 60.0f + i * 5);
                dp.Scale = new(5, 25);
                dp.Delay = i == 0 ? 0 : 3000 + (i - 1) * dur;
                dp.DestoryAt = i == 0 ? 3000 : dur;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 俄罗斯方块地火安全区绘图 第{i}轮 4";
                dp.Position = new(107.5f, 0, 45.0f + i * 5);
                dp.Scale = new(5, 20);
                dp.Delay = i == 0 ? 0 : 3000 + (i - 1) * dur;
                dp.DestoryAt = i == 0 ? 3000 : dur;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
            }
        }
        [ScriptMethod(name: "太空步阶段重置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:42847"], userControl: false)]
        public void 太空步阶段重置(Event @event, ScriptAccessory accessory)
        {
            parse = 4;

        }
        [ScriptMethod(name: "太空步Aoe", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42867|42868)$"])]
        public void 太空步Aoe(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"M5s 连续钢铁月环_钢铁";
            dp.Owner = @event.SourceId;
            dp.Scale = new(15, 40);
            dp.Offset = new(@event["ActionId"] == "42867" ? 7.5f : -7.5f, 0, 0);
            dp.DestoryAt = 10500;
            dp.Color = accessory.Data.DefaultDangerColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "太空步 分摊分散", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(42844|42846)$"])]
        public void 太空步_分摊分散(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4) return;
            if (@event["ActionId"] == "42844")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 俄罗斯方块_分摊";
                dp.Owner = @event.TargetId;
                dp.Scale = new(4);
                dp.DestoryAt = 5000;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"M5s 俄罗斯方块_分散";
                dp.Owner = @event.TargetId;
                dp.Scale = new(5);
                dp.DestoryAt = 5000;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }

        }
        

    }
}

