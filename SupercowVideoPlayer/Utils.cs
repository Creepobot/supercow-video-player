using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Govno
{
    /// <summary>
    /// All kinds of crap that this program uses
    /// </summary>
    internal class Utils
    {
        /// <summary>
        /// Compresses the <paramref name="image"/> to the desired <paramref name="width"/> and <paramref name="height"/>
        /// </summary>
        /// <param name="image">Image to be compressed</param>
        /// <param name="width">Output image width</param>
        /// <param name="height">Output image height</param>
        /// <returns>
        /// Compressed image
        /// </returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);
            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        /// <summary>
        /// Takes a screenshot according to the specified parameters in <paramref name="rectangle"/>
        /// </summary>
        /// <param name="rectangle">
        /// Screenshot options
        /// <list type="bullet">
        /// <item>
        /// <term>X</term>
        /// <description>X position on the screen</description>
        /// </item>
        /// <item>
        /// <term>Y</term>
        /// <description>Y position on the screen</description>
        /// </item>
        /// <item>
        /// <term>Width</term>
        /// <description>Screenshot width</description>
        /// </item>
        /// <item>
        /// <term>Height</term>
        /// <description>Screenshot height</description>
        /// </item>
        /// </list>
        /// </param>
        /// <returns>
        /// Screenshot image
        /// </returns>
        public static Bitmap TakeScreenshot(Rectangle rectangle)
        {
            var bmpScreenshot = new Bitmap(rectangle.Width, rectangle.Height, PixelFormat.Format32bppArgb);
            using (var gfxScreenshot = Graphics.FromImage(bmpScreenshot))
                gfxScreenshot.CopyFromScreen(rectangle.X, rectangle.Y, 0, 0,
                    new Size(rectangle.Width, rectangle.Height), CopyPixelOperation.SourceCopy);
            return bmpScreenshot;
        }

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(uint x, uint y);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        /// <summary>
        /// Moves the cursor to a given <paramref name="X"/> and <paramref name="Y"/> position and makes a left click
        /// </summary>
        /// <param name="X">Desired X mouse position</param>
        /// <param name="Y">Desired Y mouse position</param>
        public static void LeftClick(int X, int Y)
        {
            SetCursorPos((uint)X, (uint)Y);
            mouse_event(0x02 | 0x04, (uint)X, (uint)Y, 0, 0);
        }
    }
}