﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VRCFaceTracking;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.OSC;
using System.Buffers.Binary;

namespace SteamLinkVRCFTModule
{
    using static UnifiedExpressions;

    public class OSCHandler
    {
        public readonly float[] eyeTrackData = new float[3];

        public readonly Dictionary<UnifiedExpressions, float> ueData = new (){
            {EyeWideLeft, 0.0f },
            {EyeWideRight, 0.0f },
            {EyeSquintLeft, 0.0f },
            {EyeSquintRight, 0.0f },
            {BrowInnerUpLeft, 0.0f },
            {BrowInnerUpRight, 0.0f },
            {BrowOuterUpLeft, 0.0f },
            {BrowOuterUpRight, 0.0f },
            {BrowPinchLeft, 0.0f },
            {BrowLowererLeft, 0.0f },
            {BrowPinchRight, 0.0f },

            { JawOpen, 0.0f},
            { JawLeft, 0.0f},
            { JawRight, 0.0f},
            { JawForward, 0.0f},
            { MouthLowerLeft, 0.0f},
            { MouthUpperLeft, 0.0f},
            { MouthLowerRight, 0.0f},
            { MouthUpperRight, 0.0f},
            { MouthRaiserUpper, 0.0f},
            { MouthRaiserLower, 0.0f},

            {MouthDimpleLeft ,0.0f},
            { MouthDimpleRight,0.0f},
            { MouthClosed,0.0f},
            { MouthCornerPullLeft,0.0f},
            { MouthCornerSlantLeft,0.0f},
            { MouthCornerPullRight,0.0f},
            { MouthCornerSlantRight,0.0f},

            {MouthFrownLeft ,0.0f},
            { MouthFrownRight,0.0f},
            { MouthLowerDownLeft,0.0f},
            {MouthLowerDownRight ,0.0f},
            { MouthUpperUpLeft,0.0f}, //TODO CHECK
            {MouthUpperUpRight ,0.0f}, //TODO CHECK
            {MouthTightenerLeft ,0.0f},
            { MouthTightenerRight,0.0f},
            {MouthPressLeft ,0.0f},
            {MouthPressRight ,0.0f},
            {MouthStretchLeft ,0.0f},
            {MouthStretchRight ,0.0f},
            {LipPuckerLowerLeft ,0.0f},
            { LipPuckerUpperLeft,0.0f},
            { LipPuckerLowerRight,0.0f},
            {LipPuckerUpperRight ,0.0f},
            { LipFunnelLowerLeft,0.0f},
            { LipFunnelUpperLeft,0.0f},
            { LipFunnelLowerRight,0.0f},
            { LipFunnelUpperRight,0.0f},
            {LipSuckLowerLeft,0.0f},
            { LipSuckUpperLeft,0.0f},
            {LipSuckLowerRight ,0.0f},
            { LipSuckUpperRight,0.0f},
            {CheekPuffLeft ,0.0f},
            { CheekPuffRight,0.0f},
            {CheekSuckLeft ,0.0f},
            { CheekSuckRight,0.0f},
            { CheekSquintLeft,0.0f},
            {CheekSquintRight ,0.0f},
            { NoseSneerLeft,0.0f},
            {NoseSneerRight ,0.0f},

            };


        //based on https://docs.google.com/spreadsheets/d/118jo960co3Mgw8eREFVBsaJ7z0GtKNr52IB4Bz99VTA/edit#gid=0
        private static readonly Dictionary<string, List<UnifiedExpressions>> mapOSCDirectXRFBUnifiedExpressions = new Dictionary<string, List<UnifiedExpressions>>
            {
                {"/sl/xrfb/facew/UpperLidRaiserL", new List<UnifiedExpressions>{EyeWideLeft}},
                {"/sl/xrfb/facew/UpperLidRaiserR", new List<UnifiedExpressions>{EyeWideRight}},
                {"/sl/xrfb/facew/LidTightenerL", new List<UnifiedExpressions>{EyeSquintLeft}},
                {"/sl/xrfb/facew/LidTightenerR", new List<UnifiedExpressions>{EyeSquintRight}},
                {"/sl/xrfb/facew/InnerBrowRaiserL", new List<UnifiedExpressions>{BrowInnerUpLeft}},
                {"/sl/xrfb/facew/InnerBrowRaiserR", new List<UnifiedExpressions>{BrowInnerUpRight}},
                {"/sl/xrfb/facew/OuterBrowRaiserL", new List<UnifiedExpressions>{BrowOuterUpLeft}},
                {"/sl/xrfb/facew/OuterBrowRaiserR", new List<UnifiedExpressions>{BrowOuterUpRight}},
                {"/sl/xrfb/facew/BrowLowererL", new List<UnifiedExpressions>{BrowPinchLeft, BrowLowererLeft}},
                {"/sl/xrfb/facew/BrowLowererR", new List<UnifiedExpressions>{BrowPinchRight, BrowLowererRight}},

                {"/sl/xrfb/facew/JawDrop", new List<UnifiedExpressions>{JawOpen}},
                {"/sl/xrfb/facew/JawSidewaysLeft", new List<UnifiedExpressions>{JawLeft}},
                {"/sl/xrfb/facew/JawSidewaysRight", new List<UnifiedExpressions>{JawRight}},
                {"/sl/xrfb/facew/JawThrust", new List<UnifiedExpressions>{JawForward}},

                {"/sl/xrfb/facew/MouthLeft", new List<UnifiedExpressions>{MouthLowerLeft, MouthUpperLeft}},
                {"/sl/xrfb/facew/MouthRight", new List<UnifiedExpressions>{MouthLowerRight, MouthUpperRight}},

                {"/sl/xrfb/facew/ChinRaiserT", new List<UnifiedExpressions>{MouthRaiserUpper} },
                {"/sl/xrfb/facew/ChinRaiserB", new List<UnifiedExpressions>{MouthRaiserLower} },

                {"/sl/xrfb/facew/DimplerL", new List<UnifiedExpressions>{MouthDimpleLeft} },
                {"/sl/xrfb/facew/DimplerR", new List<UnifiedExpressions>{MouthDimpleRight} },

                {"/sl/xrfb/facew/LipsToward", new List<UnifiedExpressions>{MouthClosed}},
                {"/sl/xrfb/facew/LipCornerPullerL", new List<UnifiedExpressions>{ MouthCornerPullLeft, MouthCornerSlantLeft} },
                {"/sl/xrfb/facew/LipCornerPullerR", new List<UnifiedExpressions>{ MouthCornerPullRight, MouthCornerSlantRight} },
                {"/sl/xrfb/facew/LipCornerDepressoL", new List<UnifiedExpressions>{ MouthFrownLeft} },
                {"/sl/xrfb/facew/LipCornerDepressoR", new List<UnifiedExpressions>{ MouthFrownRight} },
                {"/sl/xrfb/facew/LowerLipDepressorL", new List<UnifiedExpressions>{ MouthLowerDownLeft} },
                {"/sl/xrfb/facew/LowerLipDepressorR", new List<UnifiedExpressions>{ MouthLowerDownRight} },
                {"/sl/xrfb/facew/UpperLipRaiserL", new List<UnifiedExpressions>{ MouthUpperUpLeft} }, //something odd here
                {"/sl/xrfb/facew/UpperLipRaiserR", new List<UnifiedExpressions>{ MouthUpperUpRight } }, //something odd here
                {"/sl/xrfb/facew/LipTightenerL", new List<UnifiedExpressions>{MouthTightenerLeft} },
                {"/sl/xrfb/facew/LipTightenerR", new List<UnifiedExpressions>{MouthTightenerRight} },
                {"/sl/xrfb/facew/LipPressorL", new List<UnifiedExpressions>{MouthPressLeft} },
                {"/sl/xrfb/facew/LipPressorR", new List<UnifiedExpressions>{MouthPressRight} },
                {"/sl/xrfb/facew/LipStretcherL", new List<UnifiedExpressions>{MouthStretchLeft} },
                {"/sl/xrfb/facew/LipStretcherR", new List<UnifiedExpressions>{MouthStretchRight} },
                {"/sl/xrfb/facew/LipPuckerL", new List<UnifiedExpressions>{ LipPuckerLowerLeft, LipPuckerUpperLeft } },
                {"/sl/xrfb/facew/LipPuckerR", new List<UnifiedExpressions>{ LipPuckerLowerRight, LipPuckerUpperRight } },
                {"/sl/xrfb/facew/LipFunnelerLB", new List<UnifiedExpressions>{LipFunnelLowerLeft} },
                {"/sl/xrfb/facew/LipFunnelerLT", new List<UnifiedExpressions>{LipFunnelUpperLeft} },
                {"/sl/xrfb/facew/LipFunnelerRB", new List<UnifiedExpressions>{LipFunnelLowerRight} },
                {"/sl/xrfb/facew/LipFunnelerRT", new List<UnifiedExpressions>{LipFunnelUpperRight} },
                {"/sl/xrfb/facew/LipSuckLB", new List<UnifiedExpressions>{LipSuckLowerLeft} },
                {"/sl/xrfb/facew/LipSuckLT", new List<UnifiedExpressions>{LipSuckUpperLeft} },
                {"/sl/xrfb/facew/LipSuckRB", new List<UnifiedExpressions>{LipSuckLowerRight} },
                {"/sl/xrfb/facew/LipSuckRT", new List<UnifiedExpressions>{LipSuckUpperRight} },

                {"/sl/xrfb/facew/CheekPuffL", new List<UnifiedExpressions>{CheekPuffLeft} },
                {"/sl/xrfb/facew/CheekPuffR", new List<UnifiedExpressions>{CheekPuffRight} },
                {"/sl/xrfb/facew/CheekSuckL", new List<UnifiedExpressions>{CheekSuckLeft} },
                {"/sl/xrfb/facew/CheekSuckR", new List<UnifiedExpressions>{CheekSuckRight} },
                {"/sl/xrfb/facew/CheekRaiserL", new List<UnifiedExpressions>{CheekSquintLeft} },
                {"/sl/xrfb/facew/CheekRaiserR", new List<UnifiedExpressions>{CheekSquintRight} },

                {"/sl/xrfb/facew/NoseWrinklerL", new List<UnifiedExpressions>{NoseSneerLeft} },
                {"/sl/xrfb/facew/NoseWrinklerR", new List<UnifiedExpressions>{NoseSneerRight} },
            };



        private Socket _receiver;
        private bool _loop = true;
        private readonly Thread _thread;
        private readonly ILogger _logger;
        private readonly int _resolvedPort;
        private const int DEFAULT_PORT = 9015;
        private const int TIMEOUT_MS = 10_000;

        public OSCHandler(ILogger iLogger, int? port = null)
        {
            _logger = iLogger;
            if (_receiver != null)
            {
                _logger.LogError("OSCHandler connection already exists.");
                return;
            }

            _receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _resolvedPort = port ?? DEFAULT_PORT;
            _receiver.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), _resolvedPort));
            _receiver.ReceiveTimeout = TIMEOUT_MS;

            _loop = true;
            _thread = new Thread(new ThreadStart(ListenLoop));
            _thread.Start();
        }

        private void ListenLoop()
        {
            var buffer = new byte[4096];
            while (_loop)
            {
                try
                {
                    if (_receiver.IsBound)
                    {

                        var length = _receiver.Receive(buffer);
                        var offset = 0;
                        List<OSCM> msgList = new List<OSCM>();
                        if (OSCParser.IsBundle(ref buffer))
                        {
                            int i = 16;
                            var elLength = new byte[4];
                            //_logger.LogInformation("Length is: {len}", length);
                            while (i < length)
                            {
                                int messageLength = 0;
                                if (BitConverter.IsLittleEndian)
                                {
                                    byte[] msgsize = new byte[4];
                                    msgsize[0] = buffer[i]; msgsize[1] = buffer[i + 1]; msgsize[2] = buffer[i + 2]; msgsize[3] = buffer[i + 3];
                                    Array.Reverse(msgsize);
                                    messageLength = BitConverter.ToInt32(msgsize, 0);
                                }
                                else
                                {
                                    messageLength = BitConverter.ToInt32(buffer, i);
                                }
                                //_logger.LogInformation("i: {i}, message l: {messageLength}",i,messageLength);
                                int adjustLength = 4 - messageLength % 4;
                                byte[] temp = new byte[messageLength];
                                Array.Copy(buffer, i + 4, temp, 0, messageLength);
                                msgList.Add(new OSCM(ref temp, _logger));
                                i = i + messageLength + adjustLength;

                            }
                        }
                        else
                        {
                            msgList.Add(new OSCM(ref buffer, _logger));
                        }
                        foreach (OSCM oscMessage in msgList)
                        {
                            if (oscMessage == null) continue;
                            if (oscMessage.Values.Count <= 0) continue;
                            if (oscMessage.Address == "/sl/eyeTrackedGazePoint")
                            {
                                _logger.LogInformation("gaze!");
                            }

                            //TODO this may need to be reafactored into update going to try as is.
                            if (mapOSCDirectXRFBUnifiedExpressions.ContainsKey(oscMessage.Address))
                            {
                                foreach (UnifiedExpressions unifiedExpression in mapOSCDirectXRFBUnifiedExpressions[oscMessage.Address])
                                {
                                    ueData[unifiedExpression] = (float)Convert.ToDouble(oscMessage.Values[0].Item2);
                                    UnifiedTracking.Data.Shapes[(int)unifiedExpression].Weight = (float)Convert.ToDouble(oscMessage.Values[0].Item2);
                                }
                            }
                        }
                    }
                    else
                    {
                        _receiver.Close();
                        _receiver.Dispose();
                        _receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        _receiver.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), _resolvedPort));
                        _receiver.ReceiveTimeout = TIMEOUT_MS;
                    }
                }
                catch (Exception) { }
            }
        }

        public void Teardown()
        {
            _loop = false;
            _receiver.Close();
            _receiver.Dispose();
            _thread.Join();
        }

    }
}