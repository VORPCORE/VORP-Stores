using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace vorpstores_sv
{
    public class LoadConfig : BaseScript
    {
        public static JObject Config = new JObject();
        public static string ConfigString;
        public static Dictionary<string, string> Langs = new Dictionary<string, string>();
        public static string resourcePath = $"{API.GetResourcePath(API.GetCurrentResourceName())}";

        public static Dictionary<string, Dictionary<string, object>> ItemsFromDB = new Dictionary<string, Dictionary<string, object>>();

        public LoadConfig()
        {
            LoadConfigAndLang();

            EventHandlers[$"{API.GetCurrentResourceName()}:getConfig"] += new Action<Player>(getConfig);
        }

        private void LoadConfigAndLang()
        {
            Debug.WriteLine("Load config and lang");
            if (File.Exists($"{resourcePath}/Config.json"))
            {
                ConfigString = File.ReadAllText($"{resourcePath}/Config.json", Encoding.UTF8);
                Config = JObject.Parse(ConfigString);
                if (File.Exists($"{resourcePath}/{Config["defaultlang"]}.json"))
                {
                    string langstring = File.ReadAllText($"{resourcePath}/{Config["defaultlang"]}.json", Encoding.UTF8);
                    Langs = JsonConvert.DeserializeObject<Dictionary<string, string>>(langstring);
                    Debug.WriteLine($"{API.GetCurrentResourceName()}: Language {Config["defaultlang"]}.json loaded!");
                }
                else
                {
                    Debug.WriteLine($"{API.GetCurrentResourceName()}: {Config["defaultlang"]}.json Not Found");
                }
            }
            else
            {
                Debug.WriteLine($"{API.GetCurrentResourceName()}: Config.json Not Found");
            }

            LoadItemsFromDB();
        }

        public async Task LoadItemsFromDB()
        {
            Debug.WriteLine("Load Items From DB");
            Exports["ghmattimysql"].execute("SELECT * FROM items", new[] { "" }, new Action<dynamic>((result) =>
            {
                if (result.Count == 0)
                {
                    Debug.WriteLine("WARNING: No Items In DB");
                }
                else
                {

                    foreach (var i in result)
                    {
                        Dictionary<string, object> data_item = new Dictionary<string, object>();
                        data_item.Add("label", i.label);
                        data_item.Add("limit", i.limit);
                        data_item.Add("type", i.type);
                        ItemsFromDB.Add(i.item, data_item);
                        //Debug.WriteLine($"label {i.label}, limit {i.limit}, type {i.type}");
                    }

                }

            }));
        }

        private void getConfig([FromSource]Player source)
        {
            Debug.WriteLine($"Shops: {source.Name} request config");
            string SItemsFromDB = JsonConvert.SerializeObject(ItemsFromDB);
            source.TriggerEvent($"{API.GetCurrentResourceName()}:SendConfig", ConfigString, Langs, SItemsFromDB);
        }
    }
}
