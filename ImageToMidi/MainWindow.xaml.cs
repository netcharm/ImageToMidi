using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        Commons.Music.Midi.MidiMusic standard_midi;
        Commons.Music.Midi.MidiMusic multinote_midi;

        PixelMidi p2m = new PixelMidi();

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

        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog();
            dlgOpen.Multiselect = false;
            dlgOpen.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                            "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                            "Portable Network Graphic (*.png)|*.png";
            if (dlgOpen.ShowDialog() == true)
            {
                p2m.FileName = System.IO.Path.GetFileName(dlgOpen.FileName);

                bitmap = PixelMidi.ToGrayScale(PixelMidi.LoadBitmap(dlgOpen.FileName));
                image.Source = PixelMidi.BitmapToImageSource(bitmap);
            }
                

            //bitmap = PixelMidi.ToGrayScale(PixelMidi.LoadBitmap("d:\\downloads\\_firefox\\220px-Sine_one_period.svg.png"));
            //image.Source = PixelMidi.BitmapToImageSource(bitmap);

            //standard_midi = PixelMidi.Load("d:\\downloads\\_firefox\\standard_v1.mid");
            //multinote_midi = PixelMidi.Load("d:\\downloads\\_firefox\\multinote.mid");
            //var test = PixelMidi.Load("d:\\downloads\\_firefox\\test.mid");

            //p2m.FileName = "220px-Sine_one_period";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlgSave = new SaveFileDialog();
            dlgSave.Filter = "All supported MIDI|*.mid;*.midi;*.smf";
            dlgSave.FileName = $"{p2m.FileName}.mid";
            if (dlgSave.ShowDialog() == true)
            {
                var midi = p2m.GetMidi(bitmap);
                p2m.Save(midi, $"{dlgSave.FileName}");
            }


            //var demo = p2m.DemoMidi();
            //p2m.Save(demo, "d:\\downloads\\_firefox\\demo.mid");

        }

        private void NoteType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            p2m.NoteType = ((KeyValuePair<string, double>)NoteType.SelectedValue).Value;
        }

        private void NoteCenter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(NoteCenter.SelectedItem.ToString().ToUpper())
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
    }
}
