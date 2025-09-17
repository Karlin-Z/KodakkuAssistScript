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
using static Dalamud.Interface.Utility.Raii.ImRaii;
using KodakkuAssist.Module.GameOperate;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;
using KodakkuAssist.Extensions;

namespace KarlinScriptNamespace
{
    [ScriptType(name: "M2s绘图", territorys:[1228],guid: "d99c7e91-9b56-432d-a3a8-49a8586915b7e2a", version:"0.0.0.3", author: "Karlin")]
    public class M2s绘图绘图
    {
        [UserSetting("地面爱心轨迹长度")]
        public float heartLenth { get; set; } = 4;


        int? firstTargetIcon = null;
        int parse;
        bool spreadFlow;

        int beeChargeCount1;
        int[] party2HeartStart = [-1, -1, -1, -1, -1, -1, -1, -1];
        bool party2Circle;

        bool longCircle;
        int TowerCount;
        object towerLock = new();


        public void Init(ScriptAccessory accessory)
        {
            firstTargetIcon = null;
            beeChargeCount1 = 0;
            parse = 0;
            TowerCount = 0;
            //accessory.Method.MarkClear();

        }
        [ScriptMethod(name: "直播1记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39972"], userControl: false)]
        public void 直播1记录(Event @event, ScriptAccessory accessory)
        {
            parse = 1;
        }
        [ScriptMethod(name: "直播2记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39973"],userControl:false)]
        public void 直播2记录(Event @event, ScriptAccessory accessory)
        {
            parse = 2;
            party2HeartStart = [-1, -1, -1, -1, -1, -1, -1, -1];
            party2Circle=false;
        }
        [ScriptMethod(name: "直播3记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39974"], userControl: false)]
        public void 直播3记录(Event @event, ScriptAccessory accessory)
        {
            parse = 3;
            TowerCount = 0;

        }



        //39626 月环 30
        //39625 直线 14,60
        [ScriptMethod(name: "分散分摊收集", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3725[23]|3968[89])$"],userControl:false)]
        public void 分散分摊收集(Event @event, ScriptAccessory accessory)
        {
            //37252 39688 接分散 
            //37253 39689 接分摊
            spreadFlow = (@event["ActionId"] == "37252"|| @event["ActionId"] == "39688");
        }
        [ScriptMethod(name: "直线斩", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37254|39692)$"])]
        public void 直线斩(Event @event, ScriptAccessory accessory)
        {
            
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"直线斩";
            dp.Scale = new(14,40);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt =6200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
        }
        [ScriptMethod(name: "月环", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37255|39693)$"])]
        public void 月环(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"月环";
            dp.Scale = new(20);
            dp.InnerScale = new(6);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.DestoryAt = 6200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        [ScriptMethod(name: "分散分摊", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3725[45]|3969[23])$"])]
        public void 分散分摊(Event @event, ScriptAccessory accessory)
        {
            //37252 接分散 
            //37253 接分摊
            if (spreadFlow)
            {
                foreach (var id in accessory.Data.PartyList)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分散";
                    dp.Scale = new(6);
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Owner = id;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                accessory.Method.TextInfo("分散", 5000);
                accessory.Method.TTS("分散");
            }
            else
            {
                int[] partner = [6, 7, 4, 5, 2, 3, 0, 1];
                var myIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
                var myPartener = partner[myIndex];
                for (int i = 0; i < 4; i++)
                {
                    var id = accessory.Data.PartyList[i];
                    

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分摊";
                    dp.Scale = new(6);
                    dp.Color = (i == myIndex || i == myPartener) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.Owner = id;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                accessory.Method.TextInfo("分摊", 5000);
                accessory.Method.TTS("分摊");
            }
        }
        [ScriptMethod(name: "四四分摊", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37289"])]
        public void 四四分摊(Event @event, ScriptAccessory accessory)
        {
            //37252 接分散 
            //37253 接分摊
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            int[] TNGroup = [0, 1, 2, 3];
            var isMyGroup = TNGroup.Contains(accessory.Data.PartyList.IndexOf(tid)) == TNGroup.Contains(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"地面黄圈";
            dp.Scale = new(6);
            dp.Color = isMyGroup ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "地面黄圈", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(37229|39690)$"])]
        public void 地面黄圈(Event @event, ScriptAccessory accessory)
        {
            //37252 接分散 
            //37253 接分摊
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"地面黄圈";
            dp.Scale = new(8);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }

        [ScriptMethod(name: "三连组合技", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3729[23])$"])]
        public void 三连组合技(Event @event, ScriptAccessory accessory)
        {
            //37292 先月环
            //月环打正 钢铁打斜
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var donutFirst = @event["ActionId"] == "37292";
            var fanRadian = float.Pi / 4;

            #region 月环

            //月环
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"三连组合技-月环";
            dp.Scale = new(20);
            dp.InnerScale = new(7);
            dp.Radian = float.Pi * 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = donutFirst ? 0 : 9000;
            dp.DestoryAt = donutFirst?6000:3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"三连组合技-月环正点扇形1";
            dp.Scale = new(30);
            dp.Radian = fanRadian;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = donutFirst ? 0 : 9000;
            dp.DestoryAt = donutFirst ? 6000 : 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"三连组合技-月环正点扇形2";
            dp.Scale = new(30);
            dp.Radian = fanRadian;
            dp.Rotation = float.Pi / 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = donutFirst ? 0 : 9000;
            dp.DestoryAt = donutFirst ? 6000 : 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"三连组合技-月环正点扇形3";
            dp.Scale = new(30);
            dp.Radian = fanRadian;
            dp.Rotation = float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = donutFirst ? 0 : 9000;
            dp.DestoryAt = donutFirst ? 6000 : 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"三连组合技-月环正点扇形4";
            dp.Scale = new(30);
            dp.Radian = fanRadian;
            dp.Rotation = float.Pi / -2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = donutFirst ? 0 : 9000;
            dp.DestoryAt = donutFirst ? 6000 : 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            #endregion

            #region 钢铁
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"三连组合技-钢铁";
            dp.Scale = new(7);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = donutFirst ? 9000 : 0;
            dp.DestoryAt = donutFirst ? 3000 : 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"三连组合技-钢铁斜点扇形1";
            dp.Scale = new(30);
            dp.Radian = fanRadian;
            dp.Rotation = float.Pi / 4 + 0;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = donutFirst ? 9000 : 0;
            dp.DestoryAt = donutFirst ? 3000 : 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"三连组合技-钢铁斜点扇形2";
            dp.Scale = new(30);
            dp.Radian = fanRadian;
            dp.Rotation = float.Pi / 4 + float.Pi / 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = donutFirst ? 9000 : 0;
            dp.DestoryAt = donutFirst ? 3000 : 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"三连组合技-钢铁斜点扇形3";
            dp.Scale = new(30);
            dp.Radian = fanRadian;
            dp.Rotation = float.Pi / 4 + float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = donutFirst ? 9000 : 0;
            dp.DestoryAt = donutFirst ? 3000 : 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"三连组合技-钢铁斜点扇形4";
            dp.Scale = new(30);
            dp.Radian = fanRadian;
            dp.Rotation = float.Pi / 4 + float.Pi / -2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = donutFirst ? 9000 : 0;
            dp.DestoryAt = donutFirst ? 3000 : 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            #endregion

            #region 十字
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"三连组合技-十字";
            dp.Scale = new(14,40);
            dp.Rotation = 0;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = 6000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"三连组合技-十字";
            dp.Scale = new(14, 40);
            dp.Rotation = float.Pi/2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = 6000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
            #endregion
        }

        [ScriptMethod(name: "一二仇扇形死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37275"])]
        public void 一二仇扇形死刑(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"一仇扇形死刑";
            dp.Scale = new(30);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
            dp.TargetOrderIndex = 1;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"二仇扇形死刑";
            dp.Scale = new(30);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
            dp.TargetOrderIndex = 2;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "一二仇分摊死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37276"])]
        public void 一二仇分摊死刑(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var myIndex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            var dp = accessory.Data.GetDefaultDrawProperties();
            
            dp.Name = $"一二仇分摊死刑 ";
            dp.Scale =new(6);
            dp.Color = (myIndex == 1 || myIndex == 0) ?accessory.Data.DefaultSafeColor: accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.OwnerEnmityOrder;
            dp.CentreOrderIndex = 1;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            
        }
        [ScriptMethod(name: "小蜜蜂点名冲锋", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39629"])]
        public void 小蜜蜂点名冲锋(Event @event, ScriptAccessory accessory)
        {


            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            beeChargeCount1++;
            dp.Name = $"小蜜蜂点名冲锋";
            dp.Scale = new(8, 45);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = beeChargeCount1 > 6 ? 3000 : 0;
            dp.DestoryAt = beeChargeCount1 > 6 ? 4000 : 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "小蜜蜂冲锋", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3962[78])$"])]
        public void 小蜜蜂冲锋(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"小蜜蜂冲锋1";
            dp.Scale = new(10, 45);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        //二运塔
        
        [ScriptMethod(name: "直播1地面爱心", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:16943"])]
        public void 地面爱心(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"爱心{sid}";
            dp.Scale = new(2f, heartLenth);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp.Owner = sid;
            dp.DestoryAt = 40000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "直播1地面爱心删除", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:16943"],userControl:false)]
        public void 地面爱心删除(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            accessory.Method.RemoveDraw($"爱心{sid}");
            
        }
        [ScriptMethod(name: "直播1地面爱心删除2", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:37285"], userControl: false)]
        public void 地面爱心删除2(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            accessory.Method.RemoveDraw($"爱心{sid}");

        }
        [ScriptMethod(name: "直播2爱心Buff收集", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(392[23456])$"],userControl:false)]
        public void 直播2爱心Buff收集(Event @event, ScriptAccessory accessory)
        {
            if(parse!=2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (!int.TryParse(@event["StatusID"], out var statusId)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            if (party2HeartStart[index]==-1) party2HeartStart[index] = statusId - 3922;
        }
        [ScriptMethod(name: "直播2放圈收集", eventType: EventTypeEnum.TargetIcon, userControl: false)]
        public void 直播2放圈收集(Event @event, ScriptAccessory accessory)
        {
            if (ParsTargetIcon(@event["Id"]) != 44 && ParsTargetIcon(@event["Id"]) != 256) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid == accessory.Data.Me)
            {
                party2Circle=true;
            }
        }
        [ScriptMethod(name: "直播2放圈提示", eventType: EventTypeEnum.TargetIcon)]
        public void 直播2放圈提示(Event @event, ScriptAccessory accessory)
        {
            if (ParsTargetIcon(@event["Id"]) != 44 && ParsTargetIcon(@event["Id"]) != 256) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid==accessory.Data.Me)
            {
                accessory.Method.TextInfo("场外放圈", 5000, true);
                accessory.Method.TTS("场外放圈");
            }
        }
        [ScriptMethod(name: "直播2踩塔", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37283"])]
        public void 直播2踩塔(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2) return;
            if (party2Circle) return;
            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (party2HeartStart[index]!=1) return;

            lock (towerLock)
            {
                var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                var dir4 = PositionRoundTo4Dir(pos, new(100, 0, 100));
                var ismy = false;
                if ((dir4 == 0 || dir4 == 3) && index < 4) ismy = true;
                if ((dir4 == 2 || dir4 == 1) && index > 3) ismy = true;
                if (ismy)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"直播2踩塔";
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Scale = new(4);
                    dp.Position = pos;
                    dp.DestoryAt = 8000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"直播2踩塔位置";
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Scale = new(3);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = pos;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.DestoryAt = 8000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


                    accessory.Method.TextInfo("踩塔", 3000);
                    accessory.Method.TTS("踩塔");

                }
            }
        }

        [ScriptMethod(name: "直播3大圈长短收集", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3934"],userControl:false)]
        public void 直播3大圈长短收集(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var during)) return;
            longCircle = during > 30000;
        }
        [ScriptMethod(name: "直播3场边大圈放置位置", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3934"])]
        public void 直播3场边大圈放置位置(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if(tid!=accessory.Data.Me) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var during)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            Vector3 pos = new(100, 0, 81);
            Vector3 centre = new(100, 0, 100);
            if (index == 0 || index == 6) pos = RotatePoint(pos, centre, float.Pi / 2 * 0 - float.Pi / 4);
            if (index == 1 || index == 7) pos = RotatePoint(pos, centre, float.Pi / 2 * 1 - float.Pi / 4);
            if (index == 2 || index == 4) pos = RotatePoint(pos, centre, float.Pi / 2 * 3 - float.Pi / 4);
            if (index == 3 || index == 5) pos = RotatePoint(pos, centre, float.Pi / 2 * 2 - float.Pi / 4);

            var delay= during - 5000;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"场边大圈放置位置";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new(3);
            dp.Owner = tid;
            dp.TargetPosition= pos;
            dp.ScaleMode|=ScaleMode.YByDistance;
            dp.Delay = delay;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            Task.Delay(delay).ContinueWith(t =>
            {
                accessory.Method.TextInfo("场边放圈", 5000, true);
                accessory.Method.TTS("场边放圈");
            });
        }
        [ScriptMethod(name: "直播3场边大圈范围", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3934"])]
        public void 直播3场边大圈范围(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var during)) return;
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"场边大圈放置位置";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(14);
            dp.Owner = tid;
            dp.Delay = during - 3000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        //三运塔
        [ScriptMethod(name: "直播3踩塔", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37283"])]
        public void 直播3踩塔(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            
            lock(towerLock)
            {
                TowerCount++;
                if (longCircle && TowerCount > 4) return;
                if (!longCircle && TowerCount < 5) return;
                var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var dir4= PositionFloorTo4Dir(pos,new(100,0,100));
                var ismy = false;
                if (dir4 == 0 && (index == 1 || index == 7)) ismy = true;
                if (dir4 == 1 && (index == 3 || index == 5)) ismy = true;
                if (dir4 == 2 && (index == 2 || index == 4)) ismy = true;
                if (dir4 == 3 && (index == 0 || index == 6)) ismy = true;
                if(ismy)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"直播3踩塔";
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Scale = new(4);
                    dp.Position = pos;
                    dp.DestoryAt = 8000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"直播3踩塔位置";
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Scale = new(3);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = pos;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Delay = 5000;
                    dp.DestoryAt = 3000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    Task.Delay(5000).ContinueWith(t =>
                    {
                        accessory.Method.TextInfo("踩塔", 3000);
                        accessory.Method.TTS("踩塔");
                    });
                    
                }
            }
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
        private Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
        {

            Vector2 v2 = new(point.X - centre.X, point.Z - centre.Z);

            var rot = (MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian);
            var lenth = v2.Length();
            return new(centre.X + MathF.Sin(rot) * lenth, centre.Y, centre.Z - MathF.Cos(rot) * lenth);
        }

        /// <summary>
        /// 向下取
        /// </summary>
        /// <param name="point"></param>
        /// <param name="centre"></param>
        /// <returns></returns>
        private int PositionFloorTo4Dir(Vector3 point, Vector3 centre)
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r = Math.Floor(2 - 2 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 4;
            return (int)r;

        }

        /// <summary>
        /// 向近的取
        /// </summary>
        /// <param name="point"></param>
        /// <param name="centre"></param>
        /// <returns></returns>
        private int PositionRoundTo4Dir(Vector3 point, Vector3 centre)
        {

            var r = Math.Round(2 - 2 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 4;
            return (int)r;
        }

        /// <summary>
        /// 向近的取
        /// </summary>
        /// <param name="point"></param>
        /// <param name="centre"></param>
        /// <returns></returns>
        private int PositionTo8Dir(Vector3 point, Vector3 centre)
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r = Math.Round(4 - 4 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 8;
            return (int)r;

        }
        private int PositionTo12Dir(Vector3 point, Vector3 centre)
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r = Math.Round(6 - 6 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 12;
            return (int)r;

        }
    }
}

