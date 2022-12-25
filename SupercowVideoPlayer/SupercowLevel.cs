using System.Text;
using System.Drawing;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System;

namespace Nevosoft.Supercow
{
    /// <summary>
    /// Сlass of a typical object in <see cref="Level"/>
    /// </summary>
    public struct LevelObject
    {
        /// <summary>
        /// Name of the gameobject whose type this object is
        /// </summary>
        public string Name;
        /// <summary>
        /// Position and size of the object
        /// </summary>
        public RectangleF Rectangle;
        /// <summary>
        /// Layer on which the object is to be displayed
        /// </summary>
        public int DrawLayer;
        /// <summary>
        /// Angle of rotation of the object
        /// </summary>
        public float Rotation;
        /// <summary>
        /// The object's endpoint, in case the object can use it (platforms, enemies, triangles)
        /// </summary>
        public PointF EndPosition;
        /// <summary>
        /// Whether the view of the object is inverted or not
        /// </summary>
        public bool Inverted;
        /// <summary>
        /// Extraparameter of the object
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="string"/></term>
        /// <description>If the object is tablet, it looks for a line with that name in the tips.txt file of the game
        /// <br/>Example:<code>tip_1_1_1</code></description>
        /// </item>
        /// <item>
        /// <term><see cref="int"/></term>
        /// <description>If the object is an enemy, sets the size (maximum 3)<br/>Example:<code>2</code></description>
        /// </item>
        /// <item>
        /// <term><see cref="decimal"/></term>
        /// <description>If the object is a jewel or power up or even guillotine, sets the respawn speed in seconds<br/>Example:<code>0.5</code></description>
        /// </item>
        /// <item>
        /// <term><see cref="decimal"/></term>
        /// <description>If the object is a moving platform, sets the speed
        /// <br/>(not in seconds, in who the fuck knows what)
        /// <br/>It can also have a <see cref="string"/> meaning sound playback:
        /// <list type="bullet">
        /// <item>
        /// <term>s</term>
        /// <description>Platform floats and plays a corresponding sound</description>
        /// </item>
        /// <item>
        /// <term>m</term>
        /// <description>Platform floats silently</description>
        /// </item>
        /// </list><br/>Example:<code>69.420s</code></description>
        /// </item>
        /// </list>
        /// </summary>
        public string ExtraParameter;
    }

    /// <summary>
    /// Level class from the Supercow game. Has the ".lev" extension
    /// </summary>
    public class Level
    {
        /// <summary>
        /// Level name. Not used in the game, but exists in file, so why not
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// Number of the background to be used in the level. Depends on the set backgrounds in the game
        /// </summary>
        public int Background { get; set; } = 0;
        /// <summary>
        /// Number of the task to be used in the level
        /// <list type="bullet">
        /// <item>
        /// <term>0</term>
        /// <description>Pass to exit</description>
        /// </item>
        /// <item>
        /// <term>1</term>
        /// <description>Find keys</description>
        /// </item>
        /// <item>
        /// <term>2</term>
        /// <description>Eliminate enemies</description>
        /// </item>
        /// <item>
        /// <term>3</term>
        /// <description>Find gems</description>
        /// </item>
        /// <item>
        /// <term>4</term>
        /// <description>Clear garbage</description>
        /// </item>
        /// <item>
        /// <term>5</term>
        /// <description>Eliminate boss</description>
        /// </item>
        /// </list>
        /// </summary>
        public int Task { get; set; } = 0;
        /// <summary>
        /// Number of the music to be used in the level. Depends on the set music in the game
        /// </summary>
        public int Music { get; set; } = 0;
        /// <summary>
        /// List of <see cref="LevelObject"/>s on the level
        /// </summary>
        public List<LevelObject> Objects { get; set; } = new List<LevelObject>();
        /// <summary>
        /// Array of grounds located on the level<br/>First number is the array layer (maximum 5)
        /// <br/>The second and third numbers are the X and Y positions on the array
        /// </summary>
        /// <returns>
        /// Ground number at the given coordinates
        /// </returns>
        public int[][,] Grounds { get; set; } = new int[6][,]
        { new int[256,64], new int[256,64], new int[256,64],
           new int[256,64], new int[256,64], new int[256,64] };

        /// <summary>
        /// Create object of class
        /// </summary>
        public Level() { }
        /// <summary>
        /// Open level from file
        /// </summary>
        public Level(string filename) => Load(filename);
        /// <summary>
        /// Open level from <see cref="Stream"/>
        /// </summary>
        public Level(Stream stream) => Load(stream);

        /// <summary>
        /// Create object of class from file
        /// </summary>
        public static Level FromFile(string filename) => new Level(filename);
        /// <summary>
        /// Create object of class from <see cref="Stream"/>
        /// </summary>
        public static Level FromStream(Stream stream) => new Level(stream);

        /// <summary>
        /// Save level to file
        /// </summary>
        public void Save(string filename)
        {
            using (FileStream fs = new FileStream(filename,
                FileMode.Create, FileAccess.Write, FileShare.None))
                Save(fs);
        }

        private void Load(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("File \"" + filename + "\" not found!");
            using (FileStream FS = new FileStream(filename, FileMode.Open))
                    Load(FS);
        }

        private void Load(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(null);
            if (!(stream.CanRead && stream.CanSeek))
                throw new FileLoadException("Stream reading or seeking is not avaiable!");

            stream.Seek(0, SeekOrigin.Begin);
            using (StreamReader sr = new StreamReader(stream))
            {
                string content = sr.ReadToEnd();
                Name = ParseString(content, "levelname=");
                Objects.Capacity = ParseInt(content, "numlevelobjs=");
                Background = ParseInt(content, "curlevelback=");
                Task = ParseInt(content, "curleveltask=");
                Music = ParseInt(content, "curlevelmusic=");

                var index = 0;
                for (var i = 0; i < Objects.Capacity; i++)
                {
                    index = content.IndexOf("<obj", index);
                    if (index == -1) break;
                    var obj = new LevelObject();
                    var endIndex = content.IndexOf("</obj", index);

                    obj.Name = ParseString(content, "name=", index);

                    var rect = new float[4];
                    ParseFloatArray(content, "rect=", rect, index);
                    obj.Rectangle = RectangleF.FromLTRB(rect[0], rect[1], rect[2], rect[3]);

                    obj.DrawLayer = ParseInt(content, "drawlayer=", index);
                    obj.Rotation = ParseFloat(content, "rotation=", index);

                    var endPos = new float[2];
                    ParseFloatArray(content, "endpos=", endPos, index);
                    obj.EndPosition = new PointF(endPos[0], endPos[1]);

                    obj.Inverted = ParseInt(content, "invertobj=", index) != 0;

                    var extraparamIndex = content.IndexOf("extraparam=", index);
                    if (extraparamIndex != -1 && extraparamIndex < endIndex)
                        obj.ExtraParameter = ParseString(content, "extraparam=", index);

                    Objects.Add(obj);
                    index = endIndex;
                }

                for (var j = 0; j < 6; j++)
                {
                    var prefix = $"groundlayer{j}=";
                    var layerIndex = content.IndexOf(prefix);
                    if (layerIndex == -1) break;

                    layerIndex += prefix.Length;
                    layerIndex += 2;

                    for (var k = 0; k < 64; k++)
                    {
                        for (var m = 0; m < 256; m++)
                        {
                            Grounds[j][m, k] = content[layerIndex] - '0';
                            layerIndex++;
                        }
                        layerIndex += 2;
                    }
                }
            }
        }

        static string ParseString(string content, string prefix, int startIndex = 0)
        {
            var res = content.IndexOf(prefix, startIndex);
            if (res == -1) return "";

            var src = res + prefix.Length;
            var end = content.IndexOf("\r", src);
            return content.Substring(src, end - src);
        }

        static int ParseInt(string content, string prefix, int startIndex = 0)
        {
            var value = ParseString(content, prefix, startIndex);
            if (value != "") return (int)double.Parse(value);
            return 0;
        }

        static float ParseFloat(string content, string prefix, int startIndex = 0)
        {
            var value = ParseString(content, prefix, startIndex);
            if (value != "") return (float)double.Parse(value, CultureInfo.InvariantCulture);
            return 0;
        }

        static void ParseFloatArray(string content, string prefix, float[] arr, int startIndex = 0)
        {
            var value = ParseString(content, prefix, startIndex);
            if (value == "") return;

            var endIndex = value.IndexOf(")");
            if (endIndex == -1) endIndex = value.IndexOf("}");
            if (endIndex == -1) return;

            var index = 1;
            var i = 0;
            while (true)
            {
                var currEndIndex = value.IndexOf(",", index);
                if (currEndIndex == -1) currEndIndex = value.IndexOf("}", index);
                if (currEndIndex == -1) currEndIndex = value.IndexOf(")", index);
                if (currEndIndex == -1)
                    break;

                arr[i] = (float)double.Parse(value.Substring(index, currEndIndex - index), CultureInfo.InvariantCulture);

                index = currEndIndex + 1;
                if (index >= value.Length) break;
                i++;
            }
        }

        /// <summary>
        /// Save level to <see cref="Stream"/>
        /// </summary>
        public void Save(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(null);
            if (!(stream.CanWrite && stream.CanSeek))
                throw new FileLoadException("Stream writing or seeking is not avaiable!");

            stream.Seek(0, SeekOrigin.Begin);
            using (StreamWriter sw = new StreamWriter(stream, Encoding.UTF8))
            {
                sw.Write($"levelname={Name}\r\n");
                sw.Write($"numlevelobjs={Objects.Count}\r\n\r\n");
                sw.Write($"curlevelback={Background}\r\n\r\n");
                sw.Write($"curleveltask={Task}\r\n\r\n");
                sw.Write($"curlevelmusic={Music}\r\n\r\n");

                for (int i = 0; i < Objects.Count; i++)
                {
                    var obj = Objects[i];
                    sw.Write($"<obj{i}/>\r\n");
                    sw.Write($"    name={obj.Name}\r\n");
                    var r = obj.Rectangle;
                    sw.Write($"    rect={{{DecToStr(r.Left, "0.00")},{DecToStr(r.Top, "0.00")}," +
                        $"{DecToStr(r.Right, "0.00")},{DecToStr(r.Bottom, "0.00")}}}\r\n");
                    sw.Write($"    drawlayer={obj.DrawLayer}\r\n");
                    sw.Write($"    rotation={DecToStr(obj.Rotation, "0.000")}\r\n");
                    var ep = obj.EndPosition;
                    sw.Write($"    endpos={{{DecToStr(ep.X, "0.00")},{DecToStr(ep.Y, "0.00")}}}\r\n");
                    sw.Write($"    invertobj={(obj.Inverted ? 1 : 0)}\r\n");
                    if (obj.ExtraParameter != "" && obj.ExtraParameter != null)
                        sw.Write($"    extraparam={obj.ExtraParameter}\r\n\r\n");
                    sw.Write($"</obj{i}>\r\n\r\n\r\n");
                }

                sw.Write($"\r\n\r\n");
                for (var j = 0; j < 6; j++)
                {
                    sw.Write($"groundlayer{j}=\r\n");
                    for (var k = 0; k < 64; k++)
                    {
                        for (var m = 0; m < 256; m++)
                        {
                            sw.Write(Grounds[j][m, k]);
                        }
                        sw.Write($"\r\n");
                    }
                    sw.Write($"\r\n");
                }
                sw.Write($"\r\n\r\n");
            }
        }

        private static string DecToStr(object obj, string format)
        {
            if (obj is float dec) return dec.ToString(format, CultureInfo.InvariantCulture);
            return "";
        }
    }
}