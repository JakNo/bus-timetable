using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Rpi3
{
    public class Przystanek
    {
        public string Name;
        public string Link1;
        public string Link2;
        public string Link3;
        public string Link4;
        public bool czySprawdzonoRozklad;
        public Przystanek(string name, string link1, string link2 = "", string link3 = "", string link4 = "", bool czySprawdzonoRozklad = false)
        {
            Name = name; Link1 = link1; Link2 = link2; Link3 = link3; Link4 = link4;
        }
        public override string ToString()
        {
            return Name;
        }
    }
    /// <summary>
    /// Pusta strona, która może być używana samodzielnie lub do której można nawigować wewnątrz ramki.
    /// </summary>
    public sealed partial class startPage : Page
    {
        public startPage()
        {
            this.InitializeComponent();
            textBlock1.Visibility = Visibility.Collapsed;
            comboBox2.Visibility = Visibility.Collapsed;
            textBox1.Visibility = Visibility.Collapsed;
            textBox2.Visibility = Visibility.Collapsed;
            textBox3.Visibility = Visibility.Collapsed;
            textBox4.Visibility = Visibility.Collapsed;
            zapisz.Visibility = Visibility.Collapsed;
            nazwaPrzystanku.Visibility = Visibility.Collapsed;
            wstecz.Visibility = Visibility.Collapsed;
            comboBox1.Items.Add(new Przystanek("Oksywie Górne", "http://www2.zkmgdynia.pl/rozklady/szukaj/businfo.cgi?x=60614&y=59274", "http://www2.zkmgdynia.pl/rozklady/szukaj/businfo.cgi?x=60603&y=59265", "http://www2.zkmgdynia.pl/rozklady/szukaj/businfo.cgi?x=60634&y=59252"));
            comboBox1.Items.Add(new Przystanek("Węzeł Ofiar Grudnia '70", "http://www.zkmgdynia.pl/rozklady/szukaj/businfo.cgi?x=59344&y=60811", "http://www.zkmgdynia.pl/rozklady/szukaj/businfo.cgi?x=59337&y=60784"));
            comboBox1.Items.Add(new Przystanek("Bosmańska-Zielona", "http://www.zkmgdynia.pl/rozklady/szukaj/businfo.cgi?x=59943&y=59276", "http://www.zkmgdynia.pl/rozklady/szukaj/businfo.cgi?x=59874&y=59287", "http://www.zkmgdynia.pl/rozklady/szukaj/businfo.cgi?x=59900&y=59255"));
            comboBox2.Items.Add("1");
            comboBox2.Items.Add("2");
            comboBox2.Items.Add("3");
            comboBox2.Items.Add("4");
        }

        private void dodajWlasnyPrzystanek_Click(object sender, RoutedEventArgs e)
        {
            comboBox1.Visibility = Visibility.Collapsed;
            info.Visibility = Visibility.Collapsed;
            textBlock1.Visibility = Visibility.Visible;
            comboBox2.Visibility = Visibility.Visible;
            nazwaPrzystanku.Visibility = Visibility.Visible;
            wstecz.Visibility = Visibility.Visible;
            dodajWlasnyPrzystanek.Visibility = Visibility.Collapsed;
            wybierzButton.Visibility = Visibility.Collapsed;
        }
        
        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox2.SelectedIndex.Equals(0))
            {
                zapisz.Visibility = Visibility.Visible;
                textBox1.Visibility = Visibility.Visible;
                textBox2.Visibility = Visibility.Collapsed;
                textBox3.Visibility = Visibility.Collapsed;
                textBox4.Visibility = Visibility.Collapsed;
            }
            if (comboBox2.SelectedIndex.Equals(1))
            {
                zapisz.Visibility = Visibility.Visible;
                textBox1.Visibility = Visibility.Visible;
                textBox2.Visibility = Visibility.Visible;
                textBox3.Visibility = Visibility.Collapsed;
                textBox4.Visibility = Visibility.Collapsed;
            }
            if (comboBox2.SelectedIndex.Equals(2))
            {
                zapisz.Visibility = Visibility.Visible;
                textBox1.Visibility = Visibility.Visible;
                textBox2.Visibility = Visibility.Visible;
                textBox3.Visibility = Visibility.Visible;
                textBox4.Visibility = Visibility.Collapsed;
            }
            if (comboBox2.SelectedIndex.Equals(3))
            {
                zapisz.Visibility = Visibility.Visible;
                textBox1.Visibility = Visibility.Visible;
                textBox2.Visibility = Visibility.Visible;
                textBox3.Visibility = Visibility.Visible;
                textBox4.Visibility = Visibility.Visible;
            }
        }

        private void wybierzButton_Click(object sender, RoutedEventArgs e)
        {
            Przystanek a = (Przystanek)comboBox1.SelectedItem;
            this.Frame.Navigate(typeof(MainPage), a);
        }

        private void zapisz_Click(object sender, RoutedEventArgs e)
        {
            if (!textBox1.Text.Contains("zkmgdynia") || !textBox1.Text.Contains("cgi"))
            {
                textBox1.Text = "Podaj właściwy link.";
            }
            else
            {
                if (!textBox2.Text.Contains("zkmgdynia") || !textBox2.Text.Contains("cgi"))
                    textBox2.Text = "";
                if (!textBox3.Text.Contains("zkmgdynia") || !textBox3.Text.Contains("cgi"))
                    textBox3.Text = "";
                if (!textBox4.Text.Contains("zkmgdynia") || !textBox4.Text.Contains("cgi"))
                    textBox4.Text = "";
                Przystanek current = new Przystanek(nazwaPrzystanku.Text, textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text);
                comboBox1.Items.Add(current);
                comboBox1.Visibility = Visibility.Visible;
                wybierzButton.Visibility = Visibility.Visible;
                dodajWlasnyPrzystanek.Visibility = Visibility.Visible;
                info.Visibility = Visibility.Visible;
                nazwaPrzystanku.Visibility = Visibility.Collapsed;
                zapisz.Visibility = Visibility.Collapsed;
                comboBox2.Visibility = Visibility.Collapsed;
                textBlock1.Visibility = Visibility.Collapsed;
                textBox1.Visibility = Visibility.Collapsed;
                textBox2.Visibility = Visibility.Collapsed;
                textBox3.Visibility = Visibility.Collapsed;
                textBox4.Visibility = Visibility.Collapsed;
            }
        }

        private void wstecz_Click(object sender, RoutedEventArgs e)
        {
            comboBox1.Visibility = Visibility.Visible;
            wybierzButton.Visibility = Visibility.Visible;
            dodajWlasnyPrzystanek.Visibility = Visibility.Visible;
            info.Visibility = Visibility.Visible;
            nazwaPrzystanku.Visibility = Visibility.Collapsed;
            zapisz.Visibility = Visibility.Collapsed;
            comboBox2.Visibility = Visibility.Collapsed;
            textBlock1.Visibility = Visibility.Collapsed;
            textBox1.Visibility = Visibility.Collapsed;
            textBox2.Visibility = Visibility.Collapsed;
            textBox3.Visibility = Visibility.Collapsed;
            textBox4.Visibility = Visibility.Collapsed;
            wstecz.Visibility = Visibility.Collapsed;
        }
    }
}
