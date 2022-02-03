using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace vorpstores_cl
{
    public class GetConfig : BaseScript
    {
        public static JObject Config = new JObject();
        public static Dictionary<string, string> Langs = new Dictionary<string, string>();
        public static JObject ItemsFromDB = new JObject();

        public GetConfig()
        {
            EventHandlers[$"{API.GetCurrentResourceName()}:SendConfig"] += new Action<string, ExpandoObject, string>(LoadDefaultConfig);
            TriggerServerEvent($"{API.GetCurrentResourceName()}:getConfig");
        }

        private void LoadDefaultConfig(string dc, ExpandoObject dl, string ifdb)
        {
            //Debug.WriteLine($"Shop config received");
            Config = JObject.Parse(dc);
            //Debug.WriteLine($"Config {Config.Count}");

            foreach (var l in dl)
            {
                Langs[l.Key] = l.Value.ToString();
            }

            ItemsFromDB = JObject.Parse(ifdb);
            Debug.WriteLine($"Shop Items {ItemsFromDB.Count}");
            vorpstores_init.InitStores();
        }

    }
}
