/***************************************************************************
 *   Copyright (C) 2003,2005 by Frank S. Thomas                            *
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
 *   59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.             *
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
        System.Console.WriteLine("Textual representation of dataArray");
        System.Console.WriteLine("Size: " + dataArray.Count);
        foreach (Hashtable dataEntry in this.dataArray)
        {
            foreach (DictionaryEntry field in dataEntry)
            {
                System.Console.Write(field.Key +"="+ field.Value + " ");
            }
            System.Console.WriteLine();
        }
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
        dataEntry["LogFormat"] = LogFormat.GPRMC;
        
        return true;
    }
}

public class LogOziExplorer: LogBase
{
    public override void ParseData(string dataString)
    {}

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
        
        // The following fields are generic along all LogFormats.
        dataEntry["LogFormat"] = LogFormat.OziExplorer;
        
        return true;
    }
}
