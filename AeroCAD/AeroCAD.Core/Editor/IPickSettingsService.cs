namespace Primusz.AeroCAD.Core.Editor
{
    public interface IPickSettingsService
    {
        double PickBoxSizePixels { get; set; }

        double GetPickRadiusWorld(double zoom);
    }
}
