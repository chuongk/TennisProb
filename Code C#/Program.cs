using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Net;

namespace TestGettingDataFromWebsite
{

    class Program
    {
        static string URL = "http://www.tennisabstract.com/charting/20131110-M-Tour_Finals-SF-Roger_Federer-Rafael_Nadal.html";
        //static string URL = "http://www.tennisabstract.com/charting/20131004-M-Beijing-QF-Fabio_Fognini-Rafael_Nadal.html";
        //    "SERVE BASICS","BREAKDOWN","OUTCOMES", "KEY POINTS","RALLY OUTCOMES","STATS OVERVIEW",
        //    "SHOT TYPES", "SHOT DIRECTION","NET POINTS","SERVE INFLUENCE"
        public static string getPage(string downloadedString, string nameX, string dataType)
        {
            System.IO.StreamWriter outfile = null;
            string aline = null;
            string starting = null;
            string result = "";
            if (!string.IsNullOrEmpty(nameX))
            {
                starting = "'<pre>" + nameX + " " + dataType;
                outfile = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\"  + nameX + dataType + ".txt");
            }
            else
            {
                starting = "'<pre>" + dataType;
                outfile = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\"  + dataType + ".txt");
            }
            string ending = "</pre>'";
            string nextline = "\\n";
            //Console.WriteLine("starting = " + starting);
             
            StringReader strReader = new StringReader(downloadedString);
            int start;
            int end;
            int nxtLinePos;
            string truString;
            while (true)
            {
                aline = strReader.ReadLine();
                if (aline != null)
                {
                    if (aline.Length == 0)
                    {
                        //Console.WriteLine("empty line");
                        continue;
                    }
                    start = aline.IndexOf(starting);
                    end = aline.IndexOf(ending);
                    if (start >= 0)
                    {
                        truString = aline.Substring(start + 6, end - start);
                        /*string[] words = truString.Split(' ');
                        for (int i = 0; i < words.Length; i++)
                        {
                            Console.WriteLine(words[i]);
                            Console.ReadLine();
                        }
                            Console.WriteLine(truString);
                        Console.ReadLine();*/
                        nxtLinePos = truString.IndexOf(nextline);
                        while (nxtLinePos >= 0)
                        {
                            //Console.WriteLine("next line pos = " + nxtLinePos);
                            //Console.WriteLine(truString.Substring(0, nxtLinePos));
                            result += truString.Substring(0, nxtLinePos) + "\n";
                            outfile.WriteLine(truString.Substring(0, nxtLinePos));
                            truString = truString.Substring(nxtLinePos + 2);
                            nxtLinePos = truString.IndexOf(nextline);
                            //Console.ReadLine();
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            Console.WriteLine(result);
            outfile.Close();
            return result;
        }

        public static string getPage2(string downloadedString)
        {
            System.IO.StreamWriter outfile = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\details.txt") ;
            string aline = null;
            string starting = "<table>";
            string ending = "</table>'";
            StringReader strReader = new StringReader(downloadedString);
            int start;
            int end;
            string truString;
            while (true)
            {
                aline = strReader.ReadLine();
                if (aline != null)
                {
                    if (aline.Length == 0)
                    {
                        //Console.WriteLine("empty line");
                        continue;
                    }
                    start = aline.IndexOf(starting);
                    end = aline.IndexOf(ending);
                    if (start >= 0)
                    {
                        truString = aline.Substring(start, end - start+8);
                        truString = WebUtility.HtmlDecode(truString);
                        break;
                    }
                }
            }
            outfile.Write(truString);
            outfile.Close();
            return truString;
        }
        public static void findXBreakDown(string downloadedString,string nameX)
        {
            System.IO.StreamWriter outfile = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\"+ nameX + ".txt");
            string aline = null;
            string starting = "'<pre>"+nameX+" BREAKDOWN";
            string ending = "</pre>'";
            string nextline = "\\n";
            //Console.WriteLine("starting = " + starting);
            StringReader strReader = new StringReader(downloadedString);
            int start;
            int end;
            int nxtLinePos;
            string truString;
            while (true)
            {
                aline = strReader.ReadLine();
                if (aline != null)
                {
                    if (aline.Length == 0)
                    {
                        //Console.WriteLine("empty line");
                        continue;
                    }
                    start = aline.IndexOf(starting);
                    end = aline.IndexOf(ending);
                    if (start >= 0)
                    {
                        truString = aline.Substring(start + 6, end - start);
                        /*string[] words = truString.Split(' ');
                        for (int i = 0; i < words.Length; i++)
                        {
                            Console.WriteLine(words[i]);
                            Console.ReadLine();
                        }
                            Console.WriteLine(truString);
                        Console.ReadLine();*/
                        nxtLinePos = truString.IndexOf(nextline);
                        while (nxtLinePos >= 0)
                        {
                            //Console.WriteLine("next line pos = " + nxtLinePos);
                            Console.WriteLine(truString.Substring(0, nxtLinePos));
                            outfile.WriteLine(truString.Substring(0, nxtLinePos));
                            truString = truString.Substring(nxtLinePos + 2);
                            nxtLinePos = truString.IndexOf(nextline);
                            //Console.ReadLine();
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            outfile.Close();
        }
        public static void findXDirection(string downloadedString, string nameX)
        {
            System.IO.StreamWriter outfile = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\" + nameX + "Direction.txt");
            string aline = null;
            string starting = "'<pre>" + nameX + " SHOT DIRECTION";
            string ending = "</pre>'";
            string nextline = "\\n";
            //Console.WriteLine("starting = " + starting);
            StringReader strReader = new StringReader(downloadedString);
            int start;
            int end;
            int nxtLinePos;
            string truString;
            while (true)
            {
                aline = strReader.ReadLine();
                if (aline != null)
                {
                    if (aline.Length == 0)
                    {
                        //Console.WriteLine("empty line");
                        continue;
                    }
                    start = aline.IndexOf(starting);
                    end = aline.IndexOf(ending);
                    if (start >= 0)
                    {
                        truString = aline.Substring(start + 6, end - start);
                        /*string[] words = truString.Split(' ');
                        for (int i = 0; i < words.Length; i++)
                        {
                            Console.WriteLine(words[i]);
                            Console.ReadLine();
                        }
                            Console.WriteLine(truString);
                        Console.ReadLine();*/
                        nxtLinePos = truString.IndexOf(nextline);
                        while (nxtLinePos >= 0)
                        {
                            //Console.WriteLine("next line pos = " + nxtLinePos);
                            Console.WriteLine(truString.Substring(0, nxtLinePos));
                            outfile.WriteLine(truString.Substring(0, nxtLinePos));
                            truString = truString.Substring(nxtLinePos + 2);
                            nxtLinePos = truString.IndexOf(nextline);
                            //Console.ReadLine();
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            outfile.Close();
        }
        // Not dealing w/ encoding
        public void webClient(string url)
        {
            string downloadedString;
            System.Net.WebClient client;

            client = new System.Net.WebClient();
            client.Encoding = Encoding.UTF8;
            client.DownloadFile(URL, @"D:\FYP\propabilititis tennis\local.html");
            downloadedString = client.DownloadString(URL);
            Console.WriteLine(downloadedString);
            // Compose a string that consists of three lines.
            // string lines = "First line.\r\nSecond line.\r\nThird line.";

            // Write the string to a file.
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\Test.txt");

            file.WriteLine(downloadedString);

            file.Close();
        }

        static void Main(string[] args)
        {
            HttpDownloader htDownload = new HttpDownloader(URL, null, null);
            string downloadedString = htDownload.GetPage();
            string ppDes = "";
            string gpage = "";
            DesAnalyzer DA = null;
            // Console.WriteLine(downloadedString);
            // Write the string to a file.
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\Test.txt");

            file.WriteLine(downloadedString);

            file.Close();
            gpage = getPage(downloadedString, "RF", "BREAKDOWN");
            ppDes = getPage2(downloadedString);
            DA = new DesAnalyzer(ppDes);
            DA.getData();
            ServeBreakdown serveBreak = new ServeBreakdown(gpage,"RF", "Deuce");
            serveBreak.getData();
        }

        


    }
}
