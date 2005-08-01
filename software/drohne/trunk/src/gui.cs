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
    
    private LogBase totalLog = null;
    private LogBase resultLog = null;

    private ListStore slicesStore = null;
    private ArrayList slicesArray = null;

    private FilenameHelper saveFilename = new FilenameHelper();
    
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

    /**********************************
     * signal handlers for mainWindow *
     **********************************/
    public void OnMainWindowDeleteEvent(object obj, DeleteEventArgs args)
    {
        Application.Quit();
        args.RetVal = true;
    }

    /*******************************
     * signal handlers for menubar *
     *******************************/
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
        if (this.saveFilename.IsEmpty())
        {
            this.OnMenuFileSaveAsActivate(obj, args);
            return;
        }
       
        if (this.totalLog == null)
        {
            // this.ShowCannotSaveFileDialog
            
            this.fileSaveDialog.Filename = "";
            this.saveFilename.ResetName();
            
            return;
        }
        
        this.GetSelectedSlices();
        this.resultLog.WriteFile(this.saveFilename.Filename, false);

        string status = String.Format("{0}: \"{1}\", {2}: {3}",
                Drohne.i18n("File Saved"), this.saveFilename.Filename,
                Drohne.i18n("Total Entries"), this.resultLog.dataArray.Count);

        this.statusbar.Push(1, status);
    }

    public void OnMenuFileSaveAsActivate(object obj, EventArgs args)
    {
        if (this.saveFilename.IsEmpty())
            this.saveFilename.Filename = "drohne_log";
        
        this.fileSaveDialog.Filename = this.saveFilename.Filename;

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
    
    public void OnMenuEditPreferencesActivate(object obj, EventArgs args)
    {
    }
    
    public void OnMenuHelpAboutActivate(object obj, EventArgs args)
    {
        this.aboutDialog.Show();
    }
    
    /**************************************
     * signal handlers for fileSaveDialog * 
     **************************************/
    public void OnFileSaveDialogOkButtonClicked(object obj, EventArgs args)
    {
        string fn = this.fileSaveDialog.Filename;
        if (Directory.Exists(fn))
            return;

        this.saveFilename.Filename = fn;
        
        this.fileSaveDialog.Hide();

        this.OnMenuFileSaveActivate(obj, args);
    }
   
    public void OnFileSaveDialogCancelButtonClicked(object obj, EventArgs args)
    {
        this.fileSaveDialog.Hide();

        this.fileSaveDialog.Filename = "";
        this.saveFilename.ResetName();
    }

    /**************************************
     * signal handlers for fileOpenDialog *
     **************************************/
    public void OnFileOpenDialogOkButtonClicked(object obj, EventArgs args)
    {
        string[] selections = this.fileOpenDialog.Selections;
        LogFormat format = LogFormat.Unknown;
        ArrayList logFiles = new ArrayList();
        int count = 0;
        
        foreach (string fn in selections)
        {
            if (Directory.Exists(fn))
                continue;

            format = LogBase.DetectLogFormatFromFile(fn);

            if (format == LogFormat.Unknown)
            {
                this.ShowUnknownLogFormatDialog(fn);
                continue;
            }
	    
            Hashtable logFileInfo = new Hashtable();        
            logFileInfo["Filename"] = fn;
            logFileInfo["Format"] = format;
            
            logFiles.Add(logFileInfo);
            count++;
        }
        
        if (count == 0)
            return;
        
        string status = String.Format("{0}: {1}",
                Drohne.i18n("Selected Files"), count);

        this.statusbar.Push(1, status);
        
        this.fileOpenDialog.Hide();

        this.LoadSelectedLogFiles(logFiles);
    }

    public void OnFileOpenDialogCancelButtonClicked(object obj, EventArgs args)
    {
        this.fileOpenDialog.Hide();
    }

    /**********************
     * additional dialogs *
     **********************/
    private void ShowDifferentLogFormatDialog(string filename)
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

    /******************************
     * methods for slicesTreeView *
     ******************************/
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

    private void LoadSelectedLogFiles(ArrayList logFiles)
    {
        int count = 0;
        string status = "";
        this.totalLog = null;
        
        foreach (Hashtable logFileInfo in logFiles)
        {
            string fn = (String) logFileInfo["Filename"];
            LogFormat format = (LogFormat) logFileInfo["Format"];

            if (this.totalLog == null)
                this.totalLog = LogBase.CreateLogInstanceFromFile(fn, format);
            else
                this.totalLog.Append(LogBase.CreateLogInstanceFromFile(fn,
                            format));

            if (format != this.totalLog.Format)
                this.ShowDifferentLogFormatDialog(fn);

            status = String.Format("{0}: \"{1}\"",
                    Drohne.i18n("Loaded File"), fn);

            this.statusbar.Push(1, status);

            if (count++ == 0)
                this.saveFilename.Extension = FilenameHelper.GetExtension(fn);
        }

        status = String.Format("{0}: {1}, {2}: {3}",
                Drohne.i18n("Files Loaded"), count,
                Drohne.i18n("Log Format"), this.totalLog.Format);
        
        this.statusbar.Push(1, status);
        
        this.PopulateSlicesTreeView();
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
        
        this.resultLog = LogBase.CreateLogInstance(this.totalLog.Format);
        
        for (int i = 0; i < this.slicesArray.Count; i++)
        {
            TreeIter iter;
            TreePath path = new TreePath(String.Format("{0}", i));
            
            if (this.slicesStore.GetIter(out iter, path))
            {
                bool selected = (bool) this.slicesStore.GetValue(iter, 1);
                if (selected)
                    this.resultLog.Append((LogBase) this.slicesArray[i]);
            }
        }
    }
}
