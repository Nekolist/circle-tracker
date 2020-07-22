﻿using OsuMemoryDataProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IWshRuntimeLibrary;

namespace Circle_Tracker
{
    public partial class MainForm : Form
    {
        private readonly Tracker tracker;
        private bool MinimizeToTrayEnabled = true;
        public MainForm()
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            InitializeComponent();
            //this.Icon = Properties.Resources.iconbars;
            startupCheckBox.Checked = ShortcutExists();

            try
            {
                Updater.CheckForUpdates();
            }
            catch (Exception e)
            {
                // exception we probably don't care about
            }

            tracker = new Tracker(this);

            songsFolderTextBox.Text = tracker.SongsFolder;
            sheetNameTextBox.Text = tracker.SheetName;
            spreadsheetIdTextBox.Text = tracker.SpreadsheetId;
            soundEnabledCheckbox.Checked = tracker.SubmitSoundEnabled;

            tracker.InitGoogleAPI(silent:true);
            SetCredentialsFound(System.IO.File.Exists("credentials.json"));
            updateTimer.Start();
        }

        public void SetCredentialsFound(bool found)
        {
            credentialsLabel.Text      = found ? "Found" : "Missing";
            credentialsLabel.ForeColor = found ? Color.Green : Color.Red;
        }

        public void UpdateControls()
        {
            groupBox2.BackColor = (tracker.SheetsApiReady && tracker.GameState == OsuMemoryStatus.Playing) ? Color.FromArgb(214, 241, 216) : SystemColors.Control;
            hitsTextBox.Text    = tracker.TotalBeatmapHits.ToString();
            timeTextBox.Text    = tracker.Time.ToString();
            beatmapTextBox.Text = tracker.BeatmapString;
            starsTextBox.Text   = tracker.BeatmapStars.ToString("0.00");
            aimTextBox.Text     = tracker.BeatmapAim.ToString("0.00");
            speedTextBox.Text   = tracker.BeatmapSpeed.ToString("0.00");
            modsTextBox.Text    = tracker.GetModsString();
            textBoxCS.Text      = tracker.BeatmapCs.ToString("0.0");
            textBoxAR.Text      = tracker.BeatmapAr.ToString("0.0");
            textBoxOD.Text      = tracker.BeatmapOd.ToString("0.0");
        }

        private void songsFolderTextBox_TextChanged(object sender, EventArgs e)
        {
            tracker.SetSongsFolder(songsFolderTextBox.Text);
        }

        public void SetSheetsApiReady(bool val)
        {
            statusLabel.Text = val ? "Connected" : "Not connected";
            statusLabel.ForeColor = val ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }

        private void ConnectApiButton_Click(object sender, EventArgs e)
        {
            tracker.InitGoogleAPI();
        }

        private void spreadsheetIdTextBox_TextChanged(object sender, EventArgs e)
        {
            tracker.SpreadsheetId = spreadsheetIdTextBox.Text;
        }

        private void sheetNameTextBox_TextChanged(object sender, EventArgs e)
        {
            tracker.SheetName = sheetNameTextBox.Text;
        }

        private void soundEnabledCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            tracker.SubmitSoundEnabled = soundEnabledCheckbox.Checked;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                tracker.Tick();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception occurred: {Environment.NewLine}{Environment.NewLine}" +
                    $"{ex.Message}{Environment.NewLine}{Environment.NewLine}" +
                    $"Please PLEASE tell FunOrange about this."
                    , "Error");
            }
        }

        private void startupCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (startupCheckBox.Checked)
            {
                CreateShortcut();
            }
            else
            {
                DeleteShortcut();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
             Show();
             this.WindowState = FormWindowState.Normal;
             notifyIcon1.Visible = false;
        }

        private void MinimizeToTray(object sender, EventArgs e)
        {
            if (MinimizeToTrayEnabled)
            {
                Hide();
                notifyIcon1.Visible = true;
            }
        }
        private void CreateShortcut()
        {
            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup" + @"\circle-tracker.lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Circle Tracker (startup shortcut)";
            shortcut.Hotkey = "";
            shortcut.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            shortcut.Save();
        }
        private void DeleteShortcut()
        {
            string shortcutAddress = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup" + @"\circle-tracker.lnk";
            System.IO.File.Delete(shortcutAddress);
        }
        private bool ShortcutExists()
        {
            string shortcutAddress = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup" + @"\circle-tracker.lnk";
            return System.IO.File.Exists(shortcutAddress);
        }


    }
}
