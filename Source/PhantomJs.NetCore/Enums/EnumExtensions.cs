namespace PhantomJs.NetCore.Enums
{
  public static class EnumExtensions
  {
    public static string GetValue(this DimensionUnits dimensionUnit)
    {
      switch (dimensionUnit)
      {
        case DimensionUnits.Millimeter:
          return "mm";
        case DimensionUnits.Centimeter:
          return "cm";
        case DimensionUnits.Inch:
          return "in";
        default:
        case DimensionUnits.Pixel:
          return "px";
      }
    }

    public static string GetValue(this Orientations orientation)
    {
      switch (orientation)
      {
        case Orientations.Landscape:
          return "landscape";
        default:
        case Orientations.Portrait:
          return "portrait";
      }
    }
  }
}