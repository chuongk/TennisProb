using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGettingDataFromWebsite
{
    // A class to store and compute information for a particular type of serve
    public class Analyzer
    {
        public enum courtType { Deuce, Ad };
        public enum serveType { Wide, Body, T };
        private courtType court;
        private serveType serve;

        private int totalPts;   // the total points for every serveType
        private int totalPts_curServe;  // the total points of the current serve

        private int Serve_Total_1st;    // the total point for every serveType 1st Serve

        private int Serve_Total_2nd;    // the total point for every serveType 2nd Serve

        private int Serve_In_1st;   // total point of the current 1st serve in
        //private int Serve_In_Suc_1st;   // total success point of the current 1st serve in

        private int Serve_In_Suc_1st_Percent; // Percentage of the 1st success serve in
        private int Serve_In_2nd;   // total point of the 2nd serve in
        private int Serve_In_Suc_2nd_Percent;   // Percentage of the 2nd success serve in
        private int Serve_In_Suc_2nd;   // total success point of the current 2nd serve in
        private int Serve_Err_1st;  // total error points of the 1st serve in = Serve_In_2nd
        private int Serve_Err_1st_Percent; // Percentage of the serve error first
        private int Serve_Err_2nd;  // total error points of the 2nd serve in = DF points for the current type
        private int Serve_Err_2nd_Percent;  // Perccentage of the serve error 2nd

        public Analyzer(string CourtType, string ServeType)
        {
            if (CourtType == "Deuce")
            {
                court = courtType.Deuce;
            }
            else
            {
                court = courtType.Ad;
            }
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
}
