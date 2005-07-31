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

public class Filename
{
    private string filename = "";
    private string extension = "";
    
    public string Filename
    {
        get {
            return String.Format("{0}{1}", this.filename, this.extension);
        }
        
        set {
            int lastDot = value.LastIndexOf(".");
            if (lastDot == -1)
                this.filename = value;
            else {
                int extLength = value.Length - lastDot;
                int fnLength = value.Length - extLength;
            
                this.filename = value.Substring(0, fnLength);
                this.extension = value.Substring(lastDot, extLength);
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

    public void ResetFilename()
    {
        this.filename = "";
    }

    public bool IsEmpty()
    {
        return this.filename == "";
    }

    public static string GetExtension()
    {
        
    }
}
