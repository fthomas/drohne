/* $Id$ */

using System;

public class Drohne
{
    public static void Main(string[] args)
    {
        SetCultureInfo();
	
        new GUI(args);
    }

    public delegate string DelegatedGettextMethod(string str);
    
    public static string i18n(string str){return str;}
    
    private static void SetCultureInfo() 
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
        
        //GettextResourceManager catalog = new GettextResourceManager("i18n");
        //
        //DelegatedGettextMethod i18n = 
        //    new DelegatedGettextMethod(catalog.GetString);
    }
}
