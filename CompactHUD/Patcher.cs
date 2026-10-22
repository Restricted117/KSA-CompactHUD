using Brutal.GlfwApi;
using Brutal.ImGuiApi;
using HarmonyLib;
using KSA;


namespace CompactHUD
{
    [HarmonyPatch]
    internal static class Patcher
    {
        private static Harmony? m_harmony = new Harmony("CompactUI");

        public static void Patch()
        {
            m_harmony?.PatchAll(typeof(Patcher).Assembly);
        }

        public static void Unload()
        {
            m_harmony?.UnpatchAll("CompactUI");
            m_harmony = null;
        }

    }


    //patch to reset the attitude window state after docking to prevent them from tracking previous attitude targets
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.MergeFrom))]
    public static class DockPatch
    {
        static void Postfix(Vehicle __instance, bool __result, string __state)
        {
            AttitudeWindow.ResetWindowState();
        }
    }


    //patch to reset the attitude window state after undocking to prevent them from tracking previous attitude targets
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.Split),
        new[] {
        typeof(Part.Connection),
        typeof(Part.Connection.IConnector),
        typeof(double),
        typeof(PoseChange),
        typeof(string)
        },
        new[] {
        ArgumentType.Normal,
        ArgumentType.Normal,
        ArgumentType.Normal,
        ArgumentType.Out,
        ArgumentType.Normal
        })]
    public static class UndockPatch
    {
        static void Postfix(Vehicle __instance, Vehicle __result)
        {
            AttitudeWindow.ResetWindowState();
        }
    }


    //patch to reset the attitude window state when saves are loaded
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.CreateVehicleFromSaveGameData))]
    public static class VehicleLoadedPatch
    {
        static void Postfix(Vehicle __result)
        {
            AttitudeWindow.ResetWindowState();
        }
    }


    //patch to reset the attitude window state when rockets are launched
    [HarmonyPatch(typeof(Vehicle), "StartCrewMissions")]
    public static class VehicleLaunchedPatch
    {
        static void Postfix(Vehicle __instance)
        {
            //if (__instance is KittenEva) return; //ignore EVA
            AttitudeWindow.ResetWindowState(VehicleReferenceFrame.EnuBody);
        }
    }


    //patch for using keybinds to toggle SAS and RCS since the buffer gets overwritten
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.ProcessInput))]
    public static class RcsTogglePatch
    {
        static bool Prefix(Vehicle __instance, InputAction action, GlfwKeyAction keyAction, GlfwModifier modifiers)
        {
            if (action == InputAction.ToggleRCS && keyAction == GlfwKeyAction.Press)
            {
                if (AttitudeWindow.State.ModeRCS == FlightComputerRCSMode.Disabled)
                    AttitudeWindow.State.ModeRCS = FlightComputerRCSMode.Enabled;
                else
                    AttitudeWindow.State.ModeRCS = FlightComputerRCSMode.Disabled;
            }

            if (action == InputAction.ToggleStabilization && keyAction == GlfwKeyAction.Press)
            {
                if (AttitudeWindow.State.AttitudeMode == FlightComputerAttitudeMode.Manual)
                    AttitudeWindow.State.AttitudeMode = FlightComputerAttitudeMode.Auto;
                else
                    AttitudeWindow.State.AttitudeMode = FlightComputerAttitudeMode.Manual;
            }

            return true;
        }
    }

    
    //patch to ensure that the navball frame wont be changed by the game
    [HarmonyPatch(typeof(VehicleRegionEx), nameof(VehicleRegionEx.GetVehicleReferenceFrame))]
    public static class NavballFramePatch
    {
        static void Postfix(ref VehicleReferenceFrame __result)
        {
            __result = AttitudeWindow.State.Frame;
        }
    }


    //patch to draw a tab on the top menu bar if mod menu isnt installed
    [HarmonyPatch(typeof(Program), nameof(Program.DrawProgramMenusHook))]
    public static class ProgramMenuHook
    {
        static bool Prefix()
        {
            if (Program.IsEditorOpen || !Program.IsControlledVehicleActive || Program.ControlledVehicle == null)
                return false;

            if (ImGui.BeginMenu("CompactHUD"))
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
                ImGui.EndMenu();
            }

            return false;
        }
    }

}
