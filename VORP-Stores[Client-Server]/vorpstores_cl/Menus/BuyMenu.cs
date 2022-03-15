using CitizenFX.Core;
using MenuAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vorpstores_cl.Menus
{
    public class BuyMenu
    {
        private static Menu buyMenu = new Menu(GetConfig.Langs["BuyButton"], GetConfig.Langs["BuyMenuDesc"]);
        private static Menu buyMenuConfirm = new Menu("", GetConfig.Langs["BuyMenuConfirmDesc"]);

        private static int indexItem;
        private static int quantityItem;

        private static bool setupDone = false;

        public static List<string> quantityList = new List<string>();

        private static void SetupMenu()
        {
            if (setupDone) return;
            setupDone = true;
            MenuController.AddMenu(buyMenu);

            MenuController.EnableMenuToggleKeyOnController = false;
            MenuController.MenuToggleKey = (Control)0;

            MenuController.AddSubmenu(buyMenu, buyMenuConfirm);

            for (var i = 1; i < 101; i++)
            {
                quantityList.Add($"{GetConfig.Langs["Quantity"]} #{i}");
            }

            MenuItem subMenuConfirmBuyBtnYes = new MenuItem("", " ")
            {
                RightIcon = MenuItem.Icon.TICK
            };
            MenuItem subMenuConfirmBuyBtnNo = new MenuItem(GetConfig.Langs["BuyConfirmButtonNo"], " ")
            {
                RightIcon = MenuItem.Icon.ARROW_LEFT
            };

            buyMenuConfirm.AddMenuItem(subMenuConfirmBuyBtnYes);
            buyMenuConfirm.AddMenuItem(subMenuConfirmBuyBtnNo);

            buyMenu.OnListItemSelect += (_menu, _listItem, _listIndex, _itemIndex) =>
            {
                if (isTemplateStore(StoreActions.LaststoreId))
                {
                    string storeTemplate = GetConfig.Config["Stores"][StoreActions.LaststoreId]["TemplateName"].ToString();
                    int storeId = GetTemplateShopID(storeTemplate);
                    indexItem = _itemIndex;
                    quantityItem = _listIndex + 1;
                    double totalPrice = double.Parse(GetConfig.Config["StoreTemplates"][storeId]["ItemsBuy"][_itemIndex]["BuyPrice"].ToString()) * quantityItem;
                    buyMenuConfirm.MenuTitle = GetConfig.ItemsFromDB[GetConfig.Config["StoreTemplates"][storeId]["ItemsBuy"][_itemIndex]["Name"].ToString()]["label"].ToString();
                    subMenuConfirmBuyBtnYes.Label = string.Format(GetConfig.Langs["BuyConfirmButtonYes"], (_listIndex + 1).ToString(), GetConfig.ItemsFromDB[GetConfig.Config["StoreTemplates"][storeId]["ItemsBuy"][_itemIndex]["Name"].ToString()]["label"].ToString(), totalPrice.ToString());
                }
                else
                {
                    indexItem = _itemIndex;
                    quantityItem = _listIndex + 1;
                    double totalPrice = double.Parse(GetConfig.Config["Stores"][StoreActions.LaststoreId]["ItemsBuy"][_itemIndex]["BuyPrice"].ToString()) * quantityItem;
                    buyMenuConfirm.MenuTitle = GetConfig.ItemsFromDB[GetConfig.Config["Stores"][StoreActions.LaststoreId]["ItemsBuy"][_itemIndex]["Name"].ToString()]["label"].ToString();
                    subMenuConfirmBuyBtnYes.Label = string.Format(GetConfig.Langs["BuyConfirmButtonYes"], (_listIndex + 1).ToString(), GetConfig.ItemsFromDB[GetConfig.Config["Stores"][StoreActions.LaststoreId]["ItemsBuy"][_itemIndex]["Name"].ToString()]["label"].ToString(), totalPrice.ToString());
                }

            };

            buyMenu.OnIndexChange += (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                if (isTemplateStore(StoreActions.LaststoreId))
                {
                    string storeTemplate = GetConfig.Config["Stores"][StoreActions.LaststoreId]["TemplateName"].ToString();
                    int storeId = GetTemplateShopID(storeTemplate);
                    StoreActions.CreateObjectOnTable(storeId, _newIndex, "ItemsBuy");
                }
                else
                {
                    StoreActions.CreateObjectOnTable(_newIndex, "ItemsBuy");
                }

            };

            buyMenu.OnMenuOpen += (_menu) =>
            {
                buyMenu.ClearMenuItems();
                if (isTemplateStore(StoreActions.LaststoreId))
                {
                    string storeTemplate = GetConfig.Config["Stores"][StoreActions.LaststoreId]["TemplateName"].ToString();
                    int storeId = GetTemplateShopID(storeTemplate);
                    foreach (var item in GetConfig.Config["StoreTemplates"][storeId]["ItemsBuy"])
                    {
                        MenuListItem _itemToBuy = new MenuListItem(GetConfig.ItemsFromDB[item["Name"].ToString()]["label"].ToString() + $" ${item["BuyPrice"]}", quantityList, 0, "")
                        {

                        };

                        buyMenu.AddMenuItem(_itemToBuy);
                        MenuController.BindMenuItem(buyMenu, buyMenuConfirm, _itemToBuy);
                    }
                }
                else
                {
                    foreach (var item in GetConfig.Config["Stores"][StoreActions.LaststoreId]["ItemsBuy"])
                    {
                        MenuListItem _itemToBuy = new MenuListItem(GetConfig.ItemsFromDB[item["Name"].ToString()]["label"].ToString() + $" ${item["BuyPrice"]}", quantityList, 0, "")
                        {

                        };

                        buyMenu.AddMenuItem(_itemToBuy);
                        MenuController.BindMenuItem(buyMenu, buyMenuConfirm, _itemToBuy);
                    }
                }
                if (isTemplateStore(StoreActions.LaststoreId))
                {
                    string storeTemplate = GetConfig.Config["Stores"][StoreActions.LaststoreId]["TemplateName"].ToString();
                    int storeId = GetTemplateShopID(storeTemplate);
                    StoreActions.CreateObjectOnTable(storeId, _menu.CurrentIndex, "ItemsBuy");
                }
                else
                {
                    StoreActions.CreateObjectOnTable(_menu.CurrentIndex, "ItemsBuy");
                }

            };

            buyMenuConfirm.OnItemSelect += (_menu, _item, _index) =>
            {
                if (_index == 0)
                {
                    if (isTemplateStore(StoreActions.LaststoreId))
                    {
                        string storeTemplate = GetConfig.Config["Stores"][StoreActions.LaststoreId]["TemplateName"].ToString();
                        int storeId = GetTemplateShopID(storeTemplate);
                        StoreActions.BuyItemTemplateStore(storeId, indexItem, quantityItem);
                    }
                    else
                    {
                        StoreActions.BuyItemStore(indexItem, quantityItem);
                    }

                    buyMenu.OpenMenu();
                    buyMenuConfirm.CloseMenu();
                }
                else
                {
                    buyMenu.OpenMenu();
                    buyMenuConfirm.CloseMenu();
                }
            };

        }

        public static Menu GetMenu()
        {
            SetupMenu();
            return buyMenu;
        }

        private static bool isTemplateStore(int storeId)
        {
            return bool.Parse(GetConfig.Config["Stores"][storeId]["TemplateStore"].ToString());
        }

        private static int GetTemplateShopID(string name)
        {
            int result = 0;
            for (int i = 0; i < GetConfig.Config["StoreTemplates"].Count(); i++)
            {
                if (GetConfig.Config["StoreTemplates"][i]["name"].ToString() == name)
                    result = i;
            }
            return result;
        }
    }
}
