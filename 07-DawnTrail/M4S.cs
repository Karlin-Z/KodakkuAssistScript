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
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Collections;

namespace KarlinScriptNamespace
{
    [ScriptType(name: "M4s绘图", territorys:[1232],guid: "e7f7c69b-cc82-4b74-b1ea-2f3f0eecb2e2", version:"0.0.0.5", author: "Karlin")]
    public class M4s绘图绘图
    {
        [UserSetting("奔雷炮站位方式")]
        public LaserPositionEnum LaserPosition { get; set; }


        int? firstTargetIcon = null;
        int parse;
        bool imP1lighting=false;
        bool P1LightingDone = false;
        bool stackBuff;

        int laserObjCount = 0;
        int spreadStackCount = 0;
        float fieldLaserDis;

        int[] lightingCounts = [0, 0, 0, 0, 0, 0, 0, 0];
        List<bool> long3999 = [false, false, false, false, false, false, false, false];
        int floorSpreadCount = 0;

        bool[] isYelowBuff = [false, false, false, false, false, false, false, false];
        bool[] long4000 = [false, false, false, false, false, false, false, false];
        List<bool> isYellowGun = [false, false, false, false];
        uint towerId;
        int towerCount;
        bool isSouthYellowLaser;
        bool isEastYellowLaser;
        List<int> TetherBoom = [0, 0, 0, 0, 0, 0, 0, 0];
        List<uint> TetherLighting = [];


        int[] FloorFire = [0, 0, 0];
        string storeSkill = "";
        bool southSafe1st;
        bool southSafe2nd;

        DateTime timeLock = new();

        public enum LaserPositionEnum
        {
            Game8,
            MMW
        }

        public void Init(ScriptAccessory accessory)
        {
            parse = 0;
            firstTargetIcon = null;
            laserObjCount = 0;
            spreadStackCount = 0;
            imP1lighting = false;
            P1LightingDone = false;
            lightingCounts = [0, 0, 0, 0, 0, 0, 0, 0];
            floorSpreadCount = 0;

            FloorFire = [0, 0, 0];
            TetherBoom = [0, 0, 0, 0, 0, 0, 0, 0];
            TetherLighting = [];

            isYellowGun = [false,false,false,false];
        }

        [ScriptMethod(name: "本体记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38386"],userControl:false)]
        public void 本体记录(Event @event, ScriptAccessory accessory)
        {
            parse = 1;
        }
        [ScriptMethod(name: "翅膀激光", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38377"])]
        public void 翅膀激光(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "翅膀激光";
            dp.Scale = new(5,40);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "地面扩散激光", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38378"])]
        public void 地面扩散激光(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "翅膀激光";
            dp.Scale = new(16, 40);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "场边激光", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17324"])]
        public void 场边激光(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            lock (this)
            {
                laserObjCount++;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "雷球激光";
                dp.Scale = new(5, 40);
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = laserObjCount > 4 ? 5000 : 6800;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            
        }

        [ScriptMethod(name: "雷电buff收集", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:587"],userControl:false)]
        public void 雷电buff收集(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid==accessory.Data.Me)
            {
                imP1lighting = true;
            }
        }
        [ScriptMethod(name: "地面扩散激光收集", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38378"])]
        public void 地面扩散激光收集(Event @event, ScriptAccessory accessory)
        {
            var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);

            fieldLaserDis =MathF.Abs(spos.X - 100);
            // <10场中
        }
        [ScriptMethod(name: "雷电buff分散站位", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970", "StackCount:regex:^(24[67])$"])]
        public void 雷电buff分散站位(Event @event, ScriptAccessory accessory)
        {
            if (P1LightingDone) return;
            P1LightingDone = true;
            
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var Hitnear = @event["StackCount"] == "246";
            float dy;
            if(Hitnear)
            {
                if (imP1lighting)
                {
                    if (myindex == 0 || myindex == 1 || myindex == 6 || myindex == 7) dy = -15;
                    else dy = 15;
                }
                else
                {
                    if (myindex == 0 || myindex == 1 || myindex == 6 || myindex == 7) dy = -5;
                    else dy = 5;
                }
            }
            else
            {
                if (imP1lighting)
                {
                    if (myindex == 0 || myindex == 1 || myindex == 6 || myindex == 7) dy = -5;
                    else dy = 5;
                }
                else
                {
                    if (myindex == 0 || myindex == 1 || myindex == 6 || myindex == 7) dy = -15;
                    else dy = 15;
                }
            }

            // <10场中
            var dx = fieldLaserDis < 10 ? 18f : 4;
            if (myindex % 2 == 0) dx = -dx;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "雷电buff分散站位";
            dp.Scale = new(3);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition=new(100+dx,0,100+dy);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


           
        }

        [ScriptMethod(name: "钢铁月环组合技-钢铁月环", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3836[89])$"])]
        public void 钢铁月环组合技钢铁月环(Event @event, ScriptAccessory accessory)
        {
            //38368 钢月
            //38368 月钢
            //15-3.5-3.5-3.5

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var isCircleFirst = @event["ActionId"] == "38368";


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"钢铁月环组合技-{(isCircleFirst? "钢铁" : "月环")}-1";
            dp.Scale =isCircleFirst? new(10):new(30);
            dp.InnerScale = new(10);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 15000;
            accessory.Method.SendDraw(DrawModeEnum.Default, isCircleFirst ? DrawTypeEnum.Circle : DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"钢铁月环组合技-{(!isCircleFirst ? "钢铁" : "月环")}-1";
            dp.Scale = !isCircleFirst ? new(10) : new(30);
            dp.InnerScale = new(10);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 15000;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, !isCircleFirst ? DrawTypeEnum.Circle : DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"钢铁月环组合技-{(isCircleFirst ? "钢铁" : "月环")}-2";
            dp.Scale = isCircleFirst ? new(10) : new(30);
            dp.InnerScale = new(10);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 18500;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, isCircleFirst ? DrawTypeEnum.Circle : DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"钢铁月环组合技-{(!isCircleFirst ? "钢铁" : "月环")}-2";
            dp.Scale = !isCircleFirst ? new(10) : new(30);
            dp.InnerScale = new(10);
            dp.Radian = float.Pi * 2;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 22000;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, !isCircleFirst ? DrawTypeEnum.Circle : DrawTypeEnum.Donut, dp);

        }
        [ScriptMethod(name: "钢铁月环组合技-处理位置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3836[89])$"])]
        public void 钢铁月环组合技处理位置(Event @event, ScriptAccessory accessory)
        {
            //38368 钢月
            //38368 月钢
            //15-3.5-3.5-3.5
            Vector3[,] pos = new Vector3[8, 4];

            if (@event["ActionId"] == "38368")
            {
                //ok
                pos[0, 0] = new(100.0f, 0, 089.3f); pos[0, 1] = new(100.0f, 0, 096.0f); pos[0, 2] = new(100.0f, 0, 087.8f); pos[0, 3] = new(100.0f, 0, 094.5f);
                pos[1, 0] = new(100.0f, 0, 110.7f); pos[1, 1] = new(100.0f, 0, 104.0f); pos[1, 2] = new(100.0f, 0, 112.2f); pos[1, 3] = new(100.0f, 0, 105.5f);

                pos[2, 0] = new(090.0f, 0, 110.0f); pos[2, 1] = new(093.3f, 0, 106.7f); pos[2, 2] = new(091.5f, 0, 108.5f); pos[2, 3] = new(094.0f, 0, 106.0f);
                pos[3, 0] = new(110.0f, 0, 110.0f); pos[3, 1] = new(106.7f, 0, 106.7f); pos[3, 2] = new(108.5f, 0, 108.5f); pos[3, 3] = new(106.0f, 0, 106.0f);

                pos[4, 0] = new(087.8f, 0, 100.0f); pos[4, 1] = new(094.5f, 0, 100.0f); pos[4, 2] = new(089.3f, 0, 100.0f); pos[4, 3] = new(096.0f, 0, 100.0f);
                pos[5, 0] = new(112.2f, 0, 100.0f); pos[5, 1] = new(105.5f, 0, 100.0f); pos[5, 2] = new(110.7f, 0, 100.0f); pos[5, 3] = new(104.0f, 0, 100.0f);

                pos[6, 0] = new(091.5f, 0, 091.5f); pos[6, 1] = new(094.0f, 0, 094.0f); pos[6, 2] = new(090.0f, 0, 090.0f); pos[6, 3] = new(093.3f, 0, 093.3f);
                pos[7, 0] = new(108.5f, 0, 091.5f); pos[7, 1] = new(106.0f, 0, 094.0f); pos[7, 2] = new(110.0f, 0, 090.0f); pos[7, 3] = new(106.7f, 0, 093.3f);
            }
            else
            {
                pos[0, 0] = new(100.0f, 0, 096.0f); pos[0, 1] = new(100.0f, 0, 089.3f); pos[0, 2] = new(100.0f, 0, 094.5f); pos[0, 3] = new(100.0f, 0, 087.8f);
                pos[1, 0] = new(100.0f, 0, 104.0f); pos[1, 1] = new(100.0f, 0, 110.7f); pos[1, 2] = new(100.0f, 0, 105.5f); pos[1, 3] = new(100.0f, 0, 112.2f);

                pos[2, 0] = new(093.3f, 0, 106.7f); pos[2, 1] = new(090.0f, 0, 110.0f); pos[2, 2] = new(094.0f, 0, 106.0f); pos[2, 3] = new(091.5f, 0, 108.5f);
                pos[3, 0] = new(106.7f, 0, 106.7f); pos[3, 1] = new(110.0f, 0, 110.0f); pos[3, 2] = new(106.0f, 0, 106.0f); pos[3, 3] = new(108.5f, 0, 108.5f);

                pos[4, 0] = new(094.5f, 0, 100.0f); pos[4, 1] = new(087.8f, 0, 100.0f); pos[4, 2] = new(096.0f, 0, 100.0f); pos[4, 3] = new(089.3f, 0, 100.0f);
                pos[5, 0] = new(105.5f, 0, 100.0f); pos[5, 1] = new(112.2f, 0, 100.0f); pos[5, 2] = new(104.0f, 0, 100.0f); pos[5, 3] = new(110.7f, 0, 100.0f);

                pos[6, 0] = new(094.0f, 0, 094.0f); pos[6, 1] = new(091.5f, 0, 091.5f); pos[6, 2] = new(093.3f, 0, 093.3f); pos[6, 3] = new(090.0f, 0, 090.0f);
                pos[7, 0] = new(106.0f, 0, 094.0f); pos[7, 1] = new(108.5f, 0, 091.5f); pos[7, 2] = new(106.7f, 0, 093.3f); pos[7, 3] = new(110.0f, 0, 090.0f);
            }

            var index = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "钢铁月环组合技-自身-1";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = pos[index, 0];
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 15000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "钢铁月环组合技-自身-2";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = pos[index, 1];
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 15000;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "钢铁月环组合技-自身-3";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = pos[index, 2];
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 18500;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "钢铁月环组合技-自身-4";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = pos[index, 3];
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 22000;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "钢铁月环组合技-地面-12";
            dp.Scale = new(2);
            dp.Position = pos[index, 0];
            dp.TargetPosition = pos[index, 1];
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 0;
            dp.DestoryAt = 15000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "钢铁月环组合技-地面-23";
            dp.Scale = new(2);
            dp.Position = pos[index, 1];
            dp.TargetPosition = pos[index, 2];
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 15000;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "钢铁月环组合技-地面-34";
            dp.Scale = new(2);
            dp.Position = pos[index, 2];
            dp.TargetPosition = pos[index, 3];
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 18500;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "雷球直线aoe", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:17325"])]
        public void 雷球直线aoe(Event @event, ScriptAccessory accessory)
        {
            //38345 小
            //38346 大

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"雷球方形aoe";
            dp.Scale = new(5,30);
            dp.Position = new(100, 0, 100);
            dp.TargetObject = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 10000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }

        [ScriptMethod(name: "雷球方形aoe", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3834[56])$"])]
        public void 雷球方形aoe(Event @event, ScriptAccessory accessory)
        {
            //38345 小
            //38346 大

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var isSmall = @event["ActionId"] == "38345";


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"雷球方形aoe";
            dp.Scale = isSmall ? new(10) : new(30);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default,  DrawTypeEnum.Straight, dp);

        }
        [ScriptMethod(name: "左右刀", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3838[01])$"])]
        public void 左右刀(Event @event, ScriptAccessory accessory)
        {
            //38345 小
            //38346 大

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var isEast = @event["ActionId"] == "38380";


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"雷球方形aoe-{(isEast?"右":"左")}";
            dp.Scale = new(40,20);
            dp.Owner = sid;
            dp.Rotation = isEast ? float.Pi / -2 : float.Pi / 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "分散分摊Buff收集", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970", "StackCount:regex:^(24[01])$"],userControl:false)]
        public void 分散分摊Buff收集(Event @event, ScriptAccessory accessory)
        {
            stackBuff = @event["StackCount"] == "240";
        }
        [ScriptMethod(name: "左右刀计数", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3838[01])$"],userControl:false)]
        public void 左右刀计数(Event @event, ScriptAccessory accessory)
        {
            spreadStackCount++;
        }
        [ScriptMethod(name: "左右刀分散分摊", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3838[01])$"])]
        public void 左右刀分散分摊(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(50).ContinueWith(t =>
            {
                if (spreadStackCount == 1)
                {
                    if (stackBuff)
                    {
                        int[] parterner = [4, 5, 6, 7, 0, 1, 2, 3];
                        var MyIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                        for (var i = 0; i < 4; i++)
                        {
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"分摊-{i}";
                            dp.Scale = new(6);
                            dp.Owner = accessory.Data.PartyList[i];
                            dp.Delay = 3000;
                            dp.DestoryAt = 4000;
                            dp.Color = MyIndex == i || parterner[MyIndex] == i ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                        }
                        accessory.Method.TextInfo("二二分摊", 7000);
                        accessory.Method.TTS("二二分摊");
                    }
                    else
                    {
                        for (var i = 0; i < 8; i++)
                        {
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"分散-{i}";
                            dp.Scale = new(6);
                            dp.Owner = accessory.Data.PartyList[i];
                            dp.Delay = 3000;
                            dp.DestoryAt = 4000;
                            dp.Color = accessory.Data.DefaultDangerColor;
                            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                        }
                        accessory.Method.TextInfo("分散", 7000);
                        accessory.Method.TTS("分散");
                    }
                }
                if (spreadStackCount == 2)
                {
                    if (stackBuff)
                    {
                        var MyIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

                        List<int> LargeTn = new();
                        List<int> SmallTn = new();
                        List<int> LargeDps = new();
                        List<int> SmalleDps = new();
                        for (var i = 0; i < 8; i++)
                        {
                            var isLarge = (long3999[i] && lightingCounts[i] == 2) || (!long3999[i] && lightingCounts[i] == 3);
                            if (isLarge)
                            {
                                if (i > 3) LargeDps.Add(i);
                                else LargeTn.Add(i);
                            }
                            else
                            {
                                if (i > 3) SmalleDps.Add(i);
                                else SmallTn.Add(i);
                            }
                        }



                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"分摊-1";
                        dp.Scale = new(6);
                        dp.Owner = accessory.Data.PartyList[LargeTn[0]];
                        dp.Delay = 0;
                        dp.DestoryAt = 7000;
                        dp.Color = LargeTn.Contains(MyIndex) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"分摊-2";
                        dp.Scale = new(6);
                        dp.Owner = accessory.Data.PartyList[SmallTn[0]];
                        dp.Delay = 0;
                        dp.DestoryAt = 7000;
                        dp.Color = SmallTn.Contains(MyIndex) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"分摊-3";
                        dp.Scale = new(6);
                        dp.Owner = accessory.Data.PartyList[LargeDps[0]];
                        dp.Delay = 0;
                        dp.DestoryAt = 7000;
                        dp.Color = LargeDps.Contains(MyIndex) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = $"分摊-4";
                        dp.Scale = new(6);
                        dp.Owner = accessory.Data.PartyList[SmalleDps[0]];
                        dp.Delay = 0;
                        dp.DestoryAt = 7000;
                        dp.Color = SmalleDps.Contains(MyIndex) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                        accessory.Method.TextInfo("二二分摊", 7000);
                        accessory.Method.TTS("二二分摊");
                    }
                    else
                    {
                        for (var i = 0; i < 8; i++)
                        {
                            var dp = accessory.Data.GetDefaultDrawProperties();
                            dp.Name = $"分散-{i}";
                            dp.Scale = new(6);
                            dp.Owner = accessory.Data.PartyList[i];
                            dp.Delay = 3000;
                            dp.DestoryAt = 4000;
                            dp.Color = accessory.Data.DefaultDangerColor;
                            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                        }
                        accessory.Method.TextInfo("分散", 7000);
                        accessory.Method.TTS("分散");
                    }
                }
            });
            

        }
        [ScriptMethod(name: "第二次左右刀分散分摊 分摊位置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3838[01])$"])]
        public void 第二次左右刀分散分摊分摊位置(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(50).ContinueWith(t =>
            {
                var isEast = @event["ActionId"] == "38380";
                if (spreadStackCount == 2 && stackBuff)
                {
                    var MyIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

                    List<int> LargeTn = new();
                    List<int> SmallTn = new();
                    List<int> LargeDps = new();
                    List<int> SmalleDps = new();
                    for (var i = 0; i < 8; i++)
                    {
                        var isLarge = (long3999[i] && lightingCounts[i] == 2) || (!long3999[i] && lightingCounts[i] == 3);
                        if (isLarge)
                        {
                            if (i > 3) LargeDps.Add(i);
                            else LargeTn.Add(i);
                        }
                        else
                        {
                            if (i > 3) SmalleDps.Add(i);
                            else SmallTn.Add(i);
                        }
                    }

                    Vector3 dealPos=default;
                    if (LargeTn.Contains(MyIndex)) dealPos = new(isEast ? 92 : 108, 0, 100);
                    if (SmallTn.Contains(MyIndex)) dealPos = new(isEast ? 99 : 101, 0, 92);
                    if (LargeDps.Contains(MyIndex)) dealPos = new(isEast ? 99 : 101, 0, 100);
                    if (SmalleDps.Contains(MyIndex)) dealPos = new(isEast ? 99 : 101, 0, 108);


                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分摊-1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealPos;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Delay = 0;
                    dp.DestoryAt = 7000;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                   


                }
            });


        }

        [ScriptMethod(name: "一仇直线死刑", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38384"])]
        public void 一仇直线死刑(Event @event, ScriptAccessory accessory)
        {
            //5-3

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"一仇直线死刑-1";
            dp.Scale = new(5, 40);
            dp.Owner = sid;
            dp.TargetObject=tid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"一仇直线死刑-2";
            dp.Scale = new(5, 40);
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }

        [ScriptMethod(name: "八方引导雷击", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38789"])]
        public void 八方引导雷击(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            for (var i = 0; i <8;i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"雷球方形aoe-{i}";
                dp.Scale = new(5, 30);
                dp.Owner = sid;
                dp.TargetObject = accessory.Data.PartyList[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }
        [ScriptMethod(name: "雷击计数器", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:38790"],userControl:false)]
        public void 雷击计数器(Event @event, ScriptAccessory accessory)
        {
            //5-3

            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index= accessory.Data.PartyList.IndexOf(tid);
            if (index == -1) return;
            lock (lightingCounts)
            {
                lightingCounts[index]++;
            }

        }
        [ScriptMethod(name: "Buff3999计数器", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3999"],userControl:false)]
        public void Buff3999计数器(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            if (index == -1) return;
            long3999[index] = dur > 30000;

        }
        [ScriptMethod(name: "地板分散位置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38351"])]
        public void 地板分散位置(Event @event, ScriptAccessory accessory)
        {
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var centre = new Vector3(100, 0, 100);
            if (MathF.Abs((pos - centre).Length() - 16) > 1) return;

            floorSpreadCount++;
            var dir4 = PositionRoundTo4Dir(pos, centre);
            var rot = 0 + float.Pi / 2 * dir4;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            

            var meIsSmall = (long3999[myIndex] & lightingCounts[myIndex] == 1) || (!long3999[myIndex] & lightingCounts[myIndex] == 2);

            var dealPos=new Vector3();
            if (meIsSmall)
            {
                dealPos = myIndex > 3 ? new(116, 0, 108) : new(84, 0, 108);
            }
            else
            {
                dealPos = myIndex > 3 ? new(116, 0, 84) : new(84, 0, 84);
            }

            if (long3999[myIndex] && floorSpreadCount == 1) dealPos = new(100, 0, 116);
            if (!long3999[myIndex] && floorSpreadCount != 1) dealPos = new(100, 0, 116);

            dealPos = RotatePoint(dealPos, centre, rot);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "地板分散位置";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealPos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        }


        [ScriptMethod(name: "黄蓝buff收集器", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(400[01])$"], userControl: false)]
        public void 黄蓝buff收集器(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            if (index == -1) return;
            
            isYelowBuff[index] = @event["StatusID"] == "4000";
            long4000[index] = dur > 30000;
        }
        [ScriptMethod(name: "黄激光收集", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3835[89])$"],userControl:false)]
        public void 黄激光收集(Event @event, ScriptAccessory accessory)
        {
            //38360 黄
            //38360 蓝
            var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            isSouthYellowLaser = @event["ActionId"]== "38359";
            isEastYellowLaser = spos.X > 100;
        }

        [ScriptMethod(name: "左右拆地板", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(3835[45])$"])]
        public void 左右拆地板(Event @event, ScriptAccessory accessory)
        {
            //38345 小
            //38346 大

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var isEast = @event["ActionId"] == "38355";


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"左右拆地板-{(isEast ? "右" : "左")}";
            dp.Scale = new(30, 40);
            dp.Position = new(isEast ? 105 : 95, 0, 80);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "黄蓝激光", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3836[01])$"])]
        public void 黄蓝激光(Event @event, ScriptAccessory accessory)
        {
            //38360 黄
            //38360 蓝
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var isYellowLaser = @event["ActionId"] == "38360";

            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"左右拆地板-{(isYellowLaser ? "黄" : "蓝")}";
            dp.Scale = new(10, 20);
            dp.Owner = sid;
            dp.Color = isYellowLaser==isYelowBuff[myIndex]? accessory.Data.DefaultDangerColor:accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "激光buff处理位置", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(400[23456])$"])]
        public void 激光buff处理位置(Event @event, ScriptAccessory accessory)
        {
            //4002 4003 扇形
            //4004 4005 钢月
            //4006      死宣
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if(tid!=accessory.Data.Me) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var statusID = @event["StatusID"];
            
            Vector3 dealPos = isEastYellowLaser ? new(115, 0, 100) : new(85, 0, 100);
            if (statusID == "4006")
            {
                dealPos += isYelowBuff[myIndex] != isSouthYellowLaser ? new(0, 0, 6) : new(0, 0, -6);
            }
            if (statusID == "4002" || statusID == "4003")
            {
                dealPos += isYelowBuff[myIndex] != isSouthYellowLaser ? new(0, 0, 4.5f) : new(0, 0, -4.5f);
            }
            if (statusID == "4004" || statusID == "4005")
            {
                dealPos += isYelowBuff[myIndex] != isSouthYellowLaser ? new(0, 0, 2.1f) : new(0, 0, -2.1f);
                if (LaserPosition==LaserPositionEnum.Game8)
                {
                    var dvx = isEastYellowLaser ? 1 : -1;
                    dealPos += myIndex > 3 ? new(-2.9f * dvx, 0, 0) : new(2.9f * dvx, 0, 0);
                }
                if (LaserPosition==LaserPositionEnum.MMW)
                {
                    var dvx = myIndex > 3 ? 1 : -1;
                    var isSourth = isYelowBuff[myIndex] != isSouthYellowLaser;
                    dealPos += isSourth ? new(-2.9f * dvx, 0, 0) : new(2.9f * dvx, 0, 0);
                }
                
            }

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "激光buff处理位置";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealPos;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        }
        [ScriptMethod(name: "万变水波", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(37118)$"])]
        public void 万变水波(Event @event, ScriptAccessory accessory)
        {
            //38360 黄
            //38360 蓝
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if(!float.TryParse(@event["SourceRotation"], out var rot)) return;
            var radian= float.Pi / 6;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"左右拆地板-1";
            dp.Scale = new(30);
            dp.Radian = radian;
            dp.Rotation= rot;
            dp.FixRotation = true;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"左右拆地板-2";
            dp.Scale = new(30);
            dp.Radian = radian;
            dp.Rotation = rot+ float.Pi / 8;
            dp.FixRotation = true;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"左右拆地板-3";
            dp.Scale = new(30);
            dp.Radian = radian;
            dp.Rotation = rot;
            dp.FixRotation = true;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"左右拆地板-4";
            dp.Scale = new(30);
            dp.Radian = radian;
            dp.Rotation = rot + float.Pi / 8;
            dp.FixRotation = true;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 9000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"左右拆地板-5";
            dp.Scale = new(30);
            dp.Radian = radian;
            dp.Rotation = rot;
            dp.FixRotation = true;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 12000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"左右拆地板-6";
            dp.Scale = new(30);
            dp.Radian = radian;
            dp.Rotation = rot + float.Pi / 8;
            dp.FixRotation = true;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 15000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"左右拆地板-7";
            dp.Scale = new(30);
            dp.Radian = radian;
            dp.Rotation = rot;
            dp.FixRotation = true;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 18000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);




        }
        [ScriptMethod(name: "挡激光提醒", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:39118"])]
        public void 挡激光提醒(Event @event, ScriptAccessory accessory)
        {
            //38345 小
            //38346 大

            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(tid);
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            int[] group = [4, 5, 6, 7, 0, 1, 2, 3];
            if(index==myIndex)
            {
                accessory.Method.TextInfo("靠后",3000,true);
                accessory.Method.TTS("靠后");
            }
            if (group[index] == myIndex)
            {
                accessory.Method.TextInfo("靠前", 3000, true);
                accessory.Method.TTS("靠前");
            }
        }


        [ScriptMethod(name: "地火收集", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(38389)$"],userControl:false)]
        public void 地火收集(Event @event, ScriptAccessory accessory)
        {
            if (!float.TryParse(@event["SourceRotation"], out var rot)) return;
            var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (spos.Z > 170) return;
            if (MathF.Abs(rot) < 0.1 || MathF.Abs(MathF.Abs(rot) - float.Pi) < 0.1) return;

            var line = (int)((spos.Z - 150) / 10);
            var d = rot > 0 ? 1 : -1;
            FloorFire[line] += d;
        }

        [ScriptMethod(name: "地火安全区位置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(38389)$"])]
        public void 地火安全区位置(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(100).ContinueWith(t =>
            {
                lock(FloorFire)
                {
                    int[] mtGroup = [0, 2, 4, 6];
                    var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                    var isMtg = mtGroup.Contains(myIndex);
                    var myLine = isMtg ? FloorFire.IndexOf(2) : FloorFire.IndexOf(-2);
                    if (myLine == -1) return;

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "地火安全区位置";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = new(isMtg ? 80.5f : 119.5f, 0, 165);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 8000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "地火安全区位置";
                    dp.Scale = new(2);
                    dp.Position = new(isMtg ? 80.5f : 119.5f, 0, 165);
                    dp.TargetPosition = new(isMtg ? 85f : 115f, 0, 150 + myLine * 10);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 8000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "地火安全区位置";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = new(isMtg ? 85f : 115f, 0, 150 + myLine * 10);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 8000;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    FloorFire = [0, 0, 0];
                }
            });
        }

        [ScriptMethod(name: "两侧激光", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(38419)$"])]
        public void 两侧激光(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "两侧激光";
            dp.Scale = new(15,40);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "中间激光", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(38417)$"])]
        public void 中间激光(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "中间激光";
            dp.Scale = new(20, 40);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "接线爆炸重置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38430"],userControl:false)]
        public void 接线爆炸重置(Event @event, ScriptAccessory accessory)
        {
            TetherBoom= [0, 0, 0, 0, 0, 0, 0, 0];
        }
        [ScriptMethod(name: "接线爆炸收集器", eventType: EventTypeEnum.ActionEffect, eventCondition: ["TargetIndex:1", "ActionId:regex:^(3843[12])$"], userControl: false)]
        public void 接线爆炸收集器(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index=accessory.Data.PartyList.IndexOf(tid);
            TetherBoom[index] += @event["ActionId"] == "38432" ? 1 : 10;
        }
        [ScriptMethod(name: "接线爆炸换buff同伴连线", eventType: EventTypeEnum.ActionEffect, eventCondition: ["TargetIndex:1", "ActionId:regex:^(38432)$"])]
        public void 接线爆炸换buff同伴连线(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(100).ContinueWith(t =>
            {
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                if (TetherBoom[myindex] ==1) return;
                var boom1 = TetherBoom.IndexOf(10);
                var boom2 = TetherBoom.LastIndexOf(10);
                var none1 = TetherBoom.IndexOf(0);
                var none2 = TetherBoom.LastIndexOf(0);

                int[] sort = [4,6,2,3,7,5];
                if (sort.IndexOf(none1) > sort.IndexOf(none2))
                {
                    (none2, none1) = (none1, none2);
                }
                if(boom1==myindex|| none1==myindex)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"接线爆炸换buff-{accessory.Data.PartyList[boom1]}";
                    dp.Scale = new(10);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.PartyList[none1];
                    dp.TargetObject = accessory.Data.PartyList[boom1];
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
                }
                if (boom2 == myindex || none2 == myindex)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"接线爆炸换buff-{accessory.Data.PartyList[boom2]}";
                    dp.Scale = new(10);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.PartyList[none2];
                    dp.TargetObject = accessory.Data.PartyList[boom2];
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
                }
                if(none2 == myindex || none1 == myindex)
                {
                    accessory.Method.TextInfo("换Buff", 9000);
                    accessory.Method.TTS("换Buff");
                }

            });
        }
        [ScriptMethod(name: "接线爆炸换buff同伴连线删除", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(4008)$"], userControl: false)]
        public void 接线爆炸换buff同伴连线删除(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            accessory.Method.RemoveDraw($"接线爆炸换buff-{tid}");
        }

        [ScriptMethod(name: "延迟左右技能存储", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3840[2345])$"],userControl:false)]
        public void 延迟左右技能存储(Event @event, ScriptAccessory accessory)
        {
            //38402 火左
            //38403 冰左
            //38404 火右
            //38405 冰右
            storeSkill = @event["ActionId"];
        }
        [ScriptMethod(name: "延迟左右技能范围", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3840[6789])$"])]
        public void 延迟左右技能范围(Event @event, ScriptAccessory accessory)
        {
            //38402 火左
            //38403 冰左
            //38404 火右
            //38405 冰右
            var left = storeSkill == "38402" || storeSkill == "38403";
            var fire= storeSkill == "38402" || storeSkill == "38404";

            if(fire)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"延迟左右技能范围-火{(left ? "左" : "右")}";
                dp.Scale = new(18);
                dp.Position = new(left ? 90 : 110, 0, 165);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"延迟左右技能范围-火{(left ? "左" : "右")}";
                dp.Scale = new(18);
                dp.Position = new(!left ? 90 : 110, 0, 165);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 6000;
                dp.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"延迟左右技能范围-冰{(left ? "左" : "右")}";
                dp.Scale = new(2,25);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(left ? 90 : 110, 0, 165);
                dp.Rotation = float.Pi;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"延迟左右技能范围-冰{(left ? "左" : "右")}";
                dp.Scale = new(2,25);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = new(!left ? 90 : 110, 0, 165);
                dp.Rotation = float.Pi;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 6000;
                dp.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
            }
            

        }

        [ScriptMethod(name: "分身左右刀", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:regex:^(456[6789])$"])]
        public void 分身左右刀(Event @event, ScriptAccessory accessory)
        {
            //4566 先右
            //4567 后右
            //4568 先左
            //4569 后左
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var left = @event["Id"] == "4568" || @event["Id"] == "4569";
            var first = @event["Id"] == "4566" || @event["Id"] == "4568";


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"分身左右刀-{(first ? "先" : "后")}{(left ? "左" : "右")}";
            dp.Scale = new(50);
            dp.Owner=sid;
            dp.Rotation = left ? float.Pi / 2 : float.Pi / -2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = first ? 0 : 8000;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "分身月环十字", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:regex:^(456[1234])$"])]
        public void 分身月环十字(Event @event, ScriptAccessory accessory)
        {
            //4561 先直线
            //4562 后直线
            //4563 先月环
            //4564 后月环
            if(parse==0) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var line = @event["Id"] == "4561" || @event["Id"] == "4562";
            var first = @event["Id"] == "4561" || @event["Id"] == "4563";

            if (line)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"分身月环十字-{(first ? "先" : "后")}{(line ? "直线" : "月环")}";
                dp.Scale = new(10,40);
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = first ? 0 : 8000;
                dp.DestoryAt = first? 8000:4000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"分身月环十字-{(first ? "先" : "后")}{(line ? "直线" : "月环")}";
                dp.Scale = new(15);
                dp.InnerScale = new(5);
                dp.Owner = sid;
                dp.Radian = float.Pi * 2;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = first ? 0 : 8000;
                dp.DestoryAt = first ? 8000 : 4000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
            }

        }

        [ScriptMethod(name: "分身月环十字收集", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:regex:^(456[1234])$"],userControl:false)]
        public void 分身月环十字收集(Event @event, ScriptAccessory accessory)
        {
            //4561 先直线
            //4563 先月环
            var pos= JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var dir8= PositionTo8Dir(pos, new(100, 0, 165));
            if (@event["Id"] == "4561" || @event["Id"] == "4563")
            {
                southSafe1st = (@event["Id"] == "4561" && dir8 % 2 != 0) || (@event["Id"] == "4563" && dir8 % 2 == 0);
            }
            else
            {
                southSafe2nd = (@event["Id"] == "4562" && dir8 % 2 != 0) || (@event["Id"] == "4564" && dir8 % 2 == 0);
            }
            
            
        }
        [ScriptMethod(name: "分身月环十字 分散分摊", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3844[34])$"])]
        public void 分身月环十字分散分摊(Event @event, ScriptAccessory accessory)
        {

            var spread = @event["ActionId"] == "38444";
            
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (spread)
            {
                int[] parterner = southSafe2nd ? [5, 7, 4, 6, 2, 0, 3, 1] : [4, 5, 6, 7, 0, 1, 2, 3];

                //accessory.Log.Debug($"parterner{parterner[myIndex]}");
                for (int i = 0; i < 8; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分身月环十字 先分散";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt =7000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                accessory.Method.TextInfo("分散", 7000);
                accessory.Method.TTS("分散");

                for (int i = 0; i < 4; i++)
                {
                    var ii = myIndex > 3 ? i : i + 4;
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分身月环十字 后分摊";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[ii];
                    dp.Color = myIndex == ii || myIndex == parterner[ii] ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.Delay = 7000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                Task.Delay(7000).ContinueWith(t =>
                {
                    accessory.Method.TextInfo("二二分摊", 4000);
                    accessory.Method.TTS("二二分摊");
                });
            }
            else
            {
                int[] parterner = southSafe1st ? [5, 7, 4, 6, 2, 0, 3, 1] : [4, 5, 6, 7, 0, 1, 2, 3];
                for (int i = 0;i < 4;i++)
                {
                    var ii = myIndex > 3 ? i : i + 4;
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分身月环十字 先分摊";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[ii];
                    dp.Color = myIndex == ii || myIndex == parterner[ii] ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 7000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                
                accessory.Method.TextInfo("二二分摊", 7000);
                accessory.Method.TTS("二二分摊");

                for (int i = 0; i < 8; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"分身月环十字 后分散";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 7000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                Task.Delay(7000).ContinueWith(t =>
                {
                    accessory.Method.TextInfo("分散", 4000);
                    accessory.Method.TTS("分散");
                });
                
            }

        }
        [ScriptMethod(name: "分身月环十字 分散分摊处理位置", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(3844[34])$"])]
        public void 分身月环十字分散分摊处理位置(Event @event, ScriptAccessory accessory)
        {
            var r = 18f / 180 * float.Pi;
            var spread = @event["ActionId"] == "38444";
            int[] rots1 = southSafe1st ? [0, 2, 6, 4, 6, 0, 4, 2] : [7, 1, 5, 3, 7, 1, 5, 3];
            int[] rots2 = southSafe2nd ? [0, 2, 6, 4, 6, 0, 4, 2] : [7, 1, 5, 3, 7, 1, 5, 3];
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (spread)
            {
                var r1 = 0f;
                if (southSafe1st)
                {
                    r1 = (myIndex > 3) ? -r : r;
                }
                else
                {
                    r1 = (myIndex > 3) ? r : -r;
                }
                var pos1 = RotatePoint(new(100, 0, 152), new(100, 0, 165), rots1[myIndex] * float.Pi / 4-r1);
                var pos2 = RotatePoint(new(100, 0, 152), new(100, 0, 165), rots2[myIndex] * float.Pi / 4);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "分身月环十字 先分散处理位置";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = pos1;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "分身月环十字 后分摊处理位置1";
                dp.Scale = new(2);
                dp.Position = pos1;
                dp.TargetPosition = pos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "分身月环十字 后分摊处理位置2";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = pos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 7000;
                dp.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            else
            {
                var r1 = 0f;
                if (southSafe2nd)
                {
                    r1 = (myIndex > 3) ? -r : r;
                }
                else
                {
                    r1 = (myIndex > 3) ? r : -r;
                }
                var pos1 = RotatePoint(new(100, 0, 152), new(100, 0, 165), rots1[myIndex] * float.Pi / 4);
                var pos2 = RotatePoint(new(100, 0, 152), new(100, 0, 165), rots2[myIndex] * float.Pi / 4 - r1);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "分身月环十字 先分摊处理位置";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = pos1;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "分身月环十字 后分散处理位置1";
                dp.Scale = new(2);
                dp.Position = pos1;
                dp.TargetPosition = pos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "分身月环十字 后分散处理位置2";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = pos2;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 7000;
                dp.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            
        }

        [ScriptMethod(name: "剑连线", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(011[78])$"])]
        public void 剑连线(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            lock (TetherLighting)
            {
                TetherLighting.Add(sid);
                var count = TetherLighting.Count;
                if (count ==1 || count == 2 || count == 3) 
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"剑连线";
                    dp.Scale = new(7);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 17000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (count == 4 || count == 5 || count == 6)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"剑连线";
                    dp.Scale = new(7);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 17500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (count == 7 || count == 8 || count == 9)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"剑连线";
                    dp.Scale = new(7);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 13000;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (count == 10 || count == 11 || count == 12)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"剑连线";
                    dp.Scale = new(7);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 13500;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (count == 13 || count == 14 || count == 15)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"剑连线";
                    dp.Scale = new(7);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 14000;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (count == 16 || count == 17 || count == 18)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"剑连线";
                    dp.Scale = new(7);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 15000;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (count == 19 || count == 20 || count == 21)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"剑连线";
                    dp.Scale = new(7);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 15500;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (count == 22 || count == 23 || count == 24)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"剑连线";
                    dp.Scale = new(7);
                    dp.Owner = sid;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 16500;
                    dp.DestoryAt = 5500;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }
            
        }

        //0 10000
        //18000 10000
        [ScriptMethod(name: "分身双人塔Id记录", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4565"],userControl:false)]
        public void 分身双人塔Id记录(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            towerId = sid;
            towerCount = 0;
        }
        [ScriptMethod(name: "分身双人塔处理位置", eventType: EventTypeEnum.PlayActionTimeline, eventCondition: ["Id:4565"])]
        public void 分身双人塔处理位置(Event @event, ScriptAccessory accessory)
        {
            lock (this)
            {
                if ((DateTime.Now - timeLock).TotalSeconds < 1) return;
            }
            timeLock = DateTime.Now;

            //var pos= JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            
            // 100,165 85,165
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (long4000[myIndex])
            {

                var obj = accessory.Data.Objects.SearchByEntityId(sid);
                var rot = obj?.Rotation ?? 0;

                var sourcepos = obj?.Position ?? default;
                var dueFace = MathF.Abs(MathF.Abs(rot) % (float.Pi / 2)) < 0.1;
                var dir4 = PositionRoundTo4Dir(sourcepos, new(100, 0, 165));
                var atEast = ((dir4 == 0 || dir4 == 2) && !dueFace) || ((dir4 == 1 || dir4 == 3) && dueFace);

                Vector3 dealpos = default;
                if (myIndex < 4) dealpos = atEast ? new(85, 0, 165) : new(100, 0, 180);
                else dealpos = atEast ? new(115, 0, 165) : new(100, 0, 150);


                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "分身双人塔处理位置-长buff第一轮";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 10000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "分身双人塔处理位置-长buff第一轮";
                dp.Scale = new(3);
                dp.Position = dealpos;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 10000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            }
            else
            {
                Task.Delay(15000).ContinueWith(t =>
                {
                    var obj = accessory.Data.Objects.SearchByEntityId(sid);
                    if (obj == null) return;
                    var rot = obj?.Rotation ?? 0;
                    var sourcepos = obj?.Position ?? default;
                    var dueFace = MathF.Abs(MathF.Abs(rot) % (float.Pi / 2)) < 0.1;
                    var dir4 = PositionRoundTo4Dir(sourcepos, new(100, 0, 165));
                    var atEast = ((dir4 == 0 || dir4 == 2) && !dueFace) || ((dir4 == 1 || dir4 == 3) && dueFace);

                    Vector3 dealpos = default;
                    if (myIndex < 4) dealpos = atEast ? new(85, 0, 165) : new(100, 0, 180);
                    else dealpos = atEast ? new(115, 0, 165) : new(100, 0, 150);


                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "分身双人塔处理位置-短buff第二轮";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 13000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "分身双人塔处理位置-短buff第二轮";
                    dp.Scale = new(3);
                    dp.Position = dealpos;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 13000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                });
            }
        }
        [ScriptMethod(name: "分身大炮处理位置", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970", "StackCount:regex:^(24[45])$"])]
        public void 分身大炮处理位置(Event @event, ScriptAccessory accessory)
        {
            
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
            var dir4 = PositionFloorTo4Dir(pos, new(100, 0, 165));
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            lock (isYellowGun)
            {
                towerCount++;
                isYellowGun[dir4] = @event["StackCount"] == "244";
                if (towerCount != 4 && towerCount != 8) return;
                if (long4000[myIndex] && towerCount == 4) return;
                if (!long4000[myIndex] && towerCount == 8) return;

                Vector3 centre = new(100, 0, 165);
                var obj = accessory.Data.Objects.SearchByEntityId(towerId);
                var rot = obj?.Rotation ?? 0;
                var sourcepos = obj?.Position ?? default;
                var dueFace = MathF.Abs(MathF.Abs(rot) - (float.Pi / 2)) < 0.1;
                var towerdir4 = PositionRoundTo4Dir(sourcepos, centre);
                var atEast = ((towerdir4 == 0 || towerdir4 == 2) && !dueFace) || ((towerdir4 == 1 || towerdir4 == 3) && dueFace);

                //{"X":110.58,"Y":-0.00,"Z":154.38}
                var myPosIndex = 0;
                if(isYelowBuff[myIndex]) myPosIndex=myIndex>3? isYellowGun.IndexOf(false) : isYellowGun.LastIndexOf(false);
                else myPosIndex = myIndex > 3 ? isYellowGun.IndexOf(true) : isYellowGun.LastIndexOf(true);

                Vector3 dealpos = default;
                if (myPosIndex == 0) dealpos = atEast ? new(107.6f, 0, 152.4f) : new(112.5f, 0, 157.5f);
                if (myPosIndex == 1) dealpos = atEast ? new(107.6f, 0, 177.6f) : new(112.5f, 0, 172.5f);
                if (myPosIndex == 2) dealpos = atEast ? new(092.4f, 0, 177.6f) : new(087.5f, 0, 172.5f);
                if (myPosIndex == 3) dealpos = atEast ? new(092.4f, 0, 152.4f) : new(087.5f, 0, 157.5f);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"分身大炮处理位置-第{(towerCount == 4?"1":"2")}轮";
                dp.Scale = new(2);
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            
        }
        [ScriptMethod(name: "分身大炮范围", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970", "StackCount:regex:^(24[45])$"])]
        public void 分身大炮范围(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["TargetId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"分身大炮范围";
            dp.Scale = new(12,40);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 4000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
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

