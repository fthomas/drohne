/***************************************************************************
 *   Copyright (C) 2003,2005 Frank S. Thomas                               *
 *   frank@thomas-alfeld.de                                                *
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the GNU General Public License as published by  *
 *   the Free Software Foundation; either version 2 of the License, or     *
 *   (at your option) any later version.                                   *
 *                                                                         *
 *   This program is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   GNU General Public License for more details.                          *
 *                                                                         *
 *   You should have received a copy of the GNU General Public License     *
 *   along with this program; if not, write to the                         *
 *   Free Software Foundation, Inc.,                                       *
 *   51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.         *
 ***************************************************************************/
/* $Id$ */

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

public enum LogFormat
{
    Unknown,
    GPRMC,
    OziExplorer
}

public class LogBase
{
    public ArrayList dataArray = new ArrayList();
    public DateTime start = new DateTime(0);
    public DateTime end = new DateTime(0);
    
    private LogFormat format = LogFormat.Unknown;
    public virtual LogFormat Format { get { return format; } }

    public LogBase(){}
    
    public LogBase(ArrayList dataArray, DateTime start, DateTime end)
    {
        this.dataArray = dataArray;
        this.start = start;
        this.end = end;
    }

    public virtual void ParseStringData(string dataString)
    {
        Console.WriteLine("Warning: calling virtual method {0}",
                "LogBase.ParseStringData()");
        return;
    }

    public void Clear()
    {
        this.dataArray.Clear();
        this.start = new DateTime(0);
        this.end = new DateTime(0);
    }

    public void Append(LogBase newLog)
    {
        if (newLog.end < this.start)
        {
            this.dataArray.InsertRange(0, newLog.dataArray);
            this.start = newLog.start;    
        }
        else if (newLog.start > this.end)
        {
            this.dataArray.AddRange(newLog.dataArray);
            this.end = newLog.end;
        }
        else
        {
            // TODO: sort this.dataArray after appending newLog.dataArray,
            // beause this.dataArray is then mixed up.
            this.dataArray.AddRange(newLog.dataArray);
            
            if (this.start > newLog.start)
                this.start = newLog.start;

            if (this.end < newLog.end)
                this.end = newLog.end;
        }
    }
 
    public void UpdateLogStartEnd(DateTime dateTime)
    {
        if (this.start > dateTime)
            this.start = dateTime;
        
        if (this.end < dateTime)
            this.end = dateTime;
    }
    
    public void SortByDateTime()
    {
    }

    public LogBase GetSlice(DateTime sliceBegin, DateTime sliceEnd)
    {
        ArrayList tmpArray = new ArrayList();
        foreach (Hashtable dataEntry in this.dataArray)
        {
            DateTime time = (DateTime) dataEntry["GenDateTime"];

            if (time < sliceBegin || time > sliceEnd)
                continue;

            tmpArray.Add(dataEntry);
        }
        
        return new LogBase(tmpArray, sliceBegin, sliceEnd);
    }
    
    public ArrayList SplitByBreak(TimeSpan breakTime)
    {
        ArrayList logSliceArray = new ArrayList();
            
        DateTime dayPlus0 = this.start;
        DateTime dayPlus1 = new DateTime(0);

        DateTime splitStart = this.start;
        DateTime splitEnd = new DateTime(0);
        
        bool first = true;
        int count = this.dataArray.Count;
        int i = 0;
        
        foreach (Hashtable dataEntry in this.dataArray)
        {
            i++;
            if (first)
            {
                first = false;
                continue;
            }
            
            dayPlus1 = (DateTime) dataEntry["GenDateTime"];

            if (dayPlus1 - dayPlus0 > breakTime)
            {
                splitEnd = dayPlus0;
                logSliceArray.Add(this.GetSlice(splitStart, splitEnd));
                splitStart = dayPlus1;
            }
            else if (i == count)
            {
                splitEnd = dayPlus1;
                logSliceArray.Add(this.GetSlice(splitStart, splitEnd));
            }

            dayPlus0 = (DateTime) dataEntry["GenDateTime"];
        }
        return logSliceArray;
    }

    public void ReadFile(string filename)
    {
        using (StreamReader sr = new StreamReader(filename))
        {
            string fileContent = sr.ReadToEnd();
            this.ParseStringData(fileContent);
        }
    }

    public void ReadFile(string[] filenames)
    {
        foreach (string filename in filenames)
            this.ReadFile(filename);
    }
    
    public void WriteFile(string filename, bool append)
    {
        if (append == false)
        {
            using (StreamWriter sw = File.CreateText(filename))
            {
                foreach (Hashtable dataEntry in this.dataArray)
                    sw.WriteLine(EntryToString(dataEntry));
            }
        }
        else
        {
            using (StreamWriter sw = File.AppendText(filename))
            {
                foreach (Hashtable dataEntry in this.dataArray)
                    sw.WriteLine(EntryToString(dataEntry));
            }
        }
    }

    public virtual string EntryToString(Hashtable dataEntry)
    {
        Console.WriteLine("Warning: calling virtual method {0}",
                "LogBase.EntryToString()");
        return "";
    }

    public override string ToString()
    {
        string str = String.Format("dataArray.Count: {0}\r\n"
                + "start: {1}\r\nend: {2}\r\n",
                dataArray.Count, this.start, this.end);
        
        foreach (Hashtable dataEntry in this.dataArray)
        {
            foreach (DictionaryEntry field in dataEntry)
            {
                str += String.Format("{0}={1} ", field.Key, field.Value);
            }
            str += "\r\n";
        }
        return str;
    }

    public static LogFormat DetectLogFormatFromFile(string filename)
    {
        using (StreamReader sr = new StreamReader(filename))
        {
            int c = 0, i = 0;
            const int maxChars = 32768;
            char[] buffer = new char[maxChars];
            
            while ((c = sr.Read()) != -1 && i < maxChars)
                buffer[i++] = (char) c;
                
            string partialFileContent = new String(buffer);
            return LogBase.DetectLogFormatFromString(ref partialFileContent);
        }
    }

    public static LogFormat DetectLogFormatFromString(ref string dataString)
    {
        Hashtable dummyEntry = new Hashtable();
        LogFormat format = LogFormat.Unknown;
        
        string[] lines = dataString.Split('\n');
        
        foreach (string line in lines)
        {   
            if (LogGPRMC.IsLogEntry(line, ref dummyEntry))
            {
                format = LogFormat.GPRMC;
                break;
            }

            if (LogOziExplorer.IsLogEntry(line, ref dummyEntry))
            {
                format = LogFormat.OziExplorer;
                break;
            }
        }

        return format;
    }

    public static LogBase CreateLogInstance(LogFormat format)
    {
        LogBase logInstance = null;
        switch (format)
        {
            case LogFormat.GPRMC:
                logInstance = new LogGPRMC();
                break;
            
            case LogFormat.OziExplorer:
                logInstance = new LogOziExplorer();
                break;
        }

        return logInstance;
    }
    
    public static LogBase CreateLogInstanceFromFile
        (string filename, LogFormat format)
    {
        LogBase logInstance = LogBase.CreateLogInstance(format);
        logInstance.ReadFile(filename);
        return logInstance;
    }
}

public class LogGPRMC: LogBase
{
    private LogFormat format = LogFormat.GPRMC;
    public override LogFormat Format { get { return format; } }
        
    public override void ParseStringData(string dataString)
    {
        string[] lines = dataString.Split('\n');
        foreach (string line in lines)
        {
            Hashtable dataEntry = new Hashtable();
            if (! LogGPRMC.IsLogEntry(line, ref dataEntry))
                continue;
            
            this.dataArray.Add(dataEntry);

            if (dataArray.Count == 1)
            {
                this.start = (DateTime) dataEntry["GenDateTime"];
                this.end = (DateTime) dataEntry["GenDateTime"];
            }
            this.UpdateLogStartEnd((DateTime) dataEntry["GenDateTime"]);
        }
    }

    public static bool IsLogEntry(string line, ref Hashtable dataEntry)
    {
        string pattern = @"\$GPRMC,(\d{6}),(A|V),(\d{4}\.\d{0,4}),(N|S),"
            + @"(\d{5}\.\d{0,4}),(E|W),(\d+\.\d{0,2}),(\d+\.\d{0,2}),"
            + @"(\d{6}),(\d*\.?\d*),(E|W)?\*((\d|\w){0,2})";
        
        Match lineMatch = Regex.Match(line, pattern, RegexOptions.IgnoreCase);

        if (!lineMatch.Success)
            return false;
        
        dataEntry["UTCTime"] = lineMatch.Groups[1].ToString();
        dataEntry["Status"] = lineMatch.Groups[2].ToString();
        dataEntry["Latitude"] = lineMatch.Groups[3].ToString();
        dataEntry["NSIndicator"] = lineMatch.Groups[4].ToString();
        dataEntry["Longitude"] = lineMatch.Groups[5].ToString();
        dataEntry["EWIndicator"] = lineMatch.Groups[6].ToString();
        dataEntry["Speed"] = lineMatch.Groups[7].ToString();
        dataEntry["Course"] = lineMatch.Groups[8].ToString();
        dataEntry["UTCDate"] = lineMatch.Groups[9].ToString();
        dataEntry["MagVariation"] = lineMatch.Groups[10].ToString();
        dataEntry["MagVarEWInd"] = lineMatch.Groups[11].ToString();
        dataEntry["Checksum"] = lineMatch.Groups[12].ToString();
        
        dataEntry["GenLogFormat"] = LogFormat.GPRMC;
        dataEntry["GenDateTime"] = LogGPRMC.GetEntryDateTime(
                dataEntry["UTCTime"].ToString(),
                dataEntry["UTCDate"].ToString());
        
        return true;
    }

    public override string EntryToString(Hashtable dataEntry)
    {
        return String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},"
                + "{10},{11}*{12}", "$GPRMC", dataEntry["UTCTime"],
                dataEntry["Status"],          dataEntry["Latitude"],
                dataEntry["NSIndicator"],     dataEntry["Longitude"],
                dataEntry["EWIndicator"],     dataEntry["Speed"],
                dataEntry["Course"],          dataEntry["UTCDate"],
                dataEntry["MagVariation"],    dataEntry["MagVarEWInd"],
                dataEntry["Checksum"]);
    }

    public static DateTime GetEntryDateTime(string utcTime, string utcDate)
    {
        int day = int.Parse(utcDate.Substring(0, 2));
        int month = int.Parse(utcDate.Substring(2, 2));
        int year = int.Parse(utcDate.Substring(4, 2));
        int hour = int.Parse(utcTime.Substring(0, 2));
        int minute = int.Parse(utcTime.Substring(2, 2));
        int second = int.Parse(utcTime.Substring(4, 2));

        // This code expires on 2066. ;-)
        year += (year > 65) ? 1900 : 2000;
        
        return new DateTime(year, month, day, hour, minute, second);
    }
}

public class LogOziExplorer: LogBase
{
    private LogFormat format = LogFormat.OziExplorer;
    public override LogFormat Format { get { return format; } }

    public override void ParseStringData(string dataString)
    {
        string[] lines = dataString.Split('\n');
        foreach (string line in lines)
        {
            Hashtable dataEntry = new Hashtable();
            if (! LogOziExplorer.IsLogEntry(line, ref dataEntry))
                continue;

            this.dataArray.Add(dataEntry);

            if (dataArray.Count == 1)
            {
                this.start = (DateTime) dataEntry["GenDateTime"];
                this.end = (DateTime) dataEntry["GenDateTime"];
            }
            this.UpdateLogStartEnd((DateTime) dataEntry["GenDateTime"]);
        }
    }

    public static bool IsLogEntry(string line, ref Hashtable dataEntry)
    {
        string pattern = @"(\-?\d{1,3}\.\d+),(\-?\d{1,3}\.\d+),(0|1),"
            + @"(\-?\d+),(\d+\.\d+),(\d{2}\-\D{3}\-\d{2}),"
            + @"(\d{2}:\d{2}:\d{2}),?(\d{1,2})?,?(\d\.\d)?,?([23]{1}D)?";
        
        Match lineMatch = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
        
        if (!lineMatch.Success)
            return false;
       
        dataEntry["Latitude"] = lineMatch.Groups[1].ToString();
        dataEntry["Longitude"] = lineMatch.Groups[2].ToString();
        dataEntry["Code"] = lineMatch.Groups[3].ToString();
        dataEntry["Altitude"] = lineMatch.Groups[4].ToString();
        dataEntry["Date"] = lineMatch.Groups[5].ToString();
        dataEntry["DateStr"] = lineMatch.Groups[6].ToString();
        dataEntry["TimeStr"] = lineMatch.Groups[7].ToString();
         
        NumberFormatInfo numberFormat = new NumberFormatInfo();
        numberFormat.NumberDecimalSeparator = ".";
        
        dataEntry["GenLogFormat"] = LogFormat.OziExplorer;
        dataEntry["GenDateTime"] = LogOziExplorer.GetEntryDateTime(
                Double.Parse(dataEntry["Date"].ToString(), numberFormat));
            
        return true;
    }

    public override string EntryToString(Hashtable dataEntry)
    {
        return String.Format("{0},{1},{2},{3},{4},{5},{6}",
                dataEntry["Latitude"], dataEntry["Longitude"],
                dataEntry["Code"],     dataEntry["Altitude"],
                dataEntry["Date"],     dataEntry["DateStr"],
                dataEntry["TimeStr"]);
    }

    public static DateTime GetEntryDateTime(double days)
    {
        DateTime dateTime = new DateTime(1899, 12, 30, 0 ,0 ,0);
        dateTime = dateTime.AddDays(days);
        
        return dateTime;
    }
}
