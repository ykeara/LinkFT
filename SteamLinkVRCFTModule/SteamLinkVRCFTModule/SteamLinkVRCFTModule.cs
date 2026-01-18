using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Expressions;
using static VRCFaceTracking.Core.Params.Expressions.UnifiedExpressions;

namespace SteamLinkVRCFTModule;

public class SteamLinkVrcftModule : ExtTrackingModule
{
    private OscHandler _oscHandler;
    private const int DefaultPort = 9015;

    public override (bool SupportsEye, bool SupportsExpression) Supported => (true, true);

    public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
    {
        ModuleInformation.Name = "SteamLink Module";

        var stream = System.Reflection.Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("SteamLinkVRCFTModule.Assets.steamlink.png");
        ModuleInformation.StaticImages = stream != null ? new List<Stream> { stream } : ModuleInformation.StaticImages;

        //TODO better error handling on fail? isInit for OSC Handler?
        _oscHandler = new OscHandler(Logger, DefaultPort);

        return (true, true);
    }

    private static float CalculateEyeOpenness(float fEyeClosedWeight, float fEyeTightener)
    {
        return 1.0f - Math.Clamp(fEyeClosedWeight + fEyeClosedWeight * fEyeTightener, 0.0f, 1.0f);
    }

    private void UpdateEyeTracking()
    {
        {
            var fAngleX = MathF.Atan2(_oscHandler.EyeTrackData[0], -_oscHandler.EyeTrackData[2]);
            var fAngleY = MathF.Atan2(_oscHandler.EyeTrackData[1], -_oscHandler.EyeTrackData[2]);

            var fNmAngleX = fAngleX / (MathF.PI / 2.0f) * 2.0f;
            var fNmAngleY = fAngleY / (MathF.PI / 2.0f) * 2.0f;

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
            var fLeftOpenness = CalculateEyeOpenness(_oscHandler.Eyelids[0],
                OscHandler.UeData[EyeSquintLeft]);
            var fRightOpenness = CalculateEyeOpenness(_oscHandler.Eyelids[1],
                OscHandler.UeData[EyeSquintRight]);

            UnifiedTracking.Data.Eye.Left.Openness = fLeftOpenness; // fLeftOpenness;
            UnifiedTracking.Data.Eye.Right.Openness = fRightOpenness; // fRightOpenness;
        }
    }

    private void UpdateFaceTracking()
    {
        foreach (var entry in OscHandler.UeData)
        {
            UnifiedTracking.Data.Shapes[(int)entry.Key].Weight = entry.Value;
        }
        //?? fix some weird ft things need to check (Its not needed anymore, this in fact causes upper lip to not move properly, its more notorious when you try to set an "eww" face)
        //UnifiedTracking.Data.Shapes[(int)MouthUpperUpLeft].Weight = Math.Max(0, UnifiedTracking.Data.Shapes[(int)MouthUpperUpLeft].Weight - UnifiedTracking.Data.Shapes[(int)NoseSneerLeft].Weight);
        //UnifiedTracking.Data.Shapes[(int)MouthUpperUpRight].Weight = Math.Max(0, UnifiedTracking.Data.Shapes[(int)MouthUpperUpRight].Weight - UnifiedTracking.Data.Shapes[(int)NoseSneerRight].Weight);
        //UnifiedTracking.Data.Shapes[(int)MouthUpperDeepenLeft].Weight = Math.Max(0, UnifiedTracking.Data.Shapes[(int)MouthUpperUpLeft].Weight - UnifiedTracking.Data.Shapes[(int)NoseSneerLeft].Weight);
        //UnifiedTracking.Data.Shapes[(int)MouthUpperDeepenRight].Weight = Math.Max(0, UnifiedTracking.Data.Shapes[(int)MouthUpperUpRight].Weight - UnifiedTracking.Data.Shapes[(int)NoseSneerRight].Weight);

        //lip Suck
        UnifiedTracking.Data.Shapes[(int)LipSuckUpperLeft].Weight = Math.Min(
            1.0f - (float)Math.Pow(UnifiedTracking.Data.Shapes[(int)MouthUpperLeft].Weight, 1f / 6f),
            UnifiedTracking.Data.Shapes[(int)LipSuckUpperLeft].Weight);
        UnifiedTracking.Data.Shapes[(int)LipSuckUpperRight].Weight = Math.Min(
            1.0f - (float)Math.Pow(UnifiedTracking.Data.Shapes[(int)MouthUpperRight].Weight, 1f / 6f),
            UnifiedTracking.Data.Shapes[(int)LipSuckUpperRight].Weight);
    }

    public override void Update()
    {
        Thread.Sleep(10);
        UpdateEyeTracking();
        UpdateFaceTracking();
    }

    public override void Teardown()
    {
        _oscHandler.Teardown();
    }
}