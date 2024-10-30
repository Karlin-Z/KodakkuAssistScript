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
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;
using KodakkuAssist.Module.Draw.Manager;
using Dalamud.Game.ClientState.Objects.Types;

namespace KarlinScriptNamespace
{
    [ScriptType(name: "M3s绘图", territorys:[1230],guid: "a7e12eeb-4f05-4b68-8d4f-f64e08b6d7a5", version:"0.0.0.1")]
    public class M3s绘图绘图
    {
        [UserSetting("按照TNTN顺序安排撞线位置")]
        public bool TNTN_Fuse { get; set; } =false;


        int? firstTargetIcon = null;

        int parse;
        int chainObjPos4Dir;
        Vector3 chargeSafePos = default;
        bool[] isLongFuse = [false, false, false, false, false, false, false, false];
        bool[] isLongBoom = [false, false, false, false, false, false, false, false];
        (uint, uint)[] dir8obj = [(0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0), (0, 0)];
        bool[] isLongFieldFuse = [false, false, false, false, false, false, false, false];
        int bommIndex = -1;


        public void Init(ScriptAccessory accessory)
        {
            firstTargetIcon = null;
            parse = 0;
            chargeSafePos = default;
            //accessory.Method.MarkClear();
            bommIndex = -1;

        }

        [ScriptMethod(name: "分P", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37895|37927)$"],userControl:false)]
        public void 分P(Event @event, ScriptAccessory accessory)
        {
            parse++;
            chargeSafePos = default;
        }


        [ScriptMethod(name: "分摊死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37923)$"])]
        public void 分摊死刑(Event @event, ScriptAccessory accessory)
        {
            
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var during = 8500;
            switch (parse)
            {
                case 0:
                    during = 8500;
                    break;
                case 1:
                    during = 10500;
                    break;
                case 2:
                    during = 12500;
                    break;
                default:
                    break;
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"分摊死刑";
            dp.Scale = new(6);
            dp.Color = (index == 0 || index == 1) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = during;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "钢铁月环 分摊分散", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(378(48|49|50|51))$"])]
        public void 钢铁月环分摊分散(Event @event, ScriptAccessory accessory)
        {
            //37848 钢铁分散
            //37849 月环分散
            //37850 钢铁分摊
            //37851 月环分摊
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            int[] group= [7, 4, 6, 5, 1, 3, 2, 0];
            DrawPropertiesEdit dp;
            if (@event["ActionId"] == "37848" || @event["ActionId"] == "37850")
            {
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"钢铁月环+分摊分散-钢铁";
                dp.Scale = new(10);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = sid;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            if (@event["ActionId"] == "37849" || @event["ActionId"] == "37851")
            {
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"钢铁月环+分摊分散-月环";
                dp.Scale = new(40);
                dp.InnerScale = new(10);
                dp.Radian = float.Pi * 2;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = sid;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
            }

            if (@event["ActionId"] == "37848" || @event["ActionId"] == "37849")
            {
                for (int i = 0; i < 8; i++)
                {
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"钢铁月环+分摊分散-分散";
                    dp.Scale = new(40);
                    dp.Radian = float.Pi /4;
                    dp.Owner = sid;
                    dp.TargetObject = accessory.Data.PartyList[i];
                    dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
                    dp.DestoryAt = 7000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                }
                accessory.Method.TextInfo("八方分散",7000,true);
                accessory.Method.TTS("八方分散");
            }
            if (@event["ActionId"]== "37850"|| @event["ActionId"] == "37851")
            {
                for (int i = 0; i < 4; i++)
                {
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"钢铁月环+分摊分散-分摊";
                    dp.Scale = new(40);
                    dp.Radian = float.Pi / 8;
                    dp.Owner = sid;
                    dp.TargetObject = accessory.Data.PartyList[i];
                    dp.Color = (i == index || i == group[index]) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 7000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
                }
                accessory.Method.TextInfo("四角分摊", 7000);
                accessory.Method.TTS("四角分摊");
            }

            
        }
        [ScriptMethod(name: "陨石", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(378(68|77))$"])]
        public void 陨石(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"陨石+分摊分散-陨石";
            dp.Scale = new(22);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "击退", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(378(69|78))$"])]
        public void 击退(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            DrawPropertiesEdit dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"击退+分摊分散-击退";
            dp.Scale = new(2, 20);
            dp.Color = accessory.Data.DefaultSafeColor.WithW(3);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.Rotation = float.Pi;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "陨石/击退 分摊分散", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(378(54|55|56|57))$"])]
        public void 陨石分摊分散(Event @event, ScriptAccessory accessory)
        {
            //37854 陨石分散
            //37855 击退分散
            //37856 陨石分摊
            //37857 击退分摊
            
            DrawPropertiesEdit dp;
            if (@event["ActionId"] == "37854" || @event["ActionId"] == "37855")
            {
                for (int i = 0; i < 8; i++)
                {
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"陨石分摊分散-分散";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 8000;
                    dp.DestoryAt = 3000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                accessory.Method.TextInfo("稍后分散", 11000);
                accessory.Method.TTS("稍后分散");

            }
            if (@event["ActionId"] == "37856" || @event["ActionId"] == "37857")
            {
                var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                int[] group = [4, 5, 6, 7, 0, 1, 2, 3];
                for (int i = 0; i < 4; i++)
                {
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"陨石分摊分散-分摊";
                    dp.Scale = new(5);
                    dp.Radian = float.Pi / 3;
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = (i == index || i == group[index]) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.Delay = 8000;
                    dp.DestoryAt = 3000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                accessory.Method.TextInfo("稍后分摊", 11000);
                accessory.Method.TTS("稍后分摊");
            }



        }

        [ScriptMethod(name: "击退塔", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00020004", "Index:regex:^(0000000[56])$"])]
        public void 击退塔(Event @event, ScriptAccessory accessory)
        {
            //00000005 AC
            //00000006 BD
            var isAC = @event["Index"] == "00000005";
            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            var radian1 = 18.72f / 180f * float.Pi;
            var radian2 = 29.80f / 180f * float.Pi;
            var radian3 = 30.81f / 180f * float.Pi;

            var dur1 = 10000;
            var dur2 = 3000;
            var dur3 = 3000;
            var dur4 = 3000;

            Vector3 centre=new(100,0,100);
            Vector3 tPoint1 = default;
            Vector3 tPoint2 = default;

            if (isAC)
            {
                if (index == 0 || index == 1 || index == 4 || index == 5) 
                {
                    tPoint1 = new(100, 0, 89);
                    if (index == 0 || index == 4)
                    {
                        tPoint2 = new(111, 0, 111);
                    }
                    else
                    {
                        tPoint2 = new(89, 0, 111);
                    }
                }
                if (index == 2 || index == 3 || index == 6 || index == 7)
                {
                    tPoint1 = new(100, 0, 111);
                    if (index == 2 || index == 6)
                    {
                        tPoint2 = new(89, 0, 89);
                    }
                    else
                    {
                        tPoint2 = new(111, 0, 89);
                    }
                }
            }
            else
            {
                if (index == 0 || index == 1 || index == 4 || index == 5)
                {
                    tPoint1 = new(89, 0, 100);
                    if (index == 0 || index == 4)
                    {
                        tPoint2 = new(111, 0, 89);
                    }
                    else
                    {
                        tPoint2 = new(111, 0, 111);
                    }
                }
                if (index == 2 || index == 3 || index == 6 || index == 7)
                {
                    tPoint1 = new(111, 0, 100);
                    if (index == 2 || index == 6)
                    {
                        tPoint2 = new(89, 0, 111);
                    }
                    else
                    {
                        tPoint2 = new(89, 0, 89);
                    }
                }
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"击退塔-1";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Position = tPoint1;
            dp.TargetPosition = tPoint2;
            dp.Radian = radian1;
            dp.DestoryAt = dur1;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"击退塔-1位置";
            dp.Scale = new(2);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = tPoint1;
            dp.DestoryAt = dur1;
            dp.ScaleMode |= ScaleMode.YByDistance;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"击退塔-2";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Position = tPoint2;
            dp.TargetPosition = centre;
            dp.Radian = radian2;
            dp.DestoryAt = dur1 + dur2;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "场中击退塔", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37886"])]
        public void 场中击退塔(Event @event, ScriptAccessory accessory)
        {
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);

            var radian3 = 30.81f / 180f * float.Pi;

            Vector3 centre = new(100, 0, 100);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"击退塔-3";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Position = centre;
            dp.TargetPosition = pos;
            dp.Radian = radian3;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
        }
        [ScriptMethod(name: "场中击退塔角落aoe", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37886"])]
        public void 场中击退塔角落aoe(Event @event, ScriptAccessory accessory)
        {

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"场中击退塔角落aoe";
            dp.Scale = new(34);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Radian = float.Pi / 2 * 3;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "锁链收集", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4019"],userControl:false)]
        public void 锁链收集(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if((pos-new Vector3(100,0,100)).Length()<1) return;
            chainObjPos4Dir = PositionRoundTo4Dir(pos , new(100, 0, 100));
        }
        [ScriptMethod(name: "P2分身场边冲拳", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3972[46])$"])]
        public void P2分身场边冲拳(Event @event, ScriptAccessory accessory)
        {
            //39724 右
            //39725
            //39726 左
            //39727
            
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var rightHand = @event["ActionId"] == "39724";
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            
            var pos4dir = PositionRoundTo4Dir(pos, new(100, 0, 100));

            float.TryParse(@event["SourceRotation"], out var rotation);
            Vector3 backPos = default;
            if (pos4dir == 0) backPos = rightHand ? new(105, 0, 115) : new(95, 0, 115);
            if (pos4dir == 1) backPos = rightHand ? new(85, 0, 105) : new(85, 0, 95);
            if (pos4dir == 2) backPos = rightHand ? new(95, 0, 85) : new(105, 0, 85);
            if (pos4dir == 3) backPos = rightHand ? new(115, 0, 95) : new(115, 0, 105);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"场边冲拳一段";
            dp.Scale = new(20,30);
            dp.Offset = rightHand ? new(5, 0, 0) : new(-5, 0, 0);
            dp.Owner = sid;
            dp.Color = pos4dir == chainObjPos4Dir ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6100;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"场边冲拳二段";
            dp.Scale = new(20, 30);
            dp.Position = backPos;
            dp.Rotation= rotation+float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 4500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2分身场边冲拳二段安全区位置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(397(24|26))$"])]
        public void P2场边冲拳二段安全区位置(Event @event, ScriptAccessory accessory)
        {
            //39724 右
            //39725
            //39726 左
            //39727

            
            var rightHand = @event["ActionId"] == "39724";
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var pos4dir = PositionRoundTo4Dir(pos, new(100, 0, 100));
            lock (this)
            {

                float.TryParse(@event["SourceRotation"], out var rotation);

                if (MathF.Abs(pos.X - 115) < 1) chargeSafePos.Z = rightHand ? 94 : 106;
                if (MathF.Abs(pos.X - 85) < 1) chargeSafePos.Z = rightHand ? 106 : 94;

                if (MathF.Abs(pos.Z - 115) < 1) chargeSafePos.X = rightHand ? 106 : 94;
                if (MathF.Abs(pos.Z - 85) < 1) chargeSafePos.X = rightHand ? 94 : 106;

                if (chargeSafePos.X == 0 || chargeSafePos.Z == 0) return;
                


                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P2场边冲拳二段安全区位置";
                dp.Scale = new(2f, 10);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition=chargeSafePos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.TargetColor = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 6100;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P2场边冲拳二段安全区位置";
                dp.Scale = new(2f, 10);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = chargeSafePos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 6100;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }

        [ScriptMethod(name: "引线收集", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(402[45])$"], userControl: false)]
        public void 引线收集(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            isLongFuse[accessory.Data.PartyList.IndexOf(tid)] = @event["StatusID"] == "4024";
        }

        [ScriptMethod(name: "玩家引线提示", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(402[45])$"])]
        public void 玩家引线提示(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            var shorDelay = parse == 1 ? 12000 : 20800;
            var longDelay = parse == 1 ? 17000 : 25800;
            var islong = @event["StatusID"] == "4025";
            accessory.Method.TextInfo($"{(islong ? "长" : "短")}引线", islong ? shorDelay : longDelay);
            accessory.Method.TTS($"{(islong ? "长" : "短")}引线");

        }
        [ScriptMethod(name: "玩家自爆范围", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(402[45])$"])]
        public void 玩家自爆范围(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var islong = @event["StatusID"] == "4025";

            var dur = 5000;
            var shorDelay = parse == 1 ? 12000 : 20800;
            var longDelay = parse == 1 ? 17000 : 25800;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"玩家自爆范围";
            dp.Scale = new(6);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.Delay = islong? longDelay-dur: shorDelay- dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "地面炸弹爆炸范围", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:17095"])]
        public void 地面炸弹爆炸范围(Event @event, ScriptAccessory accessory)
        {
            
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var obj= accessory.Data.Objects.SearchByEntityId(sid);
            if(obj == null) return;
            var statusCount= ((IBattleChara)obj).StatusList.Where(status => status.StatusId == 4016).Count();

            var dur = 5000;
            var shorDelay =parse==1? 12000:19000;
            var longDelay = parse==1? 17000: 24000;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"地面炸弹爆炸范围";
            dp.Scale = new(8);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = statusCount>0 ? shorDelay : 0;
            dp.DestoryAt = statusCount > 0 ? dur : shorDelay;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "玩家长短爆炸buff收集", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4020"], userControl: false)]
        public void 玩家长短爆炸buff收集(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            int.TryParse(@event["DurationMilliseconds"], out var dur);
            isLongBoom[accessory.Data.PartyList.IndexOf(tid)] = dur > 30000;
        }
        [ScriptMethod(name: "场地引线收集", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0111"], userControl: false)]
        public void 场地引线收集(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var tpos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
            var dir8 = PositionTo8Dir(spos, new(100,0,100));
            dir8obj[dir8] = (sid, tid);
            isLongFieldFuse[dir8] = (spos - tpos).Length() > 7;
        }
        [ScriptMethod(name: "场地引线分配", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37871"])]
        public void 场地引线分配(Event @event, ScriptAccessory accessory)
        {
            int[] tnGroup = TNTN_Fuse ? [0, 2, 1, 3] : [0, 1, 2, 3];
            int[] dpsGroup = [4, 5, 6, 7];
            var myPartyIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var myFuseIndex = tnGroup.IndexOf(myPartyIndex);
            myFuseIndex = myFuseIndex == -1 ? dpsGroup.IndexOf(myPartyIndex) : myFuseIndex;
            var meIslongBoom = isLongBoom[myPartyIndex];
            var fuseIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                if (isLongFieldFuse[i]== meIslongBoom)
                {
                    if(fuseIndex == myFuseIndex)
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"场地引线分配";
                        dp.Scale = new(10);
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.Owner = dir8obj[i].Item1;
                        dp.TargetObject = dir8obj[i].Item2;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.DestoryAt = meIslongBoom ? 44000 : 26000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
                    }
                    fuseIndex++;
                }
            }

        }
        [ScriptMethod(name: "场地引线撞线提示", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3787[25])$", "TargetIndex:1"])]
        public void 场地引线撞线提示(Event @event, ScriptAccessory accessory)
        {
            int[] tnGroup = TNTN_Fuse ? [0, 2, 1, 3] : [0, 1, 2, 3];
            int[] dpsGroup = [4, 5, 6, 7];

            var myPartyIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var myFuseIndex = tnGroup.IndexOf(myPartyIndex);
            myFuseIndex = myFuseIndex == -1 ? dpsGroup.IndexOf(myPartyIndex) : myFuseIndex;
            myFuseIndex = isLongBoom[myPartyIndex] ? myFuseIndex + 4 : myFuseIndex;
            bommIndex++;
            if (myFuseIndex == bommIndex) 
            {
                Task.Delay(2000).ContinueWith(t =>
                {
                    accessory.Method.TextInfo("撞线", 2000);
                    accessory.Method.TTS("撞线");
                });
            }

        }


        //37898 接分摊
        //38738 接分散

        //37904 钢铁
        //37905 月环
        //37908 击退
        [ScriptMethod(name: "P3组合技钢铁月环击退", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37898|38738)$"])]
        public void P3组合钢铁月环击退(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3组合技钢铁";
            dp.Scale = new(10);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.DestoryAt = 15800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);


            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3组合技月环";
            dp.Scale = new(40);
            dp.InnerScale = new(6);
            dp.Radian = float.Pi * 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = sid;
            dp.Delay = 15800;
            dp.DestoryAt = 2500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = $"P3组合技击退";
            dp2.Scale = new(1.5f,10);
            dp2.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp2.Owner = accessory.Data.Me;
            dp2.TargetObject = sid;
            dp2.Rotation = float.Pi;
            dp2.Delay = 18300;
            dp2.DestoryAt = 8200;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp2);

        }

        [ScriptMethod(name: "P3组合技分散分摊", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37898|38738)$"])]
        public void P3组合技分散分摊(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var actionId = @event["ActionId"];

            DrawPropertiesEdit dp;
            if (@event["ActionId"] == "38738")
            {
                for (int i = 0; i < 8; i++)
                {
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"P3组合技-分散";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 26500;
                    dp.DestoryAt = 3500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                accessory.Method.TextInfo("稍后分散", 26500);
                accessory.Method.TTS("稍后分散");
                Task.Delay(26500).ContinueWith(t =>
                {
                    accessory.Method.TextInfo("分散", 3500,true);
                    accessory.Method.TTS("分散");
                });
            }
            if (@event["ActionId"] == "37898")
            {
                var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                int[] group = [4, 5, 6, 7, 0, 1, 2, 3];
                for (int i = 0; i < 4; i++)
                {
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"P3组合技-分摊";
                    dp.Scale = new(5);
                    dp.Radian = float.Pi / 3;
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = (i == index || i == group[index]) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.Delay = 26500;
                    dp.DestoryAt = 3500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                accessory.Method.TextInfo("稍后分摊", 26500);
                accessory.Method.TTS("稍后分摊");
                Task.Delay(26500).ContinueWith(t =>
                {
                    accessory.Method.TextInfo("二二分摊", 3500, true);
                    accessory.Method.TTS("二二分摊");
                });
            }




        }

        

        [ScriptMethod(name: "P3分身场边冲拳", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3989[68])$"])]
        public void P3分身场边冲拳(Event @event, ScriptAccessory accessory)
        {
            //39896 右
            //39897
            //39898 左
            //39899

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var rightHand = @event["ActionId"] == "39896";
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);

            var pos4dir = PositionRoundTo4Dir(pos, new(100, 0, 100));

            float.TryParse(@event["SourceRotation"], out var rotation);
            Vector3 backPos = default;
            if (pos4dir == 0) backPos = rightHand ? new(105, 0, 115) : new(95, 0, 115);
            if (pos4dir == 1) backPos = rightHand ? new(85, 0, 105) : new(85, 0, 95);
            if (pos4dir == 2) backPos = rightHand ? new(95, 0, 85) : new(105, 0, 85);
            if (pos4dir == 3) backPos = rightHand ? new(115, 0, 95) : new(115, 0, 105);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3场边冲拳一段";
            dp.Scale = new(20, 30);
            dp.Offset = rightHand ? new(5, 0, 0) : new(-5, 0, 0);
            dp.Owner = sid;
            dp.Color = pos4dir == chainObjPos4Dir ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8100;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3场边冲拳二段";
            dp.Scale = new(20, 30);
            dp.Position = backPos;
            dp.Rotation = rotation + float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 8000;
            dp.DestoryAt = 4500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "P3分身场边冲拳二段安全区位置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3989[68])$"])]
        public void P3分身场边冲拳二段安全区位置(Event @event, ScriptAccessory accessory)
        {
            //39896 右
            //39897
            //39898 左
            //39899


            var rightHand = @event["ActionId"] == "39896";
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var pos4dir = PositionRoundTo4Dir(pos, new(100, 0, 100));
            lock (this)
            {

                float.TryParse(@event["SourceRotation"], out var rotation);

                if (MathF.Abs(pos.X - 115) < 1) chargeSafePos.Z = rightHand ? 94 : 106;
                if (MathF.Abs(pos.X - 85) < 1) chargeSafePos.Z = rightHand ? 106 : 94;

                if (MathF.Abs(pos.Z - 115) < 1) chargeSafePos.X = rightHand ? 106 : 94;
                if (MathF.Abs(pos.Z - 85) < 1) chargeSafePos.X = rightHand ? 94 : 106;


                if (chargeSafePos.X == 0 || chargeSafePos.Z == 0) return;



                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P2场边冲拳二段安全区位置";
                dp.Scale = new(2f, 10);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = chargeSafePos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.TargetColor = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 8100;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P2场边冲拳二段安全区位置";
                dp.Scale = new(2f, 10);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = chargeSafePos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 8100;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }

        [ScriptMethod(name: "P3场边冲拳_场中扇形危险区", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39895"])]
        public void P3场边冲拳_场中扇形危险区(Event @event, ScriptAccessory accessory)
        {

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3场边冲拳_场中扇形危险区";
            dp.Scale = new(21.2f);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.Owner = sid;
            dp.Rotation = float.Pi;
            dp.Radian = float.Pi / 2;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "P3击退塔冲拳", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3966[4567])$"])]
        public void P3击退塔冲拳(Event @event, ScriptAccessory accessory)
        {
            //39664 右
            //39665 左
            //39666 右
            //39667 左
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var rightHand = @event["ActionId"] == "39664" || @event["ActionId"] == "39666";
            var dur = @event["ActionId"] == "39664" || @event["ActionId"] == "39665" ? 6100 : 3100;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3击退塔冲拳一段";
            dp.Scale = new(20, 30);
            dp.Offset = rightHand ? new(5, 0, 0) : new(-5, 0, 0);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "P3场中击退塔", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3966[45])$"])]
        public void P3场中击退塔(Event @event, ScriptAccessory accessory)
        {
            
            //39664 右
            //39665 左
            //39666 右
            //39667 左

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3场中击退塔";
            dp.Scale = new(1.5f, 15);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = new(100, 0, 100);
            dp.Rotation = float.Pi;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            var rightHand = @event["ActionId"] == "39664";
            float.TryParse(@event["SourceRotation"], out var rotation);
            var r = 22.74f / 180 * float.Pi;
            var rot = 45.06f / 180 * float.Pi;
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P3场中击退塔位置";
            dp.Scale = new(2);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Position = new(100,0,100);
            dp.Radian = r;
            dp.Rotation = rightHand ? rotation + rot : rotation - rot;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
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
        private int PositionTo8Dir (Vector3 point, Vector3 centre) 
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r= Math.Round(4 - 4 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 8;
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

