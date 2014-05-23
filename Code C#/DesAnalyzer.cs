using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGettingDataFromWebsite
{
    class DesAnalyzer
    {
        enum Turn { Federer = 0, Nadal = 1, None = 2 };
        enum Hand{FH = 0, BH = 1};

        enum ReturnType_Nad_BH_De { CrossDeep = 0, CrossShort = 1, DLDeep = 2, DLShort = 3, RE = 4 }; // RE is return error
        enum ReturnType_Nad_BH_Ad { InsideOutDeep = 0, InsideOutShort = 1, InsideInDeep = 2, InsideInShort = 3, RE = 4 };

        enum ReturnType_Nad_FH_De { InsideOutDeep = 0, InsideOutShort = 1, InsideInDeep = 2, InsideInShort = 3, RE = 4 }
        enum ReturnType_Nad_FH_Ad { CrossDeep = 0, CrossShort = 1, DLDeep = 2, DLShort = 3, RE = 4 }

        enum ReturnType_Fed_BH_De { InsideOutDeep = 0, InsideOutShort=1, DLDeep=2, DLShort = 3, RE = 4 };
        enum ReturnType_Fed_BH_Ad { CrossDeep = 0, CrossShort = 1, DLDeep = 2, DLShort = 3, RE = 4 };
        
        enum ReturnType_Fed_FH_De { CrossDeep = 0, CrossShort = 1, DLDeep = 2, DLShort = 3, RE = 4 };
        enum ReturnType_Fed_FH_Ad { InsideOutDeep = 0, InsideOutShort = 1, InsideInDeep = 2, InsideInShort = 3, RE = 4 };

        enum Shot { CrossCourt = 0, DownLine = 1, RE = 2 };
        enum courtType { deuce = 0, ad = 1 };
        enum ServeDepth { deep = 0, shallow = 1 };

        string[] shots = { "crosscourt", "down the line" };
        private Turn turn;
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

        // array to store Nadal backhand return count
        private int[][] Nad_BH_R;
        //array to store Nadal forehand return count
        private int[][] Nad_FH_R;
        // array to store Federer backhand return count
        private int[][] Fed_BH_R;
        //array to store Federer forehand return count
        private int[][] Fed_FH_R;

        //array to store Fed backhand return percentage
        private int[][] Fed_BH_R_Percent;
        //array to store Fed backhand return percentage
        private int[][] Fed_FH_R_Percent;
        //array to store Fed backhand return percentage
        private int[][] Nad_BH_R_Percent;
        //array to store Fed backhand return percentage
        private int[][] Nad_FH_R_Percent;

        // array to store Fed shot
        private int[][] Fed_Shot;
        // array to store Nad shot
        private int[][] Nad_Shot;

        //array to store Fed shot percentage
        private int[][] Fed_Shot_Percent;
        //array to store Nad shot percentage
        private int[][] Nad_Shot_Percent;

        string _sourceString;

        public DesAnalyzer(string sourceString)
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

        public void getData()
        {
            int startIndex = findServe();
            Turn curTurn = (turn == Turn.Federer ? Turn.Federer : Turn.Nadal);
            int endcurset = 0;
            // first serve of each always in deuce court
            courtType courtServe = courtType.deuce;
            string subMatch ="";
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

            Fed_BH_Serve_toString();
            Nad_BH_Serve_toString();

            Fed_Shot_toString();
            Nad_Shot_toString();
        }

        // Find who serve next
        private int findServe()
        {
            // Why it can't find if we put to variable ?
            int RFindex = _sourceString.IndexOf("Roger Federer");
            int RNindex = _sourceString.IndexOf("Rafael Nadal");
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
            else if (RFindex < RNindex )
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

        private void analyzeMatch(string subMatch, Turn curTurn, courtType courtServe)
        {
            // If the serve is ace, don't care
            // We already find the serve statistic in other classes
            if (subMatch.IndexOf(ace) >= 0) return;
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
                    else{
                        if (courtServe == courtType.deuce)
                            countReturn_De(lines[i], nxtTurn);
                        else
                            countReturn_Ad(lines[i], nxtTurn);
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
                        countShot(lines[i],nxtTurn);
                    }
                }
                nxtTurn = 1 - nxtTurn;
            }
        }

        // count return for deuce serve
        private void countReturn_De(string RetDes, Turn iTurn, string opResponse = null)
        {
            if (iTurn == Turn.Federer)
                countReturn_De_Fed(RetDes, opResponse);
            else
                countReturn_De_Nad(RetDes, opResponse);
        }

        // count return for ad serve
        private void countReturn_Ad(string RetDes, Turn iTurn, string opResponse = null)
        {
            if (iTurn == Turn.Federer)
                countReturn_Ad_Fed(RetDes, opResponse);
            else
                countReturn_Ad_Nad(RetDes, opResponse);
        }

        // count the shot
        private void countShot(string RetDes, Turn iTurn, string opResponse = null){
            if (iTurn == Turn.Federer)
            {
                countFed_Shot(RetDes, opResponse);
            }
            else
            {
                countNad_Shot(RetDes, opResponse);
            }

        }

        private void countReturn_De_Nad(string RetDes, string opResponse = null)
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
                    else{
                        if (sd == ServeDepth.deep)
                            Nad_BH_R[(int)courtType.deuce][(int)ReturnType_Nad_BH_De.CrossDeep]++;
                        else
                            Nad_BH_R[(int)courtType.deuce][(int)ReturnType_Nad_BH_De.CrossShort]++;
                    }
                }else{
                    // Nad FH return
                     if (oppType == Hand.BH)
                    {
                        if (sd == ServeDepth.deep)
                            Nad_FH_R[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.InsideInDeep]++;
                        else
                            Nad_FH_R[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.InsideInShort]++;
                    }
                    else{
                        if (sd == ServeDepth.deep)
                            Nad_FH_R[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.InsideOutDeep]++;
                        else
                            Nad_FH_R[(int)courtType.deuce][(int)ReturnType_Nad_FH_De.InsideOutShort]++;
                    }
                }
            }
        }
        private void countReturn_Ad_Nad(string RetDes, string opResponse = null)
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
                    else{
                        if (sd == ServeDepth.deep)
                            Nad_BH_R[(int)courtType.ad][(int)ReturnType_Nad_BH_Ad.InsideInDeep]++;
                        else
                            Nad_BH_R[(int)courtType.ad][(int)ReturnType_Nad_BH_Ad.InsideInShort]++;
                    }
                }else{
                    // Nad FH return
                     if (oppType == Hand.BH)
                    {
                        if (sd == ServeDepth.deep)
                            Nad_FH_R[(int)courtType.ad][(int)ReturnType_Nad_FH_Ad.CrossDeep]++;
                        else
                            Nad_FH_R[(int)courtType.ad][(int)ReturnType_Nad_FH_Ad.CrossShort]++;
                    }
                    else{
                        if (sd == ServeDepth.deep)
                            Nad_FH_R[(int)courtType.ad][(int)ReturnType_Nad_FH_Ad.DLDeep]++;
                        else
                            Nad_FH_R[(int)courtType.ad][(int)ReturnType_Nad_FH_Ad.DLShort]++;
                    }
                }
            }
        }

        private void countReturn_De_Fed(string RetDes, string opResponse = null)
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
        private void countReturn_Ad_Fed(string RetDes, string opResponse = null)
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

        public void Compute_Fed_Return_Percentage()
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
        private void Compute_Fed_BH_Return_Percentage()
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
        private void Compute_Fed_FH_Return_Percentage()
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

        public void Compute_Nad_Return_Percentage()
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
        private void Compute_Nad_BH_Return_Percentage()
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
        private void Compute_Nad_FH_Return_Percentage()
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

        public void Fed_BH_Serve_toString()
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
        public void Fed_FH_Serve_toString()
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

        public void Nad_BH_Serve_toString()
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

        public void Nad_FH_Serve_toString()
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

        public void countFed_Shot(string RetDes, string opResponse = null)
        {
            Hand hType = Hand.BH;
            Hand opType = Hand.BH;
            if (RetDes.IndexOf(BackHand)<0) hType = Hand.FH;

            if (RetDes.IndexOf(UFE) >= 0 || RetDes.IndexOf(Winner) >=0)
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

        public void countNad_Shot(string RetDes, string opResponse = null)
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

        public void Compute_Fed_Shot_Percentage()
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
                    Fed_Shot_Percent[i][j] = (int)((Fed_Shot[i][j] * 1.0 / totalReturn * 100 )+ 0.5);
                }
            }
        }

        public void Compute_Nad_Shot_Percentage()
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
                    Nad_Shot_Percent[i][j] = (int)(Nad_Shot[i][j] * 1.0 / totalReturn *100 + 0.5);
                }
            }
        }

        public void Fed_Shot_toString()
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

        public void Nad_Shot_toString()
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


        public int[][] getFed_BH_R_Percent(){return Fed_BH_R_Percent;}

        public int[][] getFed_FH_R_Percent(){return Fed_FH_R_Percent;}

        public int[][] getNad_BH_R_Percent(){return Nad_BH_R_Percent;}

        public int[][] getNad_FH_R_Percent(){return Nad_FH_R_Percent;}
    }
}
