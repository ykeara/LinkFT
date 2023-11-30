# SteamLink OSC Specification

The Implementation is **EXPERIMENTAL** and may change without notice causing app to fail.
Included for Reference is my understanding of Valve's OSC specification for SteamLink


### **OSC Specification**

Specification can be found [here.](https://opensoundcontrol.stanford.edu/spec-1_0.html) 


## Paramaters

### **Eye Tracking:**
Enable in SteamLink Settings in Headset

```
Bundle
/sl/eyeTrackedGazePoint - 3 floats, representing a 3d point in space the user is looking at
/avatar/parameters/LeftEyeX
/avatar/parameters/LeftEyeY
/avatar/parameters/RightEyeX
/avatar/parameters/RightEyeY
/tracking/eye/CenterVecFull
/avatar/parameters/RightEyeLid
/avatar/parameters/RightEyeLidExpandedSqueeze
/avatar/parameters/RightEyeSqueezeToggle
/avatar/parameters/RightEyeWidenToggle
/avatar/parameters/LeftEyeLid
/avatar/parameters/LeftEyeLidExpandedSqueeze
/avatar/parameters/LeftEyeSqueezeToggle
/avatar/parameters/LeftEyeWidenToggle
/tracking/eye/EyesClosedAmount
```

### **Face Tracking:**
Enable in SteamLink Settings in Headset

```
BUNDLE
/sl/xrfb/facec/LowerFace
/sl/xrfb/facec/UpperFace
/sl/xrfb/facew/BrowLowererL
/sl/xrfb/facew/BrowLowererR
/sl/xrfb/facew/CheekPuffL
/sl/xrfb/facew/CheekPuffR
/sl/xrfb/facew/CheekRaiserL
/sl/xrfb/facew/CheekRaiserR
/sl/xrfb/facew/CheekSuckL
/sl/xrfb/facew/CheekSuckR
/sl/xrfb/facew/ChinRaiserB
/sl/xrfb/facew/ChinRaiserT
/sl/xrfb/facew/DimplerL
/sl/xrfb/facew/DimplerR
/sl/xrfb/facew/EyesClosedL
/sl/xrfb/facew/EyesClosedR
/sl/xrfb/facew/EyesLookDownL
/sl/xrfb/facew/EyesLookDownR
/sl/xrfb/facew/EyesLookLeftL
/sl/xrfb/facew/EyesLookLeftR
/sl/xrfb/facew/EyesLookRightL
/sl/xrfb/facew/EyesLookRightR
/sl/xrfb/facew/EyesLookUpL

BUNDLE
/sl/xrfb/facew/EyesLookUpR
/sl/xrfb/facew/InnerBrowRaiserL
/sl/xrfb/facew/InnerBrowRaiserR
/sl/xrfb/facew/JawDrop
/sl/xrfb/facew/JawSidewaysLeft
/sl/xrfb/facew/JawSidewaysRight
/sl/xrfb/facew/JawThrust
/sl/xrfb/facew/LidTightenerL
/sl/xrfb/facew/LidTightenerR
/sl/xrfb/facew/LipCornerDepressorL
/sl/xrfb/facew/LipCornerDepressorR
/sl/xrfb/facew/LipCornerPullerL
/sl/xrfb/facew/LipCornerPullerR
/sl/xrfb/facew/LipFunnelerLB
/sl/xrfb/facew/LipFunnelerLT
/sl/xrfb/facew/LipFunnelerRB
/sl/xrfb/facew/LipFunnelerRT
/sl/xrfb/facew/LipPressorL
/sl/xrfb/facew/LipPressorR
/sl/xrfb/facew/LipPuckerL

BUNDLE
/sl/xrfb/facew/LipPuckerR
/sl/xrfb/facew/LipStretcherL
/sl/xrfb/facew/LipStretcherR
/sl/xrfb/facew/LipSuckLB
/sl/xrfb/facew/LipSuckLT
/sl/xrfb/facew/LipSuckRB
/sl/xrfb/facew/LipSuckRT
/sl/xrfb/facew/LipTightenerL
/sl/xrfb/facew/LipTightenerR
/sl/xrfb/facew/LipsToward
/sl/xrfb/facew/LowerLipDepressorL
/sl/xrfb/facew/LowerLipDepressorR
/sl/xrfb/facew/MouthLeft
/sl/xrfb/facew/MouthRight
/sl/xrfb/facew/NoseWrinklerL
/sl/xrfb/facew/NoseWrinklerR
/sl/xrfb/facew/OuterBrowRaiserL
/sl/xrfb/facew/OuterBrowRaiserR
/sl/xrfb/facew/UpperLidRaiserL
/sl/xrfb/facew/UpperLidRaiserR
/sl/xrfb/facew/UpperLipRaiserL
/sl/xrfb/facew/UpperLipRaiserR
```

Bundles appear to be somewhat fluid based on Network Conditions.
