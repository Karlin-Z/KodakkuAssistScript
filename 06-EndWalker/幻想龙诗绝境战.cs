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

namespace KarlinScriptNamespace
{
    [ScriptType(name:"绝龙诗绘图",guid: "d9c97e91-9b59-432d-a3a1-42a8475b7e2a", version:"0.0.0.1")]
    public class DragongSingDraw
    {
        
        [UserSetting("Another Test Property")]
        public bool prop2 { get; set; } = false;
        
        private bool p1Charge=false;

        int firstTargetIcon = -1;
        uint p1BossId = 0;
        uint tordanId = 0;
        float parse = 0;
        Dictionary<string, HashSet<uint>> p3majong=new Dictionary<string, HashSet<uint>>();
        
        public void Init()
        {
            firstTargetIcon = -1;
            p1Charge = false;
            parse= 0;
            p3majong=new Dictionary<string, HashSet<uint>>();
        }

        #region P1
        [ScriptMethod(name: "P1 BossId", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:28532"],userControl:false)]
        public void P1_BossId(Event @event, ScriptAccessory accessory)
        {
            parse = 1;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                p1BossId=sid;
            }
            
        }
        [ScriptMethod(name: "P1 钢铁",eventType: EventTypeEnum.StartCasting,eventCondition: ["ActionId:25307"])]
        public void P1_钢铁(Event @event, ScriptAccessory accessory)
        {
            var dp=accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(6);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"],out var sid))
            {
                dp.Owner= sid;
            }
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);
        }
        [ScriptMethod(name: "P1 月环", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25306"])]
        public void P1_月环(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(70);
            dp.InnerScale = new(6);
            dp.Radian = float.Pi * 2;
            dp.Color=accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }

        [ScriptMethod(name: "P1 苍穹炽焰", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25309"])]
        public void P1_苍穹炽焰(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_苍穹炽焰";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }

        [ScriptMethod(name: "P1 直线多维空间斩", eventType: EventTypeEnum.TargetIcon)]
        public void P1_直线多维空间斩(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            if (ParsTargetIcon(@event["Id"]) != 0) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
           
            dp.Scale = new(8,70);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = p1BossId;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.TargetObject = tid;
            }
            dp.DestoryAt = 6000;
            dp.Name = $"P1 直线多维空间斩{tid:X}";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "P1 次元裂缝危险区", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:13071"])]
        public void P1_次元裂缝危险区(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(9);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.DestoryAt = 60000;
            dp.Name = $"P1 次元裂缝危险区{id:X}";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P1 次元裂缝危险区移除", eventType: EventTypeEnum.RemoveCombatant, eventCondition: ["DataId:13071"],userControl:false)]
        public void P1_次元裂缝危险区移除(Event @event, ScriptAccessory accessory)
        {
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                accessory.Method.RemoveDraw($"P1 次元裂缝危险区{id:X}");
            }
        }

        [ScriptMethod(name: "P1 光芒剑(火神冲)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25294"])]
        public void P1_光芒剑(Event @event, ScriptAccessory accessory)
        {
            if (p1Charge) return;
            p1Charge = true;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_光芒剑(火神冲)";
            dp.Scale = new(9);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7000;

            if (float.TryParse(@event["SourceRotation"],out var r))
            {
                if(MathF.Abs(r+float.Pi/4)<0.1 || MathF.Abs(r - float.Pi *0.75f) < 0.1)
                {
                    dp.Position = new(111, 0, 111);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    dp.Position = new(89, 0, 89);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (MathF.Abs(r - float.Pi / 4) < 0.1 || MathF.Abs(r + float.Pi * 0.75f) < 0.1)
                {
                    dp.Position = new(111, 0, 89);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    dp.Position = new(89, 0, 111);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }


            dp.Position = new(78, 0, 100);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Position = new(92.52f, 0, 100);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Position = new(107.48f, 0, 100);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Position = new(122, 0, 100);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Position = new(100, 0, 78);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Position = new(100, 0, 92.52f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Position = new(100, 0, 107.48f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Position = new(100, 0, 122);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }

        [ScriptMethod(name: "P1 击退预测", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25308"])]
        public void P1_击退预测(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(1.5f,16);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.Owner = accessory.Data.Me;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.TargetObject = sid;
            }
            dp.Rotation = float.Pi;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "P1 光翼闪", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25316"])]
        public void P1_光翼闪(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(18);
            dp.Radian = float.Pi / 6;
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.Delay = 10000;
            dp.DestoryAt = 20000;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan , dp);
            dp.TargetOrderIndex = 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp); 
        }

        [ScriptMethod(name: "P1 苍穹刻印玩家", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2661"])]
        public void P1_苍穹刻印玩家(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(3);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            
        }
        [ScriptMethod(name: "P1 苍穹刻印落地", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25370"])]
        public void P1_苍穹刻印落地(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_苍穹刻印落地";
            dp.Scale = new(3);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.DestoryAt = 2500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        #endregion

        #region P2
        #region 一运
        [ScriptMethod(name: "P2 1运记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25555"],userControl:false)]
        public void P2_1运记录(Event @event, ScriptAccessory accessory)
        {
            parse = 2.1f;
            firstTargetIcon = -1;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                tordanId = id;
            }
        }
        [ScriptMethod(name: "P2 1运波勒克兰冲锋", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:3781"])]
        public void P2_1运波勒克兰冲锋(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1f) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(16,52);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2 1运伊尼亚斯冲锋", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:3782"])]
        public void P2_1运伊尼亚斯冲锋(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1f) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(16, 52);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2 1运韦尔吉纳冲锋", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:3783"])]
        public void P2_1运韦尔吉纳冲锋(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1f) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(16, 52);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2 1运地震", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25558"])]
        public void P2_1运地震(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1f) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }

            dp.Scale = new(6);
            dp.DestoryAt = 6000;
            dp.Radian = float.Pi * 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp.Scale = new(12);
            dp.InnerScale= new(6);
            dp.Delay = 4000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp.Scale = new(18);
            dp.InnerScale = new(12);
            dp.Delay = 6000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp.Scale = new(24);
            dp.InnerScale = new(18);
            dp.Delay = 8000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp.Scale = new(30);
            dp.InnerScale = new(24);
            dp.Delay = 10000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        [ScriptMethod(name: "P2 1运穿天(大圈)", eventType: EventTypeEnum.TargetIcon)]
        public void P2_1运穿天(Event @event, ScriptAccessory accessory)
        {
            if(parse != 2.1f) return;
            if (ParsTargetIcon(@event["Id"]) != 0) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(24);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Delay = 7000;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            
        }
        [ScriptMethod(name: "P2 1运让勒努冲锋", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:2551"])]
        public void P2_1运让勒努冲锋(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1f) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(8, 50);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
            dp.Delay = 3000;
            dp.DestoryAt = 3000;
            dp.ScalingByDistance = true;
            dp.ScalingY = true;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2 1运阿代尔菲尔冲锋", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:2550"])]
        public void P2_1运阿代尔菲尔冲锋(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1f) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(8, 50);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
            dp.Delay = 3000;
            dp.DestoryAt = 3000;
            dp.ScalingByDistance = true;
            dp.ScalingY = true;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2 1运骑神位置(Imgui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25563"])]
        public void P2_1运骑神位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1f) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(8, 50);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.TargetObject = tordanId;
            dp.Owner = accessory.Data.Me;
            dp.ScalingByDistance = true;
            dp.ScalingY = true;
            dp.Delay = 500;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
        }

        #endregion

        #region 二运
        [ScriptMethod(name: "P2 2运记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25569"], userControl: false)]
        public void P2_2运记录(Event @event, ScriptAccessory accessory)
        {
            parse = 2.2f;
        }
        [ScriptMethod(name: "P2 2运龙眼背对", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:8003759A", "Id:00020001"])]
        public void P2_2运龙眼背对(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2f) return;
            var index=int.Parse(@event["Index"],System.Globalization.NumberStyles.HexNumber);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.Me;
            dp.Delay = 4500;
            dp.DestoryAt = 5000;
            if (index == 0) dp.TargetPosition = new(100, 0, 65);
            if (index == 1) dp.TargetPosition = new(124.75f, 0, 75.25f);
            if (index == 2) dp.TargetPosition = new(135, 0, 100);
            if (index == 3) dp.TargetPosition = new(124.75f, 0, 124.75f);
            if (index == 4) dp.TargetPosition = new(100, 0, 135);
            if (index == 5) dp.TargetPosition = new(75.25f, 0, 124.75f);
            if (index == 6) dp.TargetPosition = new(65, 0, 100);
            if (index == 7) dp.TargetPosition = new(75.25f, 0, 75.25f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp);
        }
        [ScriptMethod(name: "P2 2运骑神背对", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25552"])]
        public void P2_2运骑神背对(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2f) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.Me;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.TargetObject = id;
            }
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp);

        }
        [ScriptMethod(name: "P2 2运泽菲兰位置(Imgui)", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:2549"])]
        public void P2_2运泽菲兰位置(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.TargetObject = id;
            }
            dp.Owner = accessory.Data.Me;
            dp.DestoryAt = 5000;
            dp.ScalingByDistance =true;
            dp.ScalingY =true;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
        }
        [ScriptMethod(name: "P2 2运光球爆炸范围", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:13070"])]
        public void P2_2运光球爆炸范围(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2f) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(9);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.DestoryAt = 2000;
            
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }


        #endregion
        [ScriptMethod(name: "P2 骑神奋力一挥（右)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25536"])]
        public void P2_骑神奋力一挥_右(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(40);
            dp.Radian = float.Pi / 180 * 130;
            dp.Rotation= float.Pi / 180 * -65;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp.Color = accessory.Data.DefaultSafeColor;
            dp.TargetColor = accessory.Data.DefaultDangerColor;
            dp.Rotation = float.Pi;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "P2 骑神奋力一挥（左)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25537"])]
        public void P2_骑神奋力一挥_左(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(40);
            dp.Radian = float.Pi / 180 * 130;
            dp.Rotation = float.Pi / 180 * 65;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp.Color = accessory.Data.DefaultSafeColor;
            dp.TargetColor = accessory.Data.DefaultDangerColor;
            dp.Rotation = float.Pi;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }

        #endregion

        #region P3
        [ScriptMethod(name: "P3 记录", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26376"],userControl:false)]
        public void P3_记录(Event @event, ScriptAccessory accessory)
        {
            parse = 3f;
        }
        [ScriptMethod(name: "P3 牙尾连旋(钢铁月环)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26386"])]
        public void P3_牙尾连旋(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(8);
            dp.Radian = float.Pi *2;
            dp.DestoryAt = 11500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp.Scale = new(40);
            dp.InnerScale = new(8);
            dp.Delay = 11500;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

        }
        [ScriptMethod(name: "P3 尾牙连旋(月环钢铁)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26387"])]
        public void P3_尾牙连旋(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(40);
            dp.InnerScale= new(8);
            dp.Radian = float.Pi * 2;
            dp.DestoryAt = 11500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp.Delay = 11500;
            dp.Scale = new(8);
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "P3 原地塔预测", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26382"])]
        public void P3_原地塔预测(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(5);
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P3 上箭头塔预测", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26383"])]
        public void P3_上箭头塔预测(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Offset = new(0, 0, -14);
            dp.Scale = new(5);
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P3 下箭头塔预测", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26384"])]
        public void P3_下箭头塔预测(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Offset = new(0, 0, 14);
            dp.Scale = new(5);
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P3 塔位置确定", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26385"])]
        public void P3_塔位置确定(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(5);
            dp.DestoryAt = 2500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P3 武神枪引导", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26385"])]
        public void P3_武神枪引导(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(8,62);
            dp.DestoryAt = 2500;
            dp.TargetResolvePattern=PositionResolvePatternEnum.PlayerNearestOrder;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P3 武神枪确定", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26378"])]
        public void P3_武神枪确定(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(8, 62);
            dp.DestoryAt = 4500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P3 同组麻将连线(ImGui)", eventType: EventTypeEnum.StatusAdd)]
        public void P3_同组麻将连线(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3f) return;
            var stasusid = @event["StatusID"];
            if (stasusid != "3004" && stasusid != "3005" && stasusid != "3006") return;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                if (p3majong.ContainsKey(stasusid))
                {
                    p3majong[stasusid].Add(id);
                }
                else
                {
                    p3majong.Add(stasusid, []);
                    p3majong[stasusid].Add(id);
                }
            }


            if (id == accessory.Data.Me)
            {
                Task.Delay(100).ContinueWith((o) =>
                {
                    foreach (var tid in p3majong[stasusid])
                    {
                        var dp=accessory.Data.GetDefaultDrawProperties();
                        dp.Owner = id;
                        dp.TargetObject = tid;
                        dp.Color=accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 6000;
                        dp.ScalingByDistance= true;
                        dp.ScalingY= true;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
                    }
                });

            }
        }
        [ScriptMethod(name: "P3 腾龙枪", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:26380"])]
        public void P3_腾龙枪(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(13);
            dp.Radian = float.Pi / 2;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
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
        private int ParsTargetIcon(string id)
        {
            if (firstTargetIcon == -1)
            {
                firstTargetIcon= int.Parse(id, System.Globalization.NumberStyles.HexNumber);
            }
            return int.Parse(id, System.Globalization.NumberStyles.HexNumber) - firstTargetIcon;
        }
    }
}

