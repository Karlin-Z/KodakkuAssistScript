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

namespace KarlinScriptNamespace
{
    [ScriptType(name: "极佐拉加绘图",territorys:[1201],guid: "d9c97e91-9b59-432d-a3a1-42a8586985b7e2a", version:"0.0.0.1", author: "Karlin")]
    public class ZoraalaExDraw
    {
        
        
        
        int? firstTargetIcon = null;

        float[] posXIndex = [66.41f, 69.94f, 73.48f, 77.02f, 122.92f, 126.52f, 130.06f, 133.59f];
        Vector3[] northPos =
            [
            new (87.62f,0.00f,101.76f),
            new (91.16f,0.00f,105.30f),
            new (94.70f,0.00f,108.84f),
            new (98.24f,0.00f,112.38f),
            new (101.76f,0.00f,112.38f),
            new (105.30f,0.00f,108.84f),
            new (108.84f,0.00f,105.30f),
            new (112.38f,0.00f,101.76f),
            ];
        Vector3[] southPos =
            [
            new (87.62f,0.00f,98.24f),
            new (91.16f,0.00f,94.70f),
            new (94.70f,0.00f,91.16f),
            new (98.24f,0.00f,87.62f),
            new (101.76f,0.00f,87.62f),
            new (105.30f,0.00f,91.16f),
            new (108.84f,0.00f,94.70f),
            new (112.38f,0.00f,98.24f),
            ];
        bool fireInside=false;
        int[] nDic = [];
        int[] sDic = [1, 2, 1, 2, 5, 6, 5, 6];

        Vector3 dVector= new Vector3();


        public void Init(ScriptAccessory accessory)
        {
            firstTargetIcon = null;

            //accessory.Method.MarkClear();

        }

        [ScriptMethod(name: "多向斩细", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37796"])]
        public void 多向斩细(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "多向斩";
            dp.Scale = new(4, 40);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.Delay = 5000;
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

        }
        [ScriptMethod(name: "多向斩粗", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37795"])]
        public void 多向斩粗(Event @event, ScriptAccessory accessory)
        {
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "多向斩粗1";
            dp.Scale = new(8, 40);
            dp.FixRotation=true;
            dp.Rotation = float.Pi / 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.Delay = 5000;
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "多向斩粗2";
            dp.Scale = new(8, 40);
            dp.FixRotation = true;
            dp.Rotation = float.Pi / -4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.Delay = 5000;
            dp.DestoryAt = 11000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

        }

        #region 半身残
        [ScriptMethod(name: "前跃半身残左", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37755"])]
        public void 前跃半身残左(Event @event, ScriptAccessory accessory)
        {
            ParseObjectId(@event["SourceId"], out var tid);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "前跃半身残左_前向斩";
            dp.Scale = new(60, 60);
            dp.Offset = new(0, 0, -10);
            dp.Rotation = float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "前跃半身残左_左斩";
            dp.Scale = new(60, 120);
            dp.Rotation = float.Pi / 2;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "前跃半身残右", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37756"])]
        public void 前跃半身残右(Event @event, ScriptAccessory accessory)
        {
            ParseObjectId(@event["SourceId"], out var tid);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "前跃半身残右_前向斩";
            dp.Scale = new(60, 60);
            dp.Offset = new(0, 0, -10);
            dp.Rotation = float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "前跃半身残右_右斩";
            dp.Scale = new(60, 120);
            dp.Rotation = float.Pi / -2;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "后跃半身残左", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37757"])]
        public void 后跃半身残左(Event @event, ScriptAccessory accessory)
        {
            ParseObjectId(@event["SourceId"], out var tid);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "后跃半身残左_后向斩";
            dp.Scale = new(60, 60);
            dp.Offset = new(0, 0, 10);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "后跃半身残左_左斩";
            dp.Scale = new(60, 120);
            dp.Rotation = float.Pi / -2;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "后跃半身残右", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37758"])]
        public void 后跃半身残右(Event @event, ScriptAccessory accessory)
        {
            ParseObjectId(@event["SourceId"], out var tid);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "后跃半身残右_后向斩";
            dp.Scale = new(60, 60);
            dp.Offset = new(0, 0, 10);
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "后跃半身残右_右斩";
            dp.Scale = new(60, 120);
            dp.Rotation = float.Pi / 2;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(2);
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "小场地前跃半身残左", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39322"])]
        public void 小场地前跃半身残左(Event @event, ScriptAccessory accessory)
        {
            ParseObjectId(@event["SourceId"], out var tid);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "小场地前跃半身残左_前向斩";
            dp.Scale = new(60, 60);
            dp.Offset = new(0, 0, -10);
            dp.Rotation = float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "小场地前跃半身残左_左斩";
            dp.Scale = new(60, 120);
            dp.Rotation = float.Pi / 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "小场地前跃半身残右", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39323"])]
        public void 小场地前跃半身残右(Event @event, ScriptAccessory accessory)
        {
            ParseObjectId(@event["SourceId"], out var tid);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "小场地前跃半身残右_前向斩";
            dp.Scale = new(60, 60);
            dp.Offset = new(0, 0, -10);
            dp.Rotation = float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "小场地前跃半身残右_右斩";
            dp.Scale = new(60, 120);
            dp.Rotation = float.Pi / -2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "小场地后跃半身残左", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39324"])]
        public void 小场地后跃半身残左(Event @event, ScriptAccessory accessory)
        {
            ParseObjectId(@event["SourceId"], out var tid);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "小场地后跃半身残左_后向斩";
            dp.Scale = new(60, 60);
            dp.Offset = new(0, 0, 10);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "小场地后跃半身残左_左斩";
            dp.Scale = new(60, 120);
            dp.Rotation = float.Pi / -2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "小场地后跃半身残右", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:39325"])]
        public void 小场地后跃半身残右(Event @event, ScriptAccessory accessory)
        {
            ParseObjectId(@event["SourceId"], out var tid);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "后跃半身残右_后向斩";
            dp.Scale = new(60, 60);
            dp.Offset = new(0, 0, 10);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "后跃半身残右_右斩";
            dp.Scale = new(60, 120);
            dp.Rotation = float.Pi / 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }

        [ScriptMethod(name: "回旋半身残钢铁", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37793"])]
        public void 回旋半身残钢铁(Event @event, ScriptAccessory accessory)
        {
            ParseObjectId(@event["SourceId"], out var tid);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "回旋半身残月环";
            dp.Scale = new(10);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "回旋半身残月环", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37792"])]
        public void 回旋半身残月环(Event @event, ScriptAccessory accessory)
        {
            ParseObjectId(@event["SourceId"], out var tid);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "回旋半身残月环";
            dp.Radian = float.Pi * 2;
            dp.Scale = new(30);
            dp.InnerScale = new(10);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
        }
        [ScriptMethod(name: "回旋半身残左右刀", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37791"])]
        public void 回旋半身残左右刀(Event @event, ScriptAccessory accessory)
        {
            ParseObjectId(@event["SourceId"], out var tid);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "回旋半身残左右刀";
            dp.Scale = new(120, 60);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = 7300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);


        }


        [ScriptMethod(name: "半身残单独", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37790"])]
        public void 半身残单独(Event @event, ScriptAccessory accessory)
        {
            ParseObjectId(@event["SourceId"], out var tid);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "半身残单独";
            dp.Scale = new(120, 60);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Owner = tid;
            dp.DestoryAt = 6300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);


        }
        #endregion

        #region 地板剑 方块 无敌裂斩
        [ScriptMethod(name: "无敌裂斩", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37785"])]
        public void 无敌裂斩(Event @event, ScriptAccessory accessory)
        {
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var de=new Vector2(pos.X-100, pos.Z-100);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "无敌裂斩";
            dp.Scale = new(5,5);
            dp.Color = accessory.Data.DefaultDangerColor;
            if (ParseObjectId(@event["SourceId"], out var tid))
            {
                dp.Owner = tid;
            }
            dp.DestoryAt = 8000;
            if (de.X > 0)
            {
                if (de.Y > 0)
                {
                    dp.Offset = new(0, 0, 30);
                }
                else
                {
                    dp.Offset = new(30, 0, 0);
                }
            }
            else
            {
                if (de.Y > 0)
                {
                    dp.Offset = new(-30, 0, 0);
                }
                else
                {
                    dp.Offset = new(0, 0, -30);
                }
            }
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

        }

        [ScriptMethod(name: "地板收集器", eventType: EventTypeEnum.EnvControl, eventCondition: ["Index:0000000B"], userControl: false)]
        public void 地板收集器(Event @event, ScriptAccessory accessory)
        {
            if (@event["Id"] == "02000100") dVector = new(21.21f, 0, 21.21f);//左上
            if (@event["Id"] == "00800040") dVector = new(-21.21f, 0, 21.21f);//右上
            //if (@event["Id"] == "02000100") dVector = new(21.21f, 0, -21.21f);//左下
            //if (@event["Id"] == "00800040") dVector = new(-21.21f, 0, -21.21f);//右上

        }
        [ScriptMethod(name: "无敌刃", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37779"])]
        public void 无敌刃(Event @event, ScriptAccessory accessory)
        {
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var centre = new Vector3(100, 0, 100);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "无敌刃";
            dp.Scale = new(10, 10);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Rotation = float.Pi / 4;
            dp.Position = (pos-centre).Length()>10?pos+dVector:pos-dVector;
            dp.DestoryAt = 8000;
            
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, dp);

        }
        #endregion

        #region 连线冲
        [ScriptMethod(name: "连线冲收集器火内", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00020001", "Index:00000005"],userControl:false)]
        public void 连线冲收集器火内(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug(fireInside.ToString());
            fireInside =true;
        }
        [ScriptMethod(name: "连线冲收集器火外", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:00020001", "Index:00000008"], userControl: false)]
        public void 连线冲收集器火外(Event @event, ScriptAccessory accessory)
        {
            fireInside = false;
        }
        [ScriptMethod(name: "连线冲收集器swCross", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:02000100", "Index:00000002"], userControl: false)]
        public void 连线冲收集器swCross(Event @event, ScriptAccessory accessory)
        {
            nDic = [1, 3, 0, 2, 6, 4, 7, 5];
        }
        [ScriptMethod(name: "连线冲收集器seCross", eventType: EventTypeEnum.EnvControl, eventCondition: ["Id:02000100", "Index:00000003"], userControl: false)]
        public void 连线冲收集器seCross(Event @event, ScriptAccessory accessory)
        {
            nDic = [2, 0, 3, 1, 5, 7, 4, 6];
        }

        [ScriptMethod(name: "利刃冲", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:37788"])]
        public void 利刃冲(Event @event, ScriptAccessory accessory)
        {
            
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var n = 0;
            for (int i = 0;i<posXIndex.Length;i++)
            {
                if (MathF.Abs(pos.X - posXIndex[i]) < 1) n = i;
            }
            if (pos.Z>100)//南方普通
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "南方普通利刃冲";
                dp.Scale = new(5, 20);
                dp.Position = northPos[nDic[n]];
                dp.FixRotation = true;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Rotation = n > 3 ? float.Pi / -4 * 3 : float.Pi / -4 * 5;
                dp.DestoryAt = 12000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
            if (pos.Z < 100)//北方火
            {
                
                var fire = false;
                if((fireInside && (n==2||n==3||n==4||n==5))||(!fireInside && (n == 0 || n == 1 || n == 6 || n == 7)))
                    fire=true;
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = fire? "北方火冲": "北方风冲";
                dp.Scale = fire ? new(15, 20): new(5, 20);
                dp.Position = southPos[sDic[n]];
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Rotation = n > 3 ? float.Pi / 4*7 : float.Pi / 4;
                dp.DestoryAt = 12000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }

        }
        #endregion

        #region 步进钢铁月环
        [ScriptMethod(name: "步进钢铁", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:16726"])]
        public void 步进钢铁(Event @event, ScriptAccessory accessory)
        {
            Vector3 dv = new();
            var dis = 7.07f;
            Vector3 centre = new(100, 0, 100);
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            dv.X = pos.X > 100 ? -1f : 1f;
            dv.Z = pos.Z > 100 ? -1f : 1f;

            Vector3 pos1 = new(centre.X - dv.X * dis * 0, 0, centre.Z - dv.Z * dis * 3);
            Vector3 pos2 = new(centre.X - dv.X * dis * 1, 0, centre.Z - dv.Z * dis * 2);
            Vector3 pos3 = new(centre.X - dv.X * dis * 2, 0, centre.Z - dv.Z * dis * 1);
            Vector3 pos4 = new(centre.X - dv.X * dis * 3, 0, centre.Z - dv.Z * dis * 0);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(4);
            dp.Color = accessory.Data.DefaultDangerColor;

            for (int i = 0; i < 4; i++)
            {
                dp.Name = $"步进钢铁{i}";
                dp.Delay = i == 0 ? 0 : 4000 + i * 5000;
                dp.DestoryAt = i == 0 ? 9000 : 5000;

                dp.Position = pos1 + i * dv * dis;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                dp.Position = pos2 + i * dv * dis;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                dp.Position = pos3 + i * dv * dis;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                dp.Position = pos4 + i * dv * dis;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            

        }

        [ScriptMethod(name: "步进月环", eventType: EventTypeEnum.AddCombatant, eventCondition: ["DataId:16727"])]
        public void 步进月环(Event @event, ScriptAccessory accessory)
        {
            Vector3 dv = new();
            var dis = 7.07f;
            Vector3 centre = new(100, 0, 100);
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            dv.X = pos.X > 100 ? -1f : 1f;
            dv.Z = pos.Z > 100 ? -1f : 1f;

            Vector3 pos1 = new(centre.X - dv.X * dis * 0, 0, centre.Z - dv.Z * dis * 3);
            Vector3 pos2 = new(centre.X - dv.X * dis * 1, 0, centre.Z - dv.Z * dis * 2);
            Vector3 pos3 = new(centre.X - dv.X * dis * 2, 0, centre.Z - dv.Z * dis * 1);
            Vector3 pos4 = new(centre.X - dv.X * dis * 3, 0, centre.Z - dv.Z * dis * 0);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Scale = new(8);
            dp.InnerScale = new(3);
            dp.Radian = float.Pi * 2;
            dp.Color = accessory.Data.DefaultDangerColor;

            for (int i = 0; i < 4; i++)
            {
                dp.Name = $"步进月环{i}";
                dp.Delay = i == 0 ? 0 : 4000 + i * 5000;
                dp.DestoryAt = i == 0 ? 9000 : 5000;

                dp.Position = pos1 + i * dv * dis;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                dp.Position = pos2 + i * dv * dis;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                dp.Position = pos3 + i * dv * dis;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
                dp.Position = pos4 + i * dv * dis;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
            }


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

