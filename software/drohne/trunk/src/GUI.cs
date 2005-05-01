using System;
using Gtk;
using Glade;

public class GUI
{
    public GUI(string[] args)
    {
	Application.Init();
	
	Glade.XML gxml = new Glade.XML(null, "gui.glade", "window1", null);
	gxml.Autoconnect(this);
	Application.Run();
    }
    
}
