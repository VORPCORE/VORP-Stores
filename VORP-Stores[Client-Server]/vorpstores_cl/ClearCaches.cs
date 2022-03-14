using CitizenFX.Core;
using MenuAPI;
using System;
using static CitizenFX.Core.Native.API;

namespace vorpstores_cl
{
    class ClearCaches : BaseScript
    {
        public ClearCaches()
        {
            EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
        }

        private void OnResourceStop(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            foreach (int blip in vorpstores_init.StoreBlips)
            {
                int _blip = blip;
                RemoveBlip(ref _blip);
            }

            foreach (int npc in vorpstores_init.StorePeds)
            {
                int _ped = npc;
                DeletePed(ref _ped);
            }
            StoreActions.ExitBuyStore();
            MenuController.CloseAllMenus();
        }

    }
}
