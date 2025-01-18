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
using ECommons.Logging;
using ImGuiNET;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using KodakkuAssist.Module.GameOperate;

namespace KarlinScriptNamespace
{
    [ScriptType(name: "极艳翼蛇鸟绘图", territorys:[1196],guid: "d9c97e91-9b59-432d-a8a2-42a8586985b7e2a", version:"0.0.0.3", author: "Karlin")]
    public class ValigarmandaExDraw
    {
        
        int? firstTargetIcon = null;

        /// <summary>
        /// 1=fire,2=storm,3=ice
        /// </summary>
        int parse = 0;
        bool iceCross=false;
        object icelock = new();

        public void Init(ScriptAccessory accessory)
        {
            firstTargetIcon = null;
            accessory.Method.RemoveDraw(@".*");
            //accessory.Method.MarkClear();

        }
        [ScriptMethod(name: "火阶段", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38340"], userControl: false)]
        public void 火阶段(Event @event, ScriptAccessory accessory)
        {
            parse = 1;
        }
        [ScriptMethod(name: "风阶段", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:38339"], userControl: false)]
        public void 风阶段(Event @event, ScriptAccessory accessory)
        {
            parse = 2;
        }
        [ScriptMethod(name: "冰阶段", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36817"],userControl:false)]
        public void 冰阶段(Event @event, ScriptAccessory accessory)
        {
            parse = 3;
        }

        #region 开场指路

        private int 第几次冰 = 0;

        [ScriptMethod(name: "开场指路", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36853"])]
        public void 开场指路(Event @event, ScriptAccessory accessory)
        {
            第几次冰++;
            if (第几次冰 != 2) return;

            var firstLeft = @event["SourceRotation"] == "0.79";
            var pos1 = firstLeft ? new Vector3(98, 0, 100) : new Vector3(102, 0, 100);
            var pos2 = firstLeft ? new Vector3(104, 0, 101) : new Vector3(96, 0, 101);
            var pos3 = new Vector3(100, 0, 99);
            
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "开场指路1";
            dp.Scale = new Vector2(3);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = pos1;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Delay = 1400;
            dp.DestoryAt = 1500;
            
            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = "开场指路2";
            dp2.Scale = new Vector2(3);
            dp2.Color = accessory.Data.DefaultSafeColor;
            dp2.Owner = accessory.Data.Me;
            dp2.TargetPosition = pos2;
            dp2.ScaleMode |= ScaleMode.YByDistance;
            dp2.Delay = 2900;
            dp2.DestoryAt = 5000;
            
            var dp3 = accessory.Data.GetDefaultDrawProperties();
            dp3.Name = "开场指路3";
            dp3.Scale = new Vector2(3);
            dp3.Color = accessory.Data.DefaultSafeColor;
            dp3.Owner = accessory.Data.Me;
            dp3.TargetPosition = pos3;
            dp3.ScaleMode |= ScaleMode.YByDistance;
            dp3.Delay = 7900;
            dp3.DestoryAt = 10000;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp2);
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp3);
        }

        #endregion

        #region Buff
        [ScriptMethod(name: "火分摊", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3817"])]
        public void 火分摊(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"火分摊";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "强化火分摊", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3818"])]
        public void 强化火分摊(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"强化火分摊";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.Delay = 70000;
            dp.DestoryAt = 12000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "三人火分摊", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3819"])]
        public void 三人火分摊(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"三人火分摊";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "雷分散", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3823"])]
        public void 雷分散(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"雷分散";
            dp.Scale = new(5);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 6000;
            dp.Delay = int.Parse(@event["DurationMilliseconds"])- dp.DestoryAt;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "强化雷分散", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3824"])]
        public void 强化雷分散(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"强化雷分散";
            dp.Scale = new(8);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 6000;
            dp.Delay = int.Parse(@event["DurationMilliseconds"]) - dp.DestoryAt;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "强化冰大圈", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:3821"])]
        public void 强化冰大圈(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"强化冰大圈";
            dp.Scale = new(16);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.Delay = 6000;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        #endregion

        #region 动作技能
        
        [ScriptMethod(name: "体操钢铁", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36812"])]
        public void 体操钢铁(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "动作钢铁";
            dp.Scale = new(24);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 7300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }
        [ScriptMethod(name: "体操月环", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36816"])]
        public void 体操月环(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "体操月环";
            dp.Scale = new(30);
            dp.Radian = float.Pi * 2;
            dp.InnerScale = new(8);

            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = new(100, 0, 100);

            dp.DestoryAt = 7300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

        }
        [ScriptMethod(name: "体操扇形", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36808"])]
        public void 体操扇形(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "体操扇形";
            dp.Scale = new(50);
            dp.Radian = float.Pi / 18 * 8;
            dp.Offset = new(0, 0, 10);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 7300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }

        [ScriptMethod(name: "体操分摊", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(36808|36812|36816)$"])]
        public void 体操分摊(Event @event, ScriptAccessory accessory)
        {
            if (parse != 1) return;
            accessory.Log.Debug(@event["ActionId"]);
            //if (@event["ActionId"]!= "36812" && @event["ActionId"] != "36816" && @event["ActionId"] != "36808") return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "体操分摊";
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 8000;
            
            dp.Owner = accessory.Data.PartyList[0];
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Owner = accessory.Data.PartyList[1];
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Owner = accessory.Data.PartyList[2];
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp.Owner = accessory.Data.PartyList[3];
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        


        #endregion

        #region 火阶段

        [ScriptMethod(name: "T踩塔扇形", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36889"])]
        public void T踩塔扇形(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "动作扇形";
            dp.Scale = new(40);
            dp.Radian = float.Pi / 18 * 3;
            dp.Rotation = float.Pi;
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["SourceId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "火山喷发左", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00200010", "Index:0000000F"])]
        public void 火山喷发左(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "火山喷发左";
            dp.Scale = new(22);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = new(85, 0, 100);
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "火山喷发右", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00200010", "Index:0000000E"])]
        public void 火山喷发右(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "火山喷发右";
            dp.Scale = new(22);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = new(115, 0, 100);
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        #endregion

        #region 雷阶段
        [ScriptMethod(name: "羽毛AOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36833"])]
        public void 羽毛AOE(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"羽毛AOE";
            dp.Scale = new(8);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 5800;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }

        //[ScriptMethod(name: "原地黄圈", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:36801"])]
        //public void 原地黄圈(Event @event, ScriptAccessory accessory)
        //{
        //    var dp = accessory.Data.GetDefaultDrawProperties();
        //    dp.Name = $"原地黄圈";
        //    dp.Scale = new(6);
        //    dp.Color = accessory.Data.DefaultDangerColor;
        //    if (ParseObjectId(@event["SourceId"], out var tid))
        //    {
        //        dp.Owner = tid;
        //    }
        //    dp.DestoryAt = 3000;
        //    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        //}

        [ScriptMethod(name: "场边激光", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:16770"])]
        public void 场边激光(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"场边激光";
            dp.Scale = new(5,50);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        #endregion

        #region 冰
        [ScriptMethod(name: "雪崩右前", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00200010", "Index:00000003"])]
        public void 雪崩右前(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "雪崩右前";
            dp.Scale = new(50,24);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = new(100, 0, 100);
            dp.Rotation = 53.13f / 180 * float.Pi + float.Pi / 2;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }
        [ScriptMethod(name: "雪崩左后", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00020001", "Index:00000003"])]
        public void 雪崩左后(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "雪崩右前";
            dp.Scale = new(50,24);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Position = new(100, 0, 100);
            dp.Rotation = 36.87f / 180 * -float.Pi;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
        }

        [ScriptMethod(name: "冰花米字安全区", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:16667"])]
        public void 冰花米字安全区(Event @event, ScriptAccessory accessory)
        {
            lock (icelock)
            {
                if (iceCross) return;
                var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                Vector3 pos11 = new(97.5f, 0, 107.5f);
                Vector3 pos12 = new(92.5f, 0, 87.5f);
                Vector3 pos13 = new(117.5f, 0, 97.5f);
                Vector3 pos14 = new(87.5f, 0, 102.5f);
                Vector3 pos15 = new(82.5f, 0, 112.5f);

                Vector3 pos21 = new(82.5f, 0, 97.5f);
                Vector3 pos22 = new(102.5f, 0, 107.5f);
                Vector3 pos23 = new(107.5f, 0, 87.5f);
                Vector3 pos24 = new(112.5f, 0, 102.5f);
                Vector3 pos25 = new(117.5f, 0, 112.5f);
                if ((pos - pos11).Length() < 1f || (pos - pos12).Length() < 1f || (pos - pos13).Length() < 1f || (pos - pos14).Length() < 1f || (pos - pos15).Length() < 1f)
                {
                    iceCross = true;
                    Task.Delay(1000).ContinueWith(y => { iceCross = false; });
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"冰花米字安全区";
                    dp.Scale = new(2.07f, 3.54f);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Position = new(108.23f, 0, 91.77f);
                    dp.Rotation = float.Pi / -4;
                    dp.DestoryAt = 5000;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
                }
                if ((pos - pos21).Length() < 1f || (pos - pos22).Length() < 1f || (pos - pos23).Length() < 1f || (pos - pos24).Length() < 1f || (pos - pos25).Length() < 1f)
                {
                    iceCross = true;
                    Task.Delay(1000).ContinueWith(y => { iceCross = false; });
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = $"冰花米字安全区";
                    dp.Scale = new(2.07f, 3.54f);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Position = new(91.77f, 0, 91.77f);
                    dp.Rotation = float.Pi / 4;
                    dp.DestoryAt = 5000;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Rect, dp);
                }
            }


        }
        #endregion

        #region 分摊打柱子
        [ScriptMethod(name: "分摊打柱子分摊范围", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:34722"])]
        public void 分摊打柱子(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"分摊打柱子";
            dp.Scale = new(6,80);
            dp.Color = accessory.Data.DefaultSafeColor;
            if (ParseObjectId(@event["SourceId"], out var cid))
            {
                dp.Owner = cid;
            }
            if (ParseObjectId(@event["TargetId"], out var tid))
            {
                dp.TargetObject = tid;
            }
            dp.DestoryAt = 99000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "移除分摊范围", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:38245"],userControl:false)]
        public void 移除分摊范围(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(@".*");
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
