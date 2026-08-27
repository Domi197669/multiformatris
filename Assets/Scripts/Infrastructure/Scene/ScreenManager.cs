using UnityEngine;

namespace Multiformatris.Infrastructure.Scene
{
    public enum ScreenFitMode
    {
        PortraitMobile,
        LandscapeTablet
    }

    public static class ScreenManager
    {
        public const float PortraitDeviceBreakpoint = 0.62f;

        public static ScreenFitMode CurrentMode
        {
            get
            {
                float ratio = AspectRatio;
                return ratio < PortraitDeviceBreakpoint ? ScreenFitMode.PortraitMobile : ScreenFitMode.LandscapeTablet;
            }
        }

        public static bool IsPortrait => CurrentMode == ScreenFitMode.PortraitMobile;

        public static float AspectRatio
        {
            get
            {
                if (Screen.height <= 0) return 1f;
                return (float)Screen.width / Screen.height;
            }
        }

        public static void ApplyAutoOrientation()
        {
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }
}
