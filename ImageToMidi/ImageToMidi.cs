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

        public static Bitmap ToIndex256(Bitmap Bmp)
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

        public Bitmap Resize(Bitmap bitmap, int width, int height)
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

        public MidiMusic GetMidi(Bitmap bitmap, bool singleTrack = true)
        {
            MidiMusic result = null;

            if (bitmap is Bitmap)
            {
                result = new MidiMusic();

                short noteLen = 0x03C0;
                int delta = (int)(noteLen * NoteType);
                result.DeltaTimeSpec = noteLen;
                result.Format = 1;

                var track_sys = new MidiTrack();
                track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x03, 0, Encoding.Default.GetBytes(FileName))));
                track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x02, 0, Encoding.Default.GetBytes("Copyright 2018 NetCharm"))));
                track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x01, 0, Encoding.Default.GetBytes("NetCharm"))));
                track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x58, 0, Encoding.Default.GetBytes("\x04\x02\x18\x08"))));
                track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x59, 0, Encoding.Default.GetBytes("\x00\x00"))));
                track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x51, 0, Encoding.Default.GetBytes("\x09\x27\xC0"))));
                track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x2F, 0, Encoding.Default.GetBytes(""))));
                result.AddTrack(track_sys);

                var gray = GrayScale(bitmap.Height > 128 ? Resize(bitmap, 0, 128): bitmap);

                Color c;
                Color co = gray.GetPixel(0,0);
                int count = 0;

                int[,] om = new int[gray.Width, gray.Height];
                count = 0;
                bool[] blank = new bool[gray.Width];
                for (int x = 0; x < gray.Width; x++)
                {
                    for (int y = 0; y < gray.Height; y++)
                    {
                        c = gray.GetPixel(x, y);
                        var alpha = (int)(c.A / 4.0);
                        if (co.R == c.R && co.G == c.G && co.B == c.B || c.GetBrightness() > 0.9) alpha = 0;
                        if (alpha > 0)
                        {
                            om[x, y] = count * noteLen;
                            blank[x] = false;
                            count = 0;
                            break;
                        }
                        else count++;
                    }
                }

                if (singleTrack)
                {
                    var track = new MidiTrack();
                    track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 3, 0, Encoding.Default.GetBytes(FileName))));
                    int offset = (int)(NoteCenter - gray.Height / 2.0);
                    if (offset < 0) offset = 0;
                    float[] alpha = new float[128];
                    for (int x = 0; x < gray.Width; x++)
                    {
                        bool IsDelta = false;
                        for (int y = gray.Height - 1; y >= 0; y--)
                        {
                            c = gray.GetPixel(x, y);
                            alpha[y] = (int)(c.A / 4.0);
                            if (co.R == c.R && co.G == c.G && co.B == c.B || c.GetBrightness() > 0.9) alpha[y] = 0;
                            var velocity = (byte)(((1.0 - c.GetBrightness()) / 2.0)*255);
                            if (alpha[y] > 0)
                            {
                                var noteOn  = new MidiEvent(MidiEvent.NoteOn,  (byte)(gray.Height - y - 1 + offset), velocity, Encoding.Default.GetBytes(""));
                                if (!blank[x] && IsDelta)
                                    track.AddMessage(new MidiMessage(0, noteOn));
                                else if (!blank[x])
                                {
                                    track.AddMessage(new MidiMessage(om[x, y], noteOn));
                                    IsDelta = true;
                                }
                            }
                        }
                        IsDelta = false;
                        for (int y = 0; y < gray.Height; y++)
                        {
                            if (alpha[y] > 0)
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
                    track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x2F, 0, Encoding.Default.GetBytes(""))));
                    result.AddTrack(track);
                }
                else
                {
                    for (int y = 0; y < gray.Height; y++)
                    {
                        var track = new MidiTrack();
                        track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 3, 0, Encoding.Default.GetBytes($"{FileName}_{y}"))));
                        for (int x = 0; x < gray.Width; x++)
                        {
                            c = gray.GetPixel(x, y);
                            var trans = (int)((255.0 - c.A) / 4);
                            var velocity = (byte)(((1.0 - c.GetBrightness()) / 4.0)*255);
                            if (trans > 0)
                            {
                                var noteOn  = new MidiEvent(MidiEvent.NoteOn,  0x40, velocity, Encoding.Default.GetBytes(""));
                                var noteOff = new MidiEvent(MidiEvent.NoteOn, 0x40, 0x00, Encoding.Default.GetBytes(""));
                                track.AddMessage(new MidiMessage(om[x,y], noteOn));
                                track.AddMessage(new MidiMessage());
                                track.AddMessage(new MidiMessage(delta, noteOff));
                                track.AddMessage(new MidiMessage());
                            }
                        }
                        track.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x2F, 0, Encoding.Default.GetBytes(""))));
                        result.AddTrack(new MidiTrack());
                    }
                }
            }

            Music = result;
            return (result);
        }

        public MidiMusic DemoMidi()
        {
            MidiMusic result = new MidiMusic();

            short noteLen = 0x03C0;
            int offset = 4 * noteLen;
            result.DeltaTimeSpec = noteLen;
            result.Format = 1;

            var track_sys = new MidiTrack();
            track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x03, 0, Encoding.Default.GetBytes("Demo"))));
            track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x02, 0, Encoding.Default.GetBytes("Copyright 2018 NetCharm"))));
            track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x01, 0, Encoding.Default.GetBytes("NetCharm"))));
            track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x58, 0, Encoding.Default.GetBytes("\x04\x02\x18\x08"))));
            track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x59, 0, Encoding.Default.GetBytes("\x00\x00"))));
            track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x51, 0, Encoding.Default.GetBytes("\x09\x27\xC0"))));
            track_sys.AddMessage(new MidiMessage(0, new MidiEvent(MidiEvent.Meta, 0x2F, 0, Encoding.Default.GetBytes(""))));
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
