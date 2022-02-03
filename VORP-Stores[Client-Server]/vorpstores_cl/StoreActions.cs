using System;
using CitizenFX.Core;
using MenuAPI;
using System.Threading;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using System.Collections.Generic;

namespace vorpstores_cl
{
    public class StoreActions : BaseScript
    {
        private static int ObjectStore;
        private static int CamStore;
        public static int LaststoreId;
        public static List<int> _objectStoreList;

        public static async Task EnterBuyStore(int storeId)
        {
            //Debug.WriteLine($"Entering shop {storeId}");

            _objectStoreList = new List<int>();

            LaststoreId = storeId;
            float Camerax = float.Parse(GetConfig.Config["Stores"][storeId]["CameraMain"][0].ToString());
            float Cameray = float.Parse(GetConfig.Config["Stores"][storeId]["CameraMain"][1].ToString());
            float Cameraz = float.Parse(GetConfig.Config["Stores"][storeId]["CameraMain"][2].ToString());
            float CameraRotx = float.Parse(GetConfig.Config["Stores"][storeId]["CameraMain"][3].ToString());
            float CameraRoty = float.Parse(GetConfig.Config["Stores"][storeId]["CameraMain"][4].ToString());
            float CameraRotz = float.Parse(GetConfig.Config["Stores"][storeId]["CameraMain"][5].ToString());

            TriggerEvent("vorp:setInstancePlayer", true);
            NetworkSetInSpectatorMode(true, PlayerPedId());
            FreezeEntityPosition(PlayerPedId(), true);
            //SetEntityVisible(PlayerPedId(), false);

            CamStore = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", Camerax, Cameray, Cameraz, CameraRotx, CameraRoty, CameraRotz, 50.00f, false, 0);
            SetCamActive(CamStore, true);
            RenderScriptCams(true, true, 500, true, true, 0);

            MenuController.MainMenu.MenuTitle = GetConfig.Config["Stores"][storeId]["name"].ToString();

            MenuController.MainMenu.OpenMenu();

        }

        public static void ClearObjectsOnTable()
        {
            foreach (int objectStore in _objectStoreList)
            {
                try
                {
                    int o = objectStore;
                    DeleteObject(ref o);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public static async Task CreateObjectOnTable(int index, string list)
        {
            object __lockObj = ObjectStore;
            bool __lockWasTaken = false;
            try
            {
                System.Threading.Monitor.Enter(__lockObj, ref __lockWasTaken);
                //Debug.WriteLine($"Deleting {ObjectStore}");
                DeleteObject(ref ObjectStore);
                ClearObjectsOnTable();

                float objectX = float.Parse(GetConfig.Config["Stores"][LaststoreId]["SpawnObjectStore"][0].ToString());
                float objectY = float.Parse(GetConfig.Config["Stores"][LaststoreId]["SpawnObjectStore"][1].ToString());
                float objectZ = float.Parse(GetConfig.Config["Stores"][LaststoreId]["SpawnObjectStore"][2].ToString());
                float objectH = float.Parse(GetConfig.Config["Stores"][LaststoreId]["SpawnObjectStore"][3].ToString());

                uint idObject = (uint)GetHashKey(GetConfig.Config["Stores"][LaststoreId][list][index]["ObjectModel"].ToString());

                await vorpstores_init.LoadModel(idObject);
                ObjectStore = CreateObject(idObject, objectX, objectY, objectZ, false, true, true, true, true);
                //Debug.WriteLine($"Created {ObjectStore}");
                _objectStoreList.Add(ObjectStore);
            }
            finally
            {
                if (__lockWasTaken) System.Threading.Monitor.Exit(__lockObj);
            }
        }

        public static async Task ExitBuyStore()
        {
            //Debug.WriteLine("Exiting shop");
            await Delay(100);
            if (!MenuController.IsAnyMenuOpen())
            {
                TriggerEvent("vorp:setInstancePlayer", false);
                NetworkSetInSpectatorMode(false, PlayerPedId());
                FreezeEntityPosition(PlayerPedId(), false);
                //SetEntityVisible(PlayerPedId(), true);
                SetCamActive(CamStore, false);
                RenderScriptCams(false, true, 1000, true, true, 0);
                DestroyCam(CamStore, true);

                DeleteObject(ref ObjectStore);
                ClearObjectsOnTable();
            }

            _objectStoreList = new List<int>();
        }

        public static async Task BuyItemStore(int indexItem, int quantityItem)
        {
            TriggerServerEvent("vorpstores:buyItems", GetConfig.Config["Stores"][StoreActions.LaststoreId]["ItemsBuy"][indexItem]["Name"].ToString(), quantityItem, GetConfig.Config["Stores"][StoreActions.LaststoreId]["ItemsBuy"][indexItem]["BuyPrice"].ToObject<double>());
        }

        public static async Task SellItemStore(int indexItem, int quantityItem)
        {
            TriggerServerEvent("vorpstores:sellItems", GetConfig.Config["Stores"][StoreActions.LaststoreId]["ItemsSell"][indexItem]["Name"].ToString(), quantityItem, GetConfig.Config["Stores"][StoreActions.LaststoreId]["ItemsSell"][indexItem]["SellPrice"].ToObject<double>());
        }

    }
}
