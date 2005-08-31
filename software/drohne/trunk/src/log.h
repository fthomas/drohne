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

#ifndef LOG_H
#define LOG_H

enum LogFormat { Unknown, GPRMC, OziExplorer };

class LogBase 
{
    public:
        LogBase();
        LogBase(ArrayList dataArray, DateTime start, DateTime end);

    public ArrayList dataArray = new ArrayList();
    public DateTime start = new DateTime(0);
    public DateTime end = new DateTime(0);
    private LogFormat format = LogFormat.Unknown;
    public virtual LogFormat Format { get { return format; } }
    public virtual void ParseStringData(string dataString)
    public void Clear()
    public void Append(LogBase newLog)
    public void UpdateLogStartEnd(DateTime dateTime)
    public void SortByDateTime()
    public LogBase GetSlice(DateTime sliceBegin, DateTime sliceEnd)
    public ArrayList SplitByBreak(TimeSpan breakTime)
    public void ReadFile(string filename)
    public void ReadFile(string[] filenames)
    public void WriteFile(string filename, bool append)
    public virtual string EntryToString(Hashtable dataEntry)
    public override string ToString()
    public static LogFormat DetectLogFormatFromFile(string filename)
    public static LogFormat DetectLogFormatFromString(ref string dataString)
    public static LogBase CreateLogInstance(LogFormat format)
    public static LogBase CreateLogInstanceFromFile(string filename, LogFormat format)
};

class LogGPRMC : public LogBase
{
    private LogFormat format = LogFormat.GPRMC;
    public override LogFormat Format { get { return format; } }
        
    public override void ParseStringData(string dataString)
    public static bool IsLogEntry(string line, ref Hashtable dataEntry)
    public override string EntryToString(Hashtable dataEntry)
    public static DateTime GetEntryDateTime(string utcTime, string utcDate)
};

class LogOziExplorer : public LogBase
{
    private LogFormat format = LogFormat.OziExplorer;
    public override LogFormat Format { get { return format; } }
    public override void ParseStringData(string dataString)
    public static bool IsLogEntry(string line, ref Hashtable dataEntry)
    public override string EntryToString(Hashtable dataEntry)
    public static DateTime GetEntryDateTime(double days)
};

#endif
