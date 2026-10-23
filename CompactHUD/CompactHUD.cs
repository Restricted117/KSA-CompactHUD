using Brutal.ImGuiApi;
using HarmonyLib;
using StarMap.API;



namespace CompactHUD
{
    
    [StarMapMod]
    public class CompactHUD
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



        [ModMenuEntry("CompactHud")]
        public static void DrawModMenu()
        {
            try 
            {
                ImGui.MenuItem("Vertical Info Window", default, ref VehicleInfoWindow.verticalLayout);
                ImGui.MenuItem("Hide Attiude Window", default, ref AttitudeWindow.hidden);
                ImGui.MenuItem("Hide Info Window", default, ref VehicleInfoWindow.hidden);

                if (ImGui.MenuItem(VehicleWorkerHook.GetIfInstalled() ? "Disable CompactHud" : "Enable CompactHud"))
                {
                    if (VehicleWorkerHook.GetIfInstalled())
                        VehicleWorkerHook.Remove();
                    if (!VehicleWorkerHook.GetIfInstalled())
                        VehicleWorkerHook.Install();
                }
            }
            catch { }
  
        }

    }

}
