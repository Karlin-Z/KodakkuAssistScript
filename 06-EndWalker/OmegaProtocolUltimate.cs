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
    
    [ScriptType(name: "OmegaProtocolUltimate", territorys: [1122],guid: "625eb340-0811-4c37-b87c-c46fe5204940", version:"0.0.0.3",note: noteStr)]
    public class OmegaProtocolUltimate
    {
        const string noteStr =
        """
        欧米茄验证绝境战
        """;

        [UserSetting("P3_开场排队顺序")]
        public P3SortEnum P3_StackSort { get; set; }
        [UserSetting("P3_小电视打法")]
        public P3TVEnum P3_TV_Strategy { get; set; }


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

        int P3_ArmCount = 0;
        List <int> P3_StartBuff= [0, 0, 0, 0, 0, 0, 0, 0];
        bool P3_StartPreDone = false;
        bool P3_StartDone=false;
        List<int> P3_TVBuff = [0, 0, 0, 0, 0, 0, 0, 0];

        List<int> P4Stack = [];

        public enum P3SortEnum
        {
            HTDH,
            THD
        }
        public enum P3TVEnum
        {
            Normal,
            Static
        }

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
            //右边两个分摊
            if (!leftGroup.Contains(P2_Stack[0]) && !leftGroup.Contains(P2_Stack[1]))
            {
                if (P2_PTBuffIsFar)
                {
                    var lowStackIndex = P2_Sony[P2_Stack[0]] < P2_Sony[P2_Stack[1]] ? P2_Stack[0] : P2_Stack[1];
                    var lowStackSony = P2_Sony[lowStackIndex];
                    var lowStackPartnerIndex = P2_Sony.IndexOf(lowStackSony) == lowStackIndex ? P2_Sony.LastIndexOf(lowStackSony) : P2_Sony.IndexOf(lowStackSony);
                    leftGroup.Remove(lowStackPartnerIndex);
                    leftGroup.Add(lowStackIndex);
                }
                else
                {
                    var lowStackIndex = P2_Sony[P2_Stack[0]] < P2_Sony[P2_Stack[1]] ? P2_Stack[1] : P2_Stack[0];
                    var lowStackSony = P2_Sony[lowStackIndex];
                    var lowStackPartnerIndex = P2_Sony.IndexOf(lowStackSony) == lowStackIndex ? P2_Sony.LastIndexOf(lowStackSony) : P2_Sony.IndexOf(lowStackSony);
                    leftGroup.Remove(lowStackPartnerIndex);
                    leftGroup.Add(lowStackIndex);
                }
                
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
            var dir8 = PositionTo8Dir(c!.Position, new(100, 0, 100));
            //accessory.Log.Debug($"P2_协同程序PT {dir8} {leftGroup.Contains(myindex)}");
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_协同程序PT_分摊处理位置";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = RotatePoint(dealpos, new(100, 0, 100), float.Pi / 4 * dir8);
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

        #region P3

        [ScriptMethod(name: "P3_开场_分P", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31507"], userControl: false)]
        public void P3_开场_分P(Event @event, ScriptAccessory accessory)
        {
            parse = 3.0;
            P3_ArmCount = 0;
            P3_StartBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            P3_StartDone = false;
            P3_StartPreDone = false;
            P3_TVBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        }
        [ScriptMethod(name: "P3_开场_Buff收集", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3425|3426)$"], userControl: false)]
        public void P3_开场_Buff收集(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            if (index == -1) return;
            lock (P3_StartBuff)
            {
                //1分散 2分摊
                P3_StartBuff[index] = @event["StatusID"] == "3425" ? 1 : 2;
            }
        }
        [ScriptMethod(name: "P3_小电视_Buff收集", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(3452|3453)$"], userControl: false)]
        public void P3_小电视_Buff收集(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            if (index == -1) return;
            lock (P3_TVBuff)
            {
                P3_TVBuff[index] = @event["StatusID"] == "3452" ? 1 : 2;
            }
        }
        [ScriptMethod(name: "P3_开场_手臂AOE", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:regex:^(774[78])$", "SourceDataId:regex:^(1571[89])$"], userControl: false)]
        public void P3_开场_手臂AOE(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            lock (this)
            {
                P3_ArmCount++;
                if (!ParseObjectId(@event["SourceId"], out var sid)) return;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_开场_手臂AOE";
                dp.Scale = new(11);
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay= P3_ArmCount > 3 ? 11000 : 0;
                dp.DestoryAt = P3_ArmCount > 3 ? 2500 : 14000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }
        [ScriptMethod(name: "P3_开场_地震", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31567"])]
        public void P3_开场_地震(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_开场_地震_1";
            dp.Scale = new(6);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 0;
            dp.DestoryAt = 4800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_开场_地震_2";
            dp.InnerScale = new(6);
            dp.Scale = new(12);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 4800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_开场_地震_3";
            dp.InnerScale = new(12);
            dp.Scale = new(18);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_开场_地震_4";
            dp.InnerScale = new(18);
            dp.Scale = new(24);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 8800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        [ScriptMethod(name: "P3_小电视_自身AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3159[56])$"])]
        public void P3_小电视_自身AOE(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_小电视_自身AOE";
            dp.Scale = new(7);
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P3_开场_Buff预站位处理位置", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:3426"])]
        public async void P3_开场_Buff预站位处理位置(Event @event, ScriptAccessory accessory)
        {
            lock (this)
            {
                if(P3_StartPreDone) return;
                P3_StartPreDone = true;
            }
            await Task.Delay(100);
            List<int> sortOrder = P3_StackSort switch
            {
                P3SortEnum.HTDH => HtdhParty,
                P3SortEnum.THD => [0, 1, 2, 3, 4, 5, 6, 7],
                _ => [0, 1, 2, 3, 4, 5, 6, 7],
            };
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            //1分散 2分摊
            var myP3StartBuff = P3_StartBuff[myindex];
            var myP3Index = 0;
            for (int i = 0; i < sortOrder.Count; i++)
            {

                var index = sortOrder[i];
                if (myP3StartBuff == P3_StartBuff[index]) myP3Index++;
                //accessory.Log.Debug($"{myindex} {index} {myP3StartBuff} {P3_StartBuff[index]} {myP3Index}");
                if (index == myindex) break;
            }
            Vector3 dealpos = default;
            if (myP3StartBuff == 2 || myP3StartBuff == 0)
            {
                dealpos = myP3Index switch
                {
                    1 =>  new(092.00f, 0, 086.14f),
                    2 =>  new(108.00f, 0, 086.14f),
                    _ => default,
                };
            }
            if (myP3StartBuff == 1)
            {
                dealpos = myP3Index switch
                {
                    1 => new(084.00f, 0, 100.00f),
                    2 => new(092.00f, 0, 113.86f),
                    3 => new(108.00f, 0, 113.86f),
                    4 => new(116.00f, 0, 100.00f),
                    _ => default,
                };
            }
            if (dealpos == default) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_开场_Buff预站位处理位置";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 3100;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P3_开场_处理位置", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:regex:^(774[78])$", "SourceDataId:regex:^(1571[89])$"])]
        public void P3_开场_处理位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3.0) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (MathF.Abs(pos.X - 100) > 1) return;
            if (P3_StartDone) return;
            P3_StartDone = true;

            var northCirle = pos.Z < 100;

            List<int> sortOrder = P3_StackSort switch
            {
                P3SortEnum.HTDH => HtdhParty,
                P3SortEnum.THD => [0, 1, 2, 3, 4, 5, 6, 7],
                _ => [0, 1, 2, 3, 4, 5, 6, 7],
            };
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            //1分散 2分摊
            var myP3StartBuff = P3_StartBuff[myindex];
            var myP3Index = 0;
            for (int i = 0; i < sortOrder.Count; i++)
            {
                
                var index = sortOrder[i];
                if (myP3StartBuff == P3_StartBuff[index]) myP3Index++;
                //accessory.Log.Debug($"{myindex} {index} {myP3StartBuff} {P3_StartBuff[index]} {myP3Index}");
                if (index == myindex) break;
            }
            
            Vector3 dealpos1 = default;
            Vector3 dealpos2 = default;
            Vector3 dealpos3 = default;
            Vector3 dealpos4 = default;
            if (myP3StartBuff == 2 || myP3StartBuff == 0)
            {
                dealpos1 = myP3Index switch
                {
                    1 => northCirle ? new(086.7f, 0, 086.7f) : new(094.8f, 0, 082.0f),
                    2 => northCirle ? new(113.3f, 0, 086.7f) : new(105.2f, 0, 082.0f),
                    _ => default,
                };
                dealpos2 = myP3Index switch
                {
                    1 => northCirle ? new(087.8f, 0, 087.8f) : new(095.0f, 0, 083.5f),
                    2 => northCirle ? new(112.2f, 0, 087.8f) : new(105.0f, 0, 083.5f),
                    _ => default,
                };
                dealpos3 = myP3Index switch
                {
                    1 => northCirle ? new(088.4f, 0, 085.5f) : new(093.1f, 0, 082.8f),
                    2 => northCirle ? new(111.6f, 0, 085.5f) : new(106.9f, 0, 082.8f),
                    _ => default,
                };
                dealpos4 = myP3Index switch
                {
                    1 => northCirle ? new(094.7f, 0, 083.7f) : new(088.5f, 0, 087.0f),
                    2 => northCirle ? new(105.3f, 0, 083.7f) : new(111.5f, 0, 087.0f),
                    _ => default,
                };
            }
            if (myP3StartBuff == 1)
            {
                dealpos1 = myP3Index switch
                {
                    1 => northCirle ? new(082.0f, 0, 095.0f) : new(082.0f, 0, 104.7f),
                    2 => northCirle ? new(095.0f, 0, 118.0f) : new(086.5f, 0, 113.0f),
                    3 => northCirle ? new(105.0f, 0, 118.0f) : new(113.5f, 0, 113.0f),
                    4 => northCirle ? new(118.0f, 0, 095.0f) : new(118.0f, 0, 104.7f),
                    _ => default,
                };
                dealpos2 = myP3Index switch
                {
                    1 => northCirle ? new(083.5f, 0, 095.5f) : new(083.5f, 0, 104.5f),
                    2 => northCirle ? new(095.0f, 0, 116.5f) : new(088.0f, 0, 112.0f),
                    3 => northCirle ? new(105.0f, 0, 116.5f) : new(112.0f, 0, 112.0f),
                    4 => northCirle ? new(116.5f, 0, 095.5f) : new(116.5f, 0, 104.5f),
                    _ => default,
                };
                dealpos3 = myP3Index switch
                {
                    1 => northCirle ? new(081.7f, 0, 097.2f) : new(081.6f, 0, 102.8f),
                    2 => northCirle ? new(093.2f, 0, 117.2f) : new(088.5f, 0, 114.5f),
                    3 => northCirle ? new(106.8f, 0, 117.2f) : new(111.5f, 0, 114.5f),
                    4 => northCirle ? new(118.3f, 0, 097.2f) : new(118.4f, 0, 102.8f),
                    _ => default,
                };
                dealpos4 = myP3Index switch
                {
                    1 => northCirle ? new(083.5f, 0, 104.0f) : new(084.0f, 0, 095.0f),
                    2 => northCirle ? new(088.5f, 0, 112.5f) : new(095.0f, 0, 116.3f),
                    3 => northCirle ? new(111.5f, 0, 112.5f) : new(105.0f, 0, 116.3f),
                    4 => northCirle ? new(116.5f, 0, 104.0f) : new(116.0f, 0, 095.0f),
                    _ => default,
                };
            }

            if (dealpos1 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_开场_处理位置_预站位";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos1;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (dealpos1 != default && dealpos2 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_开场_处理位置_1-2";
                dp.Scale = new(2);
                dp.Position = dealpos1;
                dp.TargetPosition = dealpos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_开场_处理位置_2";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 6000;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            if (dealpos2 != default && dealpos3 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_开场_处理位置_2-3";
                dp.Scale = new(2);
                dp.Position = dealpos2;
                dp.TargetPosition = dealpos3;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 8000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_开场_处理位置_3";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos3;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 8000;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            if (dealpos3 != default && dealpos4 != default)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_开场_处理位置_3-4";
                dp.Scale = new(2);
                dp.Position = dealpos3;
                dp.TargetPosition = dealpos4;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 14000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_开场_处理位置_4";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos4;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 14000;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }
        [ScriptMethod(name: "P3_小电视_处理位置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3159[56])$"])]
        public void P3_小电视_处理位置(Event @event, ScriptAccessory accessory)
        {
            //31595 东
            //31595 西
            if (parse != 3.0) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var meIsIdle = P3_TVBuff[myindex]==0;
            var myBuffIndex = 0;
            var isEast = @event["ActionId"] == "31595";
            Vector3 dealpos = default;

            for (int i = 0; i < HtdhParty.Count; i++)
            {
                var index = HtdhParty[i];
                var isIdle = P3_TVBuff[index] == 0;
                if (meIsIdle == isIdle) myBuffIndex++;
                if (index == myindex) break;
            }
            if (P3_TV_Strategy==P3TVEnum.Normal)
            {
                if (meIsIdle)
                {
                    dealpos = myBuffIndex switch
                    {
                        1 => isEast ? new(099.0f, 0, 091.0f) : new(101.0f, 0, 091.0f),
                        2 => isEast ? new(104.0f, 0, 100.0f) : new(096.0f, 0, 100.0f),
                        3 => isEast ? new(115.5f, 0, 100.0f) : new(084.5f, 0, 100.0f),
                        4 => isEast ? new(099.0f, 0, 109.0f) : new(101.0f, 0, 109.0f),
                        5 => isEast ? new(099.0f, 0, 119.0f) : new(101.0f, 0, 119.0f),
                        _ => default
                    };
                }
                else
                {
                    dealpos = myBuffIndex switch
                    {
                        1 => isEast ? new(093.0f, 0, 082.0f) : new(107.0f, 0, 082.0f),
                        2 => isEast ? new(086.0f, 0, 092.5f) : new(114.0f, 0, 092.5f),
                        3 => isEast ? new(086.0f, 0, 107.5f) : new(114.0f, 0, 107.5f),
                        _ => default
                    } ;
                }
            }
            if (P3_TV_Strategy == P3TVEnum.Static)
            {
                if (meIsIdle)
                {
                    dealpos = myBuffIndex switch
                    {
                        1 => isEast ? new(099.0f, 0, 091.0f) : new(101.0f, 0, 091.0f),
                        2 => new(109.0f, 0, 100.0f),
                        3 => new(119.0f, 0, 100.0f),
                        4 => isEast ? new(099.0f, 0, 109.0f) : new(101.0f, 0, 109.0f),
                        5 => isEast ? new(099.0f, 0, 119.0f) : new(101.0f, 0, 119.0f),
                        _ => default
                    };
                }
                else
                {
                    dealpos = myBuffIndex switch
                    {
                        1 => isEast ? new(095.0f, 0, 082.0f) : new(105.0f, 0, 082.0f),
                        2 => new(086.0f, 0, 092.0f),
                        3 => new(086.0f, 0, 108.0f),
                        _ => default
                    };
                }
            }

            if (dealpos == default) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_小电视_处理位置";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P3_小电视_面向辅助", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3159[56])$"])]
        public void P3_小电视_面向辅助(Event @event, ScriptAccessory accessory)
        {
            //31595 东
            //31595 西
            if (parse != 3.0) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var meIsIdle = P3_TVBuff[myindex] == 0;
            if (meIsIdle) return;
            var meLeft = P3_TVBuff[myindex] == 2;
            var myBuffIndex = 0;
            var isEast = @event["ActionId"] == "31595";
            float? seeRot = null;

            for (int i = 0; i < HtdhParty.Count; i++)
            {
                var index = HtdhParty[i];
                var isIdle = P3_TVBuff[index] == 0;
                if (meIsIdle == isIdle) myBuffIndex++;
                if (index == myindex) break;
            }
            //b pi/2

            seeRot = myBuffIndex switch
            {
                1 => isEast ? (meLeft ? float.Pi : 0) : (meLeft ? 0 : float.Pi),
                2 => meLeft ? float.Pi / 2 : float.Pi / -2,
                3 => meLeft ? float.Pi / -2 : float.Pi / 2,
                _ => null
            };
            if (seeRot == null) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_小电视_面向辅助_自身1";
            dp.Scale = new(5, 5);
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_小电视_面向辅助_自身2";
            dp.Scale = new(5, 1.5f);
            dp.Offset = new(0, 0, -5);
            dp.Rotation = float.Pi / 6 * 5;
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_小电视_面向辅助_自身3";
            dp.Scale = new(5, 1.5f);
            dp.Offset = new(0, 0, -5);
            dp.Rotation = float.Pi / 6 * -5;
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_小电视_面向辅助_指向1";
            dp.Scale = new(10,4);
            dp.FixRotation = true;
            dp.Rotation = seeRot.Value;
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            
        }
        #endregion

        #region P4
        [ScriptMethod(name: "P4_分P", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31559"], userControl: false)]
        public void P4_分P(Event @event, ScriptAccessory accessory)
        {
            parse = 4.0;
            P4Stack = [];
        }
        [ScriptMethod(name: "P4_分摊点名记录", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:22393"],userControl:false)]
        public void P4_分摊点名记录(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            if (index == -1) return;
            lock (P4Stack)
            {
                P4Stack.Add(index);
            }
        }
        [ScriptMethod(name: "P4_地震", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31567"])]
        public void P4_地震(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_地震_1";
            dp.Scale = new(6);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 0;
            dp.DestoryAt = 4800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_地震_2";
            dp.InnerScale = new(6);
            dp.Scale = new(12);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 4800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_地震_3";
            dp.InnerScale = new(12);
            dp.Scale = new(18);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_地震_4";
            dp.InnerScale = new(18);
            dp.Scale = new(24);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 8800;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        [ScriptMethod(name: "P4_第一段波动炮命中提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31614", "TargetIndex:1"])]
        public void P4_第一段波动炮命中提示(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            accessory.Method.TextInfo("走", 2000, true);
            accessory.Method.TTS("走");
        }
        [ScriptMethod(name: "P4_第二段波动炮", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:31616"])]
        public void P4_第二段波动炮(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_第二段八方波动炮";
            dp.Scale = new(6,50);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 0;
            dp.DestoryAt = 4800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "P4_第一段波动炮引导位置", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3161[07])$"])]
        public void P4_第一段波动炮引导位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 dealpos = myindex switch
            {
                0 => new(087.5f, 0, 094.5f),
                6 => new(086.5f, 0, 100.0f),
                2 => new(087.5f, 0, 105.0f),
                4 => new(090.5f, 0, 109.5f),
                1 => new(112.5f, 0, 094.5f),
                7 => new(113.5f, 0, 100.0f),
                3 => new(112.5f, 0, 105.0f),
                5 => new(109.5f, 0, 109.5f),
                _ => default
            };
            if (dealpos == default) return;

            if (@event["ActionId"]== "31610")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_第一段波动炮引导位置";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 14000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_第一段波动炮引导位置";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 5500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_第一段波动炮引导位置";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 15500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            
        }
        [ScriptMethod(name: "P4_第二段波动炮分摊位置", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:31614", "TargetIndex:1"])]
        public void P4_第二段波动炮分摊位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 4.0) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;

            var myindex=accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            var stack1 = P4Stack[^1];
            var stack2 = P4Stack[^2];

            List<int> leftGroup = [0, 6, 2, 4];
            List<int> rightGroup = [1, 7, 3, 5];
            if (leftGroup.Contains(stack1) && leftGroup.Contains(stack2))
            {
                var change = leftGroup.IndexOf(stack1) < leftGroup.IndexOf(stack2) ? stack2 : stack1;
                leftGroup.Remove(change);
                leftGroup.Add(5);
                rightGroup.Remove(5);
                rightGroup.Add(change);
            }
            if (rightGroup.Contains(stack1) && rightGroup.Contains(stack2))
            {
                var change = rightGroup.IndexOf(stack1) < rightGroup.IndexOf(stack2) ? stack2 : stack1;
                rightGroup.Remove(change);
                rightGroup.Add(4);
                leftGroup.Remove(4);
                rightGroup.Add(change);
            }

            Vector3 dealpos = leftGroup.Contains(myindex) ? new(96.5f, 0, 113) : new(103.5f, 0, 113);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_第二段波动炮分摊位置";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
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
        private int PositionTo8Dir(Vector3 point, Vector3 centre)
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r = Math.Round(4 - 4 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 8;
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

