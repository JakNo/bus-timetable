using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace Rpi3
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            System.Text.EncodingProvider provider = System.Text.CodePagesEncodingProvider.Instance;
            System.Text.Encoding.RegisterProvider(provider);
            alert.Visibility = Visibility.Collapsed;
        }

        string linia = "";
        string godzina = "";
        string nazwapliku = "";
        string kierunek = "";
        string doKonca = "";
        string odjazd = "";
        //string link1 = "";
        //string link2 = "";
        //string link3 = "";
        //string link4 = "";
        string jakiDzien = "";
        XDocument rozklad = new XDocument();
        XElement main = new XElement("rozklady");
        XElement dzienPowszedni = new XElement("dzienPowszedni");
        XElement sobota = new XElement("sobota");
        XElement niedziela = new XElement("niedziela");
        XElement poprzedniOdjazd = null;
        Przystanek a;

        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        private void przetwarzanieRozkladu()
        {
            while (doKonca.Contains("border"))
            {
                odjazd = getBetween(doKonca, "height=\"18\">", "/td>");
                if (odjazd.StartsWith("<b>"))
                {
                    if (getBetween(odjazd, "<b>", "</b>").Contains("mies."))
                    {
                        if (getBetween(odjazd, "<b>", "</b>").Contains("Powszedni nauki szkolnej"))
                            nazwapliku = "tydz";
                        else if (getBetween(odjazd, "<b>", "</b>").Contains("Sobota"))
                            nazwapliku = "sob";
                        else if (getBetween(odjazd, "<b>", "</b>").Contains("Niedziela"))
                            nazwapliku = "niedz";

                        odjazd = "";
                        doKonca = doKonca.Remove(0, doKonca.IndexOf("<b>") + 3);
                    }
                }
                else
                {
                    godzina = getBetween(odjazd, "", "<");
                    if (godzina.Contains("0") || godzina.Contains("1") || godzina.Contains("2"))
                    {
                        godzina = godzina.Trim();
                        linia = getBetween(odjazd, "border=\"0\">", "<");
                        doKonca = doKonca.Remove(0, doKonca.IndexOf(linia) + linia.Length);
                        linia = linia.Trim();
                        kierunek = linia.Substring(linia.IndexOf(">") + 1);
                        kierunek = kierunek.Trim();
                        linia = linia.Substring(0, linia.IndexOf(" "));
                        XElement nowyOdjazd = new XElement("odjazd",
                                                new XElement("godzina", godzina),
                                                new XElement("linia", linia),
                                                new XElement("kierunek", kierunek));

                        if (nazwapliku == "tydz")
                            dzienPowszedni.Add(nowyOdjazd);
                        else if (nazwapliku == "sob")
                            sobota.Add(nowyOdjazd);
                        else if (nazwapliku == "niedz")
                            niedziela.Add(nowyOdjazd);
                    }
                    else
                        doKonca = doKonca.Remove(0, doKonca.IndexOf("height=\"18\">") + "height=\"18\">".Length);
                }
            }
        }

        private void pobieranieRozkladu(string URL)
        {
            doKonca = "";
            //System.Uri targetUri = new System.Uri(youruri);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
            request.BeginGetResponse(new AsyncCallback(ReadWebRequestCallback), request);
        }

        private void ReadWebRequestCallback(IAsyncResult callbackResult)
        {
            HttpWebRequest myRequest = default(HttpWebRequest);
            HttpWebResponse myResponse = default(HttpWebResponse);
            myRequest = (HttpWebRequest)callbackResult.AsyncState;
            try
            {
                myResponse = (HttpWebResponse)myRequest.EndGetResponse(callbackResult);
                using (StreamReader httpwebStreamReader = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.GetEncoding("ISO-8859-2")))
                {
                    doKonca = httpwebStreamReader.ReadToEnd();
                }
            }
            catch (Exception exc)
            {
                //if (exc.InnerException.Source.Equals("System.Net.Http"))
                    //alert.Text = "kszo";
            }
            //myResponse = (HttpWebResponse)myRequest.EndGetResponse(callbackResult);
        }

        void sortowanie(XElement dzien)
        {
            var orderedTabs = dzien.Elements("odjazd")                                         // 
                                  .OrderBy(xtab => xtab.Element("godzina").Value)              // Sortowanie
                                  .ToArray();                                                  //
            dzien.RemoveAll();
            foreach (XElement nastepnyOdjazd in orderedTabs)                                   //
            {                                                                                  //
                if (poprzedniOdjazd == null)                                                   //
                {                                                                              //
                    poprzedniOdjazd = nastepnyOdjazd;                                          // Dodawanie do rozkładu danego dnia
                    dzien.Add(nastepnyOdjazd);                                                 // wyłącznie niepowtarzących się
                    continue;                                                                  // odjazdów
                }                                                                              //
                if (poprzedniOdjazd.ToString() != nastepnyOdjazd.ToString())                   //
                {                                                                              //
                    dzien.Add(nastepnyOdjazd);                                                 //
                    poprzedniOdjazd = nastepnyOdjazd;                                          //
                }                                                                              //
            }                                                                                  //
        }

        private async void zapiszPlik(string zrodlo, string nazwa)                             // Funkcja zapisu rozkładu do pliku
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            //dane.Text = storageFolder.Path.ToString();
            StorageFile storageFile = await storageFolder.CreateFileAsync(nazwa, CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(storageFile, zrodlo);
        }

        DispatcherTimer SekundaTimer = null;
        int minutaStara = 0;
        int licznik = 0;
        private void StartTimer()
        {
            SekundaTimer = new DispatcherTimer();
            SekundaTimer.Interval = new TimeSpan(0, 0, 1);
            SekundaTimer.Tick += dispatcherTimer_Tick;
            SekundaTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, object e)
        {
            godzinaTextBox.Text = DateTime.Now.ToString();
            if (minutaStara < DateTime.Now.Minute || (minutaStara == 59 && DateTime.Now.Minute == 0))
            {
                godzinaString = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();
                if (godzinaString.Equals("1:20") && !a.czySprawdzonoRozklad)
                {
                    SekundaTimer.Stop();
                    dane.Text = "\n\n\n                       Proszę czekać...";
                    a.czySprawdzonoRozklad = true;
                    this.Frame.Navigate(typeof(MainPage), a);
                }
                if (godzinaString.Equals("1:21"))
                    a.czySprawdzonoRozklad = false;
                XDocument loadedData = new XDocument();
                bool tryAgain = true;
                while (tryAgain)
                {
                    try
                    {
                        loadedData = XDocument.Load("C:\\Users\\Kuba\\AppData\\Local\\Packages\\d94f757c-2826-460f-b21e-ebd21d5cc50c_jajxm59eghp86\\LocalState\\rozklad.xml");                                // na PC
                        //loadedData = XDocument.Load("C:\\Data\\Users\\DefaultAccount\\AppData\\Local\\Packages\\d94f757c-2826-460f-b21e-ebd21d5cc50c_jajxm59eghp86\\LocalState\\rozklad.xml");              // na Raspberry
                        tryAgain = false;
                    }
                    catch (Exception ex)
                    {
                        tryAgain = true;
                    }
                }
                minutaStara = DateTime.Now.Minute;
                dane.Text = "";
                if (DateTime.Now.Minute < 10)
                    godzinaString = DateTime.Now.Hour.ToString() + ",0" + DateTime.Now.Minute.ToString();
                else
                    godzinaString = DateTime.Now.Hour.ToString() + "," + DateTime.Now.Minute.ToString();

                jakiDzien = DateTime.Now.DayOfWeek.ToString();
                if (jakiDzien.Equals("Saturday"))
                    jakiDzien = "sobota";
                else if (jakiDzien.Equals("Sunday"))
                    jakiDzien = "niedziela";
                else
                    jakiDzien = "dzienPowszedni";
                foreach (XElement xe in loadedData.Descendants(jakiDzien))
                {
                    licznik = 0;
                    foreach (XElement wpis in xe.Descendants("odjazd").Where(n => decimal.Parse(n.Element("godzina").Value.Replace(":",",")) >= decimal.Parse(godzinaString)))
                    {
                        czasOdjazduString = wpis.Element("godzina").Value;
                        czasOdjazduString = czasOdjazduString.Replace(":", ",");
                        godzinaOdjazduString = czasOdjazduString.Substring(0, czasOdjazduString.IndexOf(","));
                        minutaOdjazduString = czasOdjazduString.Substring(czasOdjazduString.IndexOf(",") + 1, 2);
                        godzinaTerazString = godzinaString.Substring(0, godzinaString.IndexOf(","));
                        minutaTerazString = godzinaString.Substring(godzinaString.IndexOf(",") + 1, 2);
                        roznicaCzasu = (((int.Parse(godzinaOdjazduString)*60 + int.Parse(minutaOdjazduString))) - ((int.Parse(godzinaTerazString)*60 + int.Parse(minutaTerazString))));
                        if (roznicaCzasu == 0)
                            dane.Text += "już odjechał " + " " + wpis.Element("linia").Value + " " + wpis.Element("kierunek").Value + "\n";
                        if (roznicaCzasu < 10)
                        {
                            if (roznicaCzasu == 1)
                                dane.Text += "za " + roznicaCzasu + " minutę   " + wpis.Element("linia").Value + " " + wpis.Element("kierunek").Value + "\n";
                            if (roznicaCzasu > 1 && roznicaCzasu < 5)
                                dane.Text += "za " + roznicaCzasu + " minuty   " + wpis.Element("linia").Value + " " + wpis.Element("kierunek").Value + "\n";
                            if (roznicaCzasu > 4)
                                dane.Text += "za " + roznicaCzasu + " minut     " + wpis.Element("linia").Value + " " + wpis.Element("kierunek").Value + "\n";
                        }
                        else
                            dane.Text += wpis.Element("godzina").Value + "             " + wpis.Element("linia").Value + " " + wpis.Element("kierunek").Value + "\n";
                        
                        licznik++;
                        if (licznik > 6)
                            break;
                    }
                }
            }
        }

        string godzinaString = "";
        string czasOdjazduString = "";
        string godzinaOdjazduString = "";
        string minutaOdjazduString = "";
        string godzinaTerazString = "";
        string minutaTerazString = "";
        decimal czasOdjazdu = 0;
        decimal godzina1 = 0;
        int roznicaCzasu = 0;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(startPage));
        }

        protected override  void OnNavigatedTo(NavigationEventArgs e)
        {
            a = e.Parameter as Przystanek;
            if (a.czySprawdzonoRozklad == true)
                dane.Text = "\n\n\nTrwa aktualizacja rozkładu jazdy. Proszę czekać...";
            else
                dane.Text = "\n\n\n                       Proszę czekać...";
            //link1 = a.Link1;
            //link2 = a.Link2;
            //link3 = a.Link3;
            //link4 = a.Link4;
            if (!a.Link1.Equals(""))
            {
                XDocument rozklad = new XDocument();
                dzienPowszedni.RemoveAll();
                sobota.RemoveAll();
                niedziela.RemoveAll();
                main.RemoveAll();
                pobieranieRozkladu(a.Link1);
                while (!doKonca.Contains("</html>"))
                {

                }
                przetwarzanieRozkladu();
                
                if (!a.Link2.Equals(""))
                {
                    pobieranieRozkladu(a.Link2);
                    while (!doKonca.Contains("</html>"))
                    {

                    }
                    przetwarzanieRozkladu();
                }
                if (!a.Link3.Equals(""))
                {
                    pobieranieRozkladu(a.Link3);
                    while (!doKonca.Contains("</html>"))
                    {

                    }
                    przetwarzanieRozkladu();
                }
                if (!a.Link4.Equals(""))
                {
                    pobieranieRozkladu(a.Link4);
                    while (!doKonca.Contains("</html>"))
                    {

                    }
                    przetwarzanieRozkladu();
                }
                sortowanie(dzienPowszedni);
                sortowanie(sobota);
                sortowanie(niedziela);
                main.Add(dzienPowszedni);
                main.Add(sobota);
                main.Add(niedziela);
                rozklad.Add(main);
                zapiszPlik(rozklad.ToString(), "rozklad.xml");
                StartTimer();
            }

        }
    }
}