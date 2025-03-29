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
    /// territorys specifies the regions where this trigger is effective. If left empty, it will be effective in all regions.
    /// Classes with the same GUID will be considered the same trigger. Please ensure your GUID is unique and does not conflict with others.
    /// </summary>
    [ScriptType(name: "SimpleScript", territorys: [179, 979],guid: "d3b6a9b4-1e0e-4e0c-b7c7-ff1fce0e6cf2",version:"0.0.0.3",author:"Karlin", note: noteStr,updateInfo:updateInfoStr)]
    public class SimpleScript
    {
        const string noteStr =
        """
        这是一个提示文本.
        他有多行显示.
        请在这里放置你的提示文本
        """;
        const string updateInfoStr =
        """
        这里是更新信息.
        他有多行显示.
        请在这里放置你的更新信息
        """;
        /// <summary>
        /// note will be displayed to the user as a tooltip.
        /// </summary>
        [UserSetting(note:"This is a test Property")]
        public int prop1 { get; set; } = 1;
        [UserSetting("Another Test Property")]
        public bool prop2 { get; set; } = false;

        [UserSetting("UserColorSetting")]
        public ScriptColor color { get; set; } = new();

        [UserSetting("EnumSetting")]
        public TestEnum enumSetting { get; set; }
        int n = 0;


        public enum TestEnum
        {
            First,
            Second
        }
        /// <summary>
        /// This method is called at the start of each battle reset.
        /// If this method is not defined, the program will execute an empty method.
        /// </summary>
        public void Init(ScriptAccessory accessory)
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
        [ScriptMethod(name: "Test StartCasting",eventType: EventTypeEnum.StartCasting,eventCondition: ["ActionId:133"])]
        public void PrintInfo(Event @event, ScriptAccessory accessory)
        {
            n++;
            accessory.Method.SendChat($"/e {@event["SourceId"]} {n}-th use the Medica II");
            accessory.Log.Debug($"Prop2 is {prop2}");
            accessory.Log.Debug($"enum is {enumSetting}");
        }

        [ScriptMethod(name: "Test Draw", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:124"])]
        public void DrawCircle(Event @event, ScriptAccessory accessory)
        {
            var sid = Convert.ToUInt32(@event["SourceId"], 16);

            var prop = accessory.Data.GetDefaultDrawProperties();
            prop.Owner = accessory.Data.Me;
            prop.DestoryAt = 100000;
            prop.Color = accessory.Data.DefaultDangerColor;
            prop.ScaleMode |= ScaleMode.YByDistance;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, prop);

            
        }

        [ScriptMethod(name: "Test Suppress", eventType: EventTypeEnum.Waymark)]
        public void TestSuppress(Event @event, ScriptAccessory accessory)
        {
            n++;
            accessory.Log.Debug($"WayMark {n} {@event.EventTriggerSource}");
            uint id = 0x40000001;
            if(accessory.Data.EnmityList.TryGetValue(id, out var objs))
            {
                var firstId = objs[0];
            }
        }

        [ScriptMethod(name: "Unconfigurable Method", eventType: EventTypeEnum.ActionEffect,eventCondition: ["ActionId:124"],userControl:false)]
        public void UnconfigurableMethod(Event @event, ScriptAccessory accessory)
        {
            accessory.Log.Debug($"The unconfigurable method has been triggered.");
        }


    }
}

