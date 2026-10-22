using Brutal.ImGuiApi;
using Brutal.Numerics;
using KSA;


namespace CompactHUD
{

    enum AttitudeWindowTarget
    {
        None,
        Prograde,
        Retrograde,
        RadIn,
        RadOut,
        Normal,
        AntiNormal,
        Burn,
        Target,
        AntiTarget
    };

    struct AttitudeWindowState
    {
        public VehicleReferenceFrame Frame;
        public AttitudeWindowTarget Target;

        public FlightComputerAttitudeMode AttitudeMode;
        public FlightComputerRCSMode ModeRCS;
        public FlightComputerAttitudeProfile AttitudeProfile;
        public FlightComputerBurnMode BurnMode;
        public FlightComputerManualThrustMode ThrustMode;
        public FlightComputerRollMode RollMode;

        public AttitudeWindowState()
        {
            Frame = VehicleReferenceFrame.Lvlh;
            Target = AttitudeWindowTarget.None;
            AttitudeMode = FlightComputerAttitudeMode.Auto;
            ModeRCS = FlightComputerRCSMode.Enabled;
            AttitudeProfile = FlightComputerAttitudeProfile.Balanced;
            BurnMode = FlightComputerBurnMode.Manual;
            ThrustMode = FlightComputerManualThrustMode.Direct;
            RollMode = FlightComputerRollMode.Decoupled;
        }

        public AttitudeWindowState(VehicleReferenceFrame frame)
        {
            Frame = frame;
            Target = AttitudeWindowTarget.None;
            AttitudeMode = FlightComputerAttitudeMode.Auto;
            ModeRCS = FlightComputerRCSMode.Enabled;
            AttitudeProfile = FlightComputerAttitudeProfile.Balanced;
            BurnMode = FlightComputerBurnMode.Manual;
            ThrustMode = FlightComputerManualThrustMode.Direct;
            RollMode = FlightComputerRollMode.Decoupled;
        }
    }

    static internal class AttitudeWindow
    {
        
        static public float2 _offsetUv = new float2(0.5f, 0.5f);
        public const string windowName = "AttitudeWindow";
        public static bool hidden = false;
        static bool treeOpen = false;
        static float2 sizeWinDef = new float2(200.0f, 200.0f);
        public static AttitudeWindowState State = new AttitudeWindowState();

        static public void SetWinPos(float2 winPos)
        {
            _offsetUv = winPos;
        }

        static public void ResetWindowState(VehicleReferenceFrame frame = VehicleReferenceFrame.Lvlh)
        {
            if (Program.ControlledVehicle is not null)
            {
                Program.ControlledVehicle.SetNavBallFrame(frame);
            }

            State = new AttitudeWindowState(frame);
        }

        static public void DrawWindow()
        {
            if (Program.IsEditorOpen || !Program.IsControlledVehicleActive || Program.ControlledVehicle == null)
                return;
            if (hidden)
                return;

            float2 winSizeUV = new float2(0.23f, 0.25f);

            if (treeOpen)
            {
                winSizeUV = new float2(0.23f, 0.325f);
            }

            var window = new ImGaugeWindow(
                "AttitudeWindow", "AttitudeWindow",
                new float2(0, 0),
                new float2(0, 0),
                _offsetUv,
                winSizeUV);

            ImGui.SetNextWindowScroll(new float2(0.0f, 0.0f));
            ImGauge.BeginWindow(in window, out float2 posWin, out float2 sizeWin);
            try
            {
                if (!treeOpen)
                {
                    sizeWinDef = sizeWin;
                }

                float2 titleHandle = new float2(sizeWinDef.X, sizeWinDef.Y * 0.075f);
                ImGui.SetCursorScreenPos(posWin);
                ImGauge.Label("ATTITUDE ", titleHandle);
                ImGauge.DrawDressedBox(posWin, sizeWin);

                Vehicle rocket = Program.ControlledVehicle;

                NavBallData navBall;
                VehicleReferenceFrame navBallFrame;
                
                bool bTarget = false;
                bool bBurn = false;
                bool bTargetFrame = false;
                bool bBurnFrame = false;
                bool rcsMode = true;
                bool rcsRateMode = true;
                string gStr = "0.00 G";

                if (rocket != null)
                {
                    navBall = rocket.NavBallData;
                    navBallFrame = navBall.Frame;
                    State.Frame = navBallFrame;

                    gStr = (rocket.FlightComputer.TotalAcceleration / 9.8f).ToString("N2") + " G";

                    rcsMode = rocket.FlightComputer.RCSMode == FlightComputerRCSMode.Enabled ? true : false;
                    rcsRateMode = rocket.FlightComputer.AttitudeMode == FlightComputerAttitudeMode.Auto ? true : false;

                    bTarget = rocket.Target != null ? true : false;
                    bBurn = rocket.FlightComputer.Burn != null ? true : false;

                    bTargetFrame = navBallFrame == VehicleReferenceFrame.Dock ? true : false;
                    bBurnFrame = navBallFrame == VehicleReferenceFrame.BurnBody ? true : false;
                }

                bool trackingNone = rocket.FlightComputer.AttitudeTrackTarget == FlightComputerAttitudeTrackTarget.None;

                float3 redColor = new float3(0.7f, 0.1f, 0.1f);
                float3 greenColor = new float3(0.1f, 0.7f, 0.1f);
                float3 blueColor = new float3(0.1f, 0.1f, 0.7f);
                float3 grayColor = new float3(0.3f, 0.3f, 0.3f);
                float4 backgroundColor = new float4(0.05f, 0.05f, 0.05f, 1.0f);

                float3 rcsColor = rcsMode ? greenColor : redColor;
                float3 rateColor = rcsRateMode ? greenColor : redColor;
                float3 targetColor = rocket.FlightComputer.AttitudeTrackTarget == FlightComputerAttitudeTrackTarget.Toward && !trackingNone ? greenColor : grayColor;
                float3 AntiTargetColor = rocket.FlightComputer.AttitudeTrackTarget == FlightComputerAttitudeTrackTarget.Away && !trackingNone ? greenColor : grayColor;
                float3 burnColor = rocket.FlightComputer.AttitudeTrackTarget == FlightComputerAttitudeTrackTarget.PositiveDv && !trackingNone ? greenColor : grayColor;
                float3 progradeColor = State.Target == AttitudeWindowTarget.Prograde && !trackingNone ? greenColor : grayColor;
                float3 retrogradeColor = State.Target == AttitudeWindowTarget.Retrograde && !trackingNone ? greenColor : grayColor;
                float3 normalColor = State.Target == AttitudeWindowTarget.Normal && !trackingNone ? greenColor : grayColor;
                float3 antiNormColor = State.Target == AttitudeWindowTarget.AntiNormal && !trackingNone ? greenColor : grayColor;
                float3 radOutColor = State.Target == AttitudeWindowTarget.RadOut && !trackingNone ? greenColor : grayColor;
                float3 radInColor = State.Target == AttitudeWindowTarget.RadIn && !trackingNone ? greenColor : grayColor;
                float3 noneColor = State.Target == AttitudeWindowTarget.None || trackingNone ? greenColor : grayColor;

                float2 btnSize = new float2(sizeWinDef.X * 0.31f, sizeWinDef.Y * 0.135f);
                float textScale = 0.25f;

                var btnStyle = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: grayColor,
                    activeColor: grayColor * 2.0f,
                    textScale: textScale,
                    disabled: false);

                var styleRCS = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,   // alpha ignored (forced 0)
                    idleColor: rcsColor,                // resting face
                    activeColor: rcsColor * 2.0f,       // face when pressed/hovered
                    textScale: textScale,
                    disabled: false);

                var styleRateRCS = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: rateColor,
                    activeColor: rateColor * 2.0f,
                    textScale: textScale,
                    disabled: false);

                var styleBurn = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: burnColor,
                    activeColor: burnColor * 2.0f,
                    textScale: textScale,
                    disabled: !bBurn);

                var styleTarget = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: targetColor,
                    activeColor: targetColor * 2.0f,
                    textScale: textScale,
                    disabled: !bTarget);

                var styleAntiTarget = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: AntiTargetColor,
                    activeColor: AntiTargetColor * 2.0f,
                    textScale: textScale,
                    disabled: !bTarget);

                var stylePrograde = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: progradeColor,
                    activeColor: progradeColor * 2.0f,
                    textScale: textScale,
                    disabled: bBurnFrame);

                var styleRetrograde = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: retrogradeColor,
                    activeColor: retrogradeColor * 2.0f,
                    textScale: textScale,
                    disabled: bBurnFrame);

                var styleNormal = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: normalColor,
                    activeColor: normalColor * 2.0f,
                    textScale: textScale,
                    disabled: bTargetFrame || bBurnFrame);

                var styleAntiNormal = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: antiNormColor,
                    activeColor: antiNormColor * 2.0f,
                    textScale: textScale,
                    disabled: bTargetFrame || bBurnFrame);

                var styleRadOut = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: radOutColor,
                    activeColor: radOutColor * 2.0f,
                    textScale: textScale,
                    disabled: bTargetFrame || bBurnFrame);

                var styleRadIn = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: radInColor,
                    activeColor: radInColor * 2.0f,
                    textScale: textScale,
                    disabled: bTargetFrame || bBurnFrame);

                var styleNone = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: noneColor,
                    activeColor: noneColor * 2.0f,
                    textScale: textScale,
                    disabled: false);


                if (ImGui.BeginTable("table1", 3))
                {
                    ImGui.TableNextColumn();

                    if (ImGauge.Button("RCS", btnSize, styleRCS) && rocket != null)
                    {
                        if (rocket.FlightComputer.RCSMode == FlightComputerRCSMode.Disabled)
                            State.ModeRCS = FlightComputerRCSMode.Enabled;
                        else
                            State.ModeRCS = FlightComputerRCSMode.Disabled;
                    }

                    ImGui.TableNextColumn();

                    if (ImGauge.Button("RATE", btnSize, styleRateRCS) && rocket != null)
                    {
                        if (rocket.FlightComputer.AttitudeMode == FlightComputerAttitudeMode.Manual)
                            State.AttitudeMode = FlightComputerAttitudeMode.Auto;
                        else
                            State.AttitudeMode = FlightComputerAttitudeMode.Manual;
                    }

                    ImGui.TableNextColumn();

                    ImGui.Text(gStr);

                    ImGui.EndTable();
                }

                string attProfileStr = "";
                string rollModeStr = "";

                if (rocket.FlightComputer.HasAttitudeProfile(FlightComputerAttitudeProfile.Balanced))
                    attProfileStr = "Balanced";
                else if (rocket.FlightComputer.HasAttitudeProfile(FlightComputerAttitudeProfile.Strict))
                    attProfileStr = "Strict";
                else if (rocket.FlightComputer.HasAttitudeProfile(FlightComputerAttitudeProfile.Relaxed))
                    attProfileStr = "Relaxed";

                if (rocket.FlightComputer.RollMode == FlightComputerRollMode.Decoupled)
                    rollModeStr = "Any";
                else if (rocket.FlightComputer.RollMode == FlightComputerRollMode.Down)
                    rollModeStr = "Norm";
                else if (rocket.FlightComputer.RollMode == FlightComputerRollMode.Up)
                    rollModeStr = "Invert";


                if (ImGui.TreeNode("RCS Settings"))
                {
                    treeOpen = true;
                    float treeTextScale = 0.75f;
                    ImGui.PushFont(ImGui.GetFont(), ImGui.GetFontSize() * treeTextScale);

                    if (ImGui.BeginCombo("##RCS profile", "Tracking: " + (ImString)attProfileStr))
                    {
                        if (ImGui.Selectable("Relaxed"))
                        {
                            State.AttitudeProfile = FlightComputerAttitudeProfile.Relaxed;
                        }

                        if (ImGui.Selectable("Balanced"))
                        {
                            State.AttitudeProfile = FlightComputerAttitudeProfile.Balanced;
                        }

                        if (ImGui.Selectable("Strict"))
                        {
                            State.AttitudeProfile = FlightComputerAttitudeProfile.Strict;
                        }

                        ImGui.EndCombo();
                    }

                    if (ImGui.BeginCombo("##Thrust Mode", "Thrust: " + (ImString)rocket.FlightComputer.ManualThrustMode.ToString()))
                    {
                        if (ImGui.Selectable("Pulse"))
                        {
                            State.ThrustMode = FlightComputerManualThrustMode.Pulse;
                        }

                        if (ImGui.Selectable("Direct"))
                        {
                            State.ThrustMode = FlightComputerManualThrustMode.Direct;
                        }

                        ImGui.EndCombo();
                    }

                    if (ImGui.BeginCombo("##Roll Mode", "Roll: " + (ImString)rollModeStr))
                    {
                        if (ImGui.Selectable("Any"))
                        {
                            State.RollMode = FlightComputerRollMode.Decoupled;
                        }

                        if (ImGui.Selectable("Norm"))
                        {
                            State.RollMode = FlightComputerRollMode.Down;
                        }

                        if (ImGui.Selectable("Invert"))
                        {
                            State.RollMode = FlightComputerRollMode.Up;
                        }

                        ImGui.EndCombo();
                    }

                    ImGui.PopFont();
                    ImGui.TreePop();
                }
                else
                    treeOpen = false;
                


                if (ImGui.BeginTable("table2", 3))
                {
                    ImGui.TableNextColumn();

                    if (ImGauge.Button("BURN", btnSize, styleBurn) && rocket != null)
                    {
                        State.Target = AttitudeWindowTarget.Burn;
                    }

                    ImGui.TableNextColumn();

                    if (ImGauge.Button("TGT", btnSize, styleTarget) && rocket != null)
                    {
                        State.Target = AttitudeWindowTarget.Target;
                    }

                    ImGui.TableNextColumn();

                    if (ImGauge.Button("ANTI TGT", btnSize, styleAntiTarget) && rocket != null)
                    {
                        State.Target = AttitudeWindowTarget.AntiTarget;
                    }

                    ImGui.EndTable();
                }

                ImGui.Separator();

                if (ImGui.BeginTable("table3", 3))
                {
                    ImGui.TableNextColumn();

                    if (ImGauge.Button("PROGDE", btnSize, stylePrograde) && rocket != null)
                    {
                        State.Target = AttitudeWindowTarget.Prograde;
                    }

                    ImGui.TableNextColumn();

                    if (ImGauge.Button("NORM", btnSize, styleNormal) && rocket != null)
                    {
                        State.Target = AttitudeWindowTarget.Normal;
                    }

                    ImGui.TableNextColumn();

                    if (ImGauge.Button("OUT", btnSize, styleRadOut) && rocket != null)
                    {
                        State.Target = AttitudeWindowTarget.RadOut;
                    }

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();

                    if (ImGauge.Button("RETRO", btnSize, styleRetrograde) && rocket != null)
                    {
                        State.Target = AttitudeWindowTarget.Retrograde;
                    }

                    ImGui.TableNextColumn();

                    if (ImGauge.Button("ANTINML", btnSize, styleAntiNormal) && rocket != null)
                    {
                        State.Target = AttitudeWindowTarget.AntiNormal;
                    }

                    ImGui.TableNextColumn();

                    if (ImGauge.Button("IN", btnSize, styleRadIn) && rocket != null)
                    {
                        State.Target = AttitudeWindowTarget.RadIn;
                    }

                    ImGui.EndTable();
                }

                if (ImGui.BeginTable("table4", 3))
                {
                    ImGui.TableNextColumn();
                    ImGui.TableNextColumn();

                    if (ImGauge.Button("NONE", btnSize, styleNone) && rocket != null)
                    {
                        State.Target = AttitudeWindowTarget.None;
                    }

                    ImGui.EndTable();
                }

                ImGui.SetCursorScreenPos(posWin);
                ImGui.InvisibleButton("##drag"u8, sizeWin);

                if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                {
                    float2 deltaUv = ScreenReference.PixelsToUv(ImGui.GetIO().MouseDelta);
                    _offsetUv += deltaUv;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("attitude window broken not drawing");
            }
            finally
            {
                ImGauge.EndWindow(); 
            }

        }


    }

}
