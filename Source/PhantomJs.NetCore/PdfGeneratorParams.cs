using PhantomJs.NetCore.Enums;

namespace PhantomJs.NetCore
{
  public class PdfGeneratorParams
  {
    public int PageWidth { get; set; }
    public int PageHeight { get; set; }
    public DimensionUnits DimensionUnit { get; set; }
    public Formats Format { get; set; } = Formats.A4;
    public Orientations Orientation { get; set; } = Orientations.Portrait;
    public double ZoomFactor { get; set; } = 1d;
  }
}