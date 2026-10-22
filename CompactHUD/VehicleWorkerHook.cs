using Brutal.Numerics;
using HarmonyLib;
using KSA;
using System.Reflection;


namespace CompactHUD
{
    static internal class VehicleWorkerHook
    {
        private static Harmony? _harmony;
        private static string HarmonyID = "AttitudeTracker";
        private static bool installed;
        private static bool remove;

        public static void Install()
        {
            if (installed)
                return;

            MethodInfo? target = typeof(Vehicle).GetMethod(nameof(Vehicle.PrepareWorker), [typeof(SimStep)]);

            try
            {
                MethodInfo prefix = typeof(VehicleWorkerHook).GetMethod(nameof(VehicleWorkerPatch), BindingFlags.NonPublic | BindingFlags.Static)!;

                _harmony = new Harmony(HarmonyID);
                _harmony.Patch(target, prefix: new HarmonyMethod(prefix));
                installed = true;
                remove = false;
            }
            catch
            {
                Console.WriteLine("patch NOT installed");
            }
        }

        public static void Remove()
        {
            if (!installed)
                return;

            remove = true;
        }

        private static void Uninstall(Vehicle vehicle)
        {
            try
            {
                installed = false;
                _harmony?.UnpatchAll(HarmonyID);
            }
            catch
            {
                Console.WriteLine("patch NOT removed");
            }

            _harmony = null;
        }

        public static bool GetIfInstalled()
        {
            return installed;
        }

        private static void SetFlightComputer(Vehicle vehicle)
        {
            vehicle.FlightComputer.AttitudeMode = AttitudeWindow.State.AttitudeMode;
            vehicle.FlightComputer.RCSMode = AttitudeWindow.State.ModeRCS;
            vehicle.FlightComputer.SetAttitudeProfile(AttitudeWindow.State.AttitudeProfile);
            vehicle.FlightComputer.BurnMode = AttitudeWindow.State.BurnMode;
            vehicle.FlightComputer.SetManualThrustMode(AttitudeWindow.State.ThrustMode);
            vehicle.FlightComputer.RollMode = AttitudeWindow.State.RollMode;
            vehicle.FlightComputer.AttitudeFrame = AttitudeWindow.State.Frame;

            VehicleReferenceFrame Frame = AttitudeWindow.State.Frame;

            FlightComputerAttitudeTrackTarget target = GetFlightComputerTarget(vehicle, Frame, AttitudeWindow.State.Target);

            if (target != FlightComputerAttitudeTrackTarget.Custom)
                vehicle.FlightComputer.CustomAttitudeTarget = double3.Zero;

            vehicle.FlightComputer.AttitudeTrackTarget = target;
        }

        private static FlightComputerAttitudeTrackTarget GetFlightComputerTarget(Vehicle vehicle, VehicleReferenceFrame frame, AttitudeWindowTarget windowTarget)
        {
            FlightComputerAttitudeTrackTarget target2track = FlightComputerAttitudeTrackTarget.None;

            if(windowTarget == AttitudeWindowTarget.Prograde)
            {
                if (frame == VehicleReferenceFrame.EnuBody)
                    target2track = FlightComputerAttitudeTrackTarget.Forward;
                else if (frame == VehicleReferenceFrame.Lvlh)
                    target2track = FlightComputerAttitudeTrackTarget.Ahead;
                else if (frame == VehicleReferenceFrame.VlfBody || frame == VehicleReferenceFrame.EclBody)
                    target2track = FlightComputerAttitudeTrackTarget.Prograde;
                else if (frame == VehicleReferenceFrame.Dock)
                {
                    target2track = FlightComputerAttitudeTrackTarget.Custom;
                    TrackDockFramePrograde(vehicle);
                }
                else if (frame == VehicleReferenceFrame.BurnBody)
                    target2track = FlightComputerAttitudeTrackTarget.None;
            }
            else if(windowTarget == AttitudeWindowTarget.Retrograde)
            {
                if (frame == VehicleReferenceFrame.EnuBody)
                    target2track = FlightComputerAttitudeTrackTarget.Backward;
                else if (frame == VehicleReferenceFrame.Lvlh)
                    target2track = FlightComputerAttitudeTrackTarget.Behind;
                else if (frame == VehicleReferenceFrame.VlfBody || frame == VehicleReferenceFrame.EclBody)
                    target2track = FlightComputerAttitudeTrackTarget.Retrograde;
                else if (frame == VehicleReferenceFrame.Dock)
                    target2track = FlightComputerAttitudeTrackTarget.Antivel;
                else if (frame == VehicleReferenceFrame.BurnBody)
                    target2track = FlightComputerAttitudeTrackTarget.None;
            }
            else if (windowTarget == AttitudeWindowTarget.RadIn)
            {
                if (frame == VehicleReferenceFrame.EnuBody)
                    target2track = FlightComputerAttitudeTrackTarget.Down;
                else if (frame == VehicleReferenceFrame.Lvlh)
                    target2track = FlightComputerAttitudeTrackTarget.RadialIn;
                else if (frame == VehicleReferenceFrame.VlfBody || frame == VehicleReferenceFrame.EclBody)
                    target2track = FlightComputerAttitudeTrackTarget.Inward;
                else if (frame == VehicleReferenceFrame.Dock)
                    target2track = FlightComputerAttitudeTrackTarget.None;
                else if (frame == VehicleReferenceFrame.BurnBody)
                    target2track = FlightComputerAttitudeTrackTarget.None;
            }
            else if (windowTarget ==  AttitudeWindowTarget.RadOut)
            {
                if (frame == VehicleReferenceFrame.EnuBody)
                    target2track = FlightComputerAttitudeTrackTarget.Up;
                else if (frame == VehicleReferenceFrame.Lvlh)
                    target2track = FlightComputerAttitudeTrackTarget.RadialOut;
                else if (frame == VehicleReferenceFrame.VlfBody || frame == VehicleReferenceFrame.EclBody)
                    target2track = FlightComputerAttitudeTrackTarget.Outward;
                else if (frame == VehicleReferenceFrame.Dock)
                    target2track = FlightComputerAttitudeTrackTarget.None;
                else if (frame == VehicleReferenceFrame.BurnBody)
                    target2track = FlightComputerAttitudeTrackTarget.None;
            }
            else if (windowTarget == AttitudeWindowTarget.Normal)
            {
                target2track = FlightComputerAttitudeTrackTarget.Normal;

                if (frame == VehicleReferenceFrame.Dock)
                    target2track = FlightComputerAttitudeTrackTarget.None;
                else if (frame == VehicleReferenceFrame.BurnBody)
                    target2track = FlightComputerAttitudeTrackTarget.None;
            }
            else if (windowTarget == AttitudeWindowTarget.AntiNormal)
            {
                target2track = FlightComputerAttitudeTrackTarget.AntiNormal;

                if (frame == VehicleReferenceFrame.Dock)
                    target2track = FlightComputerAttitudeTrackTarget.None;
                else if (frame == VehicleReferenceFrame.BurnBody)
                    target2track = FlightComputerAttitudeTrackTarget.None;
            }
            else if (windowTarget == AttitudeWindowTarget.Burn)
            {
                target2track = FlightComputerAttitudeTrackTarget.PositiveDv;
            }
            else if (windowTarget == AttitudeWindowTarget.Target)
            {
                target2track = FlightComputerAttitudeTrackTarget.Toward;
            }
            else if (windowTarget == AttitudeWindowTarget.AntiTarget)
            {
                target2track = FlightComputerAttitudeTrackTarget.Away;
            }

            return target2track;
        }

        private static void TrackDockFramePrograde(Vehicle vehicle)
        {
            try
            {
                if (vehicle.Target is null)
                    return;

                double3 velParent = vehicle.GetVelocityCci();
                double3 posParent = vehicle.GetPositionCci();
                double3 velTarget = vehicle.Target.GetVelocityCci();
                double3 velDiff = velParent - velTarget;

                double3 refVec = (double3.Cross(posParent, velParent)).NormalizeOrZero();
                double3 forward = velDiff.NormalizeOrZero();

                double3 side = (double3.Cross(refVec, forward)).NormalizeOrZero();
                double3 up = (double3.Cross(forward, side)).NormalizeOrZero();

                doubleQuat tgt2Cci = doubleQuat.CreateFromRotationMatrix(new double4x4(
                                                forward.X, forward.Y, forward.Z, 0.0,
                                                side.X, side.Y, side.Z, 0.0,
                                                up.X, up.Y, up.Z, 0.0,
                                                0.0, 0.0, 0.0, 1.0));

                doubleQuat frame2Cci = VehicleReferenceFrameEx.GetEclBody2Cci(vehicle.Orbit.Parent.GetCce2Cci());
                doubleQuat frame2Desired = doubleQuat.Concatenate(tgt2Cci, frame2Cci.Inverse());

                vehicle.FlightComputer.AttitudeTrackTarget = FlightComputerAttitudeTrackTarget.Custom;
                vehicle.FlightComputer.AttitudeFrame = VehicleReferenceFrame.EclBody;
                vehicle.FlightComputer.CustomAttitudeTarget = VehicleReferenceFrame.EclBody.QuaternionToEulerAngles(frame2Desired);
            }
            catch
            {
                Console.WriteLine("something went wrong with math");
            }

        }

        
        [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.PrepareWorker))]
        private static void VehicleWorkerPatch(Vehicle __instance)
        {
            if (__instance is null)
                return;

            if (__instance != Program.ControlledVehicle)
                return;

            if (!remove)
            {
                SetFlightComputer(__instance);
            }
            else if (remove)
            {
                Uninstall(__instance);
            }
            
        }
    }
}
