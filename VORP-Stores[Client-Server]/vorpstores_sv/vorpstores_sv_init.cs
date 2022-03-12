using CitizenFX.Core;
using System;

namespace vorpstores_sv
{
    public class vorpstores_sv_init : BaseScript
    {
        public static dynamic CORE;
        public vorpstores_sv_init()
        {
            EventHandlers["vorpstores:buyItems"] += new Action<Player, string, int, double>(buyItems);
            EventHandlers["vorpstores:sellItems"] += new Action<Player, string, int, double>(sellItems);
            TriggerEvent("getCore", new Action<dynamic>((dic) => {
                CORE = dic;
            }));
        }
        private void buyItems([FromSource]Player source, string name, int quantity, double cost)
        {
            int _source = int.Parse(source.Handle);

            string sid = "steam:" + source.Identifiers["steam"];

           
            TriggerEvent("vorpCore:getItemCount", _source, new Action<dynamic>((itemcount) =>
            {
                int count = itemcount;
                int limit = int.Parse(LoadConfig.ItemsFromDB[name]["limit"].ToString());
                int hisLimit = limit - count;
                if (quantity > hisLimit && limit != -1)
                {
                    source.TriggerEvent("vorp:TipRight", string.Format(LoadConfig.Langs["NoMore"], LoadConfig.ItemsFromDB[name]["label"].ToString()), 4000);
                }
                else
                {
                    TriggerEvent("vorpCore:canCarryItems", _source, quantity, new Action<bool>((can) =>
                    {
                        if (!can)
                        {
                            source.TriggerEvent("vorp:TipRight", string.Format(LoadConfig.Langs["NoMore"], LoadConfig.ItemsFromDB[name]["label"].ToString()), 4000);
                        }
                        else
                        {
                            dynamic UserCharacter = CORE.getUser(int.Parse(source.Handle)).getUsedCharacter;
                            double money = UserCharacter.money;
                            double totalCost = (double)(cost * quantity);
                            if(totalCost <= money)
                            {
                                UserCharacter.removeCurrency(0, totalCost);
                                TriggerEvent("vorpCore:addItem", _source, name, quantity);

                                
                                source.TriggerEvent("vorp:TipBottom", string.Format(LoadConfig.Langs["Bought"], quantity,  LoadConfig.ItemsFromDB[name]["label"].ToString(), totalCost.ToString()), 4000);
                            }
                            else
                            {
                                source.TriggerEvent("vorp:TipBottom", LoadConfig.Langs["NoMoney"], 4000);
                            }
                        }

                    }));
                }

            }), name);

        }

        private void sellItems([FromSource]Player source, string name, int quantity, double cost)
        {
            int _source = int.Parse(source.Handle);

            string sid = "steam:" + source.Identifiers["steam"];

            double totalCost = (double)(cost * quantity);

            TriggerEvent("vorpCore:getItemCount", _source, new Action<dynamic>((itemcount) =>
            {
                dynamic UserCharacter = CORE.getUser(int.Parse(source.Handle)).getUsedCharacter;
                int count = itemcount;
                if (quantity > count)
                {
                    source.TriggerEvent("vorp:TipBottom", LoadConfig.Langs["NoEnought"], 4000);
                }
                else
                {
                    UserCharacter.addCurrency(0, totalCost);
                    TriggerEvent("vorpCore:subItem", _source, name, quantity);
                    
                    source.TriggerEvent("vorp:TipBottom", string.Format(LoadConfig.Langs["Sold"], quantity, LoadConfig.ItemsFromDB[name]["label"].ToString(), totalCost.ToString()), 4000);
                }
            }), name);

        }

    }
}
