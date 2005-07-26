/***************************************************************************
 *   Copyright (C) 2003,2005 Frank S. Thomas                               *
 *                           <frank@thomas-alfeld.de>                      *
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
using System.IO;
using System.Text.RegularExpressions;

public class LogBase
{
    public ArrayList dataArray = new ArrayList();
    public DateTime start = new DateTime(0);
    public DateTime end = new DateTime(0);
    
    public LogBase(){}
    
    public LogBase(ArrayList dataArray, DateTime start, DateTime end)
    {
        this.dataArray = dataArray;
        this.start = start;
        this.end = end;
    }
    
    public virtual void ParseData(string dataString){} 
    
    public void ReadFile(string filename)
    {
        using (StreamReader sr = new StreamReader(filename))
        {
            string fileContent = sr.ReadToEnd();
            this.ParseData(fileContent);
        }
    }

    public void ReadFile(string[] filenames)
    {
        foreach (string filename in filenames)
            this.ReadFile(filename);
    }
    
    public void WriteFile(string filename)
    {}

    public void WriteData()
    {
        string header = "--- Textual representation of dataArray ---\r\n"
                      + "dataArray.Count: " + dataArray.Count + "\r\n"
                      + "start: " + this.start + "\r\n" 
                      + "end: " + this.end + "\r\n";
        System.Console.Write(header);
        
        foreach (Hashtable dataEntry in this.dataArray)
        {
            foreach (DictionaryEntry field in dataEntry)
            {
                System.Console.Write(field.Key +"="+ field.Value +" ");
            }
            System.Console.WriteLine();
        }
    }

    public void UpdateLogStartEnd(DateTime dateTime)
    {
        if (this.start > dateTime)
            this.start = dateTime;
        
        if (this.end < dateTime)
            this.end = dateTime;
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
}

public enum LogFormat
{
    Unknown, GPRMC, OziExplorer
}

public class LogWrapper: LogBase
{
    private LogBase logInstance = null;
    private LogFormat format = LogFormat.Unknown;

    public LogFormat Format
    {
        get { return format; }
    }
    
    public override void ParseData(string dataString)
    {
        this.CreateLogInstance(ref dataString);
        this.logInstance.ParseData(dataString);
        //this.logInstance.SortByDateTime();
        this.SyncLogWrapper();
    }

    private void CreateLogInstance(ref string dataString)
    {
        if (this.logInstance != null)
            return;
            
        this.SpecifyLogFormat(ref dataString);
        
        switch (this.format)
        {
            case LogFormat.GPRMC:
                this.logInstance = new LogGPRMC();
                break;
            case LogFormat.OziExplorer:
                this.logInstance = new LogOziExplorer();
                break;
            case LogFormat.Unknown:
                throw new ApplicationException("Unknown LogFormat");
        }
    }
    
    private void SpecifyLogFormat(ref string dataString)
    {
        Hashtable dummyEntry = new Hashtable();
        string[] lines = dataString.Split('\n');
        
        foreach (string line in lines)
        {
            if (LogGPRMC.IsLogEntry(line, ref dummyEntry))
            {
                this.format = LogFormat.GPRMC;
                break;
            }

            if (LogOziExplorer.IsLogEntry(line, ref dummyEntry))
            {
                this.format = LogFormat.OziExplorer;
                break;
            }
        }
    }

    private void SyncLogWrapper()
    {
        this.dataArray = this.logInstance.dataArray;
        this.start = this.logInstance.start;
        this.end = this.logInstance.end;
    }
}

public class LogGPRMC: LogBase
{ 
    public override void ParseData(string dataString)
    {
        string[] lines = dataString.Split('\n');
        foreach (string line in lines)
        {
            Hashtable dataEntry = new Hashtable();
            if (! LogGPRMC.IsLogEntry(line, ref dataEntry))
                continue;
            
            this.dataArray.Add(dataEntry);

            // Use DateTime of first entry for the time boundary.
            if (dataArray.Count == 1)
            {
                this.start = (DateTime) dataEntry["GenDateTime"];
                this.end = (DateTime) dataEntry["GenDateTime"];
            }
            // And update the time boundary with every entry.
            this.UpdateLogStartEnd((DateTime) dataEntry["GenDateTime"]);
        }
    }

    public static bool IsLogEntry(string line, ref Hashtable dataEntry)
    {
        // This regex matches a single "GPRMC" line.
        string pattern = @"\$GPRMC,(\d{6}),(A|V),(\d{4}\.\d{0,4}),(N|S),"
            + @"(\d{5}\.\d{0,4}),(E|W),(\d+\.\d{0,2}),(\d+\.\d{0,2}),"
            + @"(\d{6}),(\d*\.?\d*),(E|W)?\*((\d|\w){0,2})";
        
        Match lineMatch = Regex.Match(line, pattern, RegexOptions.IgnoreCase);

        if (!lineMatch.Success)
            return false;
        
        // These fields are specific to LogFormat.GPRMC.
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
        
        // The following fields are generic along all LogFormats.
        dataEntry["GenLogFormat"] = LogFormat.GPRMC;
        dataEntry["GenDateTime"] = LogGPRMC.GetEntryDateTime(
                dataEntry["UTCTime"].ToString(),
                dataEntry["UTCDate"].ToString());
        
        return true;
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
        if (year > 65)
            year += 1900;
        else
            year += 2000;

        return new DateTime(year, month, day, hour, minute, second);
    }
}

public class LogOziExplorer: LogBase
{
    public override void ParseData(string dataString)
    {
        string[] lines = dataString.Split('\n');
        foreach (string line in lines)
        {
            Hashtable dataEntry = new Hashtable();
            if (! LogOziExplorer.IsLogEntry(line, ref dataEntry))
                continue;

            this.dataArray.Add(dataEntry);

            // Use DateTime of first entry for the time boundary.
            if (dataArray.Count == 1)
            {
                this.start = (DateTime) dataEntry["GenDateTime"];
                this.end = (DateTime) dataEntry["GenDateTime"];
            }
            // And update the time boundary with every entry.
            this.UpdateLogStartEnd((DateTime) dataEntry["GenDateTime"]);
        }
    }

    public static bool IsLogEntry(string line, ref Hashtable dataEntry)
    {
        // This regex matches a single "OziExplorer Track File" line.
        string pattern = @"(\-?\d{1,3}\.\d+),(\-?\d{1,3}\.\d+),(0|1),"
            + @"(\-?\d+),(\d+\.\d+),(\d{2}\-\D{3}\-\d{2}),(\d{2}:\d{2}:\d{2}),"
            + @"(\d{1,2})?,(\d\.\d)?,([23]{1}D)?";
        
        Match lineMatch = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
        
        if (!lineMatch.Success)
            return false;
       
        // These fields are specific to LogFormat.OziExplorer.
        dataEntry["Latitude"] = lineMatch.Groups[1].ToString();
        dataEntry["Longitude"] = lineMatch.Groups[2].ToString();
        dataEntry["Code"] = lineMatch.Groups[3].ToString();
        dataEntry["Altitude"] = lineMatch.Groups[4].ToString();
        dataEntry["Date"] = lineMatch.Groups[5].ToString();
        dataEntry["DateStr"] = lineMatch.Groups[6].ToString();
        dataEntry["TimeStr"] = lineMatch.Groups[7].ToString();
        
        // The following fields are generic along all LogFormats.
        dataEntry["GenLogFormat"] = LogFormat.OziExplorer;
        dataEntry["GenDateTime"] = LogOziExplorer.GetEntryDateTime(
                Double.Parse(dataEntry["Date"].ToString()));
            
        return true;
    }

    public static DateTime GetEntryDateTime(double days)
    {
        DateTime dateTime = new DateTime(1899, 12, 30, 0 ,0 ,0);
        dateTime = dateTime.AddDays(days);

        return dateTime;
    }
}
