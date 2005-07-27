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
    
    private LogWrapper totalLog = null;
    private LogWrapper resultLog = null;

    private ListStore slicesStore = null;
    private ArrayList slicesArray = null;

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
        this.resultLog.WriteFile(this.saveFilename, false);
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
        this.totalLog = new LogWrapper();
        string[] selections = this.fileOpenDialog.Selections;

        int count = 0;
        bool validLog = false;
        
        foreach (string fn in selections)
        {
            if (File.GetAttributes(fn) == FileAttributes.Directory)
                continue;
            
            try {
                LogWrapper tmpLogWrapper = new LogWrapper();
                tmpLogWrapper.ReadFile(fn);

                if (this.totalLog.Format == LogFormat.Unknown)
                    this.totalLog = tmpLogWrapper;
                else
                    this.totalLog.Append(tmpLogWrapper);
 
                if (tmpLogWrapper.Format != this.totalLog.Format)
                    this.ShowDifferenLogFormatDialog(fn);
                
                count++;
                validLog = true;
                
            }
            catch (ApplicationException e)
            {
                this.ShowUnknownLogFormatDialog(fn);
            }
        }

        // Sync the appended logs with this.totalLog's logInstance.
        this.totalLog.ReverseSync();

        if (validLog == false)
            return;

        string statusStr = String.Format("{0}: {1}, {2}: {3}",
                Drohne.i18n("Loaded Files"), count,
                Drohne.i18n("Log Format"), this.totalLog.Format);
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
    
    /************************************
     * begin additional (popup) dialogs *
     ************************************/

    private void ShowDifferenLogFormatDialog(string filename)
    {
        string warning = String.Format("{0}:\r\n\"{1}\"",
                Drohne.i18n("Different log format detected"), filename);
                    
        MessageDialog dialog = new MessageDialog(this.fileOpenDialog,
                DialogFlags.DestroyWithParent, MessageType.Warning,
                ButtonsType.Close, warning);

        dialog.Run();
        dialog.Destroy();
    }

    private void ShowUnknownLogFormatDialog(string filename)
    {
        string errorMessage = String.Format("{0}:\r\n\"{1}\"",
                Drohne.i18n("Unknown log format"), filename);
                
        MessageDialog dialog = new MessageDialog(this.fileOpenDialog,
                DialogFlags.DestroyWithParent, MessageType.Error,
                ButtonsType.Close, errorMessage);
                
        dialog.Run(); 
        dialog.Destroy();
    }

    /**********************************
     * end additional (popup) dialogs *
     **********************************/

    private string GetSaveFilename()
    {
        string filename = "drohne.txt";
        if (this.totalLog == null)
            return filename;

        filename = String.Format("drohne_{0}-{1}.txt",
                this.totalLog.start.ToString("yyyyMMdd"),
                this.totalLog.end.ToString("yyyyMMdd"));

        return filename;    
    }

    private void SetupSlicesTreeView()
    {
        this.slicesStore = new ListStore(typeof(int),
                typeof(bool), typeof(string), typeof(string));
                
        this.slicesTreeView.Model = slicesStore;
         
        CellRendererToggle crt = new CellRendererToggle(); 
        crt.Activatable = true;
        crt.Toggled += CellRendererToggleToggled;           
         
        this.slicesTreeView.AppendColumn(Drohne.i18n("Select"),
                crt, "active", 1);

        this.slicesTreeView.AppendColumn(Drohne.i18n("Log Start"),
                new CellRendererText(), "text", 2);
        
        this.slicesTreeView.AppendColumn(Drohne.i18n("Log End"),
                new CellRendererText(), "text", 3);
    }
    
    private void CellRendererToggleToggled(object obj, ToggledArgs args)
    {
        TreeIter iter;
        if (this.slicesStore.GetIter(out iter, new TreePath(args.Path)))
        {
            bool old = (bool) this.slicesStore.GetValue(iter, 1);
            this.slicesStore.SetValue(iter, 1, !old);
        }
    }

    private void PopulateSlicesTreeView()
    {
        int count = 0;
        this.slicesStore.Clear();
        
        TimeSpan breakTime = new TimeSpan(24, 0, 0);
        this.slicesArray = this.totalLog.SplitByBreak(breakTime);
        
        foreach (LogBase slice in this.slicesArray)
        {
            this.slicesStore.AppendValues(count++, true,
                    slice.start.ToString(), slice.end.ToString());
        }
    }

    private void GetSelectedSlices()
    {
        if (this.totalLog == null)
            return;

        this.resultLog = new LogWrapper();

        // Iterate over the ListStore and append to this.result all slices
        // that are marked as selected.
        for (int i = 0; i < this.slicesArray.Count; i++)
        {
            TreeIter iter;
            TreePath path = new TreePath(String.Format("{0}", i));
            
            if (this.slicesStore.GetIter(out iter, path))
            {
                bool selected = (bool) this.slicesStore.GetValue(iter, 1);
                if (selected)
                {
                    this.resultLog.Append((LogBase) this.slicesArray[i]);
                }
            }
        }
                
        this.resultLog.CreateLogInstance(this.totalLog.Format);
        this.resultLog.ReverseSync();
    }
}
