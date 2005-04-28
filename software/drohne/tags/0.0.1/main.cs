/* Drohne - Small GPS RMC transformation tool.
 * Copyright (C) 2003  Frank Thomas <frank@thomas-alfeld.de>
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */

// project created on 08.06.2003 at 21:51
using System;
using System.IO;
using System.Windows.Forms;

namespace Drohne 
{
	class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button         open_button;
		private System.Windows.Forms.Button         save_button;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		
		private System.String input_file;
		//private System.String output_file;

		public MainForm()
		{
			InitializeComponent();
		}

		void open_buttonClick(object sender, System.EventArgs e)
		{	
			this.openFileDialog.ShowDialog();
		}

		void save_buttonClick(object sender, System.EventArgs e)
		{
			// Opens save dialog when an input file was selected.
			if (this.openFileDialog.FileName != "")
			{	
				// Make the input filename plus the .drn extension the default output filename.
				this.saveFileDialog.FileName = this.openFileDialog.FileName + ".drn";
				this.saveFileDialog.ShowDialog();
			}
		}
		
		void openFileDialogFileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			TextReader open_stream = File.OpenText(this.openFileDialog.FileName);

			// Read entire file into member input_file.
			this.input_file = open_stream.ReadToEnd();
			open_stream.Close();
		}
		
		void saveFileDialogFileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			TextWriter save_stream = File.CreateText(this.saveFileDialog.FileName);
			
			System.String[] lines = this.input_file.Split('\n');

			foreach(System.String line in lines)
			{
				System.String[] fields = line.Split(','); 

				// Continue if line is uninteresting (unlike RMC).
				if (fields.Length != 14)
					continue;
				
				// Delete all is invalid marked lines. 
				if (fields[3] == "V")
					continue;

				if (fields[4] == "0000.0000" || fields[6] == "0000.0000")
					continue;

				if (fields[4] == "3600.0000" || fields[6] == "12000.0000")
					continue;

				save_stream.Write(line.Substring(9, line.LastIndexOf(',')-9 ) + "\r\n");
			}
				
			save_stream.Close();

			/*
			// create new gpsData instance and write modified input to member output_file.
			gpsData data = new gpsData();
			this.output_file = data.Transform(this.input_file);
			
			TextWriter save_stream = File.CreateText(this.saveFileDialog.FileName);
			
			// Write member output_file to save_stream.
			save_stream.Write(this.output_file);
			save_stream.Close();
			*/
		}
		
		void InitializeComponent()
		{
			this.open_button    = new System.Windows.Forms.Button();
			this.save_button    = new System.Windows.Forms.Button();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();

			this.SuspendLayout();
			
			// open_button
			this.open_button.Location = new System.Drawing.Point(20, 20);
			this.open_button.Name     = "open_button";
			this.open_button.Size     = new System.Drawing.Size(130, 32);
			this.open_button.TabIndex = 0;
			this.open_button.Text     = "Öffnen";
			this.open_button.Click   += new System.EventHandler(this.open_buttonClick);
 
			// save_button
			this.save_button.Location = new System.Drawing.Point(170, 20);
			this.save_button.Name     = "save_button";
			this.save_button.Size     = new System.Drawing.Size(130, 32);
			this.save_button.TabIndex = 1;
			this.save_button.Text     = "Speichern unter";
			this.save_button.Click   += new System.EventHandler(this.save_buttonClick);

			// openFileDialog
			this.openFileDialog.Filter  = "All files (*.*)|*.*";
			this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(
				this.openFileDialogFileOk);
 
			// saveFileDialog
			this.saveFileDialog.Filter  = "Angepasste Drohne Datei (*.drn)|*.drn|All files (*.*)|*.*";
			this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(
				this.saveFileDialogFileOk);
 
			// MainForm
			this.Controls.AddRange(new System.Windows.Forms.Control[]
						{	this.open_button,
							this.save_button
						});

			this.ClientSize = new System.Drawing.Size(320, 72);
			this.Text       = "Drohne";
			this.ResumeLayout(false);
		}
			
		[STAThread]
		public static void Main()
		{
			Application.Run(new MainForm());
		}
	}

	/*
	class gpsData
	{
		public System.String Transform(String file_content)
		{	
			System.String processed = "";

			// Splits the file_content by line.
			System.String[] lines = file_content.Split('\n');
			
			foreach(System.String line in lines)
			{
				System.String[] fields = line.Split(','); 

				// Continue if line is uninteresting (unlike RMC).
				if (fields.Length != 13)
					continue;

			}
		} 
	}
	*/
			
}
