using PhantomJs.NetCore.Enums;

namespace PhantomJs.NetCore
{
  /// <summary>
  /// Utility class to enable passing arguments to the generator.
  /// </summary>
  public class PdfGeneratorParams
  {
    /// <summary>
    /// Width size of the page.
    /// </summary>
    public int PageWidth { get; set; }
    
    /// <summary>
    /// Height size of the page.
    /// </summary>
    public int PageHeight { get; set; }
    
    /// <summary>
    /// Describe de DimensionUnit of the parameters Widht and Height.
    /// </summary>
    public DimensionUnits DimensionUnit { get; set; }
    
    /// <summary>
    /// Format of the document available in generator.
    /// </summary>
    public Formats Format { get; set; } = Formats.A4;
    
    /// <summary>
    /// Orientation which the document will be generated.
    /// </summary>
    public Orientations Orientation { get; set; } = Orientations.Portrait;
    
    /// <summary>
    /// Define the scaling factor for the document. The default option 
    /// <c>1</c> represents 100%.
    /// </summary>
    public double ZoomFactor { get; set; } = 1d;
  }
}