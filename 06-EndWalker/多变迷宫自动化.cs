using System;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using System.Windows.Forms;
using Dalamud.Game.ClientState.Objects.Types;

namespace MyScriptNamespace
{
    [ScriptType(name: "多变迷宫自动化", territorys: [1069,1137,1176],guid: "ad122d3e-0966-48c8-bddb-a0a3e9fe3a61", version:"0.0.0.1",author:"Karlin")]
    public class 多变迷宫自动化
    {
        
        [UserSetting(note:"低于该值对自己使用治疗")]
        public int healthThreshold { get; set; } = 20000;

        [UserSetting(note: "多变治疗栏位，左边为2右边为1")]
        public int healthSlot { get; set; } = 1;
        [UserSetting(note: "Dot栏位，左边为2右边为1")]
        public int DotSlot { get; set; } = 1;


        DateTime lasthealth=DateTime.Now;
        DateTime lastdot= DateTime.Now;

        public void Init(ScriptAccessory accessory)
        {
        }


        [ScriptMethod(name: "自动dot", eventType: EventTypeEnum.StatusRemove, eventCondition: ["StatusID:3359"])]
        public void 自动dot(Event @event, ScriptAccessory accessory)
        {
            lock (this)
            {
                if ((DateTime.Now - lastdot).TotalSeconds < 2.5) return;
                if (!ParseObjectId(@event["SourceId"], out var sid)) return;
                if (!ParseObjectId(@event["TargetId"], out var tid)) return;
                if (sid != accessory.Data.Me) return;
                var obj = accessory.Data.Objects.SearchByEntityId(tid);
                if (obj == null) return;
                if (((IBattleChara)obj).IsDead) return;
                accessory.Method.SelectTarget(tid);
                accessory.Method.SendChat($"/共通技能 任务指令{DotSlot}");
                lastdot = DateTime.Now;
            }
            

        }

        [ScriptMethod(name: "自动治疗", eventType: EventTypeEnum.UpdateHpMp)]
        public void 自动治疗(Event @event, ScriptAccessory accessory)
        {
            if ((DateTime.Now-lasthealth).TotalSeconds<2.5) return;
            if (!ParseObjectId(@event["SourceId"],out var sid)) return;
            if (sid!=accessory.Data.Me) return;
            if (!int.TryParse(@event["Hp"].Split('/')[0], out var chp)) return;
            accessory.Log.Debug($"{chp}");
            if (chp<healthThreshold)
            {
                accessory.Log.Debug("a6");
                accessory.Method.SendChat($"/共通技能 任务指令{healthSlot}");
                lasthealth = DateTime.Now;
            }

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
    }
}

