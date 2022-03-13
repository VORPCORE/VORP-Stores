using CitizenFX.Core;
using MenuAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vorpstores_cl.Menus
{
    class SellMenu
    {
        private static Menu sellMenu = new Menu(GetConfig.Langs["SellButton"], GetConfig.Langs["SellMenuDesc"]);
        private static Menu sellMenuConfirm = new Menu("", GetConfig.Langs["SellMenuConfirmDesc"]);

        private static int indexItem;
        private static int quantityItem;

        private static bool setupDone = false;

        private static void SetupMenu()
        {
            if (setupDone) return;
            setupDone = true;
            MenuController.AddMenu(sellMenu);

            MenuController.EnableMenuToggleKeyOnController = false;
            MenuController.MenuToggleKey = (Control)0;

            MenuController.AddSubmenu(sellMenu, sellMenuConfirm);

            MenuItem subMenuConfirmSellBtnYes = new MenuItem("", " ")
            {
                RightIcon = MenuItem.Icon.TICK
            };
            MenuItem subMenuConfirmSellBtnNo = new MenuItem(GetConfig.Langs["SellConfirmButtonNo"], " ")
            {
                RightIcon = MenuItem.Icon.ARROW_LEFT
            };

            sellMenuConfirm.AddMenuItem(subMenuConfirmSellBtnYes);
            sellMenuConfirm.AddMenuItem(subMenuConfirmSellBtnNo);

            sellMenu.OnListItemSelect += (_menu, _listItem, _listIndex, _itemIndex) =>
            {
                if (isTemplateStore(StoreActions.LaststoreId))
                {
                    string storeTemplate = GetConfig.Config["Stores"][StoreActions.LaststoreId]["TemplateName"].ToString();
                    int storeId = GetTemplateShopID(storeTemplate);
                    indexItem = _itemIndex;
                    quantityItem = _listIndex + 1;
                    double totalPrice = double.Parse(GetConfig.Config["StoreTemplates"][storeId]["ItemsSell"][_itemIndex]["SellPrice"].ToString()) * quantityItem;
                    sellMenuConfirm.MenuTitle = GetConfig.ItemsFromDB[GetConfig.Config["StoreTemplates"][storeId]["ItemsSell"][_itemIndex]["Name"].ToString()]["label"].ToString();
                    subMenuConfirmSellBtnYes.Label = string.Format(GetConfig.Langs["SellConfirmButtonYes"], (_listIndex + 1).ToString(), GetConfig.ItemsFromDB[GetConfig.Config["StoreTemplates"][storeId]["ItemsSell"][_itemIndex]["Name"].ToString()]["label"].ToString(), totalPrice.ToString());
                }
                else
                {
                    indexItem = _itemIndex;
                    quantityItem = _listIndex + 1;
                    double totalPrice = double.Parse(GetConfig.Config["Stores"][StoreActions.LaststoreId]["ItemsSell"][_itemIndex]["SellPrice"].ToString()) * quantityItem;
                    sellMenuConfirm.MenuTitle = GetConfig.ItemsFromDB[GetConfig.Config["Stores"][StoreActions.LaststoreId]["ItemsSell"][_itemIndex]["Name"].ToString()]["label"].ToString();
                    subMenuConfirmSellBtnYes.Label = string.Format(GetConfig.Langs["SellConfirmButtonYes"], (_listIndex + 1).ToString(), GetConfig.ItemsFromDB[GetConfig.Config["Stores"][StoreActions.LaststoreId]["ItemsSell"][_itemIndex]["Name"].ToString()]["label"].ToString(), totalPrice.ToString());
                }

            };

            sellMenu.OnIndexChange += (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                if (isTemplateStore(StoreActions.LaststoreId))
                {
                    string storeTemplate = GetConfig.Config["Stores"][StoreActions.LaststoreId]["TemplateName"].ToString();
                    int storeId = GetTemplateShopID(storeTemplate);
                    StoreActions.CreateObjectOnTable(storeId, _newIndex, "ItemsSell");
                }
                else
                {
                    StoreActions.CreateObjectOnTable(_newIndex, "ItemsSell");
                }
            };

            sellMenu.OnMenuOpen += (_menu) =>
            {
                sellMenu.ClearMenuItems();
                if (isTemplateStore(StoreActions.LaststoreId))
                {
                    string storeTemplate = GetConfig.Config["Stores"][StoreActions.LaststoreId]["TemplateName"].ToString();
                    int storeId = GetTemplateShopID(storeTemplate);
                    foreach (var item in GetConfig.Config["StoreTemplates"][storeId]["ItemsSell"])
                    {

                        List<string> quantityList = new List<string>();

                        for (var i = 1; i < 101; i++)
                        {
                            quantityList.Add($"{GetConfig.Langs["Quantity"]} #{i}");
                        }

                        MenuListItem _itemToSell = new MenuListItem(GetConfig.ItemsFromDB[item["Name"].ToString()]["label"].ToString() + $" ${item["BuySell"]}", quantityList, 0, "")
                        {

                        };

                        sellMenu.AddMenuItem(_itemToSell);
                        MenuController.BindMenuItem(sellMenu, sellMenuConfirm, _itemToSell);
                    }
                }
                else
                {
                    foreach (var item in GetConfig.Config["Stores"][StoreActions.LaststoreId]["ItemsSell"])
                    {

                        List<string> quantityList = new List<string>();

                        for (var i = 1; i < 101; i++)
                        {
                            quantityList.Add($"{GetConfig.Langs["Quantity"]} #{i}");
                        }

                        MenuListItem _itemToSell = new MenuListItem(GetConfig.ItemsFromDB[item["Name"].ToString()]["label"].ToString() + $" ${item["BuySell"]}", quantityList, 0, "")
                        {

                        };

                        sellMenu.AddMenuItem(_itemToSell);
                        MenuController.BindMenuItem(sellMenu, sellMenuConfirm, _itemToSell);
                    }
                }
                if (isTemplateStore(StoreActions.LaststoreId))
                {
                    string storeTemplate = GetConfig.Config["Stores"][StoreActions.LaststoreId]["TemplateName"].ToString();
                    int storeId = GetTemplateShopID(storeTemplate);
                    StoreActions.CreateObjectOnTable(storeId, _menu.CurrentIndex, "ItemsSell");
                }
                else
                {
                    StoreActions.CreateObjectOnTable(_menu.CurrentIndex, "ItemsSell");
                }

            };

            sellMenuConfirm.OnItemSelect += (_menu, _item, _index) =>
            {
                if (_index == 0)
                {
                    if (isTemplateStore(StoreActions.LaststoreId))
                    {
                        string storeTemplate = GetConfig.Config["Stores"][StoreActions.LaststoreId]["TemplateName"].ToString();
                        int storeId = GetTemplateShopID(storeTemplate);
                        StoreActions.SellItemTemplateStore(storeId, indexItem, quantityItem);
                    }
                    else
                    {
                        StoreActions.SellItemStore(indexItem, quantityItem);
                        sellMenu.OpenMenu();
                        sellMenuConfirm.CloseMenu();
                    }
                }
                else
                {
                    sellMenu.OpenMenu();
                    sellMenuConfirm.CloseMenu();
                }
            };

        }

        public static Menu GetMenu()
        {
            SetupMenu();
            return sellMenu;
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
                if (GetConfig.Config["StoreTemplates"][i].ToString() == name)
                    result = i;
            }
            return result;
        }
    }
}
