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
    private ArrayList dataArray = new ArrayList();
    private DateTime start = new DateTime(0);
    private DateTime end = new DateTime(0);
    
    public DateTime Start
    {
        get { return start; }
    }
    
    public DateTime End
    { 
        get { return end; } 
    }
    
    public LogBase() {}
    
    public LogBase(ArrayList dataArray, DateTime start, DateTime end)
    {
        this.dataArray = dataArray;
        this.start = start;
        this.end = end;
    }
    
    public LogBase(string dataString)
    {
        string[] lines = dataString.Split('\n');
        
        foreach(string line in lines)
            this.dataArray.Add(line);
    }
    
    public LogBase(string[] filenames)
    {
        foreach(string filename in filenames)
            this.ReadFile(filename);
    }

    public virtual void ParseData(){} 
    
    public bool ReadFile(string filename)
    {
        this.ParseData();
        return true;
    }
    
    public bool WriteFile(string filename)
    {
        return true;
    }
}

public class LogWrapper: LogBase
{
}

public class LogGPRMC: LogBase
{
}

public class LogOziExplorer: LogBase
{
}
