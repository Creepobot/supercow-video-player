using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Govno;
using ConsoleHotkeys;
using Nevosoft.Supercow;

namespace SupercowBadApple
{
    static class Program
    {

        static void Main()
        {
            string input = "./input-frames";
            string output = "./output-frames";

            HotkeyManager.RegisterHotkey(Keys.A, KeyModifiers.Alt);
            HotkeyManager.HotkeyPressed += new EventHandler<HotkeyEventArgs>((object s, HotkeyEventArgs e) => Environment.Exit(0));

            Console.WriteLine("You can stop this process at any time by clicking Alt+A");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("Enter the path to the level to start in three seconds...");
            string level = Console.ReadLine();
            Console.WriteLine("Starting, please switch focus to the Supercow editor window");
            Thread.Sleep(3000);

            StartAnim(input, output, level, new Rectangle(0, 0, 1920, 1080),
            new Point(114, 64), new Point(1900, 130), new Point(1800, 130),
                (Point pixelPosition, Color pixelColor, Level currentLevel) =>
                {
                    var brightness = pixelColor.GetBrightness();
                    #region Bad Apple
                    if (brightness < 0.5)
                        currentLevel.Grounds[0, pixelPosition.Y, pixelPosition.X] = 4;
                    else
                        currentLevel.Grounds[0, pixelPosition.Y, pixelPosition.X] = 0;
                    #endregion
                    #region Rainy Boots
                    /*if (brightness > 0.7)
                    {
                        currentLevel.Grounds[0, pixelPosition.Y, 69 + pixelPosition.X] = 2;
                        currentLevel.Grounds[1, pixelPosition.Y, 69 + pixelPosition.X] = 0;
                    }
                    else if (brightness < 0.68)
                    {
                        currentLevel.Grounds[1, pixelPosition.Y, 69 + pixelPosition.X] = 4;
                        currentLevel.Grounds[0, pixelPosition.Y, 69 + pixelPosition.X] = 0;
                    }
                    else
                    {
                        currentLevel.Grounds[1, pixelPosition.Y, 69 + pixelPosition.X] = 0;
                        currentLevel.Grounds[0, pixelPosition.Y, 69 + pixelPosition.X] = 0;
                    }*/
                    #endregion
                });

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        /// <summary>
        /// Function to play animations in Supercow frame-by-frame
        /// </summary>
        /// <param name="originalFramesFolder">Path to the folder with frames to be used</param>
        /// <param name="newFramesFolder">Path to the folder for output frames</param>
        /// <param name="levelPath">Path to the level file</param>
        /// <param name="screenshotProps">Position and size of the screenshot</param>
        /// <param name="frameSize">Size of the image to be displayed on level</param>
        /// <param name="firstButton">Level editor's "Next level" button position on screen</param>
        /// <param name="secondButton">Level editor's "Previous level" button position on screen</param>
        /// <param name="function">
        /// A function that executes every pixel of an image. In it you have to handle the manipulation of the level
        /// <list type="bullet">
        /// <item>
        /// <term>Point</term>
        /// <description>X and Y coordinates of the pixel</description>
        /// </item>
        /// <item>
        /// <term>Color</term>
        /// <description>Color of the pixel</description>
        /// </item>
        /// <item>
        /// <term>Level</term>
        /// <description>Level class</description>
        /// </item>
        /// </list>
        /// </param>
        static void StartAnim(string originalFramesFolder, string newFramesFolder,
            string levelPath, Rectangle screenshotProps, Point frameSize, Point firstButton, Point secondButton,
            Action<Point, Color, Level> function)
        {
            var frames = new DirectoryInfo(originalFramesFolder).GetFiles().Where(r => r.Name.EndsWith(".png")).OrderBy(f => f.LastWriteTime);
            int i = 0;
            foreach (var file in frames)
            {
                using (Bitmap img = new Bitmap(Utils.ResizeImage(Image.FromFile(file.FullName), frameSize.X, frameSize.Y)))
                {
                    Level lev = Level.FromFile(levelPath);
                    ImageToLevel(lev, img, function);
                    lev.Save(levelPath);
                }

                Utils.LeftClick(firstButton.X, firstButton.Y);
                Thread.Sleep(100);
                Utils.LeftClick(secondButton.X, secondButton.Y);
                Thread.Sleep(100);
                Utils.TakeScreenshot(screenshotProps).Save(Path.Combine(newFramesFolder, $"{i}.png"));

                i++;
            }
        }

        /// <summary>
        /// Function that uses the <paramref name="image"/> to change the <paramref name="level"/> with the parameters in the <paramref name="function"/>
        /// </summary>
        /// <param name="level">Level to be changed with <paramref name="image"/></param>
        /// <param name="image">Input image</param>
        /// <param name="function">
        /// A function that executes every pixel of an image. In it you have to handle the manipulation of the level
        /// <list type="bullet">
        /// <item>
        /// <term>Point</term>
        /// <description>X and Y coordinates of the pixel</description>
        /// </item>
        /// <item>
        /// <term>Color</term>
        /// <description>Color of the pixel</description>
        /// </item>
        /// <item>
        /// <term>Level</term>
        /// <description><see cref="Level"/> class</description>
        /// </item>
        /// </list>
        /// </param>
        static void ImageToLevel(Level level, Bitmap image, Action<Point, Color, Level> function)
        {
            using (LockBitmap lb = new LockBitmap(image))
            {
                lb.LockBits();
                for (int x = 0; x < lb.Width; x++)
                    for (int y = 0; y < lb.Height; y++)
                    {
                        Color clr = lb.GetPixel(x, y);
                        function(new Point(x, y), clr, level);
                    }
                lb.UnlockBits();
            }
        }
    }
}