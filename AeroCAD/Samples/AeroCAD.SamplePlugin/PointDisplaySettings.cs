using Primusz.AeroCAD.Core.Editor;

namespace Primusz.AeroCAD.SamplePlugin
{
    public static class PointDisplaySettings
    {
        public static int GetPdMode(ISystemVariableService variables)
        {
            return variables?.Get(SystemVariableService.PdMode, 0) ?? 0;
        }

        public static double GetPdSize(ISystemVariableService variables)
        {
            return variables?.Get(SystemVariableService.PdSize, 0d) ?? 0d;
        }

        public static double ResolveDisplaySize(ISystemVariableService variables, double zoom = 1d, double viewportHeight = 100d)
        {
            var pdSize = GetPdSize(variables);
            var effectiveZoom = zoom > 1e-6 ? zoom : 1d;
            if (pdSize > 0d)
                return pdSize / effectiveZoom;

            var effectiveHeight = viewportHeight > 1e-6 ? viewportHeight : 100d;
            if (pdSize < 0d)
                return (effectiveHeight / effectiveZoom) * (-pdSize / 100d);

            return (effectiveHeight / effectiveZoom) * 0.05d;
        }
    }
}
