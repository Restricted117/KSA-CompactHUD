using Brutal.ImGuiApi;
using Brutal.Numerics;
using KSA;
using System.Globalization;


namespace CompactHUD
{
    static internal class VehicleInfoWindow
    {
        static public float2 _offsetUv = new float2(0.5f, 0.5f);
        public const string windowName = "InfoWindow";
        public static bool hidden = false;
        public static bool verticalLayout = false;

        static public void SetWinPos(float2 winPos)
        {
            _offsetUv = winPos;
        }

        static public void DrawWindow()
        {
            if (Program.IsEditorOpen || !Program.IsControlledVehicleActive || Program.ControlledVehicle == null)
                return;
            if (hidden)
                return;

            float2 winSizeUV = new float2(0.25f * 2.85f, 0.175f);

            if (verticalLayout)
            {
                winSizeUV = new float2(0.25f, 0.25f * 2.0f);
            }
            
            var window = new ImGaugeWindow(
                "InfoWindow", "InfoWindow",
                new float2(0, 0),
                new float2(0, 0),
                _offsetUv,
                winSizeUV);

            ImGui.SetNextWindowScroll(new float2(0.0f, 0.0f));
            ImGauge.BeginWindow(in window, out float2 posWin, out float2 sizeWin);
            try
            {
                ImGui.SetCursorScreenPos(posWin);
                ImGauge.DrawDressedBox(posWin, sizeWin);

                var rocket = Program.ControlledVehicle;

                NavBallData navBall;

                string velStr = "VEL: ";
                string velStrTarget = "TGT REL: ";
                string deltaVStr = "dV: ";
                string closestApproachStr = "Distance: ";
                string closestApproachTimeStr = "Time to: ";
                string targetDistStr = "Distance: ";
                string TWRStr = "TWR: ";
                string apoapsisStr = "Ap: ";
                string periapsisStr = "Pe: ";
                string apTimeStr = " | ";
                string peTimeStr = " | ";
                string throttleStr = "Throttle: ";
                string timeToBurnStr = "Maneuver in: ";
                string startBurnTimeStr = "Begin Burn: ";
                string burnDurationTimeStr = "Burn Length: ";
                string burnDVStr = "Burn dV: ";

                bool bTarget = false;
                bool bBurn = false;
                bool rcsMode = true;
                bool rcsRateMode = true;

                if (rocket != null)
                {
                    navBall = rocket.NavBallData;
                    
                    deltaVStr += Utilities.MetersToString(rocket.Parts.PerformanceSequences.TotalDeltaV, true);
                    TWRStr += rocket.NavBallData.ThrustWeightRatio.ToString("0.00");

                    try
                    {
                        if (rocket.Orbit.ApoapsisAltitude < 0)
                        {
                            apTimeStr += "N/A";
                            apoapsisStr += "N/A" + apTimeStr;
                        }
                        else
                        {
                            apTimeStr += Utilities.SecondsToString((rocket.NextApoapsisTime - Universe.GetElapsedTime()).Seconds());
                            apoapsisStr += Utilities.MetersToString(rocket.Orbit.ApoapsisAltitude) + apTimeStr;
                        }
                    }
                    catch 
                    {
                        apTimeStr += "N/A";
                        apoapsisStr += "N/A" + apTimeStr;
                    }

                    try
                    {
                        if (rocket.Orbit.PeriapsisAltitude < 0)
                        {
                            peTimeStr += "N/A";
                            periapsisStr += "N/A" + peTimeStr;
                        }
                        else
                        {
                            peTimeStr += Utilities.SecondsToString((rocket.NextPeriapsisTime - Universe.GetElapsedTime()).Seconds());
                            periapsisStr += Utilities.MetersToString(rocket.Orbit.PeriapsisAltitude) + peTimeStr;
                        }
                    }
                    catch
                    {
                        peTimeStr += "N/A";
                        periapsisStr += "N/A" + peTimeStr;
                    }
                    
                    throttleStr += rocket.GetManualThrottle().ToString("P", CultureInfo.InvariantCulture) + " %";
                    
                    rcsMode = rocket.FlightComputer.RCSMode == FlightComputerRCSMode.Enabled ? true : false;
                    rcsRateMode = rocket.FlightComputer.AttitudeMode == FlightComputerAttitudeMode.Auto ? true : false;

                    bTarget = rocket.Target != null ? true : false;
                    bBurn = rocket.FlightComputer.Burn != null ? true : false;

                    
                    if (AttitudeWindow.State.Frame == VehicleReferenceFrame.EnuBody)
                    {
                        double vel = rocket.GetSurfaceVelocityCci().Length();
                        velStr = "SURF " + velStr + Utilities.MetersToString(vel, true);
                    }
                    else
                    {
                        double vel = rocket.GetVelocityOrb().Length();
                        velStr = "ORBIT " + velStr + Utilities.MetersToString(vel, true);
                    }

                    if (bBurn)
                    {
                        timeToBurnStr += Utilities.SecondsToString(rocket.FlightComputer.Burn.ImpulsiveInstant.Seconds() - Universe.GetElapsedTime().Seconds());
                        startBurnTimeStr += Utilities.SecondsToString(rocket.FlightComputer.Burn.IgnitionTime.Seconds() - Universe.GetElapsedTime().Seconds());
                        burnDVStr += Utilities.MetersToString(rocket.FlightComputer.Burn.DeltaVToGoCci.Length(), true);
                        burnDurationTimeStr += Utilities.SecondsToString(rocket.FlightComputer.Burn.BurnDuration);
                    }
                    else
                    {
                        timeToBurnStr += "N/A";
                        startBurnTimeStr += "N/A";
                        burnDVStr += "N/A";
                        burnDurationTimeStr += "N/A";
                    }

                    if (bTarget)
                    {
                        try
                        {
                            double3 relVel = rocket.GetVelocityEcl() - rocket.Target.GetVelocityEcl();
                            velStrTarget += Utilities.MetersToString(relVel.Length(), true);
                            targetDistStr += Utilities.MetersToString((rocket.GetPositionEcl() - rocket.Target.GetPositionEcl()).Length());
                            closestApproachStr += Utilities.MetersToString((double)rocket.FlightPlan.TryFindNextClosestApproach(rocket.Target, Universe.GetElapsedTime())?.ClosestDistance);
                            closestApproachTimeStr += Utilities.SecondsToString((double)(rocket.FlightPlan.TryFindNextClosestApproach(rocket.Target, Universe.GetElapsedTime())?.GameTime.Seconds() - Universe.GetElapsedTime().Seconds()));
                        }
                        catch
                        {
                            //target and rocket must be in same soi for closest approach
                            closestApproachStr += "N/A";
                            closestApproachTimeStr += "N/A";
                        }
                    }
                    else
                    {
                        velStrTarget += "N/A";
                        targetDistStr += "N/A";
                        closestApproachStr += "N/A";
                        closestApproachTimeStr += "N/A";
                    }

                }

                bool trackingNone = rocket.FlightComputer.AttitudeTrackTarget == FlightComputerAttitudeTrackTarget.None;

                float3 redColor = new float3(0.7f, 0.1f, 0.1f);
                float3 greenColor = new float3(0.1f, 0.7f, 0.1f);
                float3 blueColor = new float3(0.1f, 0.1f, 0.7f);
                float3 grayColor = new float3(0.3f, 0.3f, 0.3f);
                float4 backgroundColor = new float4(0.05f, 0.05f, 0.05f, 1.0f);
                float4 importantTextColor = new float4(0.1f, 1.0f, 0.1f, 1.0f);

                float3 autoBtnColor = AttitudeWindow.State.BurnMode == FlightComputerBurnMode.Auto ? greenColor : redColor;
                float3 deleteBtnColor = bBurn ? redColor : grayColor;

                float2 btnSize = new float2(sizeWin.X / 10, sizeWin.Y * 0.15f);
                float2 titleTextSize = new float2(sizeWin.X / 3.0f, sizeWin.Y * 0.1f);
                float textScale = 0.4f;

                if (verticalLayout)
                {
                    btnSize = new float2(sizeWin.X * 0.3f, sizeWin.Y * 0.15f * 0.33f);
                    titleTextSize = new float2(sizeWin.X, sizeWin.Y * 0.1f * 0.33f);
                }

                var warpBtnStyle = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: grayColor,
                    activeColor: grayColor * 2.0f,
                    textScale: textScale,
                    disabled: !bBurn);

                var autoBtnStyle = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: autoBtnColor,
                    activeColor: autoBtnColor * 2.0f,
                    textScale: textScale,
                    disabled: false);

                var deleteBtnStyle = new ImGaugeStyle(
                    textColor: new float3(1f, 1f, 1f),
                    backgroundColor: backgroundColor,
                    idleColor: deleteBtnColor,
                    activeColor: deleteBtnColor * 2.0f,
                    textScale: textScale,
                    disabled: !bBurn);


                ImGuiTableFlags tableFlags = ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.PadOuterX;

                if (!verticalLayout)
                {
                    if (ImGui.BeginTable("horizontal", 3, tableFlags))
                    {
                        ImGui.TableNextColumn();

                        float textScaleInfo = 0.9f;
                        ImGui.PushFont(ImGui.GetFont(), ImGui.GetFontSize() * textScaleInfo);

                        ImGauge.Label("TARGET", titleTextSize);
                        ImGui.Text(velStrTarget);
                        ImGui.Text(targetDistStr);
                        ImGui.Separator();
                        ImGui.Text("Closest Approach");
                        ImGui.Text(closestApproachStr);
                        ImGui.Text(closestApproachTimeStr);
                        ImGui.NewLine();

                        ImGui.TableNextColumn();

                        ImGauge.Label("BURN", titleTextSize);
                        ImGui.TextColored(importantTextColor, startBurnTimeStr);
                        ImGui.TextColored(importantTextColor, burnDurationTimeStr);

                        if (ImGauge.Button("WARP", btnSize, warpBtnStyle))
                        {
                            if (bBurn)
                                Utilities.SetFC(rocket, FlightComputerAction.WarpToNextBurn, true);
                        }
                        ImGui.SameLine();

                        if (ImGauge.Button("AUTO", btnSize, autoBtnStyle))
                        {
                            if (AttitudeWindow.State.BurnMode == FlightComputerBurnMode.Manual)
                            {
                                AttitudeWindow.State.BurnMode = FlightComputerBurnMode.Auto;
                                AttitudeWindow.State.Target = AttitudeWindowTarget.Burn;
                            }
                            else
                                AttitudeWindow.State.BurnMode = FlightComputerBurnMode.Manual;
                        }
                        ImGui.SameLine();

                        if (ImGauge.Button("DELETE", btnSize, deleteBtnStyle))
                        {
                            Utilities.SetFC(rocket, FlightComputerAction.DeleteNextBurn, false);
                        }

                        ImGui.Text(burnDVStr);
                        ImGui.Text(timeToBurnStr);

                        ImGui.TableNextColumn();

                        ImGauge.Label("ORBIT", titleTextSize);
                        ImGui.TextColored(importantTextColor, velStr);
                        ImGui.Text(apoapsisStr);
                        ImGui.Text(periapsisStr);
                        ImGui.Separator();
                        ImGui.TextColored(importantTextColor, throttleStr);
                        ImGui.Text(TWRStr);
                        ImGui.TextColored(importantTextColor, deltaVStr);


                        for (int i = 0; i < 5; ++i)
                        {
                            ImGui.NewLine();
                        }

                        ImGui.PopFont();
                        ImGui.EndTable();
                    }

                }
                else
                {
                    float textScaleInfo = 0.9f;
                    ImGui.PushFont(ImGui.GetFont(), ImGui.GetFontSize() * textScaleInfo);

                    if (ImGui.BeginTable("vertical", 1, tableFlags))
                    {
                        ImGui.TableNextColumn();

                        ImGauge.Label("TARGET", titleTextSize);
                        ImGui.Text(velStrTarget);
                        ImGui.Text(targetDistStr);
                        ImGui.NewLine();
                        ImGui.Text("Closest Approach");
                        ImGui.Text(closestApproachStr);
                        ImGui.Text(closestApproachTimeStr);
                        ImGui.Separator();

                        ImGauge.Label("BURN", titleTextSize);
                        ImGui.TextColored(importantTextColor, startBurnTimeStr);
                        ImGui.TextColored(importantTextColor, burnDurationTimeStr);

                        if (ImGui.BeginTable("buttons", 3))
                        {
                            ImGui.TableNextColumn();

                            if (ImGauge.Button("WARP", btnSize, warpBtnStyle))
                            {
                                if (bBurn)
                                    Utilities.SetFC(rocket, FlightComputerAction.WarpToNextBurn, true);
                            }
                            ImGui.TableNextColumn();

                            if (ImGauge.Button("AUTO", btnSize, autoBtnStyle))
                            {
                                if (AttitudeWindow.State.BurnMode == FlightComputerBurnMode.Manual)
                                {
                                    AttitudeWindow.State.BurnMode = FlightComputerBurnMode.Auto;
                                    AttitudeWindow.State.Target = AttitudeWindowTarget.Burn;
                                }
                                else
                                    AttitudeWindow.State.BurnMode = FlightComputerBurnMode.Manual;
                            }
                            ImGui.TableNextColumn();

                            if (ImGauge.Button("DELETE", btnSize, deleteBtnStyle))
                            {
                                Utilities.SetFC(rocket, FlightComputerAction.DeleteNextBurn, false);
                            }

                            ImGui.EndTable();
                        }

                        ImGui.Text(burnDVStr);
                        ImGui.Text(timeToBurnStr);
                        ImGui.Separator();


                        ImGauge.Label("ORBIT", titleTextSize);
                        ImGui.TextColored(importantTextColor, velStr);
                        ImGui.Text(apoapsisStr);
                        ImGui.Text(periapsisStr);
                        ImGui.NewLine();
                        ImGui.TextColored(importantTextColor, throttleStr);
                        ImGui.Text(TWRStr);
                        ImGui.TextColored(importantTextColor, deltaVStr);

                        ImGui.EndTable();
                    }    

                    ImGui.PopFont();
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
                Console.WriteLine("info window broken not drawing");
            }
            finally
            {
                ImGauge.EndWindow(); 
            }

        }

    }
}
