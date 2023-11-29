using Microsoft.Extensions.Logging;
using SteamLinkVRCFTModule;
using System.Net.Sockets;
using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Expressions;
using static VRCFaceTracking.Core.Params.Expressions.UnifiedExpressions;

namespace SteamLinkVRCFTModule
{
    public class SteamLinkVRCFTModule : ExtTrackingModule
    {
        private OSCHandler OSCHandler;
        private const int  DEFAULT_PORT = 9015;

        public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
        {
            ModuleInformation.Name = "SteamLink VRCFT Module";
            
            var stream = GetType().Assembly.GetManifestResourceStream("SteamLinkVRCFTModule.Assets.steamlink.png");
            ModuleInformation.StaticImages = stream != null ? new List<Stream> { stream } : ModuleInformation.StaticImages;

            //TODO better error handling on fail? isInit for OSC Handler?
            OSCHandler = new OSCHandler(Logger, DEFAULT_PORT);

            Logger.LogInformation("SteamLinkVRCFTModule successfully initialized");

            return (true, true);
        }

        private static float CalculateEyeOpenness(float fEyeClosedWeight, float fEyeTightener)
        {
            return 1.0f - Math.Clamp(fEyeClosedWeight + fEyeClosedWeight * fEyeTightener, 0.0f, 1.0f);
        }

        private void UpdateEyeTracking()
        {
            {
                //TODO
                // float fAngleX = MathF.Atan2(packet.vEyeGazePoint[0], -packet.vEyeGazePoint[2]);
                // float fAngleY = MathF.Atan2(packet.vEyeGazePoint[1], -packet.vEyeGazePoint[2]);
                float fAngleX = 0.0f;
                float fAngleY = 0.0f;

                float fNmAngleX = fAngleX / (MathF.PI / 2.0f) * 2.0f;
                float fNmAngleY = fAngleY / (MathF.PI / 2.0f) * 2.0f;

                if (float.IsNaN(fNmAngleX))
                {
                    fNmAngleX = 0.0f;
                }
                if (float.IsNaN(fNmAngleY))
                {
                    fNmAngleY = 0.0f;
                }

                UnifiedTracking.Data.Eye.Left.Gaze.x = fAngleX;
                UnifiedTracking.Data.Eye.Left.Gaze.y = fAngleY;

                UnifiedTracking.Data.Eye.Right.Gaze.x = fAngleX;
                UnifiedTracking.Data.Eye.Right.Gaze.y = fAngleY;
            }

            {
                //float fLeftOpenness = CalculateEyeOpenness(OSCHandler.ueData.vWeights[(int)XrFBWeights.EyesClosedL], packet.vWeights[(int)XrFBWeights.LidTightenerL]);
                //float fRightOpenness = CalculateEyeOpenness(packet.vWeights[(int)XrFBWeights.EyesClosedR], packet.vWeights[(int)XrFBWeights.LidTightenerR]);

                UnifiedTracking.Data.Eye.Left.Openness = 1.0f;// fLeftOpenness;
                UnifiedTracking.Data.Eye.Right.Openness = 1.0f;//fRightOpenness;
            }

        }
        private void UpdateFaceTracking()
        {
            foreach (KeyValuePair<UnifiedExpressions, float> entry in OSCHandler.ueData)
            {
                int nWeightIndex = (int)entry.Key;
                UnifiedTracking.Data.Shapes[(int)entry.Key].Weight = entry.Value;
            }
        }

        public override void Update()
        {
            Thread.Sleep(10);
            UpdateEyeTracking();
            UpdateFaceTracking();
        }
        public override void Teardown()
        {
            OSCHandler.Teardown();
        }
}
}