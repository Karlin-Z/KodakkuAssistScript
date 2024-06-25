using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;

namespace MyScriptNamespace
{

    [ScriptType(name:"SimpleScript",guid: "d3b6a9b4-1e0e-4e0c-b7c7-ff1fce0e6cf2")]
    public class SimpleScript
    {
        [UserSetting("Test Property")]
        public int prop1 { get; set; } = 1;
        [UserSetting("Another Test Property")]
        public bool prop2 { get; set; } = false;
        int n = 0;

        public void Init()
        {
            n = 0;
        }

        [ScriptMethod(name: "Test StartCasting",eventType: EventTypeEnum.StartCasting,eventCondition: ["ActionId:0085"])]
        public void PrintInfo(Event @event, ScriptAccessory accessory)
        {
            n++;
            accessory.Method.SendChat($"{@event["SourceId"]} {n}-th use the Medica II");
        }

        [ScriptMethod(name: "Test Draw", eventType: EventTypeEnum.ActionEffect,eventCondition: ["ActionId:007C"])]
        public void DrawCircle(Event @event, ScriptAccessory accessory)
        {
            var prop = accessory.Data.GetDefaultDrawProperties();
            prop.Owner = Convert.ToUInt32(@event["SourceId"],16);
            prop.DestoryAt = 10000;
            prop.Color=accessory.Data.DefaultSafeColor;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, prop);
        }

        [ScriptMethod(name: "Unconfigurable Method", eventType: EventTypeEnum.ActionEffect,eventCondition: ["ActionId:007C"],userControl:false)]
        public void UnconfigurableMethod(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"The unconfigurable method has been triggered.");
        }


    }
}

