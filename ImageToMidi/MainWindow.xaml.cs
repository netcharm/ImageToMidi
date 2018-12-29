using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageToMidi
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Drawing.Bitmap bitmap = null;
        //Commons.Music.Midi.MidiMusic standard_midi;
        //Commons.Music.Midi.MidiMusic multinote_midi;

        PixelMidi p2m = new PixelMidi();

        private System.Drawing.Bitmap BitmapFromDib(Stream dib)
        {
            // We create a new Bitmap File in memory.
            // This is the easiest way to convert a DIB to Bitmap.
            // No PInvoke needed.
            BinaryReader reader = new BinaryReader(dib);

            int headerSize = reader.ReadInt32();
            int pixelSize  = (int) dib.Length - headerSize;
            int fileSize   = 14 + headerSize + pixelSize;

            /* Get the palette size
             * The Palette size is stored as an int32 at offset 32
             * Actually stored as number of colours, so multiply by 4
             */
            dib.Position = 32;
            int paletteSize = 4 * reader.ReadInt32();

            // Get the palette size from the bbp if none was specified
            if (paletteSize == 0)
            {
                /* Get the bits per pixel
                 * The bits per pixel is store as an int16 at offset 14
                 */
                dib.Position = 14;
                int bpp = reader.ReadInt16();

                // Only set the palette size if the bpp < 16
                if (bpp < 16)
                    paletteSize = 4 * (2 << (bpp - 1));
            }

            MemoryStream bmp = new MemoryStream(fileSize);
            BinaryWriter writer = new BinaryWriter(bmp);

            // 1. Write Bitmap File Header:			 
            writer.Write((byte)'B');
            writer.Write((byte)'M');
            writer.Write(fileSize);
            writer.Write((int)0);
            writer.Write(14 + headerSize + paletteSize);

            // 2. Copy the DIB 
            dib.Position = 0;
            byte[] data = new byte[(int) dib.Length];
            dib.Read(data, 0, (int)dib.Length);
            writer.Write(data, 0, (int)data.Length);

            // 3. Create a new Bitmap from our new stream:
            bmp.Position = 0;
            return new System.Drawing.Bitmap(bmp);
        }

        internal void LoadImage(Uri url)
        {
            try
            {
                if(url is Uri)
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.UriSource = url;
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();

                    if (img is BitmapSource)
                    {
                        bitmap = PixelMidi.ImageSourceToBitmap(img as BitmapSource);
                        bitmap = PixelMidi.GrayScale(bitmap);
                        if (bitmap.GetPixel(0, 0).GetBrightness() < 0.25) bitmap = PixelMidi.Invert(bitmap);
                        image.Source = PixelMidi.BitmapToImageSource(bitmap);
                        p2m.GetMidi(bitmap);
                    }
                }
            }
            catch (Exception) { }
        }

        internal void LoadImage(BitmapSource source)
        {
            if (source is BitmapSource)
            {
                bitmap = PixelMidi.ImageSourceToBitmap(source as BitmapSource);
                bitmap = PixelMidi.GrayScale(bitmap);
                if (bitmap.GetPixel(0, 0).GetBrightness() < 0.25) bitmap = PixelMidi.Invert(bitmap);
                image.Source = PixelMidi.BitmapToImageSource(bitmap);
                p2m.GetMidi(bitmap);
            }
        }

        internal void LoadImage(Stream stream, bool IsDib = true)
        {
            try
            {
                if(stream is Stream)
                {
                    if(IsDib)
                        bitmap = PixelMidi.GrayScale(BitmapFromDib(stream));
                    else
                        bitmap = PixelMidi.GrayScale(new System.Drawing.Bitmap(stream));
                    if (bitmap.GetPixel(0, 0).GetBrightness() < 0.25) bitmap = PixelMidi.Invert(bitmap);
                    image.Source = PixelMidi.BitmapToImageSource(bitmap);
                    p2m.GetMidi(bitmap);
                }
            }
            catch (Exception) { }
        }

        internal async void LoadImage(string text, bool IsBase64=false)
        {
            if (IsBase64)
            {
                using (MemoryStream imageStream = new MemoryStream())
                {
                    imageStream.Position = 0;
                    var base64 = Regex.Replace(text, @"data:image/.*?;base64,", "", RegexOptions.IgnoreCase);
                    byte[] arr = Convert.FromBase64String(base64);
                    await imageStream.WriteAsync(arr, 0, arr.Length);
                    await imageStream.FlushAsync();
                    imageStream.Seek(0, SeekOrigin.Begin);
                    var result = new BitmapImage();
                    result.BeginInit();
                    result.StreamSource = imageStream;
                    result.CacheOption = BitmapCacheOption.OnLoad;
                    result.EndInit();
                    LoadImage(result);
                }
            }
            else
            {
                p2m.FileName = System.IO.Path.GetFileNameWithoutExtension(text);

                bitmap = PixelMidi.GrayScale(PixelMidi.LoadBitmap(text));
                image.Source = PixelMidi.BitmapToImageSource(bitmap);

                p2m.GetMidi(bitmap);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            NoteType.Items.Add(new KeyValuePair<string, double>("Full", 4));
            NoteType.Items.Add(new KeyValuePair<string, double>("1/2", 2));
            NoteType.Items.Add(new KeyValuePair<string, double>("1/4", 1));
            NoteType.Items.Add(new KeyValuePair<string, double>("1/8", 0.5));
            NoteType.Items.Add(new KeyValuePair<string, double>("1/16", 0.25));
            NoteType.Items.Add(new KeyValuePair<string, double>("1/32", 0.128));
            NoteType.Items.Add(new KeyValuePair<string, double>("1/64", 0.064));
            NoteType.SelectedIndex = 4;

            NoteCenter.Items.Add("Eb5");
            NoteCenter.Items.Add("C4");
            NoteCenter.Items.Add("C3");
            NoteCenter.Items.Add("C2");
            NoteCenter.SelectedIndex = 2;

            Uri iconUri = new Uri("pack://application:,,,/ImageToMidi.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);

            image.Source = this.Icon;
            bitmap = PixelMidi.ImageSourceToBitmap(this.Icon as BitmapSource);
        }

        private void NoteType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            p2m.NoteType = ((KeyValuePair<string, double>)NoteType.SelectedValue).Value;
        }

        private void NoteCenter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (NoteCenter.SelectedItem.ToString().ToUpper())
            {
                case "EB5":
                    p2m.NoteCenter = 12 * 5 + 3;
                    break;
                case "C4":
                    p2m.NoteCenter = 12 * 4;
                    break;
                case "C3":
                    p2m.NoteCenter = 12 * 3;
                    break;
                case "C2":
                    p2m.NoteCenter = 12 * 2;
                    break;
                default:
                    p2m.NoteCenter = 12 * 3;
                    break;
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog();
            dlgOpen.Multiselect = false;
            dlgOpen.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|*.gif|*.bmp|" +
                            "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                            "Portable Network Graphic (*.png)|*.png|" +
                            "GIF File (*.gif)|*.gif|" +
                            "Windows Bitmap (*.bmp)|*.bmp";
            if (dlgOpen.ShowDialog() == true)
            {
                LoadImage(dlgOpen.FileName);
            }                
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlgSave = new SaveFileDialog();
            dlgSave.Filter = "All supported MIDI|*.mid;*.midi;*.smf";
            dlgSave.FileName = $"{p2m.FileName}.mid";
            if (dlgSave.ShowDialog() == true)
            {
                if(p2m.Music == null)
                    p2m.GetMidi(bitmap);
                p2m.Save($"{dlgSave.FileName}");
            }
        }

        private void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            LoadImage(Clipboard.GetImage());
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (p2m.IsPlay)
            {
                p2m.Stop();
                btnPlay.Content = "Play MIDI";
            }
            else
            {
                p2m.Play();
                btnPlay.Content = "Stop MIDI";
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            var fmts = new List<string>(e.Data.GetFormats(true));
            if (fmts.Contains("FileName") || fmts.Contains("text/html"))
            {
                e.Effects = DragDropEffects.Link;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var fmts = new List<string>(e.Data.GetFormats(true));
            if (fmts.Contains("PNG"))
            {
                using (MemoryStream ms = (MemoryStream)e.Data.GetData("PNG"))
                {
                    LoadImage(ms);
                }
            }
            else if (fmts.Contains(DataFormats.Dib))
            {
                using (MemoryStream ms = (MemoryStream)e.Data.GetData(DataFormats.Dib))
                {
                    LoadImage(ms);
                }
            }
            else if (fmts.Contains("text/html"))
            {
                using (var ms = (MemoryStream)e.Data.GetData("text/html"))
                {
                    List<string> links = new List<string>();

                    var html = Encoding.Unicode.GetString(ms.ToArray());

                    if (Regex.IsMatch(html, @"<img .*?src=""(data:image/.*?)"""))
                    {
                        foreach (Match m in Regex.Matches(html, @"<img .*?src=""(data:image/.*?)"""))
                        {
                            var link = m.Groups[1].Value;
                            if (!string.IsNullOrEmpty(link))
                            {
                                LoadImage(link, true);
                                break;
                            }
                        }
                    }
                    else if (Regex.IsMatch(html, @"<img .*?src=""(http.*?)"""))
                    {
                        foreach (Match m in Regex.Matches(html, @"<img .*?src=""(http.*?)"""))
                        {
                            var link = m.Groups[1].Value;
                            if (!string.IsNullOrEmpty(link))
                            {
                                LoadImage(new Uri(link));
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (Match m in Regex.Matches(html, @"<a .*?href =""(.*?)"""))
                        {
                            var link = m.Groups[1].Value;
                            if (!string.IsNullOrEmpty(link))
                            {
                                LoadImage(new Uri(link));
                                break;
                            }
                        }
                    }
                }
            }
            else if (fmts.Contains("FileName"))
            {
                var link = (string[])e.Data.GetData("FileName");
                if (link.Length>0 && !string.IsNullOrEmpty(link[0]))
                {
                    LoadImage(link[0]);
                }
            }
        }

    }
}
