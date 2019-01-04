using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commons.Music.Midi;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace ImageToMidi
{
    class PixelMidi
    {
        public double NoteType { get; set; } = 0.25;
        public int NoteCenter { get; set; } = 63;
        public string FileName { get; set; } = "Untitled";

        public Bitmap Source { get; set; } = null;
        public MidiMusic Music { get; set; } = null;

        public double FilterStrength { get; set; } = 0.80;
        public bool InvertSource { get; set; } = false;

        private static short noteLen = 0x03C0;

        private MidiPlayer player = null;

        public bool IsPlay
        {
            get
            {
                if (player is MidiPlayer && player.State == PlayerState.Playing)
                    return (true);
                else
                    return (false);
            }
        }

        public static Bitmap LoadBitmap(string bitmapFile)
        {
            Bitmap result = null;
            if (File.Exists(bitmapFile))
            {
                using (var fs = new FileStream(bitmapFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var bmp = Image.FromStream(fs);
                    result = (Bitmap)Image.FromStream(fs);
                }
            }
            return (result);
        }

        public static Bitmap ToBW(Bitmap Bmp)
        {
            if (Bmp is Bitmap)
                return (Bmp.Clone(new Rectangle(0, 0, Bmp.Width, Bmp.Height), PixelFormat.Format4bppIndexed));
            else return null;
        }

        public static Bitmap ToIndexed16(Bitmap Bmp)
       {
            if (Bmp is Bitmap)
                return (Bmp.Clone(new Rectangle(0, 0, Bmp.Width, Bmp.Height), PixelFormat.Format4bppIndexed));
            else return null;
        }

        public static Bitmap ToIndexed256(Bitmap Bmp)
        {
            if (Bmp is Bitmap)
                return (Bmp.Clone(new Rectangle(0, 0, Bmp.Width, Bmp.Height), PixelFormat.Format8bppIndexed));
            else return null;
        }

        public static Bitmap GrayScale(Bitmap Bmp)
        {
            Bitmap result = null;

            if (Bmp is Bitmap)
            {
                #region BT709
                ImageAttributes ia = new ImageAttributes();
                ColorMatrix icm = new ColorMatrix( new[]{
                    new float[] { 0.2125f, 0.2125f, 0.2125f, 0, 0},        // red scaling factor
                    new float[] { 0.7154f, 0.7154f, 0.7154f, 0, 0},        // green scaling factor
                    new float[] { 0.0721f, 0.0721f, 0.0721f, 0, 0},        // blue scaling factor
                    new float[] {       0,       0,       0, 1, 0},        // alpha scaling factor
                    new float[] {       0,       0,       0, 0, 1}         // three translations
                });
                ia.SetColorMatrix(icm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                #endregion

                result = new Bitmap(Bmp.Width, Bmp.Height, Bmp.PixelFormat);
                using (var g = Graphics.FromImage(result))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    g.DrawImage(Bmp,
                                new Rectangle(0, 0, Bmp.Width, Bmp.Height),
                                0, 0, Bmp.Width, Bmp.Height,
                                GraphicsUnit.Pixel,
                                ia);
                }
            }

            return ((Bitmap)result);
        }

        public static Bitmap Invert(Bitmap Bmp)
        {
            Bitmap result = null;

            if (Bmp is Bitmap)
            {
                #region Invert
                ImageAttributes ia = new ImageAttributes();
                ColorMatrix icm = new ColorMatrix( new[]{
                    new float[] { -1,  0,  0, 0, 0},        // red scaling factor
                    new float[] {  0, -1,  0, 0, 0},        // green scaling factor
                    new float[] {  0,  0, -1, 0, 0},        // blue scaling factor
                    new float[] {  0,  0,  0, 1, 0},        // alpha scaling factor
                    new float[] {  1,  1,  1, 0, 1}         // three translations
                });
                ia.SetColorMatrix(icm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                #endregion

                result = new Bitmap(Bmp.Width, Bmp.Height, Bmp.PixelFormat);
                using (var g = Graphics.FromImage(result))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    g.DrawImage(Bmp,
                                new Rectangle(0, 0, Bmp.Width, Bmp.Height),
                                0, 0, Bmp.Width, Bmp.Height,
                                GraphicsUnit.Pixel,
                                ia);
                }
            }

            return ((Bitmap)result.Clone());
        }

        public static Bitmap Resize(Bitmap bitmap, int width, int height)
        {
            Bitmap result = null;

            if (bitmap is Bitmap)
            {
                var aspect = (double)bitmap.Width / (double)bitmap.Height;

                if (width <= 0 && height <= 0)
                {
                    width = bitmap.Width;
                    height = bitmap.Height;
                }
                else if (height <= 0)
                    height = (int)(width / aspect);
                else if (width <= 0)
                    width = (int)(height * aspect);

                result = new Bitmap(width, height, bitmap.PixelFormat);
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.DrawImage(bitmap, new Rectangle(0, 0, width, height), new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
                }
            }

            return (result);
        }

        public static System.Windows.Media.Imaging.BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            System.Windows.Media.Imaging.BitmapImage result = null;

            if (bitmap is Bitmap)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    result = new System.Windows.Media.Imaging.BitmapImage();
                    result.BeginInit();
                    result.StreamSource = memory;
                    result.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    result.EndInit();
                }
            }

            return (result);
        }

        public static Bitmap ImageSourceToBitmap(System.Windows.Media.Imaging.BitmapSource bitmap)
        {
            Bitmap result = null;

            if (bitmap is System.Windows.Media.Imaging.BitmapSource)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    System.Windows.Media.Imaging.BitmapEncoder enc = new System.Windows.Media.Imaging.PngBitmapEncoder();
                    enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                    enc.Save(outStream);
                    outStream.Seek(0, SeekOrigin.Begin);
                    result = new Bitmap(outStream);
                }
            }

            return (result);
        }

        private double CalcAlpha(Color c, Color co, out byte velocity)
        {
            var brightness = c.GetBrightness();
            var alpha = c.A / 4.0;
            if (co.R == c.R && co.G == c.G && co.B == c.B || brightness > FilterStrength) alpha = 0;
            velocity = (byte)(((1.0 - brightness) / 2.0) * 255);
            return (alpha);
        }

        private enum ColorChannel { A, R, G, B }
        public class Histogram
        {
            public double[] A = new double[256];
            public double[] R = new double[256];
            public double[] G = new double[256];
            public double[] B = new double[256];
        }

        private Histogram GetHistogram(Bitmap bitmap, bool SplitChannel = true)
        {
            Histogram result = new Histogram();

            Color c;

            var gray = bitmap.Height > 128 ? Resize(bitmap, 0, 128): bitmap;
            for (int x = 0; x < gray.Width; x++)
            {
                for (int y = gray.Height - 1; y >= 0; y--)
                {
                    c = gray.GetPixel(x, y);
                    result.A[c.A]++;
                    result.R[c.R]++;
                    result.G[c.G]++;
                    result.B[c.B]++;
                }
            }

            return (result);
        }

        private Bitmap MakeHistogram(double[] channel, Color color)
        {
            Bitmap result = new Bitmap(256, 100, PixelFormat.Format32bppArgb);

            double total = 0;
            for (var i = 0; i < 256; i++)
            {
                total += channel[i];
            }
            double min=1, max=0;
            double[] v = new double[256];
            for (var i = 1; i < 256; i++)
            {
                v[i] = channel[i] / total;
                min = Math.Min(min, v[i]);
                max = Math.Max(max, v[i]);
            }
            double factor = 100 / (max - min);

            //result.SetPixel(0, 0, color);
            for (var i = 1; i < 256; i++)
            {
                int iv = 100 - (int)(v[i] * factor);
                if (iv < 0) iv = 0;
                if (iv > 99) iv = 99;
                result.SetPixel(i, iv, color);
            }

            return (result);
        }

        private Bitmap MakeHistogram(Histogram histogram)
        {
            Bitmap result = new Bitmap(256, 100, PixelFormat.Format32bppArgb);

            //var A = MakeHistogram(histogram.A, Color.White);
            var R = MakeHistogram(histogram.R, Color.Red);
            var G = MakeHistogram(histogram.G, Color.Green);
            var B = MakeHistogram(histogram.B, Color.Blue);

            using (Graphics g = Graphics.FromImage(result))
            {
                //g.DrawImage(A, 0, 0);
                g.DrawImage(R, 0, 0);
                g.DrawImage(G, 0, 0);
                g.DrawImage(B, 0, 0);
            }

            return (result);
        }

        private MidiTrack GetMetaTrack(string name)
        {
            var track = new MidiTrack();
            // Sequence or track name
            track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x03, 0, Encoding.Default.GetBytes(name))));
            // Copyright notice
            track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x02, 0, Encoding.Default.GetBytes("Copyright 2018 NetCharm"))));
            // Text event
            track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x01, 0, Encoding.Default.GetBytes("NetCharm"))));
            // Instrument name
            //track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x04, 0, Encoding.Default.GetBytes("Piano"))));
            // Time signature
            track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x58, 0, Encoding.Default.GetBytes("\x04\x02\x18\x08"))));
            // Key signature
            track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x59, 0, Encoding.Default.GetBytes("\x00\x00"))));
            // Tempo setting (us/quarter-note)
            track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x51, 0, Encoding.Default.GetBytes("\x07\xA1\x20")))); // 120  beats/minute
                                                                                                                                         //track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x51, 0, Encoding.Default.GetBytes("\x09\x27\xC0")))); // 100  beats/minute
                                                                                                                                         // End of track
            track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x2F, 0, Encoding.Default.GetBytes(""))));

            return (track);
        }

        private MidiTrack GetPixelTrack(Bitmap bitmap, string name, bool grayscale = true, bool invert=true)
        {
            MidiTrack result = null;

            if (bitmap is Bitmap)
            {
                result = new MidiTrack();

                int delta = (int)(noteLen * NoteType);

                var gray = bitmap.Height > 128 ? Resize(bitmap, 0, 128): bitmap;
                gray = grayscale ? GrayScale(gray) : gray;
                if (InvertSource && invert) gray = Invert(gray);

                Color c;
                Color co = gray.GetPixel(0,0);
                int count = 0;

                int[,] om = new int[gray.Width, gray.Height];
                Dictionary<int[,], byte> nm = new Dictionary<int[,], byte>();

                count = 0;

                bool[] blank = new bool[gray.Width];
                for (int i = 0; i < blank.Length; i++) blank[i] = true;
                int marginL = 0;
                int marginR = 0;
                for (int x = 0; x < gray.Width; x++)
                {
                    //for (int y = 0; y < gray.Height; y++)
                    for (int y = gray.Height - 1; y >= 0; y--)
                    {
                        c = gray.GetPixel(x, y);
                        byte velocity = 0x40;
                        var alpha = CalcAlpha(c, co, out velocity);
                        if (alpha > 0)
                        {
                            om[x, y] = count * delta;
                            blank[x] = false;
                            if (marginL == 0) marginL = x;
                            count = 0;
                            break;
                        }
                    }
                    if (blank[x]) count++;
                }
                marginR = count;

                //int offset = (int)(gray.Height / 2.0 - NoteCenter);
                int offset = (int)(NoteCenter - gray.Height / 2.0);
                if (offset < 0) offset = 0;

                var track = new MidiTrack();
                track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x03, 0, Encoding.Default.GetBytes(name))));
                track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.NoteOff, (byte)NoteCenter, 0, Encoding.Default.GetBytes(""))));

                double[] alphav = new double[128];
                for (int x = 0; x < gray.Width; x++)
                {
                    if (blank[x]) continue;

                    bool IsDelta = false;
                    for (int y = gray.Height - 1; y >= 0; y--)
                    {
                        c = gray.GetPixel(x, y);
                        byte velocity = 0x40;
                        alphav[y] = CalcAlpha(c, co, out velocity);
                        if (alphav[y] > 0)
                        {
                            var noteOn  = new MidiEvent(MidiEvent.NoteOn,  (byte)(gray.Height - y - 1 + offset), velocity, Encoding.Default.GetBytes(""));
                            if (IsDelta)
                                track.AddMessage(new MidiMessage(0, noteOn));
                            else
                            {
                                track.AddMessage(new MidiMessage(om[x, y], noteOn));
                                IsDelta = true;
                            }
                        }
                    }
                    IsDelta = false;
                    for (int y = 0; y < gray.Height; y++)
                    {
                        if (alphav[y] > 0)
                        {
                            var noteOff = new MidiEvent(MidiEvent.NoteOn, (byte)(gray.Height - y - 1 + offset), 0x00, Encoding.Default.GetBytes(""));
                            if (IsDelta)
                                track.AddMessage(new MidiMessage(0, noteOff));
                            else
                            {
                                track.AddMessage(new MidiMessage(delta, noteOff));
                                IsDelta = true;
                            }
                        }
                    }
                }

                track.AddMessage(new MidiMessage(marginR * delta, new MidiEvent(MidiEvent.NoteOff, (byte)NoteCenter, 0, Encoding.Default.GetBytes(""))));
                track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x2F, 0, Encoding.Default.GetBytes(""))));

                result = track;
            }

            return (result);
        }

        /// <summary>
        /// 
        /// -----------------------
        /// Type    Event
        /// -----------------------
        /// 0x00    Sequence number
        /// 0x01    Text event
        /// 0x02    Copyright notice
        /// 0x03    Sequence or track name
        /// 0x04    Instrument name
        /// 0x05    Lyric text
        /// 0x06    Marker text
        /// 0x07    Cue point
        /// 0x20    MIDI channel prefix assignment
        /// 0x2F    End of track
        /// 0x51    Tempo setting
        /// 0x54    SMPTE offset
        /// 0x58    Time signature
        /// 0x59    Key signature
        /// 0x7F    Sequencer specific event
        /// -----------------------
        /// 
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="singleTrack"></param>
        /// <returns></returns>
        public MidiMusic GetPixelMidi(Bitmap bitmap, bool singleTrack = true)
        {
            MidiMusic result = null;

            if (bitmap is Bitmap)
            {
                int delta = (int)(noteLen * NoteType);

                result = new MidiMusic();
                result.DeltaTimeSpec = noteLen;
                result.Format = 1;

                var track_sys = GetMetaTrack(FileName);
                result.AddTrack(track_sys);

                if (singleTrack)
                {
                    var track = GetPixelTrack(bitmap, FileName);
                    result.AddTrack(track);
                }
                else
                {
                    var gray = GrayScale(bitmap.Height > 128 ? Resize(bitmap, 0, 128): bitmap);
                    if (InvertSource) gray = Invert(gray);

                    Color c;
                    Color co = gray.GetPixel(0,0);
                    int count = 0;

                    for (int y = 0; y < gray.Height; y++)
                    {
                        count = 0;
                        var track = new MidiTrack();
                        track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x00, 0, Encoding.Default.GetBytes($"{y}"))));
                        track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x03, 0, Encoding.Default.GetBytes($"{FileName}_{y}"))));
                        for (int x = 0; x < gray.Width; x++)
                        {
                            c = gray.GetPixel(x, y);
                            byte velocity = 0x40;
                            var alpha = CalcAlpha(c, co, out velocity);
                            if (alpha > 0)
                            {
                                var noteOn  = new MidiEvent(MidiEvent.NoteOn,  0x40, velocity, Encoding.Default.GetBytes(""));
                                var noteOff = new MidiEvent(MidiEvent.NoteOn, 0x40, 0x00, Encoding.Default.GetBytes(""));
                                track.AddMessage(new MidiMessage(count * delta, noteOn));
                                track.AddMessage(new MidiMessage(delta, noteOff));
                                count = 0;
                            }
                            else count++;
                        }
                        track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x2F, 0, Encoding.Default.GetBytes(""))));
                        result.AddTrack(track);
                    }
                }
            }
            Music = result;

            return (result);
        }

        public MidiMusic GetHistogramMidi(Bitmap bitmap, bool singleTrack = true)
        {
            MidiMusic result = null;

            if (bitmap is Bitmap)
            {
                var lastNoteCenter = NoteCenter;
                NoteCenter = 63;

                int delta = (int)(noteLen * NoteType);

                result = new MidiMusic();
                result.DeltaTimeSpec = noteLen;
                result.Format = 1;

                var track_sys = GetMetaTrack(FileName);
                result.AddTrack(track_sys);

                var histogram = GetHistogram(bitmap, true);

                if (singleTrack)
                {
                    var hist = MakeHistogram(histogram);
                    var track = GetPixelTrack(hist, $"{FileName}_Histogram", false, false);
                    if(track is MidiTrack) result.AddTrack(track);
                }
                else
                {
                    var hist_a = MakeHistogram(histogram.A, Color.White );
                    var hist_r = MakeHistogram(histogram.R, Color.Red );
                    var hist_g = MakeHistogram(histogram.G, Color.Green );
                    var hist_b = MakeHistogram(histogram.B, Color.Blue );

                    var track_a = GetPixelTrack(hist_a, $"{FileName}_Histogram_A", false, false);
                    var track_r = GetPixelTrack(hist_r, $"{FileName}_Histogram_R", false, false);
                    var track_g = GetPixelTrack(hist_g, $"{FileName}_Histogram_G", false, false);
                    var track_b = GetPixelTrack(hist_b, $"{FileName}_Histogram_B", false, false);

                    if (track_a is MidiTrack) result.AddTrack(track_a);
                    if (track_r is MidiTrack) result.AddTrack(track_r);
                    if (track_g is MidiTrack) result.AddTrack(track_g);
                    if (track_b is MidiTrack) result.AddTrack(track_b);
                }
                NoteCenter = lastNoteCenter;

                Music = result;
            }

            return (result);
        }

        public MidiMusic DemoPixelMidi()
        {
            MidiMusic result = new MidiMusic();

            short noteLen = 0x03C0;
            int offset = 4 * noteLen;
            result.DeltaTimeSpec = noteLen;
            result.Format = 1;

            var track_sys = GetMetaTrack(FileName);
            result.AddTrack(track_sys);

            var track = new MidiTrack();
            track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 3, 0, Encoding.Default.GetBytes("Demo"))));
            var delta = new int[128];
            for (int x = 0; x < 128; x++)
            {
                var noteOn  = new MidiEvent(MidiEvent.NoteOn,  (byte)x, 0x64, Encoding.Default.GetBytes(""));
                var noteOff = new MidiEvent(MidiEvent.NoteOn, (byte)x, 0x00, Encoding.Default.GetBytes(""));
                track.AddMessage(new MidiMessage(0, noteOn));
                track.AddMessage(new MidiMessage(offset, noteOff));
            }
            track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x2F, 0, Encoding.Default.GetBytes(""))));
            result.AddTrack(track);

            return (result);
        }

        public void Play(MidiMusic music = null)
        {
            if (player is MidiPlayer) Stop();

            var access = MidiAccessManager.Default;
            var output = access.OpenOutputAsync(access.Outputs.Last().Id).Result;
            if (music is MidiMusic)
                player = new MidiPlayer(music, output);
            else if (Music is MidiMusic)
                player = new MidiPlayer(Music, output);
            else
                return;

            if(player is MidiPlayer)
            {
                if(player.State == PlayerState.Stopped || player.State == PlayerState.Paused)
                {
                    //player.EventReceived += (MidiEvent e) => {
                    //    if (e.EventType == MidiEvent.Program)
                    //        Console.WriteLine($"Program changed: Channel:{e.Channel} Instrument:{e.Msb}");
                    //};

                    player.PlayAsync();
                }
                //Console.WriteLine("Type [CR] to stop.");
                //Console.ReadLine();
                //player.Dispose();
            }
        }

        public void Stop()
        {
            try
            {
                if (player is MidiPlayer)
                {
                    player.PauseAsync();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                player.Dispose();
                player = null;
            }
        }

        public static MidiMusic Load(string filename)
        {
            MidiMusic result = null;

            if (File.Exists(filename))
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {

                    SmfReader smf = new SmfReader();
                    smf.Read(fs);
                    result = smf.Music;
                }
            }

            return (result);
        }

        public bool Save(string filename, MidiMusic music = null)
        {
            bool result = false;

            if (music == null) music = Music;

            if (music is MidiMusic && !string.IsNullOrEmpty(filename))
            {
                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    SmfWriter smf = new SmfWriter(fs);
                    smf.WriteMusic(music);
                    result = true;
                }
            }

            return (result);
        }
    }
}
