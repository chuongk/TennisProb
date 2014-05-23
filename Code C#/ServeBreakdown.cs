using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TestGettingDataFromWebsite
{
    class ServeBreakdown
    {
        private string playerName;
        private string webPage;
        private const int lines = 26;   // the format have 26 lines;
        private Analyzer T_de;
        private Analyzer T_ad;
        private Analyzer Wide_de;
        private Analyzer Wide_ad;
        private Analyzer Body_de;
        private Analyzer Body_ad;

        private string _sourceString;

        public ServeBreakdown(string sourceString,string plyName, string PageName)
        {
            playerName = plyName;
            webPage = PageName;
            _sourceString = sourceString;
        }

        // Call this one to get the data for that player
        public void getData()
        {
            string filename = "D:\\FYP\\propabilititis tennis" + "\\" + playerName + "BREAKDOWN.txt";
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
            StringReader file = new StringReader(_sourceString);
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

            //Write Everything to the file
            writeDataToFile();

            // Finish ,close file          
           // file.Close();

        }


        // Find the first number in the string, usually its the points
        private int findFirstNum(string line)
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

        // Find the double fault point in a string
        private int findDF(string line)
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

        private void writeDataToFile(bool deuce = true, bool ad = true)
        {
            System.IO.StreamWriter fileRes = new System.IO.StreamWriter(@"D:\FYP\propabilititis tennis\"+ playerName +"serveStatistic.txt");
            fileRes.WriteLine(playerName);
            fileRes.WriteLine("1st serve");
            fileRes.WriteLine("Deuce Court Data");
            fileRes.WriteLine(T_de.toString1st());
            fileRes.WriteLine(Wide_de.toString1st());
            fileRes.WriteLine(Body_de.toString1st());
            fileRes.WriteLine("2nd serve");
            fileRes.WriteLine(T_de.toString2nd());
            fileRes.WriteLine(Wide_de.toString2nd());
            fileRes.WriteLine(Body_de.toString2nd());

            fileRes.WriteLine();            
            fileRes.WriteLine("Ad Court Data");
            fileRes.WriteLine("1st serve");
            fileRes.WriteLine(T_ad.toString1st());
            fileRes.WriteLine(Wide_ad.toString1st());
            fileRes.WriteLine(Body_ad.toString1st());
            fileRes.WriteLine("2nd serve");
            fileRes.WriteLine(T_ad.toString2nd());
            fileRes.WriteLine(Wide_ad.toString2nd());
            fileRes.WriteLine(Body_ad.toString2nd());
            fileRes.Close();
        }

        // Return the anlyzer of that type
        public Analyzer getProb(Analyzer.courtType courtType, Analyzer.serveType serveType)
        {
            Analyzer needA = null;
            // Find which Analyzer class to take 
            if (courtType == Analyzer.courtType.Deuce)
            {
                switch (serveType)
                {
                    case Analyzer.serveType.T:
                        needA = T_de;
                        break;
                    case Analyzer.serveType.Body:
                        needA = Body_de;
                        break;
                    case Analyzer.serveType.Wide:
                        needA = Wide_de;
                        break;
                    default:
                        break;

                }
            }
            else
            {
                switch (serveType)
                {
                    case Analyzer.serveType.T:
                        needA = T_ad;
                        break;
                    case Analyzer.serveType.Body:
                        needA = Body_ad;
                        break;
                    case Analyzer.serveType.Wide:
                        needA = Wide_ad;
                        break;
                    default:
                        break;

                }
            }

            return needA;
        }
    }

}
