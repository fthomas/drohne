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
using Gtk;
using Glade;

public class GUI
{
    [Glade.Widget] Gtk.Window MainWindow; 
        
    public GUI(string[] args)
    {
        Application.Init();

        Glade.XML mainGlade = 
            new Glade.XML(null, "gui.glade", "MainWindow", null);	
        mainGlade.Autoconnect(this);

        Application.Run();
    }

    /* The following methods are handlers for the signals defined in Glade.
     * They are autoconnected with the signals by Glade.XML.Autoconnect().
     */ 
    public void OnMainWindowDeleteEvent(object obj, DeleteEventArgs args)
    {
        Application.Quit();
        args.RetVal = true;
    }
    
    public void OnMenuFileQuitActivate(object obj, EventArgs args)
    {
        Application.Quit();
    }

    public void OnMenuFileOpenActivate(object obj, EventArgs args)
    {
        FileSelection fs = new FileSelection(Drohne.i18n("Logdatei öffnen"));
        if ( (ResponseType) fs.Run() != ResponseType.Ok)
        {
            fs.Hide();
            return;
        }
    }
}
