using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using System.Windows.Forms;

namespace MyScriptNamespace
{
    /// <summary>
    /// name and version affect the script name and version number displayed in the user interface.
    /// Classes with the same GUID will be considered the same trigger. Please ensure your GUID is unique and does not conflict with others.
    /// </summary>
    [ScriptType(name:"SimpleScript",guid: "d3b6a9b4-1e0e-4e0c-b7c7-ff1fce0e6cf2",version:"0.0.0.1")]
    public class SimpleScript
    {
        /// <summary>
        /// note will be displayed to the user as a tooltip.
        /// </summary>
        [UserSetting(note:"This is a test Property")]
        public int prop1 { get; set; } = 1;
        [UserSetting("Another Test Property")]
        public bool prop2 { get; set; } = false;
        int n = 0;

        /// <summary>
        /// This method is called at the start of each battle reset.
        /// If this method is not defined, the program will execute an empty method.
        /// </summary>
        public void Init()
        {
            n = 0;
        }

        /// <summary>
        /// name is the name of the method as presented to the user.
        /// eventType is the type of event that triggers this method.
        /// eventCondition is an array of strings specifying the properties that the event must have,
        /// in the format name:value,For specific details, please refer to the GameEvent of the plugin.
        /// userControl set to false will make the method not be shown to the user
        /// and cannot be disabled by the user.
        /// Please note, the method will be executed asynchronously.
        /// </summary>
        /// <param name="event">The event instance that triggers this method.</param>
        /// <param name="accessory">Pass the instances of methods and data that might be needed.</param>
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

