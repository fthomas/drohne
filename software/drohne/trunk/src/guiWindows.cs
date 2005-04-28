using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Drohne
{
    ///
    public class GUI : Form
    {
        private int            progPosition = 0;
        private string         clipCont;
        private string[]       sourceFiles;
        private TextBox        sourceResult = new TextBox();
        private Form           aboutWindow  = new Form();
        private CheckedListBox filterList   = new CheckedListBox();
        private LogRMC         rmcInstance;
        private ArrayList      tourLogs;
        private DateTime       minDate      = new DateTime();
        private DateTime       maxDate      = new DateTime();
        private DateTimePicker dateBegin    = new DateTimePicker();
        private DateTimePicker dateEnd      = new DateTimePicker();
        private CheckedListBox tourSelect   = new CheckedListBox();
        
        ///
        public static void RunLoop()
        {
            Application.Run(new GUI());
        }
        
        ///
        public GUI()
        {
            // Set some properties for GUI.
            this.ClientSize = new Size(640, 480);
            this.Text       = i18n.progName;
            this.Font        = new Font("Arial", 11);
            
            try
            {
                // Add icon to GUI.
                this.Icon = new Icon("share/pixmaps/drohne_16_transp.ico");
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            this.DrawWelcomeForm();
        }

        ///
        public void DrawWelcomeForm()
        {
            // We are in WelcomeForm.
            this.progPosition = 0;
            
            // Clear all components from GUI.
            this.Controls.Clear();
            
            this.DrawFrame();

            TextBox welcomeText = this.DrawTextBox(0);
            
            welcomeText.Text = i18n.welcomeText;
                             
            
            // Add widgets to this form.
            Control[] widgets = new Control[]
                {welcomeText};

            this.Controls.AddRange(widgets);
        }

        ///
        public void DrawSourceSelect()
        {
            // Set new position in program flow.
            this.progPosition = 1;

            // Clear all components from GUI.
            this.Controls.Clear();
        
            this.DrawFrame();
            
            TextBox sourceText = this.DrawTextBox(128);
            sourceText.Text    = i18n.sourceText;
            
            Button selectFiles   = new Button();
            selectFiles.Text     = i18n.selectFiles;
            selectFiles.Size     = new Size(188, selectFiles.Height);
            selectFiles.Location = new Point(128 + 30, sourceText.Height + 20);
            selectFiles.Anchor   = AnchorStyles.Bottom | AnchorStyles.Left;
            selectFiles.Click   += new EventHandler(this.selectFilesEvent);
            
            Button readClipboard   = new Button();
            readClipboard.Text     = i18n.readClipboard;
            readClipboard.Size     = new Size(188, readClipboard.Height);
            readClipboard.Location = new Point(selectFiles.Left + selectFiles.Width + 5, sourceText.Height + 20);
            readClipboard.Anchor   = AnchorStyles.Bottom | AnchorStyles.Left;
            readClipboard.Click   += new EventHandler(this.GetClipboard);

            this.sourceResult.Location   = new Point(128 + 30, selectFiles.Top + selectFiles.Height + 10);
            this.sourceResult.Size       = new Size(this.ClientSize.Width - 168,
                    this.ClientSize.Height - this.sourceResult.Top - 84);
            this.sourceResult.ScrollBars = ScrollBars.Both;
            this.sourceResult.Multiline  = true;
            this.sourceResult.ReadOnly   = true;
            this.sourceResult.WordWrap   = false;
            this.sourceResult.Anchor     = AnchorStyles.Bottom | AnchorStyles.Left |
                                           AnchorStyles.Right;
            
            if (this.sourceResult.Text == "")
                this.sourceResult.Text = i18n.sourceInfoNone;
            
            // Add widgets to this form.
            Control[] widgets = new Control[]
                {sourceText, readClipboard, selectFiles,
                 this.sourceResult};

            this.Controls.AddRange(widgets);
        }

        ///
        public void DrawFilterSelect()
        {
            // We are now in the filter selection form.
            this.progPosition = 2;

            // Clear all components from GUI.
            this.Controls.Clear();
        
            this.DrawFrame();

            TextBox filterText = this.DrawTextBox(128);
            filterText.Text    = i18n.filterText;

            this.filterList.Location            = new Point(128 + 30, filterText.Height + 20);
            this.filterList.Size                = new Size(this.ClientSize.Width - 168,
                    this.ClientSize.Height - this.filterList.Top - 84);
            this.filterList.ScrollAlwaysVisible = true;
            this.filterList.CheckOnClick        = true;
            this.filterList.Anchor              = AnchorStyles.Right | AnchorStyles.Bottom |
                                                  AnchorStyles.Left;
            
            if (this.filterList.Items.Count == 0)
            {
                string[] filter = {i18n.filterInvalid, i18n.filterNullVal,
                                   i18n.filterIVCoord, i18n.filterChecksum,};
                this.filterList.Items.AddRange(filter);

                // Set default filter.
                this.filterList.SetItemChecked(0, true);
                this.filterList.SetItemChecked(1, true);
                this.filterList.SetItemChecked(2, true);
            }
            
            // Add widgets to this form.
            Control[] widgets = new Control[]
                {filterText, this.filterList};

            this.Controls.AddRange(widgets);
        }
        
        ///
        public void DrawSaveForm()
        {   
            // We are now in the save form.
            this.progPosition = 3;

            // Clear all components from GUI.
            this.Controls.Clear();
        
            this.DrawFrame();
            
            TextBox saveText = this.DrawTextBox(128);
            saveText.Text    = i18n.saveText;

            this.tourSelect          = new CheckedListBox();
            this.tourSelect.Location = new Point(128 + 30, saveText.Top + saveText.Height + 10);
            this.tourSelect.Size     = new Size(this.ClientSize.Width - 168 - 250,
                    this.ClientSize.Height - tourSelect.Top - 84);
            this.tourSelect.ScrollAlwaysVisible = true;
            this.tourSelect.CheckOnClick        = true;
            this.tourSelect.Items.Clear();
            this.tourSelect.Anchor              = AnchorStyles.Right | AnchorStyles.Bottom |
                                                  AnchorStyles.Left;
            
            // Populate the ListBox.
            this.tourLogs = this.rmcInstance.Split();
            foreach (LogRMC log in this.tourLogs)
            {
                string text = string.Format("{0} - {1}", log.logBegin.ToShortDateString(),
                        log.logEnd.ToShortDateString());
                int i = this.tourSelect.Items.Add(text, true);
            }

            //ItemCheckEventArgs args    = new ItemCheckEventArgs(ItemCheckEventArgs.NewValue);            
            this.tourSelect.ItemCheck += new ItemCheckEventHandler(this.UpdateDateSelect);            
            

            Label labelBegin     = new Label();
            labelBegin.Size      = new Size(150, labelBegin.Height);
            labelBegin.Location  = new Point(this.tourSelect.Left + this.tourSelect.Width + 10,
                    saveText.Top + saveText.Height + 10);
            labelBegin.Text      = i18n.labelBegin;
            labelBegin.TextAlign = ContentAlignment.MiddleLeft;
            labelBegin.Anchor    = AnchorStyles.Bottom | AnchorStyles.Right;
            
            if (this.minDate < DateTimePicker.MinDateTime | this.minDate > DateTimePicker.MaxDateTime)
                this.minDate = DateTimePicker.MinDateTime;
                    
            if (this.maxDate > DateTimePicker.MaxDateTime | this.maxDate < DateTimePicker.MinDateTime)
                this.maxDate = DateTimePicker.MaxDateTime;
                    
            this.dateBegin.Location       = new Point(labelBegin.Left, labelBegin.Top + labelBegin.Height);
            this.dateBegin.MinDate        = this.minDate;
            this.dateBegin.MaxDate        = this.maxDate;
            this.dateBegin.Value          = this.minDate;
            this.dateBegin.ShowUpDown     = true;
            this.dateBegin.Format         = DateTimePickerFormat.Custom;
            this.dateBegin.CustomFormat   = i18n.dateFormat;
            this.dateBegin.Anchor         = AnchorStyles.Bottom | AnchorStyles.Right;

            Label labelEnd     = new Label();
            labelEnd.Size      = new Size(150, labelEnd.Height);
            labelEnd.Location  = new Point(this.tourSelect.Left + this.tourSelect.Width + 10,
                    this.dateBegin.Top + this.dateBegin.Height + 15);
            labelEnd.Text      = i18n.labelEnd;
            labelEnd.TextAlign = ContentAlignment.MiddleLeft;
            labelEnd.Anchor    = AnchorStyles.Bottom | AnchorStyles.Right;
            
            this.dateEnd.Location       = new Point(labelBegin.Left, labelEnd.Top + labelEnd.Height);
            this.dateEnd.MinDate        = this.minDate;
            this.dateEnd.MaxDate        = this.maxDate;
            this.dateEnd.Value          = this.maxDate;
            this.dateEnd.ShowUpDown     = true;
            this.dateEnd.Format         = DateTimePickerFormat.Custom;
            this.dateEnd.CustomFormat   = i18n.dateFormat;
            this.dateEnd.Anchor         = AnchorStyles.Bottom | AnchorStyles.Right;
            
            /*
            Button saveLog   = new Button();
            saveLog.Location = new Point(128 + 30, labelEnd.Top + labelEnd.Height + 10);
            saveLog.Size     = new Size(188, saveLog.Height);
            saveLog.Text     = i18n.saveLog;
            saveLog.Anchor   = AnchorStyles.Bottom | AnchorStyles.Left;
            saveLog.Click   += new EventHandler(this.saveFileEvent);
            */

            
            // Add widgets to this form.
            Control[] widgets = new Control[]
                {saveText, labelBegin, labelEnd,
                 this.dateBegin, this.dateEnd, /*saveLog*/
                 tourSelect};

            this.Controls.AddRange(widgets);          
        }
        
        private void DrawFrame()
        {
            // Set Drohne picture.
            PictureBox drohneBox = new PictureBox();
            drohneBox.Location   = new Point(10,10);
            drohneBox.SizeMode   = PictureBoxSizeMode.AutoSize;
                
            try
            {
                Bitmap drohneImage = new Bitmap("share/pixmaps/drohne.png");
                drohneBox.Image    = (Image) drohneImage;
            }
            
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
            }

            Button buttonAbout = new Button();
            Button buttonPrev  = new Button();
            Button buttonNext  = new Button();            
            
            buttonAbout.Text = "&" + i18n.buttonAbout;
            buttonPrev.Text  = "&" + i18n.buttonPrev;
            buttonNext.Text  = "&" + i18n.buttonNext;
            
            buttonAbout.Location = new Point(10, 
                    this.ClientSize.Height - buttonAbout.Height - 10);
            
            buttonPrev.Location  = new Point(this.ClientSize.Width - buttonPrev.Width - buttonNext.Width - 15,
                    this.ClientSize.Height - buttonPrev.Height - 10);
            
            buttonNext.Location  = new Point(this.ClientSize.Width - buttonNext.Width - 10,
                    this.ClientSize.Height - buttonNext.Height - 10);
            
            buttonAbout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonPrev.Anchor  = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonNext.Anchor  = AnchorStyles.Bottom | AnchorStyles.Right;
            
            buttonAbout.Click += new EventHandler(this.ShowAbout);
            buttonPrev.Click  += new EventHandler(this.ShowPrev);
            buttonNext.Click  += new EventHandler(this.ShowNext);
            
            if (this.progPosition == 0)
                buttonPrev.Enabled = false;
            
            // Add widgets to this form.
            Control[] widgets = new Control[]
                {buttonAbout, buttonPrev, buttonNext,
                 drohneBox};

            this.Controls.AddRange(widgets);
        }

        private void DrawAbout()
        {
            this.aboutWindow.Controls.Clear();
            this.aboutWindow.ClientSize    = new Size(400,300);
            this.aboutWindow.Text          = i18n.aboutWindow;
            this.aboutWindow.ShowInTaskbar = false;
            this.aboutWindow.Font          = new Font("Arial", 11);
            this.aboutWindow.Show();
            
            PictureBox iconBox = new PictureBox();
            iconBox.Location   = new Point(10, 10);
            iconBox.SizeMode   = PictureBoxSizeMode.AutoSize;

            try
            {
                this.aboutWindow.Icon = new Icon("share/pixmaps/drohne_16_transp.ico");   
                Bitmap drohneIcon     = new Bitmap("share/pixmaps/drohne_32.png");
                iconBox.Image         = (Image) drohneIcon;     
            }
            
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Label progLabel     = new Label();
            progLabel.Location  = new Point(52, 26 - progLabel.Height / 2);
            progLabel.TextAlign = ContentAlignment.MiddleLeft;
            progLabel.Text      = i18n.progName;
            progLabel.Font      = new Font(progLabel.Font, FontStyle.Bold);
            
            Button buttonClose   = new Button();
            buttonClose.Text     = "&" + i18n.buttonClose;
            buttonClose.Anchor   = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonClose.Size     = new Size(100, buttonClose.Height);
            buttonClose.Location = new Point(this.aboutWindow.ClientSize.Width - buttonClose.Width - 10,
                    290 - buttonClose.Height);
            buttonClose.Click   += new EventHandler(this.HideForm);
            
            TabControl aboutTab = new TabControl();
            aboutTab.Location   = new Point(10, 50);
            aboutTab.Size       = new Size(this.aboutWindow.ClientSize.Width - 20, 250 - 20 - buttonClose.Height);
            aboutTab.Anchor     = AnchorStyles.Bottom | AnchorStyles.Top |
                                  AnchorStyles.Left   | AnchorStyles.Right;

            // about page
            TabPage pageAbout = new TabPage();
            pageAbout.Text    = i18n.pageAbout;
            
            Label progDesc    = new Label();
            Label copyright   = new Label();
            LinkLabel progUrl = new LinkLabel();

            progDesc.Location  = new Point(pageAbout.Width / 2 - progDesc.Width  / 2, 0);
            copyright.Location = new Point(pageAbout.Width / 2 - copyright.Width / 2, 37);
            progUrl.Location   = new Point(pageAbout.Width / 2 - progUrl.Width   / 2, 75);        
            
            progDesc.Anchor  = AnchorStyles.Left |AnchorStyles.Right;
            copyright.Anchor = AnchorStyles.Left |AnchorStyles.Right;
            progUrl.Anchor   = AnchorStyles.Left |AnchorStyles.Right; 
           
            progDesc.TextAlign  = ContentAlignment.MiddleCenter;
            copyright.TextAlign = ContentAlignment.MiddleCenter;
            progUrl.TextAlign   = ContentAlignment.MiddleCenter;  
 
    
            progDesc.Text  = i18n.progDesc;
            copyright.Text = string.Format("{0} 2003 Frank Thomas", (char)0x00A9);
            progUrl.Text   = "http://www.thomas-alfeld.de/frank/";
           
            // Add labels to TabPage.
            Control[] labels = new Control[]
                {progDesc, copyright, progUrl}; 
            
            pageAbout.Controls.AddRange(labels);


            TextBox licenseText    = new TextBox();
            licenseText.Multiline  = true;
            licenseText.ReadOnly   = true;
            licenseText.WordWrap   = false;            
            licenseText.ScrollBars = ScrollBars.Both;
            licenseText.Dock       = DockStyle.Fill;

            // Add GPL to TextBox widget.
            try
            {
                using (StreamReader sr = new StreamReader("doc/license.txt"))
                {
                    licenseText.Text = sr.ReadToEnd();
                }
            }
            
            catch (Exception e)
            {
                licenseText.Text = e.Message;
            }
            
            // license page
            TabPage pageLicense = new TabPage();
            pageLicense.Text    = i18n.pageLicense;
            pageLicense.Controls.Add(licenseText);
             
            // Add pages to TabControl.
            Control[] pages = new Control[]
                {pageAbout, pageLicense};

            aboutTab.Controls.AddRange(pages);
            
            // Add widgets to this form.
            Control[] widgets = new Control[]
                {progLabel, iconBox, aboutTab, buttonClose};

            this.aboutWindow.Controls.AddRange(widgets);
        }

        private TextBox DrawTextBox(int offset)
        {
            TextBox textBox     = new TextBox();
            textBox.Location    = new Point(128 + 30, 10);
            textBox.Size        = new Size(this.ClientSize.Width - 168, this.ClientSize.Height - 94 - offset);
            textBox.Multiline   = true;
            textBox.ReadOnly    = true;
            textBox.WordWrap    = true;
            textBox.ScrollBars  = ScrollBars.Both;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Anchor      = AnchorStyles.Bottom | AnchorStyles.Top |
                                  AnchorStyles.Right  | AnchorStyles.Left;          
            return textBox;
        }

        private void GetClipboard(object sender, EventArgs e)
        {
            try
            {
                IDataObject iData = Clipboard.GetDataObject();
                this.clipCont     = (string)iData.GetData(DataFormats.Text);

                if (this.clipCont.Trim() == "")
                    this.sourceResult.Text = string.Format("{0}\r\n\r\n{1}", i18n.sourceInfoClip, i18n.sourceNoClip);
                
                else  
                    this.sourceResult.Text = string.Format("{0}\r\n\r\n{1}", i18n.sourceInfoClip, this.clipCont);
            
                // reset sourceFiles
                this.sourceFiles = null;
            }
            
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
            }
        }

        private void ShowAbout(object sender, EventArgs e)
        {
            // Ugly, hmm? This whole class needs to be redesigned.
            if (this.aboutWindow.IsDisposed)
            {
                this.aboutWindow = new Form();
                this.DrawAbout();
            }
            
            if(!this.aboutWindow.Visible)
                this.DrawAbout();
            
            this.aboutWindow.Focus();
        }

        private void FilterSource()
        {
            Hashtable methods = new Hashtable();
            methods["StatusInvalid"] = this.filterList.GetItemCheckState(0) == CheckState.Checked;
            methods["NullValues"]    = this.filterList.GetItemCheckState(1) == CheckState.Checked;
            methods["InvalidCoords"] = this.filterList.GetItemCheckState(2) == CheckState.Checked;
            methods["ValidChecksum"] = this.filterList.GetItemCheckState(3) == CheckState.Checked;
            
            if (this.sourceFiles != null)
            {
                this.rmcInstance = new LogRMC(this.sourceFiles);
            }

            else if (this.clipCont != null)
            {
                this.rmcInstance = new LogRMC(this.clipCont);
            }

            this.rmcInstance.Clean(methods);
   
            this.minDate = this.rmcInstance.logBegin;
            this.maxDate = this.rmcInstance.logEnd;
        }

        private void ShowNext(object sender, EventArgs e)
        {
            switch (this.progPosition)
            {
                case 0:
                    this.DrawSourceSelect();
                    break;
                    
                case 1:
                    if (this.SourceSelected())
                        this.DrawFilterSelect();
                    break;

                case 2:
                    this.FilterSource();
                    this.DrawSaveForm();
                    break;

                case 3:
                    this.saveFileEvent(sender, e);
                    break;
            }
        }
        
        private void ShowPrev(object sender, EventArgs e)
        {
            switch (this.progPosition)
            {
                case 1:
                    this.DrawWelcomeForm();
                    break;

                case 2:
                    this.DrawSourceSelect();
                    break;

                case 3:
                    this.DrawFilterSelect();
                    break;
            }
        }

        private void UpdateDateSelect(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                LogRMC log = (LogRMC) this.tourLogs[e.Index];

                if (this.dateBegin.Value > log.logBegin)
                {
                    this.dateBegin.MinDate = log.logBegin;                    
                    this.dateBegin.Value   = log.logBegin;
                }

                if (this.dateEnd.Value < log.logEnd)
                {
                    this.dateEnd.MaxDate = log.logEnd;                    
                    this.dateEnd.Value   = log.logEnd;
                }
                
                if (this.tourSelect.CheckedIndices.Count == 0)
                {
                    this.dateBegin.MinDate = log.logBegin;                    
                    this.dateBegin.Value   = log.logBegin;
                    
                    this.dateEnd.MaxDate = log.logEnd;                    
                    this.dateEnd.Value   = log.logEnd; 
                }
            }
            
            if (e.NewValue == CheckState.Unchecked)
            {
                DateTime lowMark  = this.dateBegin.Value;
                DateTime highMark = this.dateEnd.Value;
                int i = 0;
                
                foreach (int index in this.tourSelect.CheckedIndices)
                {
                    // The index that was unchecked won´t be used.
                    if (index == e.Index)
                        continue;
                    
                    LogRMC log = (LogRMC) this.tourLogs[index];

                    if (i == 0)
                        lowMark = log.logBegin;

                    if (i == this.tourSelect.CheckedIndices.Count - 2)
                        highMark = log.logEnd;
                    
                    if (this.tourSelect.CheckedIndices.Count == 1)
                    {
                        lowMark  = log.logBegin;
                        highMark = log.logEnd;  
                    }
                    
                    i++;
                }

                this.dateBegin.MinDate = lowMark;                    
                this.dateBegin.Value   = lowMark;
                
                this.dateEnd.MaxDate = highMark;                    
                this.dateEnd.Value   = highMark;
            }
        }

        private void selectFilesEvent(object sender, EventArgs e)
        {
            OpenFileDialog selectDialog   = new OpenFileDialog();
            selectDialog.Multiselect      = true;
            selectDialog.RestoreDirectory = true;
            selectDialog.Title            = i18n.selectFiles;
            selectDialog.FileOk          += new CancelEventHandler(this.selectFilesOk);
            selectDialog.ShowDialog();
        }

        private void selectFilesOk(object sender, CancelEventArgs e)
        {
            sourceResult.Text = i18n.sourceInfoFile + "\r\n\r\n";
            
            OpenFileDialog senderDialog = (OpenFileDialog)sender;    
            
            foreach (string filename in senderDialog.FileNames)
                sourceResult.Text += filename + "\r\n";

            // reset clipCont
            this.clipCont    = null;
            
            this.sourceFiles = senderDialog.FileNames;
        }

        private void saveFileEvent(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog   = new SaveFileDialog();
            saveDialog.RestoreDirectory = true;
            saveDialog.Title            = i18n.saveLog;
            saveDialog.FileName         = this.dateBegin.Value.ToShortDateString() + "_" +
                                          this.dateEnd.Value.ToShortDateString()   + ".txt";
            saveDialog.FileOk          += new CancelEventHandler(this.saveFileOk);
            saveDialog.ShowDialog();
        }

        private void saveFileOk(object sender, CancelEventArgs e)
        {
            SaveFileDialog senderDialog = (SaveFileDialog)sender;
            
            //LogRMC saveRMC = this.rmcInstance.GetSegment(this.dateBegin.Value, this.dateEnd.Value);
            //saveRMC.WriteLog(senderDialog.FileName);

            foreach (int index in this.tourSelect.CheckedIndices)
            {
                LogRMC log = (LogRMC) this.tourLogs[index];
                log = log.GetSegment(this.dateBegin.Value, this.dateEnd.Value);
                log.WriteLog(senderDialog.FileName, true);
            }
        }
        
        private void HideForm(object sender, EventArgs e)
        {
            Control senderControl = (Control)sender;
            Form senderForm       = (Form)senderControl.Parent;
            
            senderForm.Hide();
        }

        private bool SourceSelected()
        {
            if (this.clipCont != null || this.sourceFiles != null)
                return true;
            else 
            {
                MessageBox.Show(i18n.sourceSelect, i18n.sourceInfoNone,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
        }
    }
}

// vim:fileformat=dos
