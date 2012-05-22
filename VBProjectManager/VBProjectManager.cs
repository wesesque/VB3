﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using VBTools;
using DotSpatial.Controls;
using DotSpatial.Controls.Header;


namespace VBProjectManager
{

    public partial class VBProjectManager : Extension, IFormState
    {

        //event for when a project is opened
        public event ProjectOpenedHandler ProjectOpened;
        public delegate void ProjectOpenedHandler();

        //event for when a project is saved
        public delegate void ProjectSavedHandler<TArgs>(object sender, TArgs args) where TArgs : EventArgs;
        public event ProjectSavedHandler<PackEventArgs> ProjectSaved;

        //Request that plugins unpack their state... why?
        public delegate void EventHandler<TArgs>(object sender, TArgs args) where TArgs : EventArgs;
        public event EventHandler<UnpackEventArgs> UnpackRequest;
        
        //
        private Dictionary<string, Boolean> _tabStates;
        private string strName;

        public VBProjectManager()
        {
            this.ProjectSaved += new VBProjectManager.ProjectSavedHandler<PackEventArgs>(ProjectSavedListener);
        }

        public override void Activate()
        {
            App.HeaderControl.Add(new SimpleActionItem("My Button Caption", AboutVirtualBeach));

            //Add an item to the application ("File") menu.
            var aboutButton = new SimpleActionItem(HeaderControl.ApplicationMenuKey, "About", AboutVirtualBeach);
            aboutButton.GroupCaption = HeaderControl.ApplicationMenuKey;
            aboutButton.LargeImage = Resources.info_32x32;
            aboutButton.SmallImage = Resources.info_16x16;
            aboutButton.ToolTipText = "Open the 'About VirtualBeach' dialog.";
            App.HeaderControl.Add(aboutButton);
            
            //Add a Save button to the application ("File") menu.
            var saveButton = new SimpleActionItem(HeaderControl.ApplicationMenuKey, "Save", SaveProject);
            saveButton.GroupCaption = HeaderControl.ApplicationMenuKey;
            saveButton.LargeImage = Resources.save_32x32;
            saveButton.SmallImage = Resources.save_16x16;
            saveButton.ToolTipText = "Save the current project state.";
            App.HeaderControl.Add(saveButton);
            
            base.Activate();
        }


        public override void Deactivate()
        {
            App.HeaderControl.RemoveAll();
            base.Deactivate();
        }


        public void SaveProject(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("this is a test.");
        }


        public void AboutVirtualBeach(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("this is a test.");
        }


        public Dictionary<string, Boolean> TabStates
        {
            get { return _tabStates; }
            set { _tabStates = value; }
        }


        public class ProjectChangedStatus : EventArgs
        {
            private string _status;
            public string Status
            {
                set { _status = value; }
                get { return _status; }
            }
        }


        public string ProjectName
        {
            get { return strName; }
            set { strName = value; }
        }


        private void ProjectSavedListener(object sender, UnpackEventArgs e)
        {
        }


        private void ProjectSavedListener(object sender, PackEventArgs e)
        {
        }


        public object PackState()
        {
            return null;
            //This function packs the state of the Project Manager for saving to disk.
        }


        public void UnpackState(object objPackedStates)
        {
            //This function restores the previously packed state of the Project Manager.
        }

    }
}
