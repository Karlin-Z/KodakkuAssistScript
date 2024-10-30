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
    [ScriptType(name:"绝龙诗绘图", territorys: [968], guid: "d9c97e91-9b59-432d-a3a1-42a8475b7e2a", version:"0.0.0.3")]
    public class DragongSingDraw
    {
        
        [UserSetting("P5 一运连线冲锋显示延迟(ms)")]
        public int p5TetherCrashDelay { get; set; } = 3000;

        [UserSetting("P6 分散分摊标记")]
        public bool p6Mark {  get; set; }=false;

        [UserSetting("P7 死亡轮回116分摊")]
        public bool p7_116 { get; set; } = true;

        object lockObj=new object();
        
        bool p1Charge=false;
        bool p3TowerDeal = false;
        bool p5Deal = false;
        
        int? firstTargetIcon = null;
        uint p1GrenoId = 0;
        uint p1AdelId = 0;
        uint p3BossId = 0;
        uint p6FireBallCount;
        uint p6FireBallCount2;
        uint tordanId = 0;
        uint darkDragonId = 0;
        uint whiteDragonId = 0;

        double parse = 0;
        Vector3 p2AdelPos = Vector3.Zero;
        Vector3 p2ZPos = Vector3.Zero;
        Vector3 p5DivePos = Vector3.Zero;
        Vector3 p5GrenoPos = Vector3.Zero;
        Vector3 p5GreekPos = Vector3.Zero;
        Vector3 p6WhitePos = Vector3.Zero;
        Vector3 p7Stone1 = Vector3.Zero;
        Vector3 p7Stone2 = Vector3.Zero;
        Dictionary<string, HashSet<uint>> p3majong=new Dictionary<string, HashSet<uint>>();
        List<uint> p2BlueCircle = [];
        List<int> p1sony = [];
        List<bool> p2SafeDir = [];
        List<bool> p2Stone = [];
        List<bool> p2Tower = [];
        List<bool> p3Boom = [];
        List<int> p3Tower = [];
        List<int> p2StoneTeam = [];
        List<int> p5sony = [];
        List<int> p6tether = [];
        List<int> p6lightDark = [];

        (int, int) p2Jump = (-1,-1);
        (int, int) p2StoneMem = (-1, -1);

        


        public void Init(ScriptAccessory accessory)
        {
            parse = 0;

            p6FireBallCount = 0;
            p6FireBallCount2 = 0;

            firstTargetIcon =null;
            p1Charge = false;
            p3TowerDeal = false;
            p5Deal = false;

            p3majong =new Dictionary<string, HashSet<uint>>();
            p5DivePos = Vector3.Zero;
            p5GrenoPos = Vector3.Zero;
            p5GreekPos = Vector3.Zero;
            p5sony = [0, 0, 0, 0, 0, 0, 0, 0];
            p1sony = [0, 0, 0, 0, 0, 0, 0, 0];
            p3Tower = [0,0,0,0];
            p6tether = [0, 0, 0, 0, 0, 0, 0, 0];
            p6lightDark= [0, 0, 0, 0, 0, 0, 0, 0];
            p2BlueCircle = [];
            p2SafeDir = [true, true, true, true, true, true, true, true];
            p2Stone = [false, false, false, false, false, false, false, false];
            p2Tower = [false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false];
            p3Boom = [false,false,false,false];
            p2Jump = (-1, -1);

            accessory.Method.MarkClear();

        }

        #region P1
        [ScriptMethod(name: "P1 BossId", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:28532"],userControl:false)]
        public void P1_BossId(Event @event, ScriptAccessory accessory)
        {
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                p1GrenoId=sid;
            }
        }
        [ScriptMethod(name: "P1 阶段记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25300"],userControl:false)]
        public void P1_阶段记录(Event @event, ScriptAccessory accessory)
        {
            if(parse==0) { parse = 1; }
            parse = Math.Round(parse + 0.1, 1);
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                p1AdelId = sid;
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
            if (parse <1|| parse>=2) return;
            if (ParsTargetIcon(@event["Id"]) != 0) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
           
            dp.Scale = new(8,70);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = p1GrenoId;
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

        [ScriptMethod(name: "P1 光芒剑阿代尔斐尔位置(ImGui)", eventType: EventTypeEnum.Targetable, eventCondition: ["Targetable:True"])]
        public void P1_光芒剑阿代尔斐尔位置(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1.1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (sid != p1AdelId) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_光芒剑阿代尔斐尔位置";
            dp.TargetObject = sid;
            dp.Owner = accessory.Data.Me;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);


        }
        [ScriptMethod(name: "P1 光芒剑(火神冲)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25294"])]
        public void P1_光芒剑(Event @event, ScriptAccessory accessory)
        {
            if (p1Charge) return;
            p1Charge = true;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(9);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7000;

            if (float.TryParse(@event["SourceRotation"],out var r))
            {
                if(MathF.Abs(r+float.Pi/4)<0.1 || MathF.Abs(r - float.Pi *0.75f) < 0.1)
                {
                    dp.Name = "P1_光芒剑(111.00,111.00)";
                    dp.Position = new(111, 0, 111);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    dp.Name = "P1_光芒剑(89.00,89.00)";
                    dp.Position = new(89, 0, 89);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
                if (MathF.Abs(r - float.Pi / 4) < 0.1 || MathF.Abs(r + float.Pi * 0.75f) < 0.1)
                {
                    dp.Name = "P1_光芒剑(111.00,89.00)";
                    dp.Position = new(111, 0, 89);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    dp.Name = "P1_光芒剑(89.00,111.00)";
                    dp.Position = new(89, 0, 111);
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            }

            dp.Name = "P1_光芒剑(78.00,100.00)";
            dp.Position = new(78, 0, 100);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_光芒剑(92.52,100.00)";
            dp.Position = new(92.52f, 0, 100);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_光芒剑(107.48,100.00)";
            dp.Position = new(107.48f, 0, 100);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_光芒剑(122.00,100.00)";
            dp.Position = new(122, 0, 100);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_光芒剑(100.00,78.00)";
            dp.Position = new(100, 0, 78);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_光芒剑(100,92.52.00)";
            dp.Position = new(100, 0, 92.52f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_光芒剑(100.00,107.48)";
            dp.Position = new(100, 0, 107.48f);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Name = "P1_光芒剑(100.00,122.00)";
            dp.Position = new(100, 0, 122);
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "P1 光球爆炸范围移除", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25295"], userControl: false)]
        public void P1_光球爆炸范围移除(Event @event, ScriptAccessory accessory)
        {
            if (parse > 2) return;
            var pos= JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var name = $"P1_光芒剑\\({pos.X:f2},{pos.Z:f2}\\)";
            accessory.Method.RemoveDraw(name);
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
        [ScriptMethod(name: "P1 索尼记录", eventType: EventTypeEnum.TargetIcon, userControl: false)]
        public void P1_索尼记录(Event @event, ScriptAccessory accessory)
        {
            
            if (parse != 1.2) return;
            var sony = ParsTargetIcon(@event["Id"]) - 47;
            if (sony < 0 || sony > 3) return;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                var index = accessory.Data.PartyList.ToList().IndexOf(id);
                p1sony[index] = sony;
            }
        }
        [ScriptMethod(name: "P1 索尼击退位置(ImGui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25308"])]
        public void P1_索尼击退位置(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"parse{parse}");
            if (parse !=1.2) return;
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new(1);
            dp.Owner = accessory.Data.Me;
            dp.DestoryAt = 4000;
            dp.ScaleMode |= ScaleMode.YByDistance;

            var index = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            
                
            var cpos = new Vector3(100, 0, 100);
            var npos = new Vector3(100, 0, 96);
            
            //○
            if (p1sony[index] == 0)
            {
                var p1= RotatePoint(npos, cpos, float.Pi / 2);
                var p2= RotatePoint(npos, cpos, float.Pi / -2);
                
                dp.Name= "P1索尼○1";
                dp.TargetPosition = p1;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                dp.Name = "P1索尼○2";
                dp.TargetPosition = p2;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            //▽
            if (p1sony[index] == 1)
            {
                if(index==2||index==3)
                {
                    var p = RotatePoint(npos, cpos, float.Pi / -4); 
                    dp.Name = "P1索尼▽奶";
                    dp.TargetPosition = p;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                else
                {
                    var p = RotatePoint(npos, cpos, float.Pi / 4 * 3);
                    dp.Name = "P1索尼▽D";
                    dp.TargetPosition = p;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }
            //□
            if (p1sony[index] == 2)
            {
                if (index == 0 || index == 1)
                {
                    var p = RotatePoint(npos, cpos, float.Pi / 4);
                    dp.Name = "P1索尼□T";
                    dp.TargetPosition = p;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                else
                {
                    var p = RotatePoint(npos, cpos, float.Pi / -4 * 3);
                    dp.Name = "P1索尼□D";
                    dp.TargetPosition = p;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }
            //×
            if (p1sony[index] == 3)
            {
                if (index == 0 || index == 1)
                {
                    var p = npos;
                    dp.Name = "P1索尼×T";
                    dp.TargetPosition = p;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                else
                {
                    var p = RotatePoint(npos, cpos, float.Pi);
                    dp.Name = "P1索尼×D";
                    dp.TargetPosition = p;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }


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
            parse = 2.1;
            firstTargetIcon = null;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                tordanId = id;
            }
        }
        [ScriptMethod(name: "P2 1运波勒克兰冲锋", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:3781"])]
        public void P2_1运波勒克兰冲锋(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
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
            if (parse != 2.1) return;
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
            if (parse != 2.1) return;
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
        [ScriptMethod(name: "P2 1运冲锋位置记录", eventType: EventTypeEnum.NpcYell,userControl:false)]
        public void P2_1运冲锋位置记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var str= @event["Id"];
            if (str != "3781" && str != "3782" && str != "3783") return;
            var sourcePos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var dir= PositionTo8Dir(sourcePos, new(100, 0, 100));
            if (dir == 0 || dir == 4)
            {
                p2SafeDir[0] = false;
                p2SafeDir[4] = false;
            }
            if (dir == 1 || dir == 5)
            {
                p2SafeDir[1] = false;
                p2SafeDir[5] = false;
            }
            if (dir == 2 || dir == 6)
            {
                p2SafeDir[2] = false;
                p2SafeDir[6] = false;
            }
            if (dir == 3 || dir == 7)
            {
                p2SafeDir[3] = false;
                p2SafeDir[7] = false;
            }
        }
        [ScriptMethod(name: "P2 1运冲锋安全区位置(Imgui)", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:3781"])]
        public void P2_1运冲锋安全区位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            Task.Delay(100).ContinueWith(y =>
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2 1运冲锋安全区位置";
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.DestoryAt = 7000;

                var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
                var cpos = new Vector3(100, 0, 100);
                var npos = new Vector3(100, 0, 82);
                //MT
                if(idIndex==0|| idIndex == 2 || idIndex == 4 || idIndex == 6)
                {
                    dp.TargetPosition = RotatePoint(npos, cpos, p2SafeDir.LastIndexOf(true) * float.Pi / 4);
                }
                else//ST
                {
                    dp.TargetPosition = RotatePoint(npos, cpos, p2SafeDir.IndexOf(true) * float.Pi / 4);
                }

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            });
            
        }
        [ScriptMethod(name: "P2 1运地震", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25558"])]
        public void P2_1运地震(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Name = "P2 1运地震";
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
        [ScriptMethod(name: "P2 1运穿天记录", eventType: EventTypeEnum.TargetIcon,userControl:false)]
        public void P2_1运穿天记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            if (ParsTargetIcon(@event["Id"]) != 0) return;
            
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                p2BlueCircle.Add(id);
            }
        }
        [ScriptMethod(name: "P2 1运空间破碎", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25564"])]
        public void P2_1运空间破碎(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2 1运空间破碎";
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }

            dp.Scale = new(9);
            dp.Delay = 3000;
            dp.DestoryAt = 9000- dp.Delay;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

           
        }
        [ScriptMethod(name: "P2 1运穿天(大圈)", eventType: EventTypeEnum.TargetIcon)]
        public void P2_1运穿天(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            if (ParsTargetIcon(@event["Id"]) != 0) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(24);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Delay = 6000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            
        }
        [ScriptMethod(name: "P2 一运穿天连线(ImGui)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25562"])]
        public void P2_一运穿天连线(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2 一运穿天连线";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = p2BlueCircle[0];
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 9000;
            for (int i = 1; i < p2BlueCircle.Count; i++)
            {
                dp.TargetObject= p2BlueCircle[i];
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            }
        }
        [ScriptMethod(name: "P2 1运让勒努冲锋", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:2551"])]
        public void P2_1运让勒努冲锋(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
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
            dp.ScaleMode |= ScaleMode.YByDistance;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2 1运阿代尔菲尔冲锋", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:2550"])]
        public void P2_1运阿代尔菲尔冲锋(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
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
            dp.ScaleMode |= ScaleMode.YByDistance;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P2 1运骑神位置(Imgui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25563"])]
        public void P2_1运骑神位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2 1运骑神位置";
            dp.Scale = new(8, 50);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.TargetObject = tordanId;
            dp.Owner = accessory.Data.Me;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = 500;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
        }

        #endregion

        #region 二运
        [ScriptMethod(name: "P2 二运记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25569"], userControl: false)]
        public void P2_二运记录(Event @event, ScriptAccessory accessory)
        {
            parse = 2.2;
        }
        [ScriptMethod(name: "P2 二运龙眼背对", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:8003759A", "Id:00020001"])]
        public void P2_二运龙眼背对(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            var index=int.Parse(@event["Index"],System.Globalization.NumberStyles.HexNumber);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_二运龙眼背对";
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
        [ScriptMethod(name: "P2 二运骑神背对", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25552"])]
        public void P2_二运骑神背对(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_二运骑神背对";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.Me;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.TargetObject = id;
            }
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp);

        }
        [ScriptMethod(name: "P2 二运泽菲兰位置记录", eventType: EventTypeEnum.NpcYell, eventCondition: ["Id:2549"], userControl: false)]
        public void P2_二运泽菲兰位置记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            p2ZPos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            
        }
        [ScriptMethod(name: "P2 二运劈刀记录", eventType: EventTypeEnum.TargetIcon, userControl: false)]
        public void P2_二运劈刀记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            var tid = ParsTargetIcon(@event["Id"]);
            if (tid != -279 && tid != -280) return;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                var index = accessory.Data.PartyList.ToList().IndexOf(id);
                if (tid == -280) p2Jump.Item1 = index;
                if (tid == -279) p2Jump.Item2 = index;
            }
        }
        [ScriptMethod(name: "P2 二运阿代尔菲尔位置", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:12601"], userControl: false)]
        public void P2_二运阿代尔菲尔位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            p2AdelPos=JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        }
        [ScriptMethod(name: "P2 二运劈刀起跑位置(Imgui)", eventType: EventTypeEnum.TargetIcon)]
        public void P2_二运劈刀起跑位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (ParsTargetIcon(@event["Id"]) != -279) return;
            Task.Delay(100).ContinueWith(t =>
            {
                List<int> g1Mem = [];
                List<int> g2Mem = [];

                g1Mem.Add(p2Jump.Item1);
                g2Mem.Add(p2Jump.Item2);
                if (g1Mem.IndexOf(0) == -1 && g2Mem.IndexOf(0) == -1) g2Mem.Add(0);
                if (g1Mem.IndexOf(1) == -1 && g2Mem.IndexOf(1) == -1) g1Mem.Add(1);
                if (g1Mem.IndexOf(2) == -1 && g2Mem.IndexOf(2) == -1) g2Mem.Add(2);
                if (g1Mem.IndexOf(3) == -1 && g2Mem.IndexOf(3) == -1) g1Mem.Add(3);
                if (g1Mem.IndexOf(6) == -1 && g2Mem.IndexOf(6) == -1) g2Mem.Add(6);
                if (g1Mem.IndexOf(7) == -1 && g2Mem.IndexOf(7) == -1) g1Mem.Add(7);
                if (g1Mem.IndexOf(4) == -1 && g1Mem.IndexOf(4) == -1)
                {
                    if (g2Mem.Count == 3) g2Mem.Add(4);
                    else g1Mem.Add(4);
                }
                if (g1Mem.IndexOf(5) == -1 && g1Mem.IndexOf(5) == -1)
                {
                    if (g1Mem.Count == 3) g1Mem.Add(4);
                    else g2Mem.Add(4);
                }
                var drot = p2AdelPos.X > 100? float.Pi / 45: float.Pi / -45;
                var meIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Scale = new(1.5f, 20);
                dp.Color = accessory.Data.DefaultSafeColor.WithW(3);
                dp.Owner = accessory.Data.Me;
                dp.DestoryAt = 5000;
                dp.ScaleMode |= ScaleMode.YByDistance;

                var cpos = new Vector3(100, 0, 100);
                var sPos = (p2ZPos - cpos) / 15 * 19.5f + cpos;
                if (g1Mem.IndexOf(meIndex) != -1)
                {
                    dp.TargetPosition = RotatePoint(sPos, cpos, float.Pi + drot * 3);
                }
                else
                {
                    dp.TargetPosition = RotatePoint(sPos, cpos, drot * 3);
                }

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);



                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Color = accessory.Data.DefaultSafeColor.WithW(3);
                dp2.Scale = new(1.5f, 20);
                dp2.ScaleMode |= ScaleMode.YByDistance;
                dp2.DestoryAt = 15000;
                dp2.Position=dp.TargetPosition;
                dp2.TargetPosition = RotatePoint(dp2.Position.Value, cpos, drot * 5);
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
            });
        }
        [ScriptMethod(name: "P2 二运光球爆炸范围", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:13070"])]
        public void P2_二运光球爆炸范围(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(9);
            dp.Color = accessory.Data.DefaultDangerColor;
            var idStr = @event["SourceId"];
            if (ParseObjectId(idStr, out var id))
            {
                dp.Owner = id;
            }
            dp.DestoryAt = 2000;
            dp.Name = $"P2二运光球爆炸范围{idStr}";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P2 光球爆炸范围移除", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25295"],userControl:false)]
        public void P2_光球爆炸范围移除(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            accessory.Method.RemoveDraw($"P2二运光球爆炸范围{@event["SourceId"]}");
        }
        [ScriptMethod(name: "P2 二运陨石记录", eventType: EventTypeEnum.TargetIcon,userControl:false)]
        public void P2_2运陨石记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            if (ParsTargetIcon(@event["Id"]) != -45) return;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                p2Stone[accessory.Data.PartyList.ToList().IndexOf(id)] = true;
            }
            var s1 = p2Stone.IndexOf(true);
            var s2 = p2Stone.LastIndexOf(true);
            //记录分组
            if (s1 != s2)
            {
                p2StoneMem = (s1, s2);
                //AB mt h2
                if (s1 == 0 && s2 == 3)
                {
                    p2StoneTeam = [0, 4, 5, 1, 3, 7, 6, 2];
                }
                //AB d14
                if (s1 == 4 && s2 == 7)
                {
                    p2StoneTeam = [4, 0, 5, 1, 7, 3, 6, 2];
                }
                //AC 双t
                if (s1 == 0 && s2 == 1)
                {
                    p2StoneTeam = [0, 4, 7, 3, 1, 5, 6, 2];
                }
                //AC d12
                if (s1 == 4 && s2 == 5)
                {
                    p2StoneTeam = [4, 0, 7, 3, 5, 1, 6, 2];
                }
                //AD mt H1
                if (s1 == 0 && s2 == 2)
                {
                    p2StoneTeam = [0, 4, 7, 3, 2, 6, 5, 1];
                }
                //AD d13
                if (s1 == 4 && s2 == 6)
                {
                    p2StoneTeam = [4, 0, 7, 3, 6, 2, 5, 1];
                }
                //BC h2 st
                if (s1 == 1 && s2 == 3)
                {
                    p2StoneTeam = [3, 7, 4, 0, 1, 5, 6, 2];
                }
                //BC d24
                if (s1 == 5 && s2 == 7)
                {
                    p2StoneTeam = [7, 3, 4, 0, 5, 1, 6, 2];
                }
                //BD h12
                if (s1 == 2 && s2 == 3)
                {
                    p2StoneTeam = [4, 0, 3, 7, 5, 1, 2, 6];
                }
                //BD d34
                if (s1 == 6 && s2 == 7)
                {
                    p2StoneTeam = [4, 0, 7, 3, 5, 1, 6, 2];
                }
                //CD st h1
                if (s1 == 1 && s2 == 2)
                {
                    p2StoneTeam = [2, 6, 7, 3, 1, 5, 4, 0];
                }
                //CD d23
                if (s1 == 5 && s2 == 6)
                {
                    p2StoneTeam = [6, 2, 7, 3, 5, 1, 4, 0];
                }
            }
        }
        [ScriptMethod(name: "P2 二运陨石连线(ImGui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25576"])]
        public void P2_2运陨石连线(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            Task.Delay(100).ContinueWith(t =>
            {
                
                var s1 = p2Stone.IndexOf(true);
                var s2 = p2Stone.LastIndexOf(true);
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Owner = accessory.Data.PartyList[s1];
                dp.TargetObject = accessory.Data.PartyList[s2];
                dp.DestoryAt = 12000;
                dp.Name = "P2 2运陨石双人连线(ImGui)";
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            });

        }
        [ScriptMethod(name: "P2 二运冰分摊位置(ImGui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25576"])]
        public void P2_2运冰分摊位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            Task.Delay(100).ContinueWith(t =>
            {
                var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
                var dir4=p2StoneTeam.IndexOf(idIndex)/2;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2 2运冰分摊位置(ImGui)";
                dp.Scale = new(3f, 10);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(new(100,0,88.5f),new(100,0,100),float.Pi/2*dir4);
                dp.DestoryAt = 7000;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            });
            

            
            //accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


        }
        [ScriptMethod(name: "P2 二运第一轮塔记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29564"],userControl:false)]
        public void P2_二运第一轮塔记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            var sourcePos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var cpos = new Vector3(100, 0, 100);
            if ((sourcePos-cpos).Length() > 7)
            {
                var dir = (PositionTo12Dir(sourcePos, cpos) + 1) % 12;
                p2Tower[dir] = true;
            }
            else
            {
                var dir = PositionTo8Dir(sourcePos, cpos) / 2 + 12;
                p2Tower[dir] = true;
            }
        }
        [ScriptMethod(name: "P2 二运第一轮塔位置(ImGui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:29563"])]
        public void P2_二运第一轮塔位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;
            Task.Delay(100).ContinueWith(t =>
            {
                List<int> towerMem = [-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1];
                List<int> alternate = [];
                //高优先级
                for (int i = 0; i < 4; i++)
                {
                    var MemIndex = p2StoneTeam[i * 2];
                    //中
                    if (p2Tower[i * 3 + 1])
                    {
                        towerMem[i * 3 + 1] = MemIndex;
                        continue;
                    }
                    //左
                    if (p2Tower[i * 3])
                    {
                        towerMem[i * 3] = MemIndex;
                        continue;
                    }
                    //右
                    if (p2Tower[i * 3 + 2])
                    {
                        towerMem[i * 3 + 2] = MemIndex;
                        continue;
                    }
                }

                //低优先级
                for (int i = 0; i < 4; i++)
                {
                    var MemIndex = p2StoneTeam[i * 2 + 1];
                    //左
                    if (p2Tower[i * 3] && towerMem[i * 3] == -1)
                    {
                        towerMem[i * 3] = MemIndex;
                        continue;
                    }
                    //右
                    if (p2Tower[i * 3 + 2] && towerMem[i * 3 + 2] == -1)
                    {
                        towerMem[i * 3 + 2] = MemIndex;
                        continue;
                    }
                    //内左
                    if (p2Tower[i + 12] && towerMem[i + 12] == -1)
                    {
                        towerMem[i + 12] = MemIndex;
                        continue;
                    }
                    //补塔
                    alternate.Add(MemIndex);
                }

                //补塔
                foreach (var mem in alternate)
                {
                    for (int i = 12; i < 16; i++)
                    {
                        if (p2Tower[i] && towerMem[i] == -1)
                        {
                            towerMem[i] = mem;
                            break;
                        }
                    }
                }

                var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
                var npos = new Vector3(100, 0, 82);
                var npos2 = new Vector3(100, 0, 94);
                var cpos = new Vector3(100, 0, 100);
                var dp = accessory.Data.GetDefaultDrawProperties();
                var tIndex = towerMem.IndexOf(idIndex);
                if (tIndex >= 0 && tIndex < 12)
                {
                    dp.Position = RotatePoint(npos, cpos, float.Pi / 6 * (tIndex - 1));
                }
                if (tIndex >= 12 && tIndex < 16)
                {
                    dp.Position = RotatePoint(npos2, cpos, float.Pi / 2 * (tIndex - 12) + float.Pi / 4);
                }

                dp.Name = "P2 2运第一轮塔位置(ImGui)";
                dp.DestoryAt = 12000;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Scale = new(3);
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = "P2 2运第一轮塔位置(ImGui)";
                dp2.Color= accessory.Data.DefaultSafeColor;
                dp2.Owner = accessory.Data.Me;
                dp2.TargetPosition = dp.Position;
                dp2.Scale = new(3f, 10);
                dp2.ScaleMode |= ScaleMode.YByDistance;
                dp2.Delay = 7500;
                dp2.DestoryAt = 4500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);

            });
        }
        [ScriptMethod(name: "P2 二运第二轮塔位置(ImGui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28650"])]
        public void P2_二运第二轮塔位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 2.2) return;

            var index = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            var posIndex =p2StoneTeam.IndexOf(index);
            if (index == p2StoneMem.Item1) posIndex = p2StoneTeam.IndexOf(p2StoneMem.Item2);
            if (index == p2StoneMem.Item2) posIndex = p2StoneTeam.IndexOf(p2StoneMem.Item1);

            var npos = new Vector3(100, 0, 82);
            var cpos = new Vector3(100, 0, 100);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.TargetPosition = RotatePoint(npos, cpos, float.Pi / 4 * posIndex);
            

            dp.Name = "P2 2运第一轮塔位置(ImGui)";
            dp.DestoryAt = 11000;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Position = dp.TargetPosition;
            dp.Scale = new(3);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);


        }
        #endregion
        [ScriptMethod(name: "P2 二运结束记录", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25533"],userControl:false)]
        public void P2_二运结束记录(Event @event, ScriptAccessory accessory)
        {
            parse = 2.3;
        }
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
            parse = 3;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                p3BossId = id;
            }
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
            dp.Offset = new(0, 0, -14);
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
        [ScriptMethod(name: "P3 麻将武神枪引导", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26385"])]
        public void P3_麻将武神枪引导(Event @event, ScriptAccessory accessory)
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
        [ScriptMethod(name: "P3 四塔武神枪引导", eventType: EventTypeEnum.StartCasting)]
        public void P3_四塔武神枪引导(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            var aid = @event["ActionId"];
            if(aid!= "26391" && aid != "26392" && aid != "26393" && aid != "26394") return;
            var str = @event["SourceId"];
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Name = $"P3_四塔武神枪引导{str}";
            if (ParseObjectId(str, out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(8, 62);
            dp.Delay = 5000;
            dp.DestoryAt = 2500;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "P3 四塔武神枪移除", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0054"],userControl:false)]
        public void P3_四塔武神枪移除(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            accessory.Method.RemoveDraw($"P3_四塔武神枪引导{@event["SourceId"]}");
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
            if (parse != 3) return;
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
                        dp.Name = "P3 同组麻将连线";
                        dp.Owner = id;
                        dp.TargetObject = tid;
                        dp.Color=accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 6000;
                        dp.ScaleMode |= ScaleMode.YByDistance;
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
        [ScriptMethod(name: "P3 四塔记录", eventType: EventTypeEnum.StartCasting,userControl:false)]
        public void P3_四塔记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            var aid = @event["ActionId"];
            if (aid != "26391" && aid != "26392" && aid != "26393" && aid != "26394") return;
            var num=int.Parse(aid)-26390;
            var sourcePos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var dir = PositionTo8Dir(sourcePos, new(100, 0, 100))/2;
            p3Tower[dir] = num;
        }
        [ScriptMethod(name: "P3 四塔站位(ImGui)", eventType: EventTypeEnum.StartCasting)]
        public void P3_四塔站位(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3 || p3TowerDeal) return;
            var aid = @event["ActionId"];
            if (aid != "26391" && aid != "26392" && aid != "26393" && aid != "26394") return;
            p3TowerDeal = true;
            
            Task.Delay(100).ContinueWith(t =>
            {
                var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
                var myTower = -1;
                //D4
                if (idIndex == 7) { myTower = 0; }
                //H2
                if (idIndex == 3) { myTower = 1; }
                //H1
                if (idIndex == 2) { myTower = 2; }
                //D3
                if (idIndex == 6) { myTower = 3; }
                //St
                if (idIndex == 1) 
                {
                    
                    if (p3Tower[0] >= 2) { myTower = 0;}
                    else
                    {
                        if (p3Tower[1] > 2) { myTower = 1; }
                        else if (p3Tower[3] > 2) { myTower = 3; }
                        else if (p3Tower[2] > 2) { myTower = 2; }
                    }
                }
                //D2
                if (idIndex == 5)
                {
                    if (p3Tower[1] >= 2) { myTower = 1; }
                    else
                    {
                        if (p3Tower[2] > 2) { myTower = 2; }
                        else if (p3Tower[0] > 2) { myTower = 0; }
                        else if (p3Tower[3] > 2) { myTower = 3; }
                    }
                }
                //D1
                if (idIndex == 4)
                {
                    if (p3Tower[2] >= 2) { myTower = 2; }
                    else
                    {
                        if (p3Tower[3] > 2) { myTower = 3; }
                        else if (p3Tower[1] > 2) { myTower = 1; }
                        else if (p3Tower[0] > 2) { myTower = 0; }
                    }
                }
                //Mt
                if (idIndex == 0)
                {
                    if (p3Tower[3] >= 2) { myTower = 3; }
                    else
                    {
                        if (p3Tower[0] > 2) { myTower = 0; }
                        else if (p3Tower[2] > 2) { myTower = 2; }
                        else if (p3Tower[1] > 2) { myTower = 1; }
                    }
                }

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Position=RotatePoint(new(108,0,92),new(100,0,100),float.Pi/2*myTower);
                dp.Scale = new(5);
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

            });
        }
        [ScriptMethod(name: "P3 追魂炮T辅助(ImGui)", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0054"])]
        public void P3_追魂T炮辅助(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            if (!ParseObjectId(@event["SourceId"], out var id)) return;
            var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            if ((idIndex == 0 && id == p3BossId && !p3Boom[0]) || (idIndex == 1 && id != p3BossId && !p3Boom[1]))
            {
                p3Boom[idIndex] = true;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = $"P3 追魂炮{(idIndex==0?"M":"S")}T辅助";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = id;
                dp.Scale = new(10);
                dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);
            }
        }
        [ScriptMethod(name: "P3 追魂炮范围", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0054"])]
        public void P3_追魂炮范围(Event @event, ScriptAccessory accessory)
        {
            if (parse != 3) return;
            if (!ParseObjectId(@event["SourceId"], out var id)) return;
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_追魂炮范围";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = id;
            dp.Scale = new(5);
            dp.CentreResolvePattern = PositionResolvePatternEnum.OwnerTarget;
            dp.DestoryAt = 7000;
            if (id == p3BossId && !p3Boom[2])
            {
                p3Boom[2] = true;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            if (id != p3BossId && !p3Boom[3])
            {
                p3Boom[3] = true;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }

        #endregion

        #region P4
        [ScriptMethod(name: "P4 记录", eventType: EventTypeEnum.CancelAction, eventCondition: ["ActionId:29750"], userControl: false)]
        public void P4_记录(Event @event, ScriptAccessory accessory)
        {
            parse = 4;
        }

        #endregion

        #region P5
        #region 一运
        [ScriptMethod(name: "P5 一运记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27529"], userControl: false)]
        public void P5_一运记录(Event @event, ScriptAccessory accessory)
        {
            parse = 5.1;
        }
        [ScriptMethod(name: "P5 一运旋风冲", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27531"])]
        public void P5_一运旋风冲(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(10,60);
            dp.DestoryAt = 6000;
            dp.Name = $"P5一运旋风冲";
            
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            
        }
        [ScriptMethod(name: "P5 一运白龙位置连线(ImGui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27531"])]
        public void P5_一运白龙位置连线(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.TargetObject = id;
            }
            dp.Owner = accessory.Data.Me;
            dp.Scale = new(5);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 6000;
            dp.Name = $"P5一运白龙位置连线";

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);

        }
        [ScriptMethod(name: "P5 一运双骑士螺旋枪", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0005"])]
        public void P5_一运双骑士连线(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.TargetObject = tid;
            }

            dp.Scale = new(16, 60);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = p5TetherCrashDelay;
            dp.DestoryAt = 6000- p5TetherCrashDelay;
            dp.Name = $"P5一运双骑士连线冲锋";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5 一运雷翼", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2833"])]
        public void P5_一运雷翼(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(5);
            dp.DestoryAt = 5000;
            dp.Delay = int.TryParse(@event["DurationMilliseconds"], out var d) ? (d - dp.DestoryAt) :8000;
            dp.Name = $"P5一运雷翼{id:X8}";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "P5 一运穿天", eventType: EventTypeEnum.TargetIcon)]
        public void P5_一运穿天(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1 || ParsTargetIcon(@event["Id"]) != -316) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor.WithW(0.5f);
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(24);
            dp.Delay = 2000;
            dp.DestoryAt = 4000;
            dp.Name = $"P5一运穿天{id:X8}";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "P5 阿斯卡隆之仁・揭示", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25546"])]
        public void P5_阿斯卡隆之仁揭示(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(50);
            dp.Radian = float.Pi / 180 * 30;
            dp.DestoryAt = 4000;
            foreach (var tid in accessory.Data.PartyList)
            {
                dp.Name = $"P5 阿斯卡隆之仁・揭示 {tid:X8}";
                dp.TargetObject = tid;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }




        }
        [ScriptMethod(name: "P5 一运双龙俯冲处理位置记录", eventType: EventTypeEnum.SetObjPos,eventCondition: ["SourceDataId:12603"],userControl:false)]
        public void P5_一运双龙俯冲处理位置记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var pos= JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            p5DivePos = new((pos.X - 100) / 9 * 19 + 100, pos.Y, (pos.Z - 100) / 9 * 19 + 100);
        }
        [ScriptMethod(name: "P5 一运双龙俯冲处理位置", eventType: EventTypeEnum.TargetIcon)]
        public void P5_一运双龙俯冲处理位置(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1 || ParsTargetIcon(@event["Id"]) != -310) return;
            if (!ParseObjectId(@event["TargetId"], out var id) || id!=accessory.Data.Me) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor.WithW(2f);
            dp.Owner = id;
            dp.TargetPosition=p5DivePos;
            dp.Scale = new(1,60);
            dp.DestoryAt = 5000;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Name = $"P5一运双龙俯冲处理位置";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

        }
        [ScriptMethod(name: "P5 一运白龙俯冲", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27534"])]
        public void P5_一运白龙俯冲(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(20, 48);
            dp.DestoryAt = 6000;
            dp.Name = $"P5一运白龙俯冲";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5 一运黑龙俯冲", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27533"])]
        public void P5_一运黑龙俯冲(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(20, 48);
            dp.DestoryAt = 6000;
            dp.Name = $"P5一运黑龙俯冲";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5 一运格里诺位置记录", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:12602"], userControl: false)]
        public void P5_一运格里诺位置记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            p5GrenoPos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        }
        [ScriptMethod(name: "P5 一运连线格里诺(ImGui)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:25546"])]
        public void P5_一运连线格里诺(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5 一运连线格里诺";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = p5GrenoPos;
            dp.Scale = new(5);
            dp.DestoryAt = 8000;
            dp.ScaleMode |= ScaleMode.YByDistance;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Line, dp);

        }
        #endregion

        #region 二运
        [ScriptMethod(name: "P5 二运记录", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27538"], userControl: false)]
        public void P5_二运记录(Event @event, ScriptAccessory accessory)
        {
            parse = 5.2;
            p5sony = [0, 0, 0, 0, 0, 0, 0, 0];
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                tordanId = id;
            }
        }
        [ScriptMethod(name: "P5 二运黑龙俯冲", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27533"])]
        public void P5_二运黑龙俯冲(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(20, 48);
            dp.DestoryAt = 6000;
            dp.Name = $"P5一运黑龙俯冲";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5 二运旋风冲", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27531"])]
        public void P5_二运旋风冲(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }

            dp.Scale = new(10, 60);
            dp.DestoryAt = 6000;
            dp.Name = $"P5二运旋风冲";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5 二运战女神之枪", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27539"])]
        public void P5_二运战女神之枪(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(10, 50);
            dp.DestoryAt = 6000;
            dp.Name = $"P5二运战女神之枪";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5 二运地震", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:25558"])]
        public void P5_二运地震(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.Name = "P5_二运地震";

            dp.Scale = new(6);
            dp.DestoryAt = 6000;
            dp.Radian = float.Pi * 2;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp.Scale = new(12);
            dp.InnerScale = new(6);
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
        [ScriptMethod(name: "P5 二运龙眼背对", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:8003759A", "Id:00020001"])]
        public void P5_二运龙眼背对(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var index = int.Parse(@event["Index"], System.Globalization.NumberStyles.HexNumber);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.Me;
            dp.Delay = 16000;
            dp.DestoryAt = 7000;
            if (index == 0) dp.TargetPosition = new(100, 0, 65);
            if (index == 1) dp.TargetPosition = new(124.75f, 0, 75.25f);
            if (index == 2) dp.TargetPosition = new(135, 0, 100);
            if (index == 3) dp.TargetPosition = new(124.75f, 0, 124.75f);
            if (index == 4) dp.TargetPosition = new(100, 0, 135);
            if (index == 5) dp.TargetPosition = new(75.25f, 0, 124.75f);
            if (index == 6) dp.TargetPosition = new(65, 0, 100);
            if (index == 7) dp.TargetPosition = new(75.25f, 0, 75.25f);
            dp.Name = "P5 二运龙眼背对";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp);
        }
        [ScriptMethod(name: "P5 二运骑神背对", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:8003759A", "Id:00020001"])]
        public void P5_二运骑神背对(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = tordanId;
            dp.Delay = 16000;
            dp.DestoryAt = 7000;
            
            dp.Name = "P5 二运骑神背对";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp);
        }
        [ScriptMethod(name: "P5 二运索尼记录", eventType: EventTypeEnum.TargetIcon,userControl:false)]
        public void P5_二运索尼记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            var sony = ParsTargetIcon(@event["Id"])+49;
            if (sony < 0 || sony > 3) return;
            if(ParseObjectId(@event["TargetId"], out var id))
            {
                var index= accessory.Data.PartyList.ToList().IndexOf(id);
                p5sony[index] += sony;
            }
        }
        [ScriptMethod(name: "P5 二运死宣记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2976"],userControl: false)]
        public void P5_二运死宣记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                var index = accessory.Data.PartyList.ToList().IndexOf(id);
                p5sony[index] +=10;
            }
        }
        [ScriptMethod(name: "P5 二运盖里克位置记录", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:12637"],userControl:false)]
        public void P5_二运盖里克位置记录(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            p5GreekPos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        }
        [ScriptMethod(name: "P5 二运死宣六方站位(ImGui)", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2976"])]
        public void P5_二运死宣六方站位(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            Task.Delay(100).ContinueWith(t =>
            {
                if (p5Deal) return;
                var count = p5sony.Where(s => s > 5).Count();
                if (count != 4) return;
                p5Deal = true;
                var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
                var sony = p5sony[idIndex];
                var posid = sony > 0 ? 4 : 0;
                for (int i = 0; i < idIndex; i++)
                {
                    if(sony== p5sony[i])
                    {
                        posid++;
                    }
                }
                var cpos = new Vector3(100, 0, 100);
                var npos = 19.5f*Vector3.Normalize(new(p5GreekPos.X - 100, p5GreekPos.Y, p5GreekPos.Z - 100)) + cpos;
                if(posid==4||posid==7) { npos = 13 * Vector3.Normalize(new(p5GreekPos.X - 100, p5GreekPos.Y, p5GreekPos.Z - 100)) + cpos; }
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.Scale = new(1.5f, 60);
                dp.DestoryAt = 7000;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Name = $"P5二运死宣引导站位{sony}";

                var d = float.Pi / 180f;
                if (posid == 0) dp.TargetPosition = RotatePoint(npos, cpos, d * -90);
                if (posid == 1) dp.TargetPosition = RotatePoint(npos, cpos, d * -142.5f);
                if (posid == 2) dp.TargetPosition = RotatePoint(npos, cpos, d * 142.5f);
                if (posid == 3) dp.TargetPosition = RotatePoint(npos, cpos, d * 90);
                if (posid == 4) dp.TargetPosition = RotatePoint(npos, cpos, d * -90);
                if (posid == 5) dp.TargetPosition = RotatePoint(npos, cpos, d * -37.5f);
                if (posid == 6) dp.TargetPosition = RotatePoint(npos, cpos, d * 37.5f);
                if (posid == 7) dp.TargetPosition = RotatePoint(npos, cpos, d * 90);
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            });
        }
        [ScriptMethod(name: "P5 二运索尼引导站位(ImGui)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:27533"])]
        public void P5_二运索尼引导站位(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;

            var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            var sony = p5sony[idIndex];
            var posid = sony > 0 ? 4 : 0;
            for (int i = 0; i < idIndex; i++)
            {
                if (sony == p5sony[i])
                {
                    posid++;
                }
            }
            var cpos = new Vector3(100, 0, 100);
            var npos = 10 * Vector3.Normalize(new(p5GreekPos.X - 100, p5GreekPos.Y, p5GreekPos.Z - 100)) + cpos;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.Scale = new(1.5f, 60);
            dp.DestoryAt = 8000;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Name = $"P5二运索尼引导站位{sony}";

            var d = float.Pi / 180f;
            dp.TargetPosition = cpos;
            if (posid == 4) dp.TargetPosition = RotatePoint(npos, cpos, d * -90);
            if (posid == 7) dp.TargetPosition = RotatePoint(npos, cpos, d * 90);


            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P5 二运索尼处理位置(横排法)(ImGui)", eventType: EventTypeEnum.TargetIcon)]
        public void P5_二运索尼处理位置_横排法(Event @event, ScriptAccessory accessory)
        {
            if (parse != 5.2) return;
            if (!ParseObjectId(@event["TargetId"], out var id) || id != accessory.Data.Me) return;
            Task.Delay(100).ContinueWith(ca =>
            {
                var index = accessory.Data.PartyList.ToList().IndexOf(id);
                var sony =p5sony[index];
                var priority = p5sony.IndexOf(sony) == index;
                var cpos = new Vector3(100, 0, 100);
                var npos = 4*Vector3.Normalize(new(p5GreekPos.X-100,p5GreekPos.Y,p5GreekPos.Z-100))+ cpos;
                var npos2 = 20f * Vector3.Normalize(new(p5GreekPos.X - 100, p5GreekPos.Y, p5GreekPos.Z - 100)) + cpos;


                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = id;
                dp.Scale = new(3, 60);
                dp.DestoryAt = 5000;
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Name = $"P5二运索尼{sony}处理位置";

                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Color = accessory.Data.DefaultSafeColor;
                dp2.Scale = new(1f);
                dp2.DestoryAt = 5000;
                dp2.Name = $"P5二运索尼{sony}击退终点";
                //死宣○
                if (sony == 10)
                {
                    if (priority)
                    {
                        dp.TargetPosition = RotatePoint(npos, cpos, float.Pi / -2);
                        dp2.Position = RotatePoint(npos2, cpos, float.Pi / -2);
                    }
                    else 
                    { 
                        dp.TargetPosition = RotatePoint(npos, cpos, float.Pi / 2);
                        dp2.Position = RotatePoint(npos2, cpos, float.Pi / 2);
                    }
                }
                //死宣▽
                if (sony == 11)
                {
                    dp.TargetPosition = RotatePoint(npos, cpos, float.Pi * 0.75f);
                    dp2.Position = RotatePoint(npos2, cpos, float.Pi * 0.75f);
                }
                //死宣□
                if (sony == 12)
                {
                    dp.TargetPosition = RotatePoint(npos, cpos, float.Pi * -0.75f);
                    dp2.Position = RotatePoint(npos2, cpos, float.Pi * -0.75f);
                }
                //▽
                if (sony == 1)
                {
                    dp.TargetPosition = RotatePoint(npos, cpos, float.Pi * -0.25f);
                    dp2.Position = RotatePoint(npos2, cpos, float.Pi * -0.25f);
                }
                //□
                if (sony == 2)
                {
                    dp.TargetPosition = RotatePoint(npos, cpos, float.Pi * 0.25f);
                    dp2.Position = RotatePoint(npos2, cpos, float.Pi * 0.25f);
                }
                //×
                if (sony == 3)
                {
                    if (priority)
                    {
                        dp.TargetPosition = npos;
                        dp2.Position = npos2;
                    }
                    else
                    {
                        dp.TargetPosition = RotatePoint(npos, cpos, float.Pi);
                        dp2.Position = RotatePoint(npos2, cpos, float.Pi);
                    }
                }


                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp2);
            });

        }



        #endregion
        #endregion

        #region P6
        [ScriptMethod(name: "P6 开场记录", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26215"], userControl: false)]
        public void P6_开场记录(Event @event, ScriptAccessory accessory)
        {
            parse = 6.1;
            p6FireBallCount = 0;
            p6FireBallCount2 = 0;
        }
        [ScriptMethod(name: "P6 阶段累加", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27969"],userControl:false)]
        public void P6_阶段累加(Event @event, ScriptAccessory accessory)
        {
            parse=Math.Round(parse + 0.1, 1);
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                whiteDragonId = id;
            }
        }
        [ScriptMethod(name: "P6 黑龙ID", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27971"], userControl: false)]
        public void P6_黑龙ID(Event @event, ScriptAccessory accessory)
        {
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                darkDragonId = id;
            }
        }
        [ScriptMethod(name: "P6 白龙位置id记录", eventType: EventTypeEnum.SetObjPos, eventCondition: ["SourceDataId:12613"], userControl: false)]
        public void P6_白龙位置id记录(Event @event, ScriptAccessory accessory)
        {
            p6WhitePos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                whiteDragonId = sid;
            }

        }
        [ScriptMethod(name: "P6 开场冰火线收集", eventType: EventTypeEnum.Tether, userControl: false)]
        public void P6_开场冰火线收集(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.1) return;
            
            if (!ParseObjectId(@event["SourceId"],out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            p6tether[accessory.Data.PartyList.ToList().IndexOf(sid)] = tid==whiteDragonId ? 2 : 1;

        }
        [ScriptMethod(name: "P6 第一次冰火线站位(Imgui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27960"])]
        public void P6_第一次冰火线站位(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.1) return;
            
            List<Vector3> postions = [new(100, 0, 109.33f), new(95.7f, 0, 119), new(104.3f, 0, 119)];
            //45 26 37
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P6 第一次冰火线站位";
            dp.Owner = accessory.Data.Me;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Scale = new(1.5f);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 7000;
            var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            //D1
            if (idIndex == 4) dp.TargetPosition = postions[0];
            if (idIndex == 2) dp.TargetPosition = postions[1];
            if (idIndex == 3) dp.TargetPosition = postions[2];
            //D2
            if (idIndex == 5)
            {
                if (p6tether[4]!= p6tether[5])
                {
                    dp.TargetPosition = postions[0];
                }
                else
                {
                    if(p6tether[2] == p6tether[6])
                    {
                        dp.TargetPosition = postions[1];
                    }
                    else
                    {
                        dp.TargetPosition = postions[2];
                    }
                }
            }
            //D3
            if (idIndex == 6)
            {
                if (p6tether[2] != p6tether[6])
                {
                    dp.TargetPosition = postions[1];
                }
                else
                {
                    if (p6tether[4] == p6tether[5])
                    {
                        dp.TargetPosition = postions[0];
                    }
                    else
                    {
                        dp.TargetPosition = postions[2];
                    }
                }
            }
            //D4
            if (idIndex == 7)
            {
                if (p6tether[3] != p6tether[7])
                {
                    dp.TargetPosition = postions[2];
                }
                else
                {
                    if (p6tether[4] == p6tether[5])
                    {
                        dp.TargetPosition = postions[0];
                    }
                    else
                    {
                        dp.TargetPosition = postions[1];
                    }
                }
            }
            if (idIndex >1)
            {
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            
        }
        [ScriptMethod(name: "P6 第一次冰火线黑龙扇形", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27955"])]
        public void P6_第一次冰火线黑龙扇形(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(50);
            dp.Radian = float.Pi / 6f;
            dp.DestoryAt = 7000;
            dp.Name = "P6 第一次冰火线黑龙扇形";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "P6 第一次冰火线白龙扇形", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27957"])]
        public void P6_第一次冰火线白龙扇形(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(50);
            dp.Radian = float.Pi / 6f;
            dp.DestoryAt = 7000;
            dp.Name = "P6 第一次冰火线白龙扇形";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }

        [ScriptMethod(name: "P6 无尽轮回", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27969"], userControl: false)]
        public void P6_无尽轮回(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P6 无尽轮回";
            dp.Scale = new(4);
            dp.DestoryAt = 8300;
            var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            if(idIndex==0|| idIndex == 2 || idIndex == 4 || idIndex == 6)
            {
                dp.Owner = accessory.Data.PartyList[2];
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default,DrawTypeEnum.Circle,dp);

                dp.Owner = accessory.Data.PartyList[3];
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else
            {
                dp.Owner = accessory.Data.PartyList[3];
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                dp.Owner = accessory.Data.PartyList[2];
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }

        }
        [ScriptMethod(name: "P6 灭杀誓言分散", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:27960"], userControl: false)]
        public void P6_灭杀誓言分散(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.DestoryAt = 7300;
            for (int i = 4; i < accessory.Data.PartyList.Count; i++)
            {
                dp.Name = $"P6 灭杀誓言分散 D{i-3}";
                dp.Owner = accessory.Data.PartyList[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }
        [ScriptMethod(name: "P6 灭杀誓言范围", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2896"])]
        public void P6_灭杀誓言范围(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(5);
            dp.DestoryAt = 5000;
            dp.Delay = int.TryParse(@event["DurationMilliseconds"], out var d) ? d - 5000 : 0;
            
            dp.Name = $"P6 灭杀誓言";

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "P6 第一次冰火线安全点(ImGui)", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:26215"])]
        public void P6_第一次冰火线安全点(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.1) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultSafeColor;
            
            dp.Scale = new(0.2f);
            dp.Delay = 20000;
            dp.DestoryAt = 15000;
            dp.Name = $"P6 第一次冰火线安全点";

            dp.Position = new(95.7f, 0, 119);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
            dp.Position = new(104.3f, 0, 119);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
            dp.Position = new(100, 0, 109.33f);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P6 神圣之翼(左近)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27939"])]
        public void P6_神圣之翼左近(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(10);
            dp.DestoryAt = 8000;
            dp.CentreResolvePattern=PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            dp.Name = "P6_神圣之翼近1";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.CentreOrderIndex = 2;
            dp.Name = "P6_神圣之翼近2";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.Scale = new(22, 50);
            dp2.Owner = id;
            dp2.DestoryAt = 8000;
            dp2.Offset = new(-11, 0, 0);
            dp2.Name = "P6_神圣之翼左";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);

        }
        [ScriptMethod(name: "P6 神圣之翼(左远)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27940"])]
        public void P6_神圣之翼左远(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(10);
            dp.DestoryAt = 8000;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.CentreOrderIndex = 1;
            dp.Name = "P6_神圣之翼远1";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.CentreOrderIndex = 2;
            dp.Name = "P6_神圣之翼远2";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.Scale = new(22, 50);
            dp2.Owner = id;
            dp2.DestoryAt = 8000;
            dp2.Offset = new(-11, 0, 0);
            dp2.Name = "P6_神圣之翼左";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);

        }
        [ScriptMethod(name: "P6 神圣之翼(右近)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27942"])]
        public void P6_神圣之翼右近(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(10);
            dp.DestoryAt = 8000;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            dp.Name = "P6_神圣之翼近1";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.CentreOrderIndex = 2;
            dp.Name = "P6_神圣之翼近2";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.Scale = new(22, 50);
            dp2.Owner = id;
            dp2.DestoryAt = 8000;
            dp2.Offset = new(11, 0, 0);
            dp2.Name = "P6_神圣之翼右";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);

        }
        [ScriptMethod(name: "P6 神圣之翼(右远)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27943"])]
        public void P6_神圣之翼右远(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(10);
            dp.DestoryAt = 8000;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.CentreOrderIndex = 1;
            dp.Name = "P6_神圣之翼远1";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.CentreOrderIndex = 2;
            dp.Name = "P6_神圣之翼远2";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Color = accessory.Data.DefaultDangerColor;
            dp2.Scale = new(22, 50);
            dp2.Owner = id;
            dp2.DestoryAt = 8000;
            dp2.Offset = new(11, 0, 0);
            dp2.Name = "P6_神圣之翼右";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp2);

        }
        [ScriptMethod(name: "P6 第一次黑龙俯冲", eventType: EventTypeEnum.StartCasting)]
        public void P6_第一次黑龙俯冲(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.2) return;
            if (!uint.TryParse(@event["ActionId"], out var actionId)) return;
            if (actionId != 27939 && actionId != 27940 && actionId != 27942 && actionId != 27943) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(22, 80);
            dp.Owner = darkDragonId;
            dp.DestoryAt = 7500;
            dp.Name = "P6_第一次黑龙俯冲";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P6 燃烧之翼", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27948"])]
        public void P6_燃烧之翼(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(21, 50);
            dp.DestoryAt = 6500;
            dp.Name = "P6 燃烧之翼";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P6 燃烧之尾", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27950"])]
        public void P6_燃烧之尾(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var id))
            {
                dp.Owner = id;
            }
            dp.Scale = new(18, 50);
            dp.DestoryAt = 6500;
            dp.Name = "P6 燃烧之尾";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P6 火球范围", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:13238"])]
        public void P6_火球范围(Event @event, ScriptAccessory accessory)
        {
            lock (lockObj)
            {
                p6FireBallCount++;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Color=accessory.Data.DefaultDangerColor;
                //第一轮
                if (p6FireBallCount == 3)
                {
                    dp.Name = "P6 火球范围1";
                    dp.Scale = new(18, 44);
                    dp.Position = new Vector3(100, 0, 100);
                    dp.DestoryAt = 12000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                    dp.Rotation = float.Pi / 2;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                }
                //第二轮
                if (p6FireBallCount == 6)
                {
                    dp.Name = "P6 火球范围2";
                    dp.Scale = new(18, 70);
                    var ipos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                    var pos = new Vector3(100, 0, 100);
                    if (ipos.X < 93.5f) pos.X = 87;
                    if (ipos.X > 106.5f) pos.X = 113;
                    if (ipos.Z < 93.5f) pos.Z = 87;
                    if (ipos.Z > 106.5f) pos.Z = 113;
                    dp.Position = pos;
                    dp.Delay = 6000;
                    dp.DestoryAt = 12000 - dp.Delay;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                    dp.Rotation = float.Pi / 2;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                }
                if (p6FireBallCount == 9)
                {
                    dp.Name = "P6 火球范围3";
                    dp.Scale = new(18, 70);
                    var ipos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                    var pos = new Vector3(100, 0, 100);
                    if (ipos.X < 93.5f) pos.X = 87;
                    if (ipos.X > 106.5f) pos.X = 113;
                    if (ipos.Z < 93.5f) pos.Z = 87;
                    if (ipos.Z > 106.5f) pos.Z = 113;
                    dp.Position = pos;
                    dp.Delay = 8000;
                    dp.DestoryAt = 12000- dp.Delay;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                    dp.Rotation = float.Pi / 2;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);
                }
            }
        }
        [ScriptMethod(name: "P6 十字火白龙俯冲", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:27973"])]
        public void P6_十字火白龙俯冲(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = whiteDragonId;
            dp.Scale = new(22, 80);
            dp.Delay = 1500;
            dp.DestoryAt = 11000- dp.Delay;
            dp.Name = "P6 十字火白龙俯冲";
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        
        [ScriptMethod(name: "P6 十字火起跑位置(ImGui)", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:13238"])]
        public void P6_十字火起跑位置(Event @event, ScriptAccessory accessory)
        {
            lock (lockObj)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                p6FireBallCount2++;
                if (p6FireBallCount2 == 6)
                {
                    dp.Name = "P6 十字火起跑位置";
                    dp.Scale = new(1.5f);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color=accessory.Data.DefaultSafeColor;
                    dp.Owner = accessory.Data.Me;

                    var ipos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                    var pos = new Vector3(100, 0, 100);
                    if (ipos.Z < 93.5f) pos.Z = 109.5f;
                    if (ipos.Z > 106.5f) pos.Z = 90.5f;
                    if (p6WhitePos.X < 99) pos.X = 121.5f;
                    else pos.X = 78.5f;

                    dp.TargetPosition = pos;
                    dp.DestoryAt = 6000;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    
                }
                
            }
        }
        [ScriptMethod(name: "P6 第二次冰火线ND站位(ImGui)", eventType: EventTypeEnum.StartCasting)]
        public void P6_第二次冰火线ND站位(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.3) return;
            var aidStr = @event["ActionId"];
            if (aidStr != "27956" && aidStr != "27957") return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P6 第二次冰火线ND站位";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.Scale = new(1.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.DestoryAt = 7000;

            var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);

            if (idIndex == 2) dp.TargetPosition = new(100, 0, 80.5f);
            if (idIndex == 3) dp.TargetPosition = new(100, 0, 119.7f);
            if (idIndex == 4) dp.TargetPosition = new(103.7f, 0, 89.2f);
            if (idIndex == 5) dp.TargetPosition = new(97, 0, 110.2f);
            if (idIndex == 6) dp.TargetPosition = new(107.2f, 0, 81.7f);
            if (idIndex == 7) dp.TargetPosition = new(92.5f, 0, 118);

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        }
        [ScriptMethod(name: "P6 双龙冰火俯冲", eventType: EventTypeEnum.StatusAdd)]
        public void P6_双龙冰火俯冲(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.3) return;
            var StatusIDStr = @event["StatusID"];
            if (StatusIDStr != "2898" && StatusIDStr != "2899") return;
            if (!ParseObjectId(@event["TargetId"], out var id) || id != accessory.Data.Me) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(22, 56);
            dp.Delay= 6500;
            dp.DestoryAt = 12500-dp.Delay;
            if (StatusIDStr == "2898")
            {
                dp.Name = "P6 双龙冰火俯冲 黑龙 火 危险";
                dp.Owner = darkDragonId;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

                dp.Name = "P6 双龙冰火俯冲 白龙 火 安全";
                dp.Owner = whiteDragonId;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            else
            {
                dp.Name = "P6 双龙冰火俯冲 黑龙 冰 安全";
                dp.Owner = darkDragonId;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

                dp.Name = "P6 双龙冰火俯冲 白龙 冰 危险";
                dp.Owner = whiteDragonId;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            

        }

        [ScriptMethod(name: "P6 双龙冰火俯冲 T黑龙", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27966"])]
        public void P6_双龙冰火俯冲_T黑龙(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.3) return;
            var index= accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            if (index == 0)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Scale = new(22, 56);
                dp.DestoryAt = 5000;
                dp.Name = "P6 双龙冰火俯冲 MT黑龙 危险";
                dp.Owner = darkDragonId;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            if (index == 1)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Scale = new(22, 56);
                dp.DestoryAt = 5000;
                dp.Name = "P6 双龙冰火俯冲 ST黑龙 安全";
                dp.Owner = darkDragonId;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }
        [ScriptMethod(name: "P6 双龙冰火俯冲 T白龙", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27966"])]
        public void P6_双龙冰火俯冲_T白龙(Event @event, ScriptAccessory accessory)
        {
            if (parse != 6.3) return;
            var index = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);
            if (index == 0)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Scale = new(22, 56);
                dp.DestoryAt = 5000;
                dp.Name = "P6 双龙冰火俯冲 MT白龙 安全";
                dp.Owner = whiteDragonId;
                dp.Color = accessory.Data.DefaultSafeColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            if (index == 1)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Scale = new(22, 56);
                dp.DestoryAt = 5000;
                dp.Name = "P6 双龙冰火俯冲 ST白龙 危险";
                dp.Owner = whiteDragonId;
                dp.Color = accessory.Data.DefaultDangerColor;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }

        [ScriptMethod(name: "P6 暗buff记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2758"], userControl: false)]
        public void P6_暗buff记录(Event @event, ScriptAccessory accessory)
        {
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                p6lightDark[accessory.Data.PartyList.ToList().IndexOf(id)] = 1;
            }
            
        }
        [ScriptMethod(name: "P6 光buff记录", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2759"], userControl: false)]
        public void P6_光buff记录(Event @event, ScriptAccessory accessory)
        {
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                p6lightDark[accessory.Data.PartyList.ToList().IndexOf(id)] = 2;
            }

        }
        [ScriptMethod(name: "P6 邪念之炎/同归于尽之炎", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27974"])]
        public void P6_邪念之炎(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(18000).ContinueWith(t =>
            {
                var plist = accessory.Data.PartyList.ToList();
                var idIndex = plist.IndexOf(accessory.Data.Me);
                for (int i = 0; i < p6lightDark.Count; i++)
                {
                    if (p6lightDark[i] == 0) continue;
                    if (p6lightDark[i] == 1)
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Owner = plist[i];
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.Scale = new(5);
                        dp.DestoryAt = 5000;
                        dp.Name = "P6 邪念之炎";
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    }
                    if(p6lightDark[i] == 2)
                    {
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Owner = plist[i];
                        dp.Scale = new(4);
                        dp.DestoryAt = 5000;
                        dp.Name = "P6 同归于尽之炎";
                        if (i==idIndex ||(p6lightDark.IndexOf(2)==i && p6lightDark.IndexOf(0)==idIndex)|| (p6lightDark.LastIndexOf(2) == i && p6lightDark.LastIndexOf(0) == idIndex))
                        {
                            dp.Color = accessory.Data.DefaultSafeColor;
                        }
                        else
                        {
                            dp.Color = accessory.Data.DefaultDangerColor;
                        }
                        accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                    }
                    
                }
                
            });
            

        }
        [ScriptMethod(name: "P6 邪念之炎/同归于尽之炎标记", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:27974"], userControl: false)]
        public void P6_邪念之炎标记(Event @event, ScriptAccessory accessory)
        {
            if (!p6Mark) return;
            accessory.Method.MarkClear();
            Task.Delay(50).ContinueWith(t =>
            {
                var plist = accessory.Data.PartyList.ToList();
                int attack = 0;
                int stop = 8;
                int bind = 5;
                for (int i = 0; i < p6lightDark.Count; i++)
                {
                    if (p6lightDark[i] == 0)
                    {
                        //无
                        stop++;
                        accessory.Method.Mark(plist[i], (MarkType)stop);
                    }
                    if (p6lightDark[i] == 1)
                    {
                        //分散
                        attack++;
                        accessory.Method.Mark(plist[i], (MarkType)attack);
                    }
                    if (p6lightDark[i] == 2)
                    {
                        //分摊
                        bind++;
                        accessory.Method.Mark(plist[i], (MarkType)bind);
                    }

                }
            });
            Task.Delay(23000).ContinueWith(t =>
            {
                accessory.Method.MarkClear();
            });
        }

        #endregion

        #region P7
        [ScriptMethod(name: "P7 开场记录", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:29752"], userControl: false)]
        public void P7_开场记录(Event @event, ScriptAccessory accessory)
        {
            parse = 7.0;
        }
        [ScriptMethod(name: "P7 阶段累加地火", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28059"], userControl: false)]
        public void P7_阶段累加地火(Event @event, ScriptAccessory accessory)
        {
            parse = Math.Round(parse + 0.1, 1);
        }
        [ScriptMethod(name: "P7 阶段累加死亡轮回", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28051"], userControl: false)]
        public void P7_阶段累加死亡轮回(Event @event, ScriptAccessory accessory)
        {
            parse = Math.Round(parse + 0.1, 1);
        }
        [ScriptMethod(name: "P7 阶段累加陨石", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28057"], userControl: false)]
        public void P7_阶段累加陨石(Event @event, ScriptAccessory accessory)
        {
            parse = Math.Round(parse + 0.1, 1);
        }
        [ScriptMethod(name: "P7 钢铁", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2056", "StackCount:42"])]
        public void P7_钢铁(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7 钢铁";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(8);
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.Owner = id;
            }
            if (parse == 7.3 || parse == 7.6 || parse == 7.9)
            {
                dp.DestoryAt = 8000;
            }
            else
            {
                dp.DestoryAt = 6000;
            }
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P7 月环", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2056", "StackCount:43"])]
        public void P7_月环(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7 月环";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Radian = float.Pi * 2;
            dp.Scale = new(50);
            dp.InnerScale = new(8);
            if (ParseObjectId(@event["TargetId"], out var id))
            {
                dp.Owner = id;
            }
            if (parse == 7.3 || parse == 7.6 || parse == 7.9)
            {
                dp.DestoryAt = 8000;
            }else
            {
                dp.DestoryAt = 6000;
            }
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        [ScriptMethod(name: "P7 脑死地火点位", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28059"])]
        public void P7_脑死地火点位(Event @event, ScriptAccessory accessory)
        {
            var cpos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var r = float.Parse(@event["SourceRotation"]);

            var pos1 = new Vector3(cpos.X + MathF.Sin(r)*-8, cpos.Y, cpos.Z + MathF.Cos(r)*-8);
            var pos2 = new Vector3(cpos.X + MathF.Sin(r) * -14, cpos.Y, cpos.Z + MathF.Cos(r) * -14);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7 脑死地火点位1";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new(1.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition=pos1;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7 脑死地火点位2";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new(1.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = pos2;
            dp.Delay = 9000;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7 脑死地火点位3";
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Scale = new(1.5f);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Position = pos1;
            dp.TargetPosition = pos2;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);



        }

        [ScriptMethod(name: "P7 死亡轮回剑分摊处(Imgui)", eventType: EventTypeEnum.StartCasting)]
        public void P7_死亡轮回剑分摊处(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(50).ContinueWith(t =>
            {
                var idstr = @event["ActionId"];
                if (idstr != "29452" && idstr != "29453" && idstr != "29454") return;

                var idIndex = accessory.Data.PartyList.ToList().IndexOf(accessory.Data.Me);

                var isme = false;
                accessory.Log.Debug($"parse:{parse}");
                if (parse == 7.2 || !p7_116)
                {
                    if (idstr == "29452" && (idIndex == 3 || idIndex == 5 || idIndex == 7)) isme = true;
                    if (idstr == "29453" && (idIndex == 2 || idIndex == 4 || idIndex == 6)) isme = true;
                    if (idstr == "29454" && (idIndex == 0 || idIndex == 1)) isme = true;
                }
                else
                {
                    if (parse == 7.5)
                    {
                        if (idstr == "29452" && (idIndex == 0)) isme = true;
                        if (idstr == "29453" && (idIndex != 0 && idIndex != 1)) isme = true;
                        if (idstr == "29454" && (idIndex == 1)) isme = true;
                    }
                    if (parse == 7.8)
                    {
                        if (idstr == "29452" && (idIndex == 1)) isme = true;
                        if (idstr == "29453" && (idIndex != 0 && idIndex != 1)) isme = true;
                        if (idstr == "29454" && (idIndex == 0)) isme = true;
                    }
                }

                if (isme)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P7 死亡轮回剑分摊处";
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Scale = new(1.5f);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    if (ParseObjectId(@event["SourceId"], out var sid))
                    {
                        dp.TargetObject = sid;
                    }
                    dp.DestoryAt = 6700;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P7 死亡轮回剑分摊范围";
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Scale = new(4);
                    if (ParseObjectId(@event["SourceId"], out var sid2))
                    {
                        dp.Owner = sid2;
                    }
                    dp.DestoryAt = 12000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }
            });
            
            
        }

        [ScriptMethod(name: "P7 一号核爆", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28058"])]
        public void P7_一号核爆(Event @event, ScriptAccessory accessory)
        {
            

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7 一号核爆";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(21f);
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P7 二号核爆", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28114"])]
        public void P7_二号核爆(Event @event, ScriptAccessory accessory)
        {


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7 二号核爆";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(21f);
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.Delay = 9000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P7 三号核爆", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28115"])]
        public void P7_三号核爆(Event @event, ScriptAccessory accessory)
        {


            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P7 二号核爆";
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Scale = new(21f);
            if (ParseObjectId(@event["SourceId"], out var sid))
            {
                dp.Owner = sid;
            }
            dp.Delay = 13000;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "P7 一号核爆位置收集", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28058"],userControl:false)]
        public void P7_一号核爆位置收集(Event @event, ScriptAccessory accessory)
        {
            p7Stone1 = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        }
        [ScriptMethod(name: "P7 二号核爆位置收集", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28114"], userControl: false)]
        public void P7_二号核爆位置收集(Event @event, ScriptAccessory accessory)
        {
            p7Stone2 = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
        }
        [ScriptMethod(name: "P7 核爆1跑2(Imgui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28114"])]
        public void P7_核爆1跑2(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(50).ContinueWith(t =>
            {
                var cpos = new Vector3(100, 0, 100);
                var dot1 = Vector3.Normalize(cpos - p7Stone1);
                var pos1 = p7Stone1 + dot1 * 21f;
                var stone2pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                var dot2 = Vector3.Normalize(cpos - p7Stone2);
                var pos2 = stone2pos + dot2 * 21f;

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P7 核爆跑1";
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = pos1;
                dp.Scale = new(1.5f);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = "P7 核爆1跑2";
                dp2.Color = accessory.Data.DefaultSafeColor;
                dp2.Position = pos1;
                dp2.TargetPosition = pos2;
                dp2.Scale = new(1.5f);
                dp2.ScaleMode |= ScaleMode.YByDistance;
                dp2.DestoryAt = 9000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);

                var dp3 = accessory.Data.GetDefaultDrawProperties();
                dp3.Name = "P7 核爆跑2";
                dp3.Color = accessory.Data.DefaultSafeColor;
                dp3.Owner = accessory.Data.Me;
                dp3.TargetPosition = pos2;
                dp3.Scale = new(1.5f);
                dp3.ScaleMode |= ScaleMode.YByDistance;
                dp3.Delay = 9000;
                dp3.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);
            });
        }
        [ScriptMethod(name: "P7 核爆2跑3(Imgui)", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:28115"])]
        public void P7_核爆2跑3(Event @event, ScriptAccessory accessory)
        {
            Task.Delay(50).ContinueWith(t =>
            {
                var cpos = new Vector3(100, 0, 100);
                var dot1 = Vector3.Normalize(cpos - p7Stone2);
                var pos1 = p7Stone2 + dot1 * 21f;
                var stone3pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                var dot2 = Vector3.Normalize(cpos - stone3pos);
                var pos2 = stone3pos + dot2 * 21f;

                

                var dp2 = accessory.Data.GetDefaultDrawProperties();
                dp2.Name = "P7 核爆2跑3";
                dp2.Color = accessory.Data.DefaultSafeColor;
                dp2.Position = pos1;
                dp2.TargetPosition = pos2;
                dp2.Scale = new(1.5f);
                dp2.ScaleMode |= ScaleMode.YByDistance;
                dp2.DestoryAt = 13000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);

                var dp3 = accessory.Data.GetDefaultDrawProperties();
                dp3.Name = "P7 核爆跑3";
                dp3.Color = accessory.Data.DefaultSafeColor;
                dp3.Owner = accessory.Data.Me;
                dp3.TargetPosition = pos2;
                dp3.Scale = new(1.5f);
                dp3.ScaleMode |= ScaleMode.YByDistance;
                dp3.Delay = 13000;
                dp3.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);
            });
        }

        #endregion

        [ScriptMethod(name: "TargetIconPrint", eventType: EventTypeEnum.TargetIcon)]
        public void TestTargetIcon(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"TargetIcon: {@event["TargetId"]} {ParsTargetIcon(@event["Id"])}"); 
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

