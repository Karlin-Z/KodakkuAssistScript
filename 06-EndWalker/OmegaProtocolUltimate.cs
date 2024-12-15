using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommons;
using System.Numerics;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using System.Xml.Linq;
using Dalamud.Utility.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.GameFunctions;

namespace MyScriptNamespace
{
    
    [ScriptType(name: "OmegaProtocolUltimate", territorys: [1122],guid: "625eb340-0811-4c37-b87c-c46fe5204940", version:"0.0.0.2",note: noteStr)]
    public class OmegaProtocolUltimate
    {
        const string noteStr =
        """
        欧米茄验证绝境战
        """;
        List<int> HtdhParty = [2, 0, 1, 4, 5, 6, 7, 3];
        double parse = 0;

        uint P1_BossId = 0;
        List<int> P1_点名Buff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<Vector3> P1_TowerPos = [];
        DateTime P1_TowerTime= DateTime.MinValue;
        DateTime P1_FanTime = DateTime.MinValue;
        int P1_LineRound = 0;
        int P1_FireCount = 0;

        bool P2_PTBuffIsFar=false;
        List<int> P2_Sony= [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P2_Stack = [0, 0, 0, 0, 0, 0, 0, 0];
        Dictionary<uint,uint> P2_刀光剑舞连线 = [];

        public void Init(ScriptAccessory accessory)
        {
            parse = 0;
        }
        #region P1
        [ScriptMethod(name: "P1_循环程序_分P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31491"],userControl:false)]
        public void P1_循环程序_分P(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            P1_BossId=tid;
            parse = 1.1;
            P1_TowerPos = [];
            P1_LineRound = 0;
        }
        [ScriptMethod(name: "P1_循环程序_Buff记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3004|3005|3006|3451)$"],userControl:false)]
        public void P1_循环程序_Buff记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            P1_点名Buff[index] = @event["StatusID"] switch
            {
                "3004" => 1,
                "3005" => 2,
                "3006" => 3,
                "3451" => 4,
                _=>0
            };
        }
        [ScriptMethod(name: "P1_循环程序_塔收集", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add", "DataId:2013245"], userControl: false)]
        public void P1_循环程序_塔收集(Event @event, ScriptAccessory accessory)
        {
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            lock (P1_TowerPos)
            {
                P1_TowerPos.Add(pos);
            }
        }
        [ScriptMethod(name: "P1_循环程序_集合提醒", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31491"])]
        public void P1_循环程序_集合提醒(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            accessory.Method.TextInfo("Boss背后集合", 2000);
            accessory.Method.TTS("Boss背后集合");
        }
        [ScriptMethod(name: "P1_循环程序_开始站位提醒", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3004|3005|3006|3451)$"])]
        public void P1_循环程序_开始站位提醒(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            if (@event["StatusID"]=="3006")
            {
                accessory.Method.TextInfo("靠前接线", 3000);
                accessory.Method.TTS("靠前接线");
            }
            else
            {
                accessory.Method.TextInfo("靠后", 3000);
                accessory.Method.TTS("靠后");
            }
           
        }
        [ScriptMethod(name: "P1_循环程序_线塔处理位置", eventType: EventTypeEnum.ObjectChanged, eventCondition: ["Operate:Add", "DataId:2013245"])]
        public async void P1_循环程序_线塔处理位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            lock (this)
            {
                if ((DateTime.Now - P1_TowerTime).TotalSeconds < 2) return;
                P1_TowerTime=DateTime.Now;
            }
            await Task.Delay(50);
            Vector3 centre = new(100, 0, 100);
            var towerCount = P1_TowerPos.Count;
            List<int> HtdhParty = [2, 0, 1, 4, 5, 6, 7, 3];
            var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var myBuff = P1_点名Buff[myindex];
            var index1 = P1_点名Buff.IndexOf(myBuff);
            var index2 = P1_点名Buff.LastIndexOf(myBuff);
            var hIndex = HtdhParty.IndexOf(index1) < HtdhParty.IndexOf(index2) ? index1 : index2;
            var meIsHigh = hIndex == myindex;
            var idle = false;
            //塔
            if (towerCount == myBuff * 2)
            {
                idle=true;
                var hPos=default(Vector3);
                var lPos=default(Vector3);
                if (PositionTo4Dir(P1_TowerPos[towerCount - 2], centre) < PositionTo4Dir(P1_TowerPos[towerCount - 1], centre))
                {
                    hPos = P1_TowerPos[towerCount - 2];
                    lPos = P1_TowerPos[towerCount - 1];
                }
                else
                {
                    hPos = P1_TowerPos[towerCount - 1];
                    lPos = P1_TowerPos[towerCount - 2];
                }
                var dealpos = meIsHigh?hPos:lPos;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_循环程序_塔站位";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_循环程序_塔范围";
                dp.Scale = new(3);
                dp.Position = dealpos;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            //线
            if (towerCount % 8 == (myBuff + 2) * 2 % 8)
            {
                
                idle = true;
                List<int> isTower = [0, 0, 0, 0];
                isTower[PositionTo4Dir(P1_TowerPos[towerCount - 2], centre)] = 1;
                isTower[PositionTo4Dir(P1_TowerPos[towerCount - 1], centre)] = 1;
                var my4Dir = meIsHigh ? isTower.IndexOf(0) : isTower.LastIndexOf(0);
                var dealpos = RotatePoint(new(100, 0, 85), centre, float.Pi / 2 * my4Dir);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_循环程序_线站位";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            //闲
            if (!idle)
            {
                //北点 100,0,86
                var myPos = accessory.Data.Objects.SearchByEntityId(accessory.Data.Me)?.Position??default;
                var drot = (myPos - P1_TowerPos[towerCount - 2]).Length() < (myPos - P1_TowerPos[towerCount - 1]).Length() ? PositionTo4Dir(P1_TowerPos[towerCount - 2],centre) : PositionTo4Dir(P1_TowerPos[towerCount - 1], centre);
                var dealpos=RotatePoint(new(100,0,86),centre,float.Pi/2*drot);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_循环程序_闲站位";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        [ScriptMethod(name: "P1_循环程序_接线标记", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31496", "TargetIndex:1"])]
        public void P1_循环程序_接线标记(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            P1_LineRound++;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var waitBuff = (P1_LineRound + 1) % 4 + 1;
            var catchBuff = (P1_LineRound + 2) % 4 + 1;
            var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (catchBuff != P1_点名Buff[myindex]) return;

            
            var myBuff = P1_点名Buff[myindex];
            var index1 = P1_点名Buff.IndexOf(myBuff);
            var index2 = P1_点名Buff.LastIndexOf(myBuff);
            var hIndex = HtdhParty.IndexOf(index1) < HtdhParty.IndexOf(index2) ? index1 : index2;
            var meIsHigh = hIndex == myindex;

            var index3 = P1_点名Buff.IndexOf(waitBuff);
            var index4 = P1_点名Buff.LastIndexOf(waitBuff);
            var hWaitIndex = HtdhParty.IndexOf(index3) < HtdhParty.IndexOf(index4) ? index3 : index4;
            var lWaitIndex = HtdhParty.IndexOf(index3) < HtdhParty.IndexOf(index4) ? index4 : index3;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_循环程序_接线标记";
            dp.Scale = new(10);
            dp.Owner = tid;
            dp.TargetObject = meIsHigh? accessory.Data.PartyList[hWaitIndex]: accessory.Data.PartyList[lWaitIndex];
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = new(1,1,0,1);
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
        }
        [ScriptMethod(name: "P1_循环程序_接线标记移除", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0059"])]
        public void P1_循环程序_接线标记移除(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (sid != accessory.Data.Me) return;
            accessory.Method.RemoveDraw("P1_循环程序_接线标记");
        }

        [ScriptMethod(name: "P1_全能之主_分P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31499"], userControl: false)]
        public void P1_全能之主_分P(Event @event, ScriptAccessory accessory)
        {
            parse = 1.2;
            P1_FireCount = 0;
        }
        [ScriptMethod(name: "P1_全能之主_Buff记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3004|3005|3006|3451)$"], userControl: false)]
        public void P1_全能之主_Buff记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            P1_点名Buff[index] = @event["StatusID"] switch
            {
                "3004" => 1,
                "3005" => 2,
                "3006" => 3,
                "3451" => 4,
                _ => 0
            };
        }
        [ScriptMethod(name: "P1_全能之主_高低顺位播报", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31499"])]
        public async void P1_全能之主_高低顺位播报(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            await Task.Delay(100);
            var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var mybuff = P1_点名Buff[myindex];
            var i1 = P1_点名Buff.IndexOf(mybuff);
            var i2 = P1_点名Buff.LastIndexOf(mybuff);
            var hIndex = HtdhParty.IndexOf(i1) < HtdhParty.IndexOf(i2) ? i1 : i2;
            if (hIndex== myindex)
            {
                accessory.Method.TextInfo("高顺位(上右)", 10000);
                accessory.Method.TTS("高顺位(上右)");
            }
            else
            {
                accessory.Method.TextInfo("低顺位(下左)", 10000);
                accessory.Method.TTS("低顺位(下左)");
            }
        }
        [ScriptMethod(name: "P1_全能之主_单点命中播报", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31502"])]
        public async void P1_全能之主_单点命中播报(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid == accessory.Data.Me) 
            {
                accessory.Method.TextInfo("回头", 2000);
                accessory.Method.TTS("回头");
            }
        }
        [ScriptMethod(name: "P1_全能之主_分摊范围", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(350[789]|3510)$"])]
        public void P1_全能之主_分摊范围(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_全能之主_分摊范围";
            dp.Scale = new(6,30);
            dp.Owner = P1_BossId;
            dp.TargetObject = tid;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = dur - 3000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P1_全能之主_最远距离顺劈", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
        public void P1_全能之主_最远距离顺劈(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            lock (this)
            {
                if ((DateTime.Now - P1_FanTime).TotalSeconds < 20) return;
                P1_FanTime = DateTime.Now;
            }
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_全能之主_最远距离顺劈1";
            dp.Scale = new(20);
            dp.Radian = float.Pi / 3 * 2;
            dp.Owner = P1_BossId;
            dp.TargetResolvePattern=PositionResolvePatternEnum.PlayerFarestOrder;
            dp.TargetOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 13000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_全能之主_最远距离顺劈2";
            dp.Scale = new(20);
            dp.Radian = float.Pi / 3 * 2;
            dp.Owner = P1_BossId;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.TargetOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 13000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }
        [ScriptMethod(name: "P1_全能之主_点名直线", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0017"])]
        public void P1_全能之主_点名直线(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_全能之主_点名直线";
            dp.Scale = new(6,50);
            dp.TargetObject = tid;
            dp.Owner = P1_BossId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P1_全能之主_T引导顺劈位置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32368"])]
        public void P1_全能之主_T引导顺劈位置(Event @event, ScriptAccessory accessory)
        {
            lock (this)
            {
                P1_FireCount++;
                if (P1_FireCount != 26) return;
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                if (myindex != 0 && myindex != 1) return;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_全能之主_T引导顺劈位置";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(100,0,86);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 11000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        #endregion

        #region P2
        [ScriptMethod(name: "P2_协同程序PT_分P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31550"], userControl: false)]
        public void P2_协同程序PT_分P(Event @event, ScriptAccessory accessory)
        {
            parse = 2.1;
            P2_Stack = [];
        }
        [ScriptMethod(name: "P2_协同程序PT_Buff记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3427|3428)$"], userControl: false)]
        public void P2_协同程序PT_Buff记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            P2_PTBuffIsFar = @event["StatusID"] == "3428";
        }
        [ScriptMethod(name: "P2_协同程序PT_索尼记录", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:regex:^(01A[0123])$"], userControl: false)]
        public void P2_协同程序PT_索尼记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            lock (P2_Sony)
            {
                P2_Sony[accessory.Data.PartyList.IndexOf(tid)] = @event["Id"] switch
                {
                    "01A0" => 1,
                    "01A1" => 3,
                    "01A2" => 4,
                    "01A3" => 2,
                    _ => 0
                };
            }
            
        }
        [ScriptMethod(name: "P2_协同程序PT_男女人AOE", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:7747", "SourceDataId:regex:^(15714|15715)$"])]
        public void P2_协同程序PT_男女人AOE(Event @event, ScriptAccessory accessory)
        {
            // 15714 男
            // 15715 女
            //男人剑 0 盾 4
            //女人标杆 0 脚刀 4
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            Vector3 centre = new(100, 0, 100);
            if ((pos - centre).Length() > 12) return;
            var c = accessory.Data.Objects.SearchById(sid);
            if (c == null) return;
            var transformationID = ((IBattleChara)c).GetTransformationID();
            if (@event["SourceDataId"] == "15714")
            {
                //男
                if (transformationID == 0)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_协同程序PT_男钢铁";
                    dp.Scale = new(10);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (transformationID == 4)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_协同程序PT_男月环";
                    dp.Scale = new(40);
                    dp.InnerScale = new(10);
                    dp.Radian = float.Pi * 2;
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                }
            }
            if (@event["SourceDataId"] == "15715")
            {
                if (transformationID == 0)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_协同程序PT_女十字1";
                    dp.Scale = new(10, 60);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_协同程序PT_女十字1";
                    dp.Scale = new(10, 60);
                    dp.Rotation = float.Pi / 2;
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                }
                if (transformationID == 4)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_协同程序PT_女辣翅1";
                    dp.Scale = new(60, 20);
                    dp.Owner = sid;
                    dp.Rotation = float.Pi / 2;
                    dp.Offset = new(-5, 0, 0);
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_协同程序PT_女辣翅2";
                    dp.Scale = new(60, 20);
                    dp.Owner = sid;
                    dp.Rotation = float.Pi / -2;
                    dp.Offset = new(5, 0, 0);
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
                }
            }
        }
        [ScriptMethod(name: "P2_协同程序PT_眼睛激光", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375AC", "Id:00020001"])]
        public void P2_协同程序PT_眼睛激光(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var rot = @event["Index"] switch
            {
                "00000001" => 0,
                "00000002" => 1,
                "00000003" => 2,
                "00000004" => 3,
                "00000005" => 4,
                "00000006" => 5,
                "00000007" => 6,
                "00000008" => 7,
                _ => -1
            };
            if (rot == -1) return;
            var pos = RotatePoint(new(100, 0, 80), new(100, 0, 100), float.Pi / 4 * rot);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_协同程序PT_眼睛激光";
            dp.Scale = new(16,40);
            dp.Position = pos;
            dp.TargetPosition = new(100, 0, 100);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7500;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

          
        }
        [ScriptMethod(name: "P2_协同程序PT_五钢铁", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31521", "TargetIndex:1"])]
        public void P2_协同程序PT_五钢铁(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            foreach (var c in accessory.Data.Objects)
            {
                if(c.DataId== 15714 || c.DataId == 15713)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_协同程序PT_五钢铁";
                    dp.Scale = new(10);
                    dp.Owner = c.EntityId;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 11000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
        }
        [ScriptMethod(name: "P2_协同程序PT_眼睛激光索尼处理位置", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375AC", "Id:00020001"])]
        public async void P2_协同程序PT_眼睛激光索尼处理位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dir = @event["Index"] switch
            {
                "00000001" => 0,
                "00000002" => 1,
                "00000003" => 2,
                "00000004" => 3,
                "00000005" => 4,
                "00000006" => 5,
                "00000007" => 6,
                "00000008" => 7,
                _ => -1
            };
            if (dir == -1) return;
            await Task.Delay(3000);
            Vector3 centre = new(100, 0, 100);

            Vector3 middleLeft1Pos =  RotatePoint(new(088.5f, 0, 085.5f), centre, float.Pi / 4 * dir);
            Vector3 middleRight1Pos = RotatePoint(new(111.5f, 0, 085.5f), centre, float.Pi / 4 * dir);
            Vector3 middleLeft2Pos = RotatePoint(new(088.5f, 0, 095.0f), centre, float.Pi / 4 * dir);
            Vector3 middleRight2Pos = RotatePoint(new(111.5f, 0, 095.0f), centre, float.Pi / 4 * dir);
            Vector3 middleLeft3Pos = RotatePoint(new(088.5f, 0, 105.0f), centre, float.Pi / 4 * dir);
            Vector3 middleRight3Pos = RotatePoint(new(111.5f, 0, 105.0f), centre, float.Pi / 4 * dir);
            Vector3 middleLeft4Pos = RotatePoint(new(088.5f, 0, 114.5f), centre, float.Pi / 4 * dir);
            Vector3 middleRight4Pos = RotatePoint(new(111.5f, 0, 114.5f), centre, float.Pi / 4 * dir);

            Vector3 farLeft1Pos = RotatePoint(new(091.5f, 0, 083.0f), centre, float.Pi / 4 * dir);
            Vector3 farRight1Pos = RotatePoint(new(108.5f, 0, 117.0f), centre, float.Pi / 4 * dir);
            Vector3 farLeft2Pos = RotatePoint(new(082.0f, 0, 093.0f), centre, float.Pi / 4 * dir);
            Vector3 farRight2Pos = RotatePoint(new(118.0f, 0, 107.0f), centre, float.Pi / 4 * dir);
            Vector3 farLeft3Pos = RotatePoint(new(082.0f, 0, 107.0f), centre, float.Pi / 4 * dir);
            Vector3 farRight3Pos = RotatePoint(new(118.0f, 0, 093.0f), centre, float.Pi / 4 * dir);
            Vector3 farLeft4Pos = RotatePoint(new(091.5f, 0, 117.0f), centre, float.Pi / 4 * dir);
            Vector3 farRight4Pos = RotatePoint(new(108.5f, 0, 083.0f), centre, float.Pi / 4 * dir);

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var mySony = P2_Sony[myindex];
            var myPartnerIndex = P2_Sony.IndexOf(mySony) == myindex ? P2_Sony.LastIndexOf(mySony) : P2_Sony.IndexOf(mySony);
            var meIsHigh = HtdhParty.IndexOf(myindex) < HtdhParty.IndexOf(myPartnerIndex);
            Vector3 dealpos = mySony switch
            {
                1 => P2_PTBuffIsFar ? (meIsHigh ? farLeft1Pos : farRight1Pos) : (meIsHigh ? middleLeft1Pos : middleRight1Pos),
                2 => P2_PTBuffIsFar ? (meIsHigh ? farLeft2Pos : farRight2Pos) : (meIsHigh ? middleLeft2Pos : middleRight2Pos),
                3 => P2_PTBuffIsFar ? (meIsHigh ? farLeft3Pos : farRight3Pos) : (meIsHigh ? middleLeft3Pos : middleRight3Pos),
                4 => P2_PTBuffIsFar ? (meIsHigh ? farLeft4Pos : farRight4Pos) : (meIsHigh ? middleLeft4Pos : middleRight4Pos),
                _ => default
            };
            if (dealpos == default) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_协同程序PT_眼睛激光索尼处理位置";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P2_协同程序PT_分摊处理位置", eventType: EventTypeEnum.TargetIcon, eventCondition: ["Id:0064"])]
        public void P2_协同程序PT_分摊处理位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            lock (P2_Stack)
            {
                P2_Stack.Add(accessory.Data.PartyList.IndexOf(tid));
                if (P2_Stack.Count != 2) return;
            }
            
            var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            List<int> leftGroup = [];
            leftGroup.Add(HtdhParty.IndexOf(P2_Sony.IndexOf(1)) < HtdhParty.IndexOf(P2_Sony.LastIndexOf(1)) ? P2_Sony.IndexOf(1) : P2_Sony.LastIndexOf(1));
            leftGroup.Add(HtdhParty.IndexOf(P2_Sony.IndexOf(2)) < HtdhParty.IndexOf(P2_Sony.LastIndexOf(2)) ? P2_Sony.IndexOf(2) : P2_Sony.LastIndexOf(2));
            leftGroup.Add(HtdhParty.IndexOf(P2_Sony.IndexOf(3)) < HtdhParty.IndexOf(P2_Sony.LastIndexOf(3)) ? P2_Sony.IndexOf(3) : P2_Sony.LastIndexOf(3));
            leftGroup.Add(HtdhParty.IndexOf(P2_Sony.IndexOf(4)) < HtdhParty.IndexOf(P2_Sony.LastIndexOf(4)) ? P2_Sony.IndexOf(4) : P2_Sony.LastIndexOf(4));
            
            //左边两个分摊
            if (leftGroup.Contains(P2_Stack[0]) && leftGroup.Contains(P2_Stack[1]))
            {
                
                var lowStackIndex = P2_Sony[P2_Stack[0]] < P2_Sony[P2_Stack[1]] ? P2_Stack[1] : P2_Stack[0];
                var lowStackSony = P2_Sony[lowStackIndex];
                var lowStackPartnerIndex = P2_Sony.IndexOf(lowStackSony) == lowStackIndex ? P2_Sony.LastIndexOf(lowStackSony) : P2_Sony.IndexOf(lowStackSony);
                leftGroup.Remove(lowStackIndex);
                leftGroup.Add(lowStackPartnerIndex);
            }
            //边两个分摊
            if (!leftGroup.Contains(P2_Stack[0]) && !leftGroup.Contains(P2_Stack[1]))
            {
                var lowStackIndex = P2_Sony[P2_Stack[0]] < P2_Sony[P2_Stack[1]] ? P2_Stack[1] : P2_Stack[0];
                var lowStackSony = P2_Sony[P2_Stack[0]] < P2_Sony[P2_Stack[1]] ? P2_Sony[P2_Stack[1]] : P2_Sony[P2_Stack[0]];
                var lowStackPartnerIndex = P2_Sony.IndexOf(lowStackSony) == lowStackIndex ? P2_Sony.LastIndexOf(lowStackSony) : P2_Sony.IndexOf(lowStackSony);
                leftGroup.Remove(lowStackPartnerIndex);
                leftGroup.Add(lowStackIndex);
            }
            
            Vector3 dealpos = default;
            if (P2_PTBuffIsFar)
            {
                dealpos = leftGroup.Contains(myindex) ? new(94, 0, 100) : new(106, 0, 100);
            }
            else
            {
                dealpos = leftGroup.Contains(myindex) ? new(97, 0, 100) : new(100, 0, 103);
            }
            var c = accessory.Data.Objects.Where(o => o.DataId == 15713).FirstOrDefault();
            if (c == null) return;
            var dir4 = PositionTo4Dir(c!.Position, new(100, 0, 100));

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_协同程序PT_分摊处理位置";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = RotatePoint(dealpos, new(100, 0, 100), float.Pi / 2 / dir4);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "P2_协同程序LB_分P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31544"], userControl: false)]
        public void P2_协同程序LB_分P(Event @event, ScriptAccessory accessory)
        {
            parse = 2.2;
            P2_刀光剑舞连线 = [];
        }
        [ScriptMethod(name: "P2_协同程序LB_射手天剑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31539"])]
        public void P2_协同程序LB_射手天剑(Event @event, ScriptAccessory accessory)
        {
            if(parse != 2.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_协同程序LB_射手天剑";
            dp.Scale = new(10,42);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 500;
            dp.DestoryAt = 7500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2_协同程序LB_刀光剑舞", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0054"])]
        public void P2_协同程序LB_刀光剑舞(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            lock (P2_刀光剑舞连线)
            {
                if (P2_刀光剑舞连线.ContainsKey(sid))
                {
                    accessory.Method.RemoveDraw($"P2_协同程序LB_刀光剑舞-{sid}-{P2_刀光剑舞连线[sid]}");
                    P2_刀光剑舞连线[sid] = tid;
                }
                else
                {
                    P2_刀光剑舞连线.Add(sid, tid);
                }

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P2_协同程序LB_刀光剑舞-{sid}-{tid}";
                dp.Scale = new(40);
                dp.Owner = sid;
                dp.TargetObject = tid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 15000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
            
        }
        [ScriptMethod(name: "P2_协同程序LB_刀光剑舞移除", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31539"])]
        public void P2_协同程序LB_刀光剑舞移除(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            foreach (var item in P2_刀光剑舞连线)
            {
                accessory.Method.RemoveDraw($"P2_协同程序LB_刀光剑舞-{item.Key}-{item.Value}");
            }
        }
        [ScriptMethod(name: "P2_协同程序LB_盾连击S", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31527"])]
        public void P2_协同程序LB_盾连击S(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P2_协同程序LB_盾连击S-1-1";
            dp.Scale = new(5);
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P2_协同程序LB_盾连击S-1-2";
            dp.Scale = new(5);
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P2_协同程序LB_盾连击S-2";
            dp.Scale = new(6);
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 5200;
            dp.DestoryAt = 2800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P2_协同程序LB_盾连击S命中提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31528"])]
        public void P2_协同程序LB_盾连击S命中提示(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (accessory.Data.Me != tid) return;
            accessory.Method.TextInfo("出去出去",3000);
            accessory.Method.TTS("出去出去");
        }


        [ScriptMethod(name: "P2_协同程序LB_射手天剑引导位置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31544"])]
        public void P2_协同程序LB_射手天剑引导位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_协同程序LB_射手天剑引导位置";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = new(100,0,94.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P2_协同程序LB_盾连击S_男人位置连线", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:32369"])]
        public void P2_协同程序LB_盾连击S_男人位置连线(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myindex != 0 && myindex != 1) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_协同程序LB_盾连击S_男人位置连线";
            dp.Scale = new(5);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 8000;
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
        }
        [ScriptMethod(name: "P2_协同程序LB_盾连击S二段处理位置", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31528"])]
        public void P2_协同程序LB_盾连击S二段处理位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            Vector3 dealpos = new(100, 0, 100);
            if (accessory.Data.Me == tid) 
            {
                var pos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
                dealpos = RotatePoint(pos, new(100, 0, 100), float.Pi/2);
            }
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_协同程序LB_盾连击S二段处理位置";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 2800;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        #endregion


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
        private int PositionTo4Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Round(2 - 2 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 4;
            return (int)r;
        }
        private Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
        {

            Vector2 v2 = new(point.X - centre.X, point.Z - centre.Z);

            var rot = (MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian);
            var lenth = v2.Length();
            return new(centre.X + MathF.Sin(rot) * lenth, centre.Y, centre.Z - MathF.Cos(rot) * lenth);
        }
    }
}

