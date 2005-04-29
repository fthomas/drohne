/* $Id$ */

using System;
using GNU.Gettext;

public class Drohne
{
    public delegate string DelegatedGettextMethod(string str);
    
    public static void Main()
    {
        String locale = System.Environment.GetEnvironmentVariable("LC_ALL");
        
        if (locale == null || locale == "")
            locale = System.Environment.GetEnvironmentVariable("LANG");
    
        if (!(locale == null || locale == "")) {
            if (locale.IndexOf('.') >= 0)
                locale = locale.Substring(0,locale.IndexOf('.'));
        
            System.Threading.Thread.CurrentThread.CurrentCulture = 
                System.Threading.Thread.CurrentThread.CurrentUICulture = 
                new System.Globalization.CultureInfo(locale.Replace('_','-'));
        }
        
        //GUI.RunLoop();

        GettextResourceManager catalog = new GettextResourceManager("i18n");

        DelegatedGettextMethod i18n = 
            new DelegatedGettextMethod(catalog.GetString);

        Console.WriteLine( catalog.GetString("hello") );
        Console.WriteLine( i18n("sleeping") );
    }
}
