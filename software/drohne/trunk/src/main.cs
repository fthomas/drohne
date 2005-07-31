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
using System.Collections;

public class Drohne
{
    public static void Main(string[] args)
    {
        Drohne.SetCultureInfo();

        //Drohne.TestBench();
        
        new GUI(args);
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

    /*
    private static void TestBench()
    {
        LogWrapper log_gprmc = new LogWrapper();
        LogWrapper log_oziexp = new LogWrapper();

        log_gprmc.ReadFile("doc/examples/GPRMC.txt");
        log_oziexp.ReadFile("doc/examples/OziExplorer.txt");

        Console.WriteLine("GPRMC.txt format:\t{0}", log_gprmc.Format);
        Console.WriteLine("OziExplorer.txt format:\t{0}", log_oziexp.Format);
    
        Console.WriteLine(log_gprmc);
        Console.WriteLine(log_oziexp);

        DateTime begin = new DateTime(2003,05,27, 16,20,00);
        DateTime end   = new DateTime(2003,05,27, 16,21,00);
        LogBase gprmc_slice = log_gprmc.GetSlice(begin, end);

        Console.WriteLine(gprmc_slice);

        log_gprmc.WriteFile("doc/examples/GPRMC.out.txt", false);
        log_oziexp.WriteFile("doc/examples/OziExplorer.out.txt", false);
    
        TimeSpan breakTime = new TimeSpan(12,0,0);
        
        ArrayList gprmc_slices = log_gprmc.SplitByBreak(breakTime);
        foreach (LogBase slice in gprmc_slices)
            Console.WriteLine(slice);

        ArrayList oziexp_slices = log_oziexp.SplitByBreak(breakTime);
        foreach (LogBase slice in oziexp_slices)
            Console.WriteLine(slice);
    }
    */
}
