using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Expressions;
using static VRCFaceTracking.Core.Params.Expressions.UnifiedExpressions;

namespace SteamLinkVRCFTModule
{
    public class SteamLinkVRCFTModule : ExtTrackingModule
    {
        private OSCHandler? OSCHandler;
        private const int DEFAULT_PORT = 9015;
        private bool _ownsEyes = false;
        private bool _ownsExpressions = false;
        private CancellationTokenSource? _cts;

        public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
        {
            ModuleInformation.Name = "SteamLink Module";

            var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SteamLinkVRCFTModule.Assets.steamlink.png");
            ModuleInformation.StaticImages = stream != null ? new List<Stream> { stream } : ModuleInformation.StaticImages;

            _cts = new CancellationTokenSource();

            //TODO better error handling on fail? isInit for OSC Handler?
            OSCHandler = new OSCHandler(_cts.Token, Logger, DEFAULT_PORT);
            if (!OSCHandler.initialized)
            {
                // make sure to teardown anything started before returning as uninitialized
                Teardown();
                return (false, false);
            }

            (_ownsEyes, _ownsExpressions) = (eyeAvailable, expressionAvailable);
            return (_ownsEyes, _ownsExpressions);
        }

        private static float CalculateEyeOpenness(float fEyeClosedWeight, float fEyeTightener)
        {
            return 1.0f - Math.Clamp(fEyeClosedWeight + fEyeClosedWeight * fEyeTightener, 0.0f, 1.0f);
        }

        private void UpdateEyeTracking()
        {
            {
                float fAngleX = MathF.Atan2(OSCHandler.eyeTrackData[0], -OSCHandler.eyeTrackData[2]);
                float fAngleY = MathF.Atan2(OSCHandler.eyeTrackData[1], -OSCHandler.eyeTrackData[2]);

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


                //Pupil Dilation, This is not supported, but if we don't set it can cause issues
                UnifiedTracking.Data.Eye.Left.PupilDiameter_MM = 5f;
                UnifiedTracking.Data.Eye.Right.PupilDiameter_MM = 5f;
                UnifiedTracking.Data.Eye._maxDilation = 10;
                UnifiedTracking.Data.Eye._minDilation = 0;

            }

            {
                float fLeftOpenness = CalculateEyeOpenness(OSCHandler.eyelids[0], OSCHandler.ueData[UnifiedExpressions.EyeSquintLeft]);
                float fRightOpenness = CalculateEyeOpenness(OSCHandler.eyelids[1], OSCHandler.ueData[UnifiedExpressions.EyeSquintRight]);

                UnifiedTracking.Data.Eye.Left.Openness = fLeftOpenness;// fLeftOpenness;
                UnifiedTracking.Data.Eye.Right.Openness = fRightOpenness;//fRightOpenness;
            }

        }
        private void UpdateFaceTracking()
        {
            foreach (KeyValuePair<UnifiedExpressions, float> entry in OSCHandler.ueData)
            {
                UnifiedTracking.Data.Shapes[(int)entry.Key].Weight = entry.Value;
            }
            //?? fix some weird ft things need to check
            UnifiedTracking.Data.Shapes[(int)MouthUpperUpLeft].Weight = Math.Max(0, UnifiedTracking.Data.Shapes[(int)MouthUpperUpLeft].Weight - UnifiedTracking.Data.Shapes[(int)NoseSneerLeft].Weight);
            UnifiedTracking.Data.Shapes[(int)MouthUpperUpRight].Weight = Math.Max(0, UnifiedTracking.Data.Shapes[(int)MouthUpperUpRight].Weight - UnifiedTracking.Data.Shapes[(int)NoseSneerRight].Weight);
            UnifiedTracking.Data.Shapes[(int)MouthUpperDeepenLeft].Weight = Math.Max(0, UnifiedTracking.Data.Shapes[(int)MouthUpperUpLeft].Weight - UnifiedTracking.Data.Shapes[(int)NoseSneerLeft].Weight);
            UnifiedTracking.Data.Shapes[(int)MouthUpperDeepenRight].Weight = Math.Max(0, UnifiedTracking.Data.Shapes[(int)MouthUpperUpRight].Weight - UnifiedTracking.Data.Shapes[(int)NoseSneerRight].Weight);

            //lip Suck
            UnifiedTracking.Data.Shapes[(int)LipSuckUpperLeft].Weight = Math.Min(1.0f - (float)Math.Pow(UnifiedTracking.Data.Shapes[(int)MouthUpperLeft].Weight, 1f / 6f), UnifiedTracking.Data.Shapes[(int)LipSuckUpperLeft].Weight);
            UnifiedTracking.Data.Shapes[(int)LipSuckUpperRight].Weight = Math.Min(1.0f - (float)Math.Pow(UnifiedTracking.Data.Shapes[(int)MouthUpperRight].Weight, 1f / 6f), UnifiedTracking.Data.Shapes[(int)LipSuckUpperRight].Weight);
        }

        public override void Update()
        {
            Thread.Sleep(10);
            if (_ownsEyes)
            {
                UpdateEyeTracking();
            }
            if (_ownsExpressions)
            {
                UpdateFaceTracking();
            }
        }

        public override void Teardown()
        {
            if (_cts != null) _cts.Cancel();
            if (OSCHandler != null) OSCHandler.Teardown();
            if (_cts != null) _cts.Dispose();
        }
    }
}