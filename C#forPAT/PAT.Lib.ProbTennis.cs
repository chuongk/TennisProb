using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Web;
using PAT.Common.Classes.Expressions.ExpressionClass;

namespace PAT.Lib
{
    class Program
    {
        //static string URL = "http://www.tennisabstract.com/charting/20131110-M-Tour_Finals-SF-Roger_Federer-Rafael_Nadal.html";
        public static string getPage(string downloadedString, string nameX, string dataType)
        {
            System.IO.StreamWriter outfile = null;
            string aline = null;
            string starting = null;
            string result = "";
            if (!string.IsNullOrEmpty(nameX))
            {
                starting = "'<pre>" + nameX + " " + dataType;
                outfile = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\" + nameX + dataType + ".txt");
            }
            else
            {
                starting = "'<pre>" + dataType;
                outfile = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\" + dataType + ".txt");
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
        static void Main(string[] args)
        {
            Console.WriteLine(DesAnalyzerPat.getFed_BH_R_Percent(0,0));
        }
    }

    class HttpDownloader
    {
        private readonly string _referer;
        private readonly string _userAgent;

        public Encoding Encoding { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public Uri Url { get; set; }

        public HttpDownloader(string url, string referer, string userAgent)
        {
            Encoding = Encoding.GetEncoding("ISO-8859-1");
            Url = new Uri(url); // verify the uri
            _userAgent = userAgent;
            _referer = referer;
        }

        public string GetPage()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            if (!string.IsNullOrEmpty(_referer))
                request.Referer = _referer;
            if (!string.IsNullOrEmpty(_userAgent))
                request.UserAgent = _userAgent;

            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Headers = response.Headers;
                Url = response.ResponseUri;
                return ProcessContent(response);
            }

        }

        private string ProcessContent(HttpWebResponse response)
        {
            SetEncodingFromHeader(response);

            Stream s = response.GetResponseStream();
            if (response.ContentEncoding.ToLower().Contains("gzip"))
                s = new GZipStream(s, CompressionMode.Decompress);
            else if (response.ContentEncoding.ToLower().Contains("deflate"))
                s = new DeflateStream(s, CompressionMode.Decompress);

            MemoryStream memStream = new MemoryStream();
            int bytesRead;
            byte[] buffer = new byte[0x1000];
            for (bytesRead = s.Read(buffer, 0, buffer.Length); bytesRead > 0; bytesRead = s.Read(buffer, 0, buffer.Length))
            {
                memStream.Write(buffer, 0, bytesRead);
            }
            s.Close();
            string html;
            memStream.Position = 0;
            using (StreamReader r = new StreamReader(memStream, Encoding))
            {
                html = r.ReadToEnd().Trim();
                html = CheckMetaCharSetAndReEncode(memStream, html);
            }

            return html;
        }

        private void SetEncodingFromHeader(HttpWebResponse response)
        {
            string charset = null;
            if (string.IsNullOrEmpty(response.CharacterSet))
            {
                Match m = Regex.Match(response.ContentType, @";\s*charset\s*=\s*(?<charset>.*)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    charset = m.Groups["charset"].Value.Trim(new[] { '\'', '"' });
                }
            }
            else
            {
                charset = response.CharacterSet;
            }
            if (!string.IsNullOrEmpty(charset))
            {
                try
                {
                    Encoding = Encoding.GetEncoding(charset);
                }
                catch (ArgumentException)
                {
                }
            }
        }

        private string CheckMetaCharSetAndReEncode(Stream memStream, string html)
        {
            Match m = new Regex(@"<meta\s+.*?charset\s*=\s*(?<charset>[A-Za-z0-9_-]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase).Match(html);
            if (m.Success)
            {
                string charset = m.Groups["charset"].Value.ToLower() ?? "iso-8859-1";
                if ((charset == "unicode") || (charset == "utf-16"))
                {
                    charset = "utf-8";
                }

                try
                {
                    Encoding metaEncoding = Encoding.GetEncoding(charset);
                    if (Encoding != metaEncoding)
                    {
                        memStream.Position = 0L;
                        StreamReader recodeReader = new StreamReader(memStream, metaEncoding);
                        html = recodeReader.ReadToEnd().Trim();
                        recodeReader.Close();
                    }
                }
                catch (ArgumentException)
                {
                }
            }

            return html;
        }
    }

    // A class to store and compute information for a particular type of serve
    public class Analyzer
    {
        public enum courtType { Deuce, Ad };
        public enum serveType { Wide, Body, T };
        //private courtType court;
        private serveType serve;

        private int totalPts;   // the total points for every serveType
        private int totalPts_curServe;  // the total points of the current serve

        private int Serve_Total_1st;    // the total point for every serveType 1st Serve

        private int Serve_Total_2nd;    // the total point for every serveType 2nd Serve

        private int Serve_In_1st;   // total point of the current 1st serve in
        //private int Serve_In_Suc_1st;   // total success point of the current 1st serve in

        public int Serve_In_Suc_1st_Percent; // Percentage of the 1st success serve in
        private int Serve_In_2nd;   // total point of the 2nd serve in
        public int Serve_In_Suc_2nd_Percent;   // Percentage of the 2nd success serve in
        private int Serve_In_Suc_2nd;   // total success point of the current 2nd serve in
        private int Serve_Err_1st;  // total error points of the 1st serve in = Serve_In_2nd
        public int Serve_Err_1st_Percent; // Percentage of the serve error first
        private int Serve_Err_2nd;  // total error points of the 2nd serve in = DF points for the current type
        public int Serve_Err_2nd_Percent;  // Perccentage of the serve error 2nd

        public Analyzer(string CourtType, string ServeType)
        {
            /*if (CourtType == "Deuce")
            {
                court = courtType.Deuce;
            }
            else
            {
                court = courtType.Ad;
            }*/
            switch (ServeType)
            {
                case "Wide":
                    serve = serveType.Wide;
                    break;
                case "Body":
                    serve = serveType.Body;
                    break;
                case "T":
                    serve = serveType.T;
                    break;
                default:
                    serve = serveType.T;
                    break;
            }
        }

        public void setTotalPoints(int totalPoints) { totalPts = totalPoints; }
        public int getTotalPoints() { return totalPts; }

        public void setCurTotalPoints(int curTotalPoints) { totalPts_curServe = curTotalPoints; }
        public int getCurTotalPoints() { return totalPts_curServe; }

        public void setTotal1st(int total1st)
        {
            Serve_Total_1st = total1st;
            // We can compute the total points of the 2nd
            Serve_Total_2nd = totalPts - Serve_Total_1st;
        }
        public int getTotal1st() { return Serve_Total_1st; }

        public void setTotal2nd(int total2nd)
        {
            Serve_Total_2nd = total2nd;
            // We can compute the total points of the 1st
            Serve_Total_1st = totalPts - Serve_Total_2nd;
        }
        public int getTotal2nd() { return Serve_Total_2nd; }

        public void setServe_In_1st(int serveIn_1st)
        {
            Serve_In_1st = serveIn_1st;
            // Can also compute the second serve in and the error of the first serve in
            Serve_In_2nd = totalPts_curServe - Serve_In_1st;
            Serve_Err_1st = Serve_In_2nd;
            // We can compute the percentage success of that current serve in
            if (Serve_Total_1st > 0)
                Serve_In_Suc_1st_Percent = (int)((Serve_In_1st * 1.0 / totalPts) * 100 + 0.5);
            else
                Serve_In_Suc_1st_Percent = 0;

            if (Serve_Total_1st > 0)
                Serve_Err_1st_Percent = (int)(((Serve_Err_1st * 1.0 / totalPts) * 100) + 0.5);
            else
                Serve_Err_1st_Percent = 0;

        }
        public int getServe_In_1st() { return Serve_In_1st; }

        // Maybe don't need this
        public void setServe_In_2nd(int serveIn_2nd)
        {
        }
        public int getServe_In_2nd() { return Serve_In_2nd; }

        public void setServe_Err_2nd(int serveErr_2nd)
        {
            // Should be the number of double faults for that type of serve
            Serve_Err_2nd = serveErr_2nd;
            Serve_In_Suc_2nd = Serve_In_2nd - Serve_Err_2nd;
            if (Serve_Total_2nd > 0)
                Serve_In_Suc_2nd_Percent = (int)((Serve_In_Suc_2nd * 1.0 / Serve_Total_2nd) * 100 + 0.5);
            else
                Serve_In_Suc_2nd_Percent = 0;

            if (Serve_Total_2nd > 0)
                Serve_Err_2nd_Percent = (int)(((Serve_Err_2nd * 1.0 / Serve_Total_2nd) * 100) + 0.5);
            else
                Serve_Err_2nd_Percent = 0;
        }
        public int getServe_Err_2nd() { return Serve_Err_2nd; }

        public string toString1st()
        {
            string result = "";
            switch (serve)
            {
                case serveType.T:
                    result += "ServeT_in: " + Serve_In_Suc_1st_Percent + "%\n";
                    result += "ServeT_err: " + Serve_Err_1st_Percent + "%\n";
                    break;
                case serveType.Wide:
                    result += "ServeWide_in: " + Serve_In_Suc_1st_Percent + "%\n";
                    result += "ServeWide_err: " + Serve_Err_1st_Percent + "%\n";
                    break;
                case serveType.Body:
                    result += "ServeBody_in: " + Serve_In_Suc_1st_Percent + "%\n";
                    result += "ServeBody_err: " + Serve_Err_1st_Percent + "%\n";
                    break;
                default:
                    break;

            }
            return result;
        }

        public string toString2nd()
        {
            string result = "";
            switch (serve)
            {
                case serveType.T:
                    result += "ServeT_in: " + Serve_In_Suc_2nd_Percent + "%\n";
                    result += "ServeT_err: " + Serve_Err_2nd_Percent + "%\n";
                    break;
                case serveType.Wide:
                    result += "ServeWide_in: " + Serve_In_Suc_2nd_Percent + "%\n";
                    result += "ServeWide_err: " + Serve_Err_2nd_Percent + "%\n";
                    break;
                case serveType.Body:
                    result += "ServeBody_in: " + Serve_In_Suc_2nd_Percent + "%\n";
                    result += "ServeBody_err: " + Serve_Err_2nd_Percent + "%\n";
                    break;
                default:
                    break;

            }
            return result;
        }
    }

    public class ServeBreakdownPat : ExpressionValue
    {
        private static int FedData = 0;
        private static int NadData = 0;
        public static string URL = "http://www.tennisabstract.com/charting/20131110-M-Tour_Finals-SF-Roger_Federer-Rafael_Nadal.html";
        static string sourceString;
        private static Analyzer T_de;
        private static Analyzer T_ad;
        private static Analyzer Wide_de;
        private static Analyzer Wide_ad;
        private static Analyzer Body_de;
        private static Analyzer Body_ad;

        private static Analyzer T_de_Nad;
        private static Analyzer T_ad_Nad;
        private static Analyzer Wide_de_Nad;
        private static Analyzer Wide_ad_Nad;
        private static Analyzer Body_de_Nad;
        private static Analyzer Body_ad_Nad;

        public static string getPage(string downloadedString, string nameX, string dataType)
        {
            string aline = null;
            string starting = null;
            string result = "";
            if (!string.IsNullOrEmpty(nameX))
            {
                starting = "'<pre>" + nameX + " " + dataType;
            }
            else
            {
                starting = "'<pre>" + dataType;
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

            return result;
        }
        private static void getDataFed(){
            string line = "";
            int deuce_total_pts = 0;
            int ad_total_pts = 0;
            int DF_Deuce_Wide = 0;
            int DF_Deuce_Body = 0;
            int DF_Deuce_T = 0;
            int DF_Ad_Wide = 0;
            int DF_Ad_Body = 0;
            int DF_Ad_T = 0;
            T_de = new Analyzer("Deuce", "T");
            T_ad = new Analyzer("Ad", "T");
            Wide_de = new Analyzer("Deuce", "Wide");
            Wide_ad = new Analyzer("Ad", "Wide");
            Body_de = new Analyzer("Deuce", "Body");
            Body_ad = new Analyzer("Ad", "Body");
            // Assume we already have the file, open it to get data
            // Read the file and display it line by line.
            StringReader file = new StringReader(sourceString);
            // Hardcode seems easier
            // ignore first line
            line = file.ReadLine();
            line = file.ReadLine();
            // find the total points for the deuce serve   
            deuce_total_pts = findFirstNum(line);
            T_de.setTotalPoints(deuce_total_pts);
            Wide_de.setTotalPoints(deuce_total_pts);
            Body_de.setTotalPoints(deuce_total_pts);
            // find the total points for the ad serve
            line = file.ReadLine();
            ad_total_pts = findFirstNum(line);
            T_ad.setTotalPoints(ad_total_pts);
            Wide_ad.setTotalPoints(ad_total_pts);
            Body_ad.setTotalPoints(ad_total_pts);

            // Don't need next 3 lines
            for (int j = 0; j < 4; j++)
                line = file.ReadLine();

            // Find the number of double faults of each serve and court, also the total ponts for each serve type
            DF_Deuce_Wide = findDF(line);
            Wide_de.setCurTotalPoints(findFirstNum(line));
            line = file.ReadLine();
            DF_Ad_Wide = findDF(line);
            Wide_ad.setCurTotalPoints(findFirstNum(line));
            line = file.ReadLine();
            DF_Deuce_Body = findDF(line);
            Body_de.setCurTotalPoints(findFirstNum(line));
            line = file.ReadLine();
            DF_Ad_Body = findDF(line);
            Body_ad.setCurTotalPoints(findFirstNum(line));
            line = file.ReadLine();
            DF_Deuce_T = findDF(line);
            T_de.setCurTotalPoints(findFirstNum(line));
            line = file.ReadLine();
            DF_Ad_T = findDF(line);
            T_ad.setCurTotalPoints(findFirstNum(line));
            // Dont need next 2 lines
            for (int j = 0; j < 3; j++)
                line = file.ReadLine();
            // read the total deuce court 1st (and 2nd)
            Wide_de.setTotal1st(findFirstNum(line));
            T_de.setTotal1st(findFirstNum(line));
            Body_de.setTotal1st(findFirstNum(line));
            // read the total ad court 1st (and 2nd) 
            line = file.ReadLine();
            Wide_ad.setTotal1st(findFirstNum(line));
            T_ad.setTotal1st(findFirstNum(line));
            Body_ad.setTotal1st(findFirstNum(line));
            // Don't need next 3 lines
            for (int j = 0; j < 4; j++)
            {
                line = file.ReadLine();
            }

            // Read the deuce wide serve 1st (and 2nd)
            Wide_de.setServe_In_1st(findFirstNum(line));
            // Read the ad wide serve 1st (and 2nd)
            line = file.ReadLine();
            Wide_ad.setServe_In_1st(findFirstNum(line));
            // Read the deuce body serve 1st (and 2nd)
            line = file.ReadLine();
            Body_de.setServe_In_1st(findFirstNum(line));
            // Read the ad body serve 1st (and 2nd)
            line = file.ReadLine();
            Body_ad.setServe_In_1st(findFirstNum(line));

            // Read the deuce T serve 1st (and 2nd)
            line = file.ReadLine();
            T_de.setServe_In_1st(findFirstNum(line));
            // Read the ad T serve 1st (and 2nd)
            line = file.ReadLine();
            T_ad.setServe_In_1st(findFirstNum(line));

            // Put in the doule fault
            T_de.setServe_Err_2nd(DF_Deuce_T);
            Wide_de.setServe_Err_2nd(DF_Deuce_Wide);
            Body_de.setServe_Err_2nd(DF_Deuce_Body);
            T_ad.setServe_Err_2nd(DF_Ad_T);
            Wide_ad.setServe_Err_2nd(DF_Ad_Wide);
            Body_ad.setServe_Err_2nd(DF_Ad_Body);
        }

        private static void getDataNad()
        {
            string line = "";
            int deuce_total_pts = 0;
            int ad_total_pts = 0;
            int DF_Deuce_Wide = 0;
            int DF_Deuce_Body = 0;
            int DF_Deuce_T = 0;
            int DF_Ad_Wide = 0;
            int DF_Ad_Body = 0;
            int DF_Ad_T = 0;
            T_de_Nad = new Analyzer("Deuce", "T");
            T_ad_Nad = new Analyzer("Ad", "T");
            Wide_de_Nad = new Analyzer("Deuce", "Wide");
            Wide_ad_Nad = new Analyzer("Ad", "Wide");
            Body_de_Nad = new Analyzer("Deuce", "Body");
            Body_ad_Nad = new Analyzer("Ad", "Body");
            // Assume we already have the file, open it to get data
            // Read the file and display it line by line.
            StringReader file = new StringReader(sourceString);
            // Hardcode seems easier
            // ignore first line
            line = file.ReadLine();
            line = file.ReadLine();
            // find the total points for the deuce serve   
            deuce_total_pts = findFirstNum(line);
            T_de_Nad.setTotalPoints(deuce_total_pts);
            Wide_de_Nad.setTotalPoints(deuce_total_pts);
            Body_de_Nad.setTotalPoints(deuce_total_pts);
            // find the total points for the ad serve
            line = file.ReadLine();
            ad_total_pts = findFirstNum(line);
            T_ad_Nad.setTotalPoints(ad_total_pts);
            Wide_ad_Nad.setTotalPoints(ad_total_pts);
            Body_ad_Nad.setTotalPoints(ad_total_pts);

            // Don't need next 3 lines
            for (int j = 0; j < 4; j++)
                line = file.ReadLine();

            // Find the number of double faults of each serve and court, also the total ponts for each serve type
            DF_Deuce_Wide = findDF(line);
            Wide_de_Nad.setCurTotalPoints(findFirstNum(line));
            line = file.ReadLine();
            DF_Ad_Wide = findDF(line);
            Wide_ad_Nad.setCurTotalPoints(findFirstNum(line));
            line = file.ReadLine();
            DF_Deuce_Body = findDF(line);
            Body_de_Nad.setCurTotalPoints(findFirstNum(line));
            line = file.ReadLine();
            DF_Ad_Body = findDF(line);
            Body_ad_Nad.setCurTotalPoints(findFirstNum(line));
            line = file.ReadLine();
            DF_Deuce_T = findDF(line);
            T_de_Nad.setCurTotalPoints(findFirstNum(line));
            line = file.ReadLine();
            DF_Ad_T = findDF(line);
            T_ad_Nad.setCurTotalPoints(findFirstNum(line));
            // Dont need next 2 lines
            for (int j = 0; j < 3; j++)
                line = file.ReadLine();
            // read the total deuce court 1st (and 2nd)
            Wide_de_Nad.setTotal1st(findFirstNum(line));
            T_de_Nad.setTotal1st(findFirstNum(line));
            Body_de_Nad.setTotal1st(findFirstNum(line));
            // read the total ad court 1st (and 2nd) 
            line = file.ReadLine();
            Wide_ad_Nad.setTotal1st(findFirstNum(line));
            T_ad_Nad.setTotal1st(findFirstNum(line));
            Body_ad_Nad.setTotal1st(findFirstNum(line));
            // Don't need next 3 lines
            for (int j = 0; j < 4; j++)
            {
                line = file.ReadLine();
            }

            // Read the deuce wide serve 1st (and 2nd)
            Wide_de_Nad.setServe_In_1st(findFirstNum(line));
            // Read the ad wide serve 1st (and 2nd)
            line = file.ReadLine();
            Wide_ad_Nad.setServe_In_1st(findFirstNum(line));
            // Read the deuce body serve 1st (and 2nd)
            line = file.ReadLine();
            Body_de_Nad.setServe_In_1st(findFirstNum(line));
            // Read the ad body serve 1st (and 2nd)
            line = file.ReadLine();
            Body_ad_Nad.setServe_In_1st(findFirstNum(line));

            // Read the deuce T serve 1st (and 2nd)
            line = file.ReadLine();
            T_de_Nad.setServe_In_1st(findFirstNum(line));
            // Read the ad T serve 1st (and 2nd)
            line = file.ReadLine();
            T_ad_Nad.setServe_In_1st(findFirstNum(line));

            // Put in the doule fault
            T_de_Nad.setServe_Err_2nd(DF_Deuce_T);
            Wide_de_Nad.setServe_Err_2nd(DF_Deuce_Wide);
            Body_de_Nad.setServe_Err_2nd(DF_Deuce_Body);
            T_ad_Nad.setServe_Err_2nd(DF_Ad_T);
            Wide_ad_Nad.setServe_Err_2nd(DF_Ad_Wide);
            Body_ad_Nad.setServe_Err_2nd(DF_Ad_Body);
        }

        public enum ServeType { T = 0, Body = 1, Wide = 2 };
        public enum CourtType { deuce = 0, ad =1}
        public static int getFed_Serve(int serveType , int courtType , int err , int serveTurn )
        {
            int result = 0;
            if (FedData == 0)
            {
                HttpDownloader htDownload = new HttpDownloader(URL, null, null);
                string downloadedString = htDownload.GetPage();
                sourceString = getPage(downloadedString, "RF", "BREAKDOWN");
                getDataFed();
                FedData = 1;
            }
            switch (serveType)
            {
                case (int)ServeType.T:
                    if (courtType ==  (int)CourtType.ad)
                    {
                        if (err == 1)
                            result = T_ad.Serve_Err_1st_Percent;
                        else
                            result = T_ad.Serve_In_Suc_1st_Percent;
                    }
                    else
                    {
                        if (err == 1)
                            result = T_de.Serve_Err_1st_Percent;
                        else
                            result = T_de.Serve_In_Suc_1st_Percent;
                    }
                    break;
                case (int)ServeType.Body:                     
                    if (courtType == (int) CourtType.ad)
                    {
                        if (err == 1)
                            result = Body_ad.Serve_Err_1st_Percent;
                        else
                            result = Body_ad.Serve_In_Suc_1st_Percent;
                    }
                    else
                    {
                        if (err == 1)
                            result = Body_de.Serve_Err_1st_Percent;
                        else
                            result = Body_ad.Serve_In_Suc_1st_Percent;
                    }
                    break;
                case (int)ServeType.Wide:
                    if (courtType == (int) CourtType.ad)
                    {
                        if (err == 1)
                            result = Wide_ad.Serve_Err_1st_Percent;
                        else
                            result = Wide_ad.Serve_In_Suc_1st_Percent;
                    }
                    else
                    {
                        if (err == 1)
                            result = Wide_de.Serve_Err_1st_Percent;
                        else
                            result = Wide_ad.Serve_In_Suc_1st_Percent;
                    }
                    break;

            }

            return result;
        }

        public static int getNad_Serve(int serveType , int courtType , int err, int serveTurn)
        {
            int result = 0;
            if (NadData == 0)
            {
                HttpDownloader htDownload = new HttpDownloader(URL, null, null);
                string downloadedString = htDownload.GetPage();
                sourceString = getPage(downloadedString, "RF", "BREAKDOWN");
                getDataNad();
                NadData = 1;
            }
            switch (serveType)
            {
                case (int)ServeType.T:
                    if (courtType == (int)CourtType.ad)
                    {
                        if (err == 1)
                            result = T_ad_Nad.Serve_Err_1st_Percent;
                        else
                            result = T_ad_Nad.Serve_In_Suc_1st_Percent;
                    }
                    else
                    {
                        if (err == 1)
                            result = T_de_Nad.Serve_Err_1st_Percent;
                        else
                            result = T_de_Nad.Serve_In_Suc_1st_Percent;
                    }
                    break;
                case (int)ServeType.Body:
                    if (courtType == (int)CourtType.ad)
                    {
                        if (err == 1)
                            result = Body_ad_Nad.Serve_Err_1st_Percent;
                        else
                            result = Body_ad_Nad.Serve_In_Suc_1st_Percent;
                    }
                    else
                    {
                        if (err == 1)
                            result = Body_de_Nad.Serve_Err_1st_Percent;
                        else
                            result = Body_ad_Nad.Serve_In_Suc_1st_Percent;
                    }
                    break;
                case (int)ServeType.Wide:
                    if (courtType == (int)CourtType.ad)
                    {
                        if (err == 1)
                            result = Wide_ad_Nad.Serve_Err_1st_Percent;
                        else
                            result = Wide_ad_Nad.Serve_In_Suc_1st_Percent;
                    }
                    else
                    {
                        if (err == 1)
                            result = Wide_de_Nad.Serve_Err_1st_Percent;
                        else
                            result = Wide_ad_Nad.Serve_In_Suc_1st_Percent;
                    }
                    break;

            }

            return result;
        }
        // Find the double fault point in a string
        private static int findDF(string line)
        {
            int result = 0;
            int count = 0;
            int z = 1;
            int l = line.Length;
            for (int i = l - 1; i >= 0; i--)
            {
                if (Char.IsDigit(line[i]))
                {
                    if (count == 0)
                    {
                        // ignore the 1st number
                        while (Char.IsDigit(line[i])) { --i; }
                        ++count;
                    }
                    else
                    {
                        // we read from the back to front
                        while (Char.IsDigit(line[i]))
                        {
                            result = result + (int)(line[i] - '0') * z;
                            z *= 10;
                            --i;
                        }
                        break;
                    }
                }
            }

            return result;
        }

        // Find the first number in the string, usually its the points
        private static int findFirstNum(string line)
        {
            int l = line.Length;
            int result = 0;
            for (int i = 0; i < l; i++)
            {
                if (Char.IsDigit(line, i))
                {
                    while (Char.IsDigit(line[i]))
                    {
                        result = result * 10 + (int)(line[i] - '0');
                        i++;
                    }
                    break;
                }
            }
            return result;
        }
    }

    class DesAnalyzerPat : ExpressionValue
    {
        enum Turn { Federer = 0, Nadal = 1, None = 2 };
        enum Hand { FH = 0, BH = 1 };

        enum ReturnType_Nad_BH_De { CrossDeep = 0, CrossShort = 1, DLDeep = 2, DLShort = 3, RE = 4 }; // RE is return error
        enum ReturnType_Nad_BH_Ad { InsideOutDeep = 0, InsideOutShort = 1, InsideInDeep = 2, InsideInShort = 3, RE = 4 };

        enum ReturnType_Nad_FH_De { InsideOutDeep = 0, InsideOutShort = 1, InsideInDeep = 2, InsideInShort = 3, RE = 4 }
        enum ReturnType_Nad_FH_Ad { CrossDeep = 0, CrossShort = 1, DLDeep = 2, DLShort = 3, RE = 4 }

        enum ReturnType_Fed_BH_De { InsideOutDeep = 0, InsideOutShort = 1, DLDeep = 2, DLShort = 3, RE = 4 };
        enum ReturnType_Fed_BH_Ad { CrossDeep = 0, CrossShort = 1, DLDeep = 2, DLShort = 3, RE = 4 };

        enum ReturnType_Fed_FH_De { CrossDeep = 0, CrossShort = 1, DLDeep = 2, DLShort = 3, RE = 4 };
        enum ReturnType_Fed_FH_Ad { InsideOutDeep = 0, InsideOutShort = 1, InsideInDeep = 2, InsideInShort = 3, RE = 4 };

        enum Shot { CrossCourt = 0, DownLine = 1, RE = 2 };
        enum courtType { deuce = 0, ad = 1 };
        enum ServeDepth { deep = 0, shallow = 1 };

        static string[] shots = { "crosscourt", "down the line" };
        private static Turn turn;
        const int numPlayer = 2;
        const int numReturn = 5;
        const int numHand = 2;
        const int numCourt = 2;
        const int numShot = 3;

        const string RF = "Roger Federer";
        const string RN = "Rafael Nadal";
        const string startset = "1st serve";
        const string startset2nd = "2nd serve";
        const string endset = "</td>";
        const string ace = "ace";
        const string BackHand = "backhand";
        const string ForeHand = "forehand";
        const string Err = "error";
        const string Deep = "deep";
        const string Short = "shallow";
        const string InsideOut = "inside-out";
        const string UFE = "unforced error"; // unforced error
        const string Winner = "winner";
        const string FE = "forced error"; // force error
        const string serveT = "down the T";
        const string serveBody = "body";
        const string serveWide = "wide";

        // array to store Nadal backhand return count
        private static int[][] Nad_BH_R;
        //array to store Nadal forehand return count
        private static int[][] Nad_FH_R;
        // array to store Federer backhand return count
        private static int[][] Fed_BH_R;
        //array to store Federer forehand return count
        private static int[][] Fed_FH_R;

        //array to store Fed backhand return percentage
        private static int[][] Fed_BH_R_Percent;
        //array to store Fed backhand return percentage
        private static int[][] Fed_FH_R_Percent;
        //array to store Fed backhand return percentage
        private static int[][] Nad_BH_R_Percent;
        //array to store Fed backhand return percentage
        private static int[][] Nad_FH_R_Percent;

        // array to store Fed shot
        private static int[][] Fed_Shot;
        // array to store Nad shot
        private static int[][] Nad_Shot;

        //array to store Fed shot percentage
        private static int[][] Fed_Shot_Percent;
        //array to store Nad shot percentage
        private static int[][] Nad_Shot_Percent;

        static string _sourceString;

        private static int dataAlr = 0;
        public DesAnalyzerPat(string sourceString)
        {
            _sourceString = sourceString;
            turn = Turn.None;

            Nad_BH_R = new int[numCourt][];
            for (int i = 0; i < numHand; i++)
                Nad_BH_R[i] = new int[numReturn];
            Fed_BH_R = new int[numCourt][];
            for (int i = 0; i < numCourt; i++)
                Fed_BH_R[i] = new int[numReturn];
            Nad_FH_R = new int[numCourt][];
            for (int i = 0; i < numCourt; i++)
                Nad_FH_R[i] = new int[numReturn];
            Fed_FH_R = new int[numCourt][];
            for (int i = 0; i < numCourt; i++)
                Fed_FH_R[i] = new int[numReturn];

            Fed_Shot = new int[numHand][];
            for (int i = 0; i < numHand; i++)
                Fed_Shot[i] = new int[numShot];
            Nad_Shot = new int[numHand][];
            for (int i = 0; i < numHand; i++)
                Nad_Shot[i] = new int[numShot];
        }

        public static void getData()
        {
            int startIndex = findServe();
            Turn curTurn = (turn == Turn.Federer ? Turn.Federer : Turn.Nadal);
            int endcurset = 0;
            // first serve of each always in deuce court
            courtType courtServe = courtType.deuce;
            string subMatch = "";
            _sourceString = _sourceString.Substring(startIndex);
            startIndex = _sourceString.IndexOf(startset);
            while (startIndex >= 0)
            {
                _sourceString = _sourceString.Substring(startIndex);
                endcurset = _sourceString.IndexOf(endset);
                // take the description line of that set, particularly that current match of the set
                // I don't know what that's called
                subMatch = _sourceString.Substring(0, endcurset);
                // getting the data from that submatch
                analyzeMatch(subMatch, curTurn, courtServe);
                _sourceString = _sourceString.Substring(subMatch.Length);
                findServe();
                if (curTurn != turn)
                {   // end set, the other player serve now
                    courtServe = courtType.deuce;
                    curTurn = turn;
                }
                else
                    courtServe = 1 - courtServe;
                startIndex = _sourceString.IndexOf(startset);
            }
            Compute_Fed_Return_Percentage();
            Compute_Nad_Return_Percentage();
            Compute_Fed_Shot_Percentage();
            Compute_Nad_Shot_Percentage();
        }

        // Find who serve next
        private static int findServe()
        {
            // Why it can't find if we put to variable ?
            int RFindex = _sourceString.IndexOf("Roger&nbsp;Federer");
            int RNindex = _sourceString.IndexOf("Rafael&nbsp;Nadal");
            if (RNindex == -1)
            {
                turn = Turn.Federer;
                return RFindex;
            }
            else if (RFindex == -1)
            {
                turn = Turn.Nadal;
                return RNindex;
            }
            else if (RFindex < RNindex)
            {
                turn = Turn.Federer;
                return RFindex;
            }
            else
            {
                turn = Turn.Nadal;
                return RNindex;
            }
        }

        private static void analyzeMatch(string subMatch, Turn curTurn, courtType courtServe)
        {
            // If the serve is ace, count to opp return error and return
            // We already find the serve statistic in other classes
            if (subMatch.IndexOf(ace) >= 0)
            {
                CountAce(subMatch, curTurn, courtServe);
                return;
            }
            // If there is a 2nd serve then we start from there
            if (subMatch.IndexOf(startset2nd) >= 0)
                subMatch = subMatch.Substring(subMatch.IndexOf(startset2nd));
            string[] lines = subMatch.Split(';');
            // curTurn is the one who serve, the return turn is the opposite
            Turn nxtTurn = 1 - curTurn;
            for (int i = 1; i < lines.Length; i++)
            {
                // analyze each line
                if (i == 1)
                {
                    // Count the return statistic
                    if (i < lines.Length - 1)
                    {   // this means the opponent can response to the return
                        if (courtServe == courtType.deuce)
                            countReturn_De(lines[i], nxtTurn, lines[i + 1]);
                        else
                            countReturn_Ad(lines[i], nxtTurn, lines[i + 1]);
                    }
                    else
                    {
                        if (courtServe == courtType.deuce)
                            countReturn_De(lines[i], nxtTurn, null);
                        else
                            countReturn_Ad(lines[i], nxtTurn, null);
                    }
                }
                else
                {
                    if (i < lines.Length - 1)
                    {   // this means the opponent can response to the return
                        countShot(lines[i], nxtTurn, lines[i + 1]);
                    }
                    else
                    {
                        countShot(lines[i], nxtTurn, null);
                    }
                }
                nxtTurn = 1 - nxtTurn;
            }
        }

        // Count to opponent return error in case of ace
        private static void CountAce(string subMatch, Turn curTurn, courtType courtServe)
        {
            Random random = new Random();
            Analyzer.serveType curServe = Analyzer.serveType.T;
            if (subMatch.IndexOf(serveBody) >= 0)
                curServe = Analyzer.serveType.Body;
            else if (subMatch.IndexOf(serveWide) >= 0)
                curServe = Analyzer.serveType.Wide;

            if (curTurn == Turn.Federer)
            {
                switch (curServe)
                {
                    case Analyzer.serveType.T:
                        if (courtServe == courtType.deuce)
                            Nad_FH_R[(int)courtServe][(int)ReturnType_Nad_FH_De.RE]++;
                        else
                            Nad_BH_R[(int)courtServe][(int)ReturnType_Nad_BH_De.RE]++;
                        break;
                    case Analyzer.serveType.Wide:
                        if (courtServe == courtType.deuce)
                            Nad_BH_R[(int)courtServe][(int)ReturnType_Nad_BH_De.RE]++;
                        else
                            Nad_FH_R[(int)courtServe][(int)ReturnType_Nad_FH_De.RE]++;
                        break;
                    case Analyzer.serveType.Body:
                        // for body we random 50/50
                        if (random.Next(0, 100) < 50)
                        {
                            Nad_BH_R[(int)courtServe][(int)ReturnType_Nad_BH_De.RE]++;
                        }
                        else
                        {
                            Nad_FH_R[(int)courtServe][(int)ReturnType_Nad_FH_De.RE]++;
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (curServe)
                {
                    case Analyzer.serveType.T:
                        if (courtServe == courtType.ad)
                            Fed_FH_R[(int)courtServe][(int)ReturnType_Fed_FH_De.RE]++;
                        else
                            Fed_BH_R[(int)courtServe][(int)ReturnType_Fed_BH_De.RE]++;
                        break;
                    case Analyzer.serveType.Wide:
                        if (courtServe == courtType.ad)
                            Fed_BH_R[(int)courtServe][(int)ReturnType_Fed_BH_De.RE]++;
                        else
                            Fed_FH_R[(int)courtServe][(int)ReturnType_Fed_FH_De.RE]++;
                        break;
                    case Analyzer.serveType.Body:
                        // for body we random 50/50
                        if (random.Next(0, 100) < 50)
                        {
                            Fed_BH_R[(int)courtServe][(int)ReturnType_Fed_BH_De.RE]++;
                        }
                        else
                        {
                            Fed_FH_R[(int)courtServe][(int)ReturnType_Fed_FH_De.RE]++;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        // count return for deuce serve
        private static void countReturn_De(string RetDes, Turn iTurn, string opResponse)
        {
            if (iTurn == Turn.Federer)
                countReturn_De_Fed(RetDes, opResponse);
            else
                countReturn_De_Nad(RetDes, opResponse);
        }

        // count return for ad serve
        private static void countReturn_Ad(string RetDes, Turn iTurn, string opResponse )
        {
            if (iTurn == Turn.Federer)
                countReturn_Ad_Fed(RetDes, opResponse);
            else
                countReturn_Ad_Nad(RetDes, opResponse);
        }

        // count the shot
        private static void countShot(string RetDes, Turn iTurn, string opResponse )
        {
            if (iTurn == Turn.Federer)
            {
                countFed_Shot(RetDes, opResponse);
            }
            else
            {
                countNad_Shot(RetDes, opResponse);
            }

        }

        private static void countReturn_De_Nad(string RetDes, string opResponse )
        {
            // op Response is what the opponent do after the serve, use to decide which type of return
            Hand hType = Hand.BH;
            Hand oppType = Hand.BH;
            ServeDepth sd = ServeDepth.deep;
            if (RetDes.IndexOf(BackHand) < 0) hType = Hand.FH;
            // Check the depth of the serve
            if (RetDes.IndexOf(Deep) < 0) sd = ServeDepth.shallow;
            if (RetDes.IndexOf(Err) >= 0)
            {
                // There is error, increment count on error
                if (hType == Hand.BH)
                    Nad_BH_R[(int)courtType.deuce][(int)ReturnType_Nad_BH_De.RE]++;
                else
                    Nad_FH_R[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.RE]++;
            }
            else if (opResponse == null)
            {
                // If oponent response is null, there should be some description about the serve type
                // For now just ignore
                return;
            }
            else
            {
                // Check the opponent response back or forehand
                if (opResponse.IndexOf(BackHand) < 0) oppType = Hand.FH;
                // A bunch of if
                if (hType == Hand.BH)
                {
                    // if return by back hand
                    // check the oponent response, we can then infere the return type
                    if (oppType == Hand.BH)
                    {
                        if (sd == ServeDepth.deep)
                            Nad_BH_R[(int)courtType.deuce][(int)ReturnType_Nad_BH_De.DLDeep]++;
                        else
                            Nad_BH_R[(int)courtType.deuce][(int)ReturnType_Nad_BH_De.DLShort]++;
                    }
                    else
                    {
                        if (sd == ServeDepth.deep)
                            Nad_BH_R[(int)courtType.deuce][(int)ReturnType_Nad_BH_De.CrossDeep]++;
                        else
                            Nad_BH_R[(int)courtType.deuce][(int)ReturnType_Nad_BH_De.CrossShort]++;
                    }
                }
                else
                {
                    // Nad FH return
                    if (oppType == Hand.BH)
                    {
                        if (sd == ServeDepth.deep)
                            Nad_FH_R[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.InsideInDeep]++;
                        else
                            Nad_FH_R[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.InsideInShort]++;
                    }
                    else
                    {
                        if (sd == ServeDepth.deep)
                            Nad_FH_R[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.InsideOutDeep]++;
                        else
                            Nad_FH_R[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.InsideOutShort]++;
                    }
                }
            }
        }
        private static void countReturn_Ad_Nad(string RetDes, string opResponse )
        {
            // op Response is what the opponent do after the serve, use to decide which type of return
            Hand hType = Hand.BH;
            Hand oppType = Hand.BH;
            ServeDepth sd = ServeDepth.deep;
            if (RetDes.IndexOf(BackHand) < 0) hType = Hand.FH;
            if (RetDes.IndexOf(Err) >= 0)
            {
                // There is error, increment count on error
                if (hType == Hand.BH)
                    Nad_BH_R[(int)courtType.ad][(int)ReturnType_Nad_BH_De.RE]++;
                else
                    Nad_FH_R[(int)courtType.ad][(int)ReturnType_Nad_FH_De.RE]++;
            }
            else if (opResponse == null)
            {
                // If oponent response is null, there should be some description about the serve type
                // For now just ignore
                return;
            }
            else
            {
                // Check the opponent response back or forehand
                if (opResponse.IndexOf(BackHand) < 0) oppType = Hand.FH;
                // Check the depth of the serve
                if (RetDes.IndexOf(Deep) < 0) sd = ServeDepth.shallow;
                // A bunch of if
                if (hType == Hand.BH)
                {
                    // if return by back hand
                    if (oppType == Hand.BH)
                    {
                        if (sd == ServeDepth.deep)
                            Nad_BH_R[(int)courtType.ad][(int)ReturnType_Nad_BH_Ad.InsideOutDeep]++;
                        else
                            Nad_BH_R[(int)courtType.ad][(int)ReturnType_Nad_BH_Ad.InsideOutShort]++;
                    }
                    else
                    {
                        if (sd == ServeDepth.deep)
                            Nad_BH_R[(int)courtType.ad][(int)ReturnType_Nad_BH_Ad.InsideInDeep]++;
                        else
                            Nad_BH_R[(int)courtType.ad][(int)ReturnType_Nad_BH_Ad.InsideInShort]++;
                    }
                }
                else
                {
                    // Nad FH return
                    if (oppType == Hand.BH)
                    {
                        if (sd == ServeDepth.deep)
                            Nad_FH_R[(int)courtType.ad][(int)ReturnType_Nad_FH_Ad.CrossDeep]++;
                        else
                            Nad_FH_R[(int)courtType.ad][(int)ReturnType_Nad_FH_Ad.CrossShort]++;
                    }
                    else
                    {
                        if (sd == ServeDepth.deep)
                            Nad_FH_R[(int)courtType.ad][(int)ReturnType_Nad_FH_Ad.DLDeep]++;
                        else
                            Nad_FH_R[(int)courtType.ad][(int)ReturnType_Nad_FH_Ad.DLShort]++;
                    }
                }
            }
        }

        private static void countReturn_De_Fed(string RetDes, string opResponse )
        {
            // op Response is what the opponent do after the serve, use to decide which type of return
            Hand hType = Hand.BH;
            Hand oppType = Hand.BH;
            ServeDepth sd = ServeDepth.deep;
            if (RetDes.IndexOf(BackHand) < 0) hType = Hand.FH;
            if (RetDes.IndexOf(Err) >= 0)
            {
                // There is error, increment count on error
                if (hType == Hand.BH)
                    Fed_BH_R[(int)courtType.deuce][(int)ReturnType_Fed_BH_De.RE]++;
                else
                    Fed_FH_R[(int)courtType.deuce][(int)ReturnType_Fed_FH_De.RE]++;
            }
            else if (opResponse == null)
            {
                // If oponent response is null, there should be some description about the serve type
                // For now just ignore
                return;
            }
            else
            {
                // Check the opponent response back or forehand
                if (opResponse.IndexOf(BackHand) < 0) oppType = Hand.FH;
                // Check the depth of the serve
                if (RetDes.IndexOf(Deep) < 0) sd = ServeDepth.shallow;
                // A bunch of if
                if (hType == Hand.BH)
                {
                    // if return by back hand
                    if (oppType == Hand.BH)
                    {
                        if (sd == ServeDepth.deep)
                            Fed_BH_R[(int)courtType.deuce][(int)ReturnType_Fed_BH_De.InsideOutDeep]++;
                        else
                            Fed_BH_R[(int)courtType.deuce][(int)ReturnType_Fed_BH_De.InsideOutShort]++;
                    }
                    else
                    {
                        if (sd == ServeDepth.deep)
                            Fed_BH_R[(int)courtType.deuce][(int)ReturnType_Fed_BH_De.DLDeep]++;
                        else
                            Fed_BH_R[(int)courtType.deuce][(int)ReturnType_Fed_BH_De.DLShort]++;
                    }
                }
                else
                {
                    // Nad FH return
                    if (oppType == Hand.BH)
                    {
                        if (sd == ServeDepth.deep)
                            Fed_FH_R[(int)courtType.deuce][(int)ReturnType_Fed_FH_De.CrossDeep]++;
                        else
                            Fed_FH_R[(int)courtType.deuce][(int)ReturnType_Fed_FH_De.CrossShort]++;
                    }
                    else
                    {
                        if (sd == ServeDepth.deep)
                            Fed_FH_R[(int)courtType.deuce][(int)ReturnType_Fed_FH_De.DLDeep]++;
                        else
                            Fed_FH_R[(int)courtType.deuce][(int)ReturnType_Fed_FH_De.DLShort]++;
                    }
                }
            }
        }
        private static void countReturn_Ad_Fed(string RetDes, string opResponse )
        {
            // op Response is what the opponent do after the serve, use to decide which type of return
            Hand hType = Hand.BH;
            Hand oppType = Hand.BH;
            ServeDepth sd = ServeDepth.deep;
            if (RetDes.IndexOf(BackHand) < 0) hType = Hand.FH;
            if (RetDes.IndexOf(Err) >= 0)
            {
                // There is error, increment count on error
                if (hType == Hand.BH)
                    Fed_BH_R[(int)courtType.ad][(int)ReturnType_Fed_BH_De.RE]++;
                else
                    Fed_FH_R[(int)courtType.ad][(int)ReturnType_Fed_FH_De.RE]++;
            }
            else if (opResponse == null)
            {
                // If oponent response is null, there should be some description about the serve type
                // For now just ignore
                return;
            }
            else
            {
                // Check the opponent response back or forehand
                if (opResponse.IndexOf(BackHand) < 0) oppType = Hand.FH;
                // Check the depth of the serve
                if (RetDes.IndexOf(Deep) < 0) sd = ServeDepth.shallow;
                // A bunch of if
                if (hType == Hand.BH)
                {
                    // if return by back hand
                    if (oppType == Hand.BH)
                    {
                        if (sd == ServeDepth.deep)
                            Fed_BH_R[(int)courtType.ad][(int)ReturnType_Fed_BH_Ad.DLDeep]++;
                        else
                            Fed_BH_R[(int)courtType.ad][(int)ReturnType_Fed_BH_Ad.DLShort]++;
                    }
                    else
                    {
                        if (sd == ServeDepth.deep)
                            Fed_BH_R[(int)courtType.ad][(int)ReturnType_Fed_BH_Ad.CrossDeep]++;
                        else
                            Fed_BH_R[(int)courtType.ad][(int)ReturnType_Fed_BH_Ad.CrossShort]++;
                    }
                }
                else
                {
                    // Nad FH return
                    if (oppType == Hand.BH)
                    {
                        if (sd == ServeDepth.deep)
                            Fed_FH_R[(int)courtType.ad][(int)ReturnType_Fed_FH_Ad.InsideInDeep]++;
                        else
                            Fed_FH_R[(int)courtType.ad][(int)ReturnType_Fed_FH_Ad.InsideInShort]++;
                    }
                    else
                    {
                        if (sd == ServeDepth.deep)
                            Fed_FH_R[(int)courtType.ad][(int)ReturnType_Fed_FH_Ad.InsideOutDeep]++;
                        else
                            Fed_FH_R[(int)courtType.ad][(int)ReturnType_Fed_FH_Ad.InsideOutShort]++;
                    }
                }
            }
        }

        public static void Compute_Fed_Return_Percentage()
        {
            if (Fed_BH_R_Percent == null)
            {
                Fed_BH_R_Percent = new int[numCourt][];
                for (int i = 0; i < numCourt; i++)
                    Fed_BH_R_Percent[i] = new int[numReturn];
            }
            if (Fed_FH_R_Percent == null)
            {
                Fed_FH_R_Percent = new int[numCourt][];
                for (int i = 0; i < numCourt; i++)
                    Fed_FH_R_Percent[i] = new int[numReturn];
            }
            Compute_Fed_BH_Return_Percentage();
            Compute_Fed_FH_Return_Percentage();

        }
        private static void Compute_Fed_BH_Return_Percentage()
        {
            int totalReturn = 0;
            // compute percentage for each court
            for (int i = 0; i < numCourt; i++)
            {
                totalReturn = 0;
                // Compute total return for each
                for (int j = 0; j < numReturn; j++)
                {
                    totalReturn += Fed_BH_R[i][j];
                }
                for (int j = 0; j < numReturn; j++)
                {
                    Fed_BH_R_Percent[i][j] = (int)((Fed_BH_R[i][j] * 1.0 / totalReturn * 100) + 0.5);
                }
            }
        }
        private static void Compute_Fed_FH_Return_Percentage()
        {
            int totalReturn = 0;
            // compute percentage for each court
            for (int i = 0; i < numCourt; i++)
            {
                totalReturn = 0;
                // Compute total return for each
                for (int j = 0; j < numReturn; j++)
                {
                    totalReturn += Fed_FH_R[i][j];
                }
                for (int j = 0; j < numReturn; j++)
                {
                    Fed_FH_R_Percent[i][j] = (int)((Fed_FH_R[i][j] * 1.0 / totalReturn * 100) + 0.5);
                }
            }
        }

        public static void Compute_Nad_Return_Percentage()
        {
            if (Nad_BH_R_Percent == null)
            {
                Nad_BH_R_Percent = new int[numCourt][];
                for (int i = 0; i < numCourt; i++)
                    Nad_BH_R_Percent[i] = new int[numReturn];
            }
            if (Nad_FH_R_Percent == null)
            {
                Nad_FH_R_Percent = new int[numCourt][];
                for (int i = 0; i < numCourt; i++)
                    Nad_FH_R_Percent[i] = new int[numReturn];
            }
            Compute_Nad_BH_Return_Percentage();
            Compute_Nad_FH_Return_Percentage();

        }
        private static void Compute_Nad_BH_Return_Percentage()
        {
            int totalReturn = 0;
            // compute percentage for each court
            for (int i = 0; i < numCourt; i++)
            {
                totalReturn = 0;
                // Compute total return for each
                for (int j = 0; j < numReturn; j++)
                {
                    totalReturn += Nad_BH_R[i][j];
                }
                for (int j = 0; j < numReturn; j++)
                {
                    Nad_BH_R_Percent[i][j] = (int)((Nad_BH_R[i][j] * 1.0 / totalReturn * 100) + 0.5);
                }
            }
        }
        private static void Compute_Nad_FH_Return_Percentage()
        {
            int totalReturn = 0;
            // compute percentage for each court
            for (int i = 0; i < numCourt; i++)
            {
                totalReturn = 0;
                // Compute total return for each
                for (int j = 0; j < numReturn; j++)
                {
                    totalReturn += Nad_FH_R[i][j];
                }
                for (int j = 0; j < numReturn; j++)
                {
                    Nad_FH_R_Percent[i][j] = (int)((Nad_FH_R[i][j] * 1.0 / totalReturn * 100) + 0.5);
                }
            }
        }

        public static void Fed_BH_Serve_toString()
        {
            System.IO.StreamWriter outfile = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\FedServeReturn.txt");
            outfile.WriteLine("Fed Backhand Retrun");
            outfile.WriteLine("Deuce Court");
            outfile.WriteLine("F_BH_InsideOutDeep " + Fed_BH_R_Percent[(int)courtType.deuce][(int)ReturnType_Fed_BH_De.InsideOutDeep]);
            outfile.WriteLine("F_BH_InsideOutShort " + Fed_BH_R_Percent[(int)courtType.deuce][(int)ReturnType_Fed_BH_De.InsideOutShort]);
            outfile.WriteLine("F_BH_DownlineDeep " + Fed_BH_R_Percent[(int)courtType.deuce][(int)ReturnType_Fed_BH_De.DLDeep]);
            outfile.WriteLine("F_BH_DownlineShort " + Fed_BH_R_Percent[(int)courtType.deuce][(int)ReturnType_Fed_BH_De.DLShort]);
            outfile.WriteLine("F_BH_ReturnError " + Fed_BH_R_Percent[(int)courtType.deuce][(int)ReturnType_Fed_BH_De.RE]);

            outfile.WriteLine("\nAd Court");
            outfile.WriteLine("F_BH_CrossDeep " + Fed_BH_R_Percent[(int)courtType.ad][(int)ReturnType_Fed_BH_Ad.CrossDeep]);
            outfile.WriteLine("F_BH_CrossShort " + Fed_BH_R_Percent[(int)courtType.ad][(int)ReturnType_Fed_BH_Ad.CrossShort]);
            outfile.WriteLine("F_BH_DownLineDeep " + Fed_BH_R_Percent[(int)courtType.ad][(int)ReturnType_Fed_BH_Ad.DLDeep]);
            outfile.WriteLine("F_BH_DownlineShort " + Fed_BH_R_Percent[(int)courtType.ad][(int)ReturnType_Fed_BH_Ad.DLShort]);
            outfile.WriteLine("F_BH_ReturnError " + Fed_BH_R_Percent[(int)courtType.ad][(int)ReturnType_Fed_BH_Ad.RE]);

            outfile.Close();
        }
        public static void Fed_FH_Serve_toString()
        {
            System.IO.StreamWriter outfile = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\FedServe.txt");
            outfile.WriteLine("Fed Forehand Retrun");
            outfile.WriteLine("Deuce Court");
            outfile.WriteLine("F_FH_CrossDeep " + Fed_FH_R_Percent[(int)courtType.deuce][(int)ReturnType_Fed_FH_De.CrossDeep]);
            outfile.WriteLine("F_FH_CrossShort " + Fed_FH_R_Percent[(int)courtType.deuce][(int)ReturnType_Fed_FH_De.CrossShort]);
            outfile.WriteLine("F_FH_DownLineDeep " + Fed_FH_R_Percent[(int)courtType.deuce][(int)ReturnType_Fed_BH_De.DLDeep]);
            outfile.WriteLine("F_FH_DownLineShort " + Fed_FH_R_Percent[(int)courtType.deuce][(int)ReturnType_Fed_BH_De.DLShort]);
            outfile.WriteLine("F_FH_ReturnError " + Fed_FH_R_Percent[(int)courtType.deuce][(int)ReturnType_Fed_FH_De.RE]);

            outfile.WriteLine("\nAd Court");
            outfile.WriteLine("F_FH_InsideOutDeep " + Fed_FH_R_Percent[(int)courtType.ad][(int)ReturnType_Fed_FH_Ad.InsideOutDeep]);
            outfile.WriteLine("F_FH_InsideOutShort " + Fed_FH_R_Percent[(int)courtType.ad][(int)ReturnType_Fed_FH_Ad.InsideOutShort]);
            outfile.WriteLine("F_FH_InsideInDeep " + Fed_FH_R_Percent[(int)courtType.ad][(int)ReturnType_Fed_FH_Ad.InsideInDeep]);
            outfile.WriteLine("F_FH_InsideInShort " + Fed_FH_R_Percent[(int)courtType.ad][(int)ReturnType_Fed_FH_Ad.InsideInShort]);
            outfile.WriteLine("F_BH_ReturnError " + Fed_FH_R_Percent[(int)courtType.ad][(int)ReturnType_Fed_FH_Ad.RE]);

            outfile.Close();
        }

        public static void Nad_BH_Serve_toString()
        {
            System.IO.StreamWriter outfile = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\NadServeReturn.txt");
            outfile.WriteLine("Nad Backhand Retrun");
            outfile.WriteLine("Deuce Court");
            outfile.WriteLine("N_BH_CrossDeep " + Nad_BH_R_Percent[(int)courtType.deuce][(int)ReturnType_Nad_BH_De.CrossDeep]);
            outfile.WriteLine("N_BH_CrossShort " + Nad_BH_R_Percent[(int)courtType.deuce][(int)ReturnType_Nad_BH_De.CrossShort]);
            outfile.WriteLine("N_BH_DownlineDeep " + Nad_BH_R_Percent[(int)courtType.deuce][(int)ReturnType_Nad_BH_De.DLDeep]);
            outfile.WriteLine("N_BH_DownlineShort " + Nad_BH_R_Percent[(int)courtType.deuce][(int)ReturnType_Nad_BH_De.DLShort]);
            outfile.WriteLine("N_BH_ReturnError " + Nad_BH_R_Percent[(int)courtType.deuce][(int)ReturnType_Nad_BH_De.RE]);

            outfile.WriteLine("\nAd Court");
            outfile.WriteLine("N_BH_InsideOutDeep " + Nad_BH_R_Percent[(int)courtType.ad][(int)ReturnType_Nad_BH_Ad.InsideOutDeep]);
            outfile.WriteLine("N_BH_InsideOutShort " + Nad_BH_R_Percent[(int)courtType.ad][(int)ReturnType_Nad_BH_Ad.InsideOutShort]);
            outfile.WriteLine("N_BH_InsideInDeep " + Nad_BH_R_Percent[(int)courtType.ad][(int)ReturnType_Nad_BH_Ad.InsideInDeep]);
            outfile.WriteLine("N_BH_InsideInShort " + Nad_BH_R_Percent[(int)courtType.ad][(int)ReturnType_Nad_BH_Ad.InsideInShort]);
            outfile.WriteLine("N_BH_ReturnError " + Nad_BH_R_Percent[(int)courtType.ad][(int)ReturnType_Nad_BH_Ad.RE]);

            outfile.Close();
        }

        public static void Nad_FH_Serve_toString()
        {
            System.IO.StreamWriter outfile = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\FedServe.txt");
            outfile.WriteLine("Nad Backhand Retrun");
            outfile.WriteLine("Deuce Court");
            outfile.WriteLine("N_FH_InsideOutDeep " + Nad_FH_R_Percent[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.InsideOutDeep]);
            outfile.WriteLine("N_FH_InsideOutShort " + Nad_FH_R_Percent[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.InsideOutShort]);
            outfile.WriteLine("N_FH_InsideInDeep " + Nad_FH_R_Percent[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.InsideInDeep]);
            outfile.WriteLine("N_FH_InsideInShort " + Nad_FH_R_Percent[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.InsideInShort]);
            outfile.WriteLine("N_FH_ReturnError " + Nad_FH_R_Percent[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.RE]);

            outfile.WriteLine("\nAd Court");
            outfile.WriteLine("N_FH_CrossDeep " + Nad_FH_R_Percent[(int)courtType.ad][(int)ReturnType_Nad_FH_Ad.CrossDeep]);
            outfile.WriteLine("N_FH_CrossShort " + Nad_FH_R_Percent[(int)courtType.ad][(int)ReturnType_Nad_FH_Ad.CrossShort]);
            outfile.WriteLine("N_FH_DownlineDeep " + Nad_FH_R_Percent[(int)courtType.ad][(int)ReturnType_Nad_FH_Ad.DLDeep]);
            outfile.WriteLine("N_FH_DownlineShort " + Nad_FH_R_Percent[(int)courtType.ad][(int)ReturnType_Nad_FH_Ad.DLShort]);
            outfile.WriteLine("N_FH_ReturnError " + Nad_FH_R_Percent[(int)courtType.ad][(int)ReturnType_Nad_FH_Ad.RE]);

            outfile.Close();
        }

        public static void countFed_Shot(string RetDes, string opResponse )
        {
            Hand hType = Hand.BH;
            Hand opType = Hand.BH;
            if (RetDes.IndexOf(BackHand) < 0) hType = Hand.FH;

            if (RetDes.IndexOf(UFE) >= 0 || RetDes.IndexOf(Winner) >= 0)
            {
                Fed_Shot[(int)hType][(int)Shot.RE]++;
            }
            else
            {
                for (int i = 0; i < shots.Length; i++)
                {
                    if (RetDes.IndexOf(shots[i]) >= 0)
                    {
                        Fed_Shot[(int)hType][i]++;
                        if (RetDes.IndexOf(FE) >= 0)
                        { //forced error means opponent can't return
                            switch (i)
                            {
                                case (int)Shot.CrossCourt:
                                    if (hType == Hand.BH)
                                        Nad_Shot[(int)Hand.FH][(int)Shot.RE]++;
                                    else
                                        Nad_Shot[(int)Hand.BH][(int)Shot.RE]++;
                                    break;
                                case (int)Shot.DownLine:
                                    if (hType == Hand.BH)
                                        Nad_Shot[(int)Hand.BH][(int)Shot.RE]++;
                                    else
                                        Nad_Shot[(int)Hand.FH][(int)Shot.RE]++;
                                    break;
                                default:
                                    break;
                            }

                        }
                        return;
                    }
                }
                // The record doesn't specify which type of shot, infere from op
                if (opResponse != null)
                {
                    if (opResponse.IndexOf(BackHand) < 0) opType = Hand.FH;
                    if (opType == Hand.BH)
                    {
                        if (hType == Hand.FH)
                            Fed_Shot[(int)hType][(int)Shot.CrossCourt]++;
                        else
                            Fed_Shot[(int)hType][(int)Shot.DownLine]++;
                    }
                    else
                    {
                        if (hType == Hand.FH)
                            Fed_Shot[(int)hType][(int)Shot.DownLine]++;
                        else
                            Fed_Shot[(int)hType][(int)Shot.CrossCourt]++;
                    }
                }
            }
        }

        public static void countNad_Shot(string RetDes, string opResponse )
        {
            Hand hType = Hand.BH;
            Hand opType = Hand.BH;
            if (RetDes.IndexOf(BackHand) < 0) hType = Hand.FH;
            if (RetDes.IndexOf(UFE) >= 0)
            {
                Nad_Shot[(int)hType][(int)Shot.RE]++;
            }
            else
            {
                for (int i = 0; i < shots.Length; i++)
                {
                    if (RetDes.IndexOf(shots[i]) >= 0)
                    {
                        Nad_Shot[(int)hType][i]++;
                        if (RetDes.IndexOf(FE) >= 0 || RetDes.IndexOf(Winner) >= 0)
                        { //forced error means opponent can't return
                            switch (i)
                            {
                                case (int)Shot.CrossCourt:
                                    if (hType == Hand.BH)
                                        Nad_Shot[(int)Hand.FH][(int)Shot.RE]++;
                                    else
                                        Nad_Shot[(int)Hand.BH][(int)Shot.RE]++;
                                    break;
                                case (int)Shot.DownLine:
                                    if (hType == Hand.BH)
                                        Nad_Shot[(int)Hand.BH][(int)Shot.RE]++;
                                    else
                                        Nad_Shot[(int)Hand.FH][(int)Shot.RE]++;
                                    break;
                                default:
                                    break;
                            }

                        }
                        return;
                    }
                }
                // The record doesn't specify which type of shot, infere from op
                if (opResponse != null)
                {
                    if (opResponse.IndexOf(BackHand) < 0) opType = Hand.FH;
                    if (opType == Hand.BH)
                    {
                        if (hType == Hand.FH)
                            Nad_Shot[(int)hType][(int)Shot.CrossCourt]++;
                        else
                            Nad_Shot[(int)hType][(int)Shot.DownLine]++;
                    }
                    else
                    {
                        if (hType == Hand.FH)
                            Nad_Shot[(int)hType][(int)Shot.DownLine]++;
                        else
                            Nad_Shot[(int)hType][(int)Shot.CrossCourt]++;

                    }
                }
            }
        }

        public static void Compute_Fed_Shot_Percentage()
        {
            if (Fed_Shot_Percent == null)
            {
                Fed_Shot_Percent = new int[numHand][];
                for (int i = 0; i < numHand; i++)
                    Fed_Shot_Percent[i] = new int[numShot];
            }
            int totalReturn = 0;
            for (int i = 0; i < numHand; i++)
            {
                totalReturn = 0;
                for (int j = 0; j < numShot; j++)
                {
                    totalReturn += Fed_Shot[i][j];
                }
                for (int j = 0; j < numShot; j++)
                {
                    Fed_Shot_Percent[i][j] = (int)((Fed_Shot[i][j] * 1.0 / totalReturn * 100) + 0.5);
                }
            }
        }

        public static void Compute_Nad_Shot_Percentage()
        {
            if (Nad_Shot_Percent == null)
            {
                Nad_Shot_Percent = new int[numHand][];
                for (int i = 0; i < numHand; i++)
                    Nad_Shot_Percent[i] = new int[numShot];
            }
            int totalReturn = 0;
            for (int i = 0; i < numHand; i++)
            {
                totalReturn = 0;
                for (int j = 0; j < numShot; j++)
                {
                    totalReturn += Nad_Shot[i][j];
                }
                for (int j = 0; j < numShot; j++)
                {
                    Nad_Shot_Percent[i][j] = (int)(Nad_Shot[i][j] * 1.0 / totalReturn * 100 + 0.5);
                }
            }
        }

        public static void Fed_Shot_toString()
        {
            System.IO.StreamWriter outfile = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\FedShot.txt");
            outfile.WriteLine("Fed Backhand Shot");
            outfile.WriteLine("CrossCourt " + Fed_Shot_Percent[(int)Hand.BH][(int)Shot.CrossCourt]);
            outfile.WriteLine("Downline " + Fed_Shot_Percent[(int)Hand.BH][(int)Shot.DownLine]);
            outfile.WriteLine("Return Error " + Fed_Shot_Percent[(int)Hand.BH][(int)Shot.RE]);

            outfile.WriteLine("\nFed Forehand Shot");
            outfile.WriteLine("CrossCourt " + Fed_Shot_Percent[(int)Hand.FH][(int)Shot.CrossCourt]);
            outfile.WriteLine("Downline " + Fed_Shot_Percent[(int)Hand.FH][(int)Shot.DownLine]);
            outfile.WriteLine("Return Error " + Fed_Shot_Percent[(int)Hand.FH][(int)Shot.RE]);

            outfile.Close();
        }

        public static void Nad_Shot_toString()
        {
            System.IO.StreamWriter outfile = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\NadShot.txt");
            outfile.WriteLine("Nad Backhand Shot");
            outfile.WriteLine("CrossCourt " + Nad_Shot_Percent[(int)Hand.BH][(int)Shot.CrossCourt]);
            outfile.WriteLine("Downline " + Nad_Shot_Percent[(int)Hand.BH][(int)Shot.DownLine]);
            outfile.WriteLine("Return Error " + Nad_Shot_Percent[(int)Hand.BH][(int)Shot.RE]);

            outfile.WriteLine("\nNad Forehand Shot");
            outfile.WriteLine("CrossCourt " + Nad_Shot_Percent[(int)Hand.FH][(int)Shot.CrossCourt]);
            outfile.WriteLine("Downline " + Nad_Shot_Percent[(int)Hand.FH][(int)Shot.DownLine]);
            outfile.WriteLine("Return Error " + Nad_Shot_Percent[(int)Hand.FH][(int)Shot.RE]);

            outfile.Close();
        }


        public static int getFed_BH_R_Percent(int court, int returnType) {
            get_Needed_Data();
            return Fed_BH_R_Percent[court][returnType]; 
        }

        public static int getFed_FH_R_Percent(int court, int returnType) {
            get_Needed_Data();
            return Fed_FH_R_Percent[court][returnType]; 
        }

        public static int getNad_BH_R_Percent(int court, int returnType) {
            get_Needed_Data();
            return Nad_BH_R_Percent[court][returnType]; 
        }

        public static int getNad_FH_R_Percent(int court, int returnType) {
            get_Needed_Data();
            return Nad_FH_R_Percent[court][returnType]; 
        }
		
		public static int getNad_Shot_Percent(int hand, int shotType, int depth)
        {
            get_Needed_Data();
            if (shotType == (int)Shot.RE)
                return Nad_Shot_Percent[hand][shotType];
            // depth = 0 is short
            if (depth == 0)
            {
                return (int)(Fed_Shot_Percent[hand][shotType] - Nad_Shot_Percent[hand][shotType] * 0.8);
            }
            else
            {
                return (int) (Nad_Shot_Percent[hand][shotType] * 0.8);
            }
        }
        public static int getFed_Shot_Percent(int hand, int shotType, int depth)
        {
            get_Needed_Data();
            // depth = 0 is short
            if (shotType == (int)Shot.RE)
                return Fed_Shot_Percent[hand][shotType];
            if (depth == 0)
            {
                return (int)(Fed_Shot_Percent[hand][shotType] - Fed_Shot_Percent[hand][shotType] * 0.8);
            }
            else
            {
                return (int)(Fed_Shot_Percent[hand][shotType] * 0.8);
            }
        }
        
        private static void get_Needed_Data()
        {
            if (dataAlr == 0)
            {
                HttpDownloader htDownload = new HttpDownloader(ServeBreakdownPat.URL, null, null);
                string downloadedString = htDownload.GetPage();
                _sourceString = getPage(downloadedString);
                Nad_BH_R = new int[numCourt][];
                for (int i = 0; i < numHand; i++)
                    Nad_BH_R[i] = new int[numReturn];
                Fed_BH_R = new int[numCourt][];
                for (int i = 0; i < numCourt; i++)
                    Fed_BH_R[i] = new int[numReturn];
                Nad_FH_R = new int[numCourt][];
                for (int i = 0; i < numCourt; i++)
                    Nad_FH_R[i] = new int[numReturn];
                Fed_FH_R = new int[numCourt][];
                for (int i = 0; i < numCourt; i++)
                    Fed_FH_R[i] = new int[numReturn];

                Fed_Shot = new int[numHand][];
                for (int i = 0; i < numHand; i++)
                    Fed_Shot[i] = new int[numShot];
                Nad_Shot = new int[numHand][];
                for (int i = 0; i < numHand; i++)
                    Nad_Shot[i] = new int[numShot];
                getData();
                dataAlr = 1;
            }
        }

        private static string getPage(string downloadedString)
        {
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
                        truString = aline.Substring(start, end - start + 8);
                        //truString = WebUtility.HtmlDecode(truString);
                        break;
                    }
                }
            }
            return truString;
        }
    }
}
