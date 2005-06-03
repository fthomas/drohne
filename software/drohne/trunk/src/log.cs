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
    {}

    public void ReadFile(string[] filenames)
    {
        foreach (string filename in filenames)
            this.ReadFile(filename);
    }
    
    public void WriteFile(string filename)
    {}
}

public enum LogFormat
{
    Unknown = 0,
    GPRMC = 1,
    OziExplorer = 2
}

public class LogWrapper: LogBase
{
    private LogBase logInstance = null;
    private LogFormat format = LogFormat.Unknown;
    
    public override void ParseData(string dataString)
    {
        if (logInstance == null){
            this.SpecifyLogFormat(ref dataString);

            switch (format)
            {
                case LogFormat.GPRMC:
                    logInstance = new LogGPRMC();
                    break;
                case LogFormat.OziExplorer:
                    break;
                case LogFormat.Unknown:
                    break;
            }
        }
    }

    private void SpecifyLogFormat(ref string dataString)
    {}
}

public class LogGPRMC: LogBase
{}

public class LogOziExplorer: LogBase
{}
