/***************************************************************************
 *   Copyright (C) 2005 Frank S. Thomas                                    *
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

public class FilenameHelper
{
    private string name = "";
    private string extension = "";
    
    public string Filename
    {
        get {
            return String.Format("{0}{1}", this.name, this.extension);
        }
        
        set {
            int lastDot = value.LastIndexOf(".");
            if (lastDot == -1)
                this.name = value;
            else
            {
                int extensionLength = value.Length - lastDot;
                
                this.name = value.Substring(0, lastDot);
                this.extension = value.Substring(lastDot, extensionLength);
            }
        }
    }

    public string Extension
    {
        get {
            return this.extension;
        }

        set {
            this.extension = value;
        }
    }

    public FilenameHelper() {}
    
    public FilenameHelper(string name, string extension)
    {
        this.name = name;
        this.extension = extension;
    }
    
    public void ResetName()
    {
        this.name = "";
    }

    public bool IsEmpty()
    {
        return this.name == "";
    }

    public static string GetExtension(string filename)
    {
        string extension = "";
        int lastDot = filename.LastIndexOf(".");
        
        if (lastDot != -1)
        {
            int extensionLength = filename.Length - lastDot;
            extension = filename.Substring(lastDot, extensionLength);
        }
        return extension;
    }
}
