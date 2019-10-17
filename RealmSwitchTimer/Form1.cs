/*
 * Copyright 2019 Tamlansoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace RealmSwitchTimer
{
    public partial class MainForm : Form
    {
        private double realmTimerDuration = 4;
        private const int UPDATE_INTERVAL = 15; 
        private int updateCounter = UPDATE_INTERVAL;

        public MainForm()
        {
            InitializeComponent();
        }

        private void SetAlarm_Click(object sender, EventArgs e)
        {
            SetAlarm.Enabled = false;
            ClearAlarm.Enabled = true;
        }

        private void ClearAlarm_Click(object sender, EventArgs e)
        {
            SetAlarm.Enabled = true;
            ClearAlarm.Enabled = false;
        }

        private void UpdateForm(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            CurrentTimeLabel.Text = now.ToString();
            if (updateCounter++ > UPDATE_INTERVAL)
            {
                updateCounter = 0;
                FileSystemInfo lastCharacterFile = getMostRecentlyUpdatedIniFile();
                if (lastCharacterFile != null)
                {
                    /*
                     * Thanks Nivex for helping me troubleshoot a frustrating bug here
                     */
                    CharacterName.Text = lastCharacterFile.Name.Split('-')[0];
                    CharacterTime.Text = lastCharacterFile.LastWriteTime.ToString();
                    DateTime expiryTime = lastCharacterFile.LastWriteTime.AddHours(realmTimerDuration);
                    if (now > expiryTime)
                    {
                        Expiry.Text = "Now";
                        Progress.Value = 100;
                        if (ClearAlarm.Enabled)
                        {
                            SystemSounds.Exclamation.Play();
                        }
                    }
                    else
                    {
                        Expiry.Text = (expiryTime - now).ToString("h' h 'm' m'");
                        Progress.Value = (int)(100 * (1 - (expiryTime - now).TotalHours / realmTimerDuration));
                    }
                }
                else
                {
                    CharacterName.Text = "-- No character files found --";
                    CharacterTime.Text = "-- No character files found --";
                    Progress.Value = 0;
                    Expiry.Text = "-- Undetermined --";
                }

                RunningStateLabel.Text = (IsPhoenixRunning())
                    ? "Phoenix is Running"
                    : "Phoenix is Not Running";
            }
        }

        private bool IsPhoenixRunning()
        {
            Process[] processes = Process.GetProcessesByName("game.dll");
            return processes.Length > 0;
        }

        private FileSystemInfo getMostRecentlyUpdatedIniFile()
        {
            DirectoryInfo DirInfo = new DirectoryInfo(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Electronic Arts",
                    "Dark Age of Camelot",
                    "phoenix"));
            return DirInfo.Exists 
                ? DirInfo.GetFileSystemInfos("*.ini").OrderByDescending(fi => fi.LastWriteTime).First()
                : null;
        }

    }
}
