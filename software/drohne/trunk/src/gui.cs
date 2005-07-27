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
using System.Collections;
using System.IO;
using Gtk;
using Glade;

public class GUI
{
    // Widgets from mainWindowGlade
    [Glade.Widget] Gtk.Statusbar statusbar;
    [Glade.Widget] Gtk.TreeView slicesTreeView;
    
    // Widgets from aboutDialogGlade
    [Glade.Widget] Gtk.Dialog aboutDialog;

    // Widgets from fileSaveDialogGlade
    [Glade.Widget] Gtk.FileSelection fileSaveDialog;
        
    // Widgets from fileOpenDialogGlade
    [Glade.Widget] Gtk.FileSelection fileOpenDialog;
    
    private LogWrapper logWrapper;
    private ListStore slicesStore = null;
    
    private ArrayList slicesArray;
    private LogWrapper result = new LogWrapper();
    private string saveFilename = "";
    
    public GUI(string[] args)
    {
        Application.Init();

        Glade.XML mainWindowGlade = new Glade.XML(null, "gui.glade",
                "mainWindow", null);
        mainWindowGlade.Autoconnect(this);

        Glade.XML aboutDialogGlade = new Glade.XML(null, "gui.glade",
                "aboutDialog", null);
        aboutDialogGlade.Autoconnect(this);

        Glade.XML fileSaveDialogGlade = new Glade.XML(null, "gui.glade",
                "fileSaveDialog", null);
        fileSaveDialogGlade.Autoconnect(this);

        Glade.XML fileOpenDialogGlade = new Glade.XML(null, "gui.glade",
                "fileOpenDialog", null);
        fileOpenDialogGlade.Autoconnect(this);
       
        this.fileOpenDialog.SelectMultiple = true;
        this.SetupSlicesTreeView();
        
        Application.Run();
    }

    /*************************
     * begin signal handlers *
     *************************/

    /*
     * signal handlers for mainWindow
     */
    public void OnMainWindowDeleteEvent(object obj, DeleteEventArgs args)
    {
        Application.Quit();
        args.RetVal = true;
    }

    /*
     * signal handlers for menubar
     */
    public void OnMenuFileOpenActivate(object obj, EventArgs args)
    {
        if ((ResponseType) this.fileOpenDialog.Run() != ResponseType.Ok)
        {
            this.fileOpenDialog.Hide();
            return;
        }
    }

    public void OnMenuFileSaveActivate(object obj, EventArgs args)
    {
        if (this.saveFilename == "")
        {
            this.OnMenuFileSaveAsActivate(obj, args);
            return;
        }
        
        this.GetSelectedSlices();
        this.result.WriteFile(this.saveFilename, false);
    }

    public void OnMenuFileSaveAsActivate(object obj, EventArgs args)
    {
        this.fileSaveDialog.Filename = this.GetSaveFilename();

        if ((ResponseType) this.fileSaveDialog.Run() != ResponseType.Ok)
        {
            this.fileSaveDialog.Hide();
            return;
        }
    }

    public void OnMenuFileQuitActivate(object obj, EventArgs args)
    {
        Application.Quit();
    }

    public void OnMenuHelpAboutActivate(object obj, EventArgs args)
    {
        this.aboutDialog.Show();
    }
    
    /*
     * signal handlers for fileSaveDialog   
     */
    public void OnFileSaveDialogOkButtonClicked(object obj, EventArgs args)
    {
        this.saveFilename = this.fileSaveDialog.Filename;
        this.fileSaveDialog.Hide();

        this.OnMenuFileSaveActivate(obj, args);
    }
   
    public void OnFileSaveDialogCancelButtonClicked(object obj, EventArgs args)
    {
        this.fileSaveDialog.Hide();
        this.saveFilename = "";
    }

    /*
     * signal handlers for fileOpenDialog
     */
    public void OnFileOpenDialogOkButtonClicked(object obj, EventArgs args)
    {
        this.logWrapper = new LogWrapper();
        string[] selections = this.fileOpenDialog.Selections;

        bool validLog = false;
        int count = 0;
        
        foreach (string fn in selections)
        {
            if (File.GetAttributes(fn) == FileAttributes.Directory)
                continue;
            
            try {
                LogWrapper tempLogWrapper = new LogWrapper();
                tempLogWrapper.ReadFile(fn);

                // Use the first detected LogBase as this.logWrapper. All
                // later detected LogBases are appended to this.logWrapper.
                if (this.logWrapper.Format == LogFormat.Unknown)
                    this.logWrapper = tempLogWrapper;
                else
                    this.logWrapper.Append(tempLogWrapper);
 
                // Check if tempLogWrapper.Format is not identical with
                // this.logWrapper.Format.
                if (tempLogWrapper.Format != this.logWrapper.Format)
                {
                    string warning = String.Format("{0}:\r\n\"{1}\"",
                            Drohne.i18n("Different log format detected"), fn);
                    
                    MessageDialog dialog = new MessageDialog(
                            this.fileOpenDialog,
                            DialogFlags.DestroyWithParent,
                            MessageType.Warning, ButtonsType.Close, warning);

                    dialog.Run();
                    dialog.Destroy();
                } 
                
                validLog = true;
                count++;
            }
            catch (ApplicationException e)
            {
                string errorMessage = String.Format(
                        "{0}:\r\n\"{1}\"", 
                        Drohne.i18n("Unknown log format"), fn);
                
                MessageDialog dialog = new MessageDialog(
                        this.fileOpenDialog, DialogFlags.DestroyWithParent,
                        MessageType.Error, ButtonsType.Close, errorMessage);
                
                dialog.Run(); 
                dialog.Destroy();
            }
        }

        // If there was no valid log file in the selections array, the
        // FileSelection dialog should stay opened.
        if (validLog == false)
            return;

        string statusStr = String.Format("{0}: {1}, {2}: {3}",
                Drohne.i18n("Loaded Files"), count,
                Drohne.i18n("Log Format"), this.logWrapper.Format);
        
        this.statusbar.Push(1, statusStr);
        
        this.fileOpenDialog.Hide();

        this.saveFilename = "";

        this.PopulateSlicesTreeView();
    }

    public void OnFileOpenDialogCancelButtonClicked(object obj, EventArgs args)
    {
        this.fileOpenDialog.Hide();
    }

    /***********************
     * end signal handlers *
     ***********************/
    
    private string GetSaveFilename()
    {
        string filename = "drohne.txt";
        if (this.logWrapper == null)
            return filename;

        filename = String.Format("drohne_{0}-{1}.txt",
                this.logWrapper.start.ToString("yyyyMMdd"),
                this.logWrapper.end.ToString("yyyyMMdd"));

        return filename;    
    }

    private void SetupSlicesTreeView()
    {
        this.slicesStore = new ListStore(typeof(bool),
                typeof(string), typeof(string));
                
        this.slicesTreeView.Model = slicesStore;
         
        CellRendererToggle crt = new CellRendererToggle(); 
        crt.Activatable = true;
        crt.Toggled += CellRendererToggleToggled;           
         
        this.slicesTreeView.AppendColumn(Drohne.i18n("Select"),
                crt, "active", 0);

        this.slicesTreeView.AppendColumn(Drohne.i18n("Log Start"),
                new CellRendererText(), "text", 1);
        
        this.slicesTreeView.AppendColumn(Drohne.i18n("Log End"),
                new CellRendererText(), "text", 2);
    }
    
    private void CellRendererToggleToggled(object obj, ToggledArgs args)
    {
        TreeIter iter;
        if (this.slicesStore.GetIter(out iter, new TreePath(args.Path)))
        {
            bool old = (bool) this.slicesStore.GetValue(iter, 0);
            this.slicesStore.SetValue(iter, 0, !old);
        }
    }

    private void PopulateSlicesTreeView()
    {
        this.slicesStore.Clear();
        
        TimeSpan breakTime = new TimeSpan(24, 0, 0);
        this.slicesArray = this.logWrapper.SplitByBreak(breakTime);
        
        foreach (LogBase slice in this.slicesArray)
        {
            this.slicesStore.AppendValues(true, slice.start.ToString(),
                    slice.end.ToString());
        }
    }

    private void GetSelectedSlices()
    {
        this.result.Clear();
        this.slicesStore.Foreach(AppendSliceForeachSelected);
        
        if (this.logWrapper == null)
            return;
                
        this.result.CreateLogInstance(this.logWrapper.Format);
        this.result.ReverseSync();
    }
    
    private bool AppendSliceForeachSelected(TreeModel model, TreePath path,
            TreeIter iter)
    {   
        bool selected = (bool) this.slicesStore.GetValue(iter, 0);
        if (selected)
        {
            // *shudder* It's ugly but works.
            int row = Int16.Parse(path.ToString());
            this.result.Append((LogBase) this.slicesArray[row]);
        }
        // Return false to keep Foreach going.
        return false;
    }
}
