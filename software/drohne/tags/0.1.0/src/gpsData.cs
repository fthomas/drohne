using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Drohne
{
    ///
    public class LogRMC : IComparable
    {
        ///
        public int CompareTo(object rhs)
        {
            LogRMC rmc = (LogRMC) rhs;
            return rmc.logBegin.CompareTo(rmc.logBegin);
        }
        
        ///
        public  DateTime   logBegin;
        ///
        public  DateTime   logEnd;
        private ArrayList  rmcData;
        private int        sentenceIndex = 0; 
        private int        removedIndex  = 0;
        
        ///
        public LogRMC()
        {
        }
        
        ///
        public LogRMC(string data)
        {
            this.rmcData   = new ArrayList();
            string[] lines = data.Split('\n');

            foreach (string line in lines)
                this.rmcData.Add(line);
        }

        ///
        public LogRMC(string[] filenames)
        {
            Hashtable methods = new Hashtable();
            methods["StatusInvalid"] = false;
            methods["NullValues"]    = false;
            methods["InvalidCoords"] = false;
            methods["ValidChecksum"] = false;
            
            LogRMC[] rmcLogs = new LogRMC[filenames.Length];
        
            for (int i = 0; i < filenames.Length; i++)
            {
                rmcLogs[i] = new LogRMC();
                rmcLogs[i].ReadFile(filenames[i]);
                rmcLogs[i].Clean(methods);
            }

            Array.Sort(rmcLogs);
            
            this.logBegin = rmcLogs[0].logBegin;
            this.logEnd   = rmcLogs[0].logEnd;
            this.rmcData  = new ArrayList();
            
            foreach (LogRMC rmc in rmcLogs)
            {
                if (this.logBegin > rmc.logBegin)
                    this.logBegin = rmc.logBegin;
                
                if (this.logEnd < rmc.logEnd)
                    this.logEnd = rmc.logEnd;                
                
                foreach (Hashtable fields in rmc.rmcData)
                    this.rmcData.Add(this.GetRMCSentence(fields));
            }
        }
        
        ///
        public LogRMC(ArrayList data, DateTime begin, DateTime end)
        {
            this.rmcData  = data;
            this.logBegin = begin;
            this.logEnd   = end;
        }
        
        ///
        public void ReadFile(string filename)
        {
            this.rmcData  = new ArrayList();
            this.logBegin = new DateTime();
            this.logEnd   = new DateTime();
            
            // This may raise an exception.
            using (StreamReader sr = new StreamReader(filename))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                    this.rmcData.Add(line);
            }
        }
        
        ///
        public void Clean(Hashtable methods)
        {
            // Create a temp ArrayList for the cleaned rmcData.
            ArrayList tmpRmcData = new ArrayList();

            foreach (string line in this.rmcData)
            {
                // Store all added lines.
                this.removedIndex++;     
                
                Hashtable fields = new Hashtable();
                
                // This is a generic test, tests checking the methods Hashtable
                // are chosen by user.
                if (!this.IsRMCSentence(line, ref fields))
                    continue;
                
                if ((bool)methods["StatusInvalid"] && (string)fields["Status"] == "V")
                    continue;

                string rmcSentence = this.GetRMCSentence(fields);
                if ((bool)methods["ValidChecksum"] && !this.IsValidChecksum(rmcSentence))
                    continue;
  
                // Create new NFI with a dot (.) as decimal number separator.
                NumberFormatInfo nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";
                                
                if ((bool)methods["NullValues"])
                {   
                    if (double.Parse((string)fields["Latitude"] , NumberStyles.Float, nfi) == 0 &&
                        double.Parse((string)fields["Longitude"], NumberStyles.Float, nfi) == 0)
                        continue;
                    
                    if (double.Parse((string)fields["Latitude"] , NumberStyles.Float, nfi) ==  3600 &&
                        double.Parse((string)fields["Longitude"], NumberStyles.Float, nfi) == 12000)
                        continue;
                }
                
                if ((bool)methods["InvalidCoords"])
                {   
                    if (double.Parse((string)fields["Latitude"] , NumberStyles.Float, nfi) >  9000 ||
                        double.Parse((string)fields["Longitude"], NumberStyles.Float, nfi) > 18000)
                        continue;              
                }
                
                // Line passed all tests and so we add it (as Hashtable fields)
                // to tmpRmcData.
                tmpRmcData.Add(fields);
                
                // Store all added RMC sentences.
                this.sentenceIndex++;
            }
            
            // Now calculate the removedIndex.
            this.removedIndex -= this.sentenceIndex; 

            this.rmcData = tmpRmcData;
        
            // Add additional complex tests here.

            //if ((bool)methods[""])
            //{}
            
            // Go through rmcData again and set logBegin and logEnd fields.
            this.SetDateTimeRange();
        }

        ///
        public LogRMC GetSegment(DateTime begin, DateTime end)
        {
            // Create a temp ArrayList for the segment rmcData.
            ArrayList tmpRmcData = new ArrayList();
            
            foreach (Hashtable fields in this.rmcData)
            {
                DateTime time = this.GetSentenceDateTime(fields);

                // Ignore all sentences not in range.
                if (time < begin || time > end)
                    continue;
                
                tmpRmcData.Add(fields);
            }
            
            return new LogRMC(tmpRmcData, begin, end);
        }

        ///
        public string GetRMCLog()
        {
            string output = "";
            
            foreach (Hashtable fields in this.rmcData)
                output += this.GetRMCSentence(fields) + "\r\n";
            
            return output;
        }

        ///
        public ArrayList Split()
        {
            ArrayList logArray   = new ArrayList(); 
            
            DateTime end   = logBegin;
            DateTime start = new DateTime();
            
            DateTime tourStart = logBegin;
            DateTime tourEnd   = new DateTime();
            
            TimeSpan pause = new TimeSpan(24, 0, 0);
            bool first = true;
            int items = this.rmcData.Count;
            int i = 0;
            
            foreach (Hashtable fields in this.rmcData)
            {
                // Count the elements in this.rmcData
                i++;

                if (first == true)
                {
                    first = false;
                    continue;
                }
                
                start = this.GetSentenceDateTime(fields);

                // Add a tour to ArrayList, if pause is greater one day or
                // the last item in rmcData is reached.
                if (start - end > pause || items == i)
                {
                    tourEnd = end;

                    // Add the tour!
                    logArray.Add( this.GetSegment(tourStart, tourEnd) );
                    
                    tourStart = start;
                }
                
                end = this.GetSentenceDateTime(fields);
            }
            
            return logArray;
        }

        ///
        public void WriteLog(string filename, bool append)
        {
            if (append == false)
            {
                using (TextWriter stream = File.CreateText(filename))
                {
                    foreach (Hashtable fields in this.rmcData)
                        stream.WriteLine(this.GetRMCSentence(fields)); 
                }
            }
            
            if (append == true)
            {
                using (StreamWriter stream = File.AppendText(filename))
                {
                    foreach (Hashtable fields in this.rmcData)
                        stream.WriteLine(this.GetRMCSentence(fields));

                    stream.Flush();
                }
            } 
        }

        private bool IsRMCSentence(string line, ref Hashtable fields) 
        {
            // Nice regex pattern, isn´t it? This matches a generic RMC line.
            string pattern = @"\$GPRMC,(\d{6}),(A|V),(\d{4}\.\d{0,4}),(N|S),"
                           + @"(\d{5}\.\d{0,4}),(E|W),(\d+\.\d{0,2}),(\d+\.\d{0,2}),"
                           + @"(\d{6}),(\d*\.?\d*),(E|W)?\*((\d|\w){0,2})";
            
            // Now test if RMC data is in line.
            Match rmcMatch = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
            
            if (!rmcMatch.Success)
                return false;
               
            // Now fill the RMC fields Hashtable.
            fields.Add("SentenceID"  , "$GPRMC");
            fields.Add("UTCTime"     , rmcMatch.Groups[1].ToString());
            fields.Add("Status"      , rmcMatch.Groups[2].ToString());
            fields.Add("Latitude"    , rmcMatch.Groups[3].ToString());
            fields.Add("NSIndicator" , rmcMatch.Groups[4].ToString());
            fields.Add("Longitude"   , rmcMatch.Groups[5].ToString());
            fields.Add("EWIndicator" , rmcMatch.Groups[6].ToString());
            fields.Add("Speed"       , rmcMatch.Groups[7].ToString());
            fields.Add("Course"      , rmcMatch.Groups[8].ToString());
            fields.Add("UTCDate"     , rmcMatch.Groups[9].ToString());
            fields.Add("MagVariation", rmcMatch.Groups[10].ToString());
            fields.Add("MagVarEWInd" , rmcMatch.Groups[11].ToString());
            fields.Add("Checksum"    , rmcMatch.Groups[12].ToString());
            
            return true;
        }

        private bool IsValidChecksum(string rmcSentence)
        {
            // Remove trailing $ and the checksum from sentence.
            char[] charArray = rmcSentence.ToCharArray(1, rmcSentence.Length - 4);
            int sum = 0;

            foreach (char c in charArray)
                sum ^= c;
            
            string checksum = rmcSentence.Substring(rmcSentence.Length - 2);
            int    orgSum   = int.Parse(checksum, NumberStyles.HexNumber);
            
            return (sum == orgSum);
        }

        private string GetRMCSentence(Hashtable fields)
        {
            return String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}*{12}",
                fields["SentenceID"] , fields["UTCTime"]     , fields["Status"]     ,
                fields["Latitude"]   , fields["NSIndicator"] , fields["Longitude"]  ,
                fields["EWIndicator"], fields["Speed"]       , fields["Course"]     ,
                fields["UTCDate"]    , fields["MagVariation"], fields["MagVarEWInd"],
                fields["Checksum"]);
        }

        private void SetDateTimeRange()
        {
            // boolean value to test if logBegin and logEnd are initialized.
            bool timeInit = false;
            
            foreach (Hashtable fields in this.rmcData)
            {
                DateTime sentenceDate = this.GetSentenceDateTime(fields);
                
                // Initialize logBegin and logEnd.
                if (!timeInit)
                {
                    this.logBegin = sentenceDate;
                    this.logEnd   = sentenceDate;
                    timeInit      = true;
                }
                else
                {
                    // Keep track of latest and oldest dates and save them to
                    // logBegin and logEnd.
                    if (sentenceDate < this.logBegin)
                        this.logBegin = sentenceDate;

                    if (sentenceDate > this.logEnd)
                        this.logEnd   = sentenceDate;
                }
            }
        }
        
        private DateTime GetSentenceDateTime(Hashtable fields)
        {
            string utcDate = (string)fields["UTCDate"];
            string utcTime = (string)fields["UTCTime"];

            int day    = int.Parse(utcDate.Substring(0, 2));
            int month  = int.Parse(utcDate.Substring(2, 2));
            int year   = int.Parse(utcDate.Substring(4, 2));
            int hour   = int.Parse(utcTime.Substring(0, 2));
            int minute = int.Parse(utcTime.Substring(2, 2));
            int second = int.Parse(utcTime.Substring(4, 2));

            // Ok, this code has an expiration date. ;) 
            if (year > 60)
                year += 1900;
            else
                year += 2000;
            
            DateTime time  = new DateTime(year, month, day, hour, minute, second);
            return time;
        }
    }
}

// vim:fileformat=dos
