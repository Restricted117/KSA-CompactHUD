using Brutal.ImGuiApi;
using Brutal.Numerics;
using HarmonyLib;
using StarMap.API;
using StarMap.API.Services;


namespace CompactHUD
{
    
    [StarMapMod]
    public sealed class CompactHUD
    {
        Harmony? _harmony;


        [StarMapAllModsLoaded]
        public void FullyLoaded()
        {
            Patcher.Patch();
            VehicleWorkerHook.Install();

            VehicleInfoWindow.SetWinPos(Utilities.LoadWinPos(VehicleInfoWindow.windowName));
            AttitudeWindow.SetWinPos(Utilities.LoadWinPos(AttitudeWindow.windowName));
        }

        [StarMapUnload]
        public void Unload()
        {
            Patcher.Unload();

            Utilities.SaveWinPos(AttitudeWindow._offsetUv, AttitudeWindow.windowName);
            Utilities.SaveWinPos(VehicleInfoWindow._offsetUv, VehicleInfoWindow.windowName, VehicleInfoWindow.verticalLayout);
        }


        [StarMapBeforeGui]
        public void DrawCompactUIWindows(double dt)
        {
            AttitudeWindow.DrawWindow();
            VehicleInfoWindow.DrawWindow();
        }

    }

}
