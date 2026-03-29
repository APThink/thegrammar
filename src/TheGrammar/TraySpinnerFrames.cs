using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace TheGrammar;

internal static class TraySpinnerFrames
{
  [DllImport("user32.dll")]
  private static extern bool DestroyIcon(nint handle);

  internal static Icon[] Generate(int frameCount = 20, int size = 32)
  {
    var frames = new Icon[frameCount];
    var sweepAngle = 270f;
    var stepAngle = 360f / frameCount;
    var margin = 3;
    var rect = new Rectangle(margin, margin, size - margin * 2 - 1, size - margin * 2 - 1);

    for (var i = 0; i < frameCount; i++)
    {
      var startAngle = i * stepAngle - 90f;

      using var bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      using var g = Graphics.FromImage(bmp);

      g.Clear(Color.Transparent);
      g.SmoothingMode = SmoothingMode.AntiAlias;

      using (var trackPen = new Pen(Color.FromArgb(55, 92, 184, 92), 2f))
        g.DrawEllipse(trackPen, rect);

      using var arcPen = new Pen(Color.FromArgb(92, 184, 92), 2.5f);
      arcPen.StartCap = LineCap.Round;
      arcPen.EndCap = LineCap.Round;
      g.DrawArc(arcPen, rect, startAngle, sweepAngle);

      var hIcon = bmp.GetHicon();
      frames[i] = (Icon)Icon.FromHandle(hIcon).Clone();
      DestroyIcon(hIcon);
    }

    return frames;
  }
}