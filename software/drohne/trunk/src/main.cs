/***************************************************************************
 *   Copyright (C) 2005 Frank S. Thomas                                    *
 *                      <frank@thomas-alfeld.de>                           *
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

public class Drohne
{
    public static void Main(string[] args)
    {
        Drohne.SetCultureInfo();

        Drohne.TestBench();
        
        //new GUI(args);
    }

    public static string i18n(string str){ return str; }
    
    private static void SetCultureInfo() 
    {
        String locale = System.Environment.GetEnvironmentVariable("LC_ALL");
        
        if (locale == null || locale == "")
            locale = System.Environment.GetEnvironmentVariable("LANG");
    
        if (!(locale == null || locale == "")){
            if (locale.IndexOf('.') >= 0)
                locale = locale.Substring(0,locale.IndexOf('.'));
        
            System.Threading.Thread.CurrentThread.CurrentCulture = 
                System.Threading.Thread.CurrentThread.CurrentUICulture = 
                new System.Globalization.CultureInfo(locale.Replace('_','-'));
        }
    }

    private static void TestBench()
    {
        LogWrapper log_gprmc = new LogWrapper();
        LogWrapper log_oziexp = new LogWrapper();

        log_gprmc.ReadFile("doc/examples/GPRMC.txt");
        log_oziexp.ReadFile("doc/examples/OziExplorer.txt");

        Console.WriteLine("GPRMC.txt format:\t{0}", log_gprmc.Format);
        Console.WriteLine("OziExplorer.txt format:\t{0}", log_oziexp.Format);
    
        log_gprmc.WriteData();
        log_oziexp.WriteData();

        DateTime begin = new DateTime(2003,05,27, 16,20,00);
        DateTime end   = new DateTime(2003,05,27, 16,21,00);
        LogBase gprmc_slice = log_gprmc.GetSlice(begin, end);

        gprmc_slice.WriteData();
    }
}
