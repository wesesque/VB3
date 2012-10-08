﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using DotSpatial.Controls;
using System.ComponentModel.Composition;
using DotSpatial.Controls.Header;
using DotSpatial.Controls.Docking;
using VBCommon;
using VBCommon.Interfaces;
using VBCommon.Controls;
using VBProjectManager;

namespace VBDatasheet
{
    //datasheet plugin
    public class DatasheetPlugIn : Extension, IPartImportsSatisfiedNotification, IPlugin
    {
        //class instance
        private frmDatasheet _frmDatasheet;
        [Import("Shell")]
        private ContainerControl Shell { get; set; }
        private Globals.PluginType pluginType = VBCommon.Globals.PluginType.Datasheet;

        private VBCommon.Signaller signaller;
        protected string strPanelKey = "DataSheetPanel";
        protected string strPanelCaption = "Global Datasheet";
        //buttons
        private SimpleActionItem btnImport;
        private SimpleActionItem btnValidate;
        private SimpleActionItem btnComputeAO;
        private SimpleActionItem btnManipulate;
        private SimpleActionItem btnTransform;
        private SimpleActionItem btnGoToModeling;
        private RootItem rootDatasheetTab;

        private Boolean boolComplete;
        private Boolean boolVisible = true;
        private Boolean boolInitialPass = true;

        //is model complete
        private Boolean boolModelComplete;
        private Boolean boolChangesMadeDS;

        //raise a message
        public delegate void MessageHandler<TArgs>(object sender, TArgs args) where TArgs : EventArgs;
        public event MessageHandler<VBCommon.PluginSupport.MessageArgs> MessageSent;

        public override void Deactivate()
        {
            App.HeaderControl.RemoveAll();
            App.DockManager.Remove(strPanelKey);
            _frmDatasheet = null;
            base.Deactivate();
        }


        //hide plugin
        public void Hide()
        {
            boolVisible = false;
            App.HeaderControl.RemoveAll();
            ((VBDockManager.VBDockManager)App.DockManager).HidePanel(strPanelKey);
        }


        //show plugin
        public void Show()
        {            
            AddRibbon("Show");
            boolVisible = true;
        }


        //make this plugin the active plugin
        public void MakeActive()
        {
            App.DockManager.SelectPanel(strPanelKey);
            App.HeaderControl.SelectRoot(strPanelKey);
        }


        //initial activation, adding the panel and the ribbon
        public override void Activate()
        {
            _frmDatasheet = new frmDatasheet();
            AddRibbon("Activate");
            AddPanel();

            //when panel is selected activate seriesview and ribbon tab
            App.DockManager.ActivePanelChanged += new EventHandler<DotSpatial.Controls.Docking.DockablePanelEventArgs>(DockManager_ActivePanelChanged);

            //when root item is selected
            App.HeaderControl.RootItemSelected += new EventHandler<RootItemEventArgs>(HeaderControl_RootItemSelected);
            _frmDatasheet.NotifiableDataEvent += new EventHandler(NotifiableDataEventHandler);
            base.Activate(); //ensures "enabled" is set to true
        }


        //select panel if it is the root item selected
        void HeaderControl_RootItemSelected(object sender, RootItemEventArgs e)
        {
            if (e.SelectedRootKey == strPanelKey)
            {
                boolVisible = true;
                App.DockManager.SelectPanel(strPanelKey);
                _frmDatasheet.Refresh();
            }
        }


        //return panel key name
        public string PanelKey
        {
            get { return strPanelKey; }
        }


        //return plugin type (datasheet)
        public Globals.PluginType PluginType
        {
            get { return pluginType; }
        }
        

        //return plugin complete flag
        public Boolean Complete
        {
            get { return boolComplete; }
        }


        //return plugin visible flag
        public Boolean Visible
        {
            get { return boolVisible; }
        }


        //add a datasheet plugin root item
        public void AddRibbon(string sender)
        {
            rootDatasheetTab = new RootItem(strPanelKey, strPanelCaption);
            rootDatasheetTab.SortOrder = (short)pluginType;
            App.HeaderControl.Add(rootDatasheetTab);

            //tell ProjMngr if this is being Shown
            if (sender == "Show")
            {
                App.HeaderControl.SelectRoot(strPanelKey);
            }

            //add sub-ribbons
            const string tGroupCaption = "Add";

            btnImport = new SimpleActionItem(strPanelKey, "Import Data", btnImport_Click);
            btnImport.LargeImage = Properties.Resources.EPAadd_data;
            btnImport.GroupCaption = tGroupCaption;
            btnImport.Enabled = true;
            App.HeaderControl.Add(btnImport);

            //section for validating
            const string grpValidate = "Validate";

            btnValidate = new SimpleActionItem(strPanelKey, "Validate Data", btnValidate_Click);
            btnValidate.LargeImage = Properties.Resources.EPAvalidate;
            btnValidate.GroupCaption = grpValidate;
            btnValidate.Enabled = false;
            App.HeaderControl.Add(btnValidate);

            //section for working with data
            const string grpManipulate = "Work with Data";

            btnComputeAO = new SimpleActionItem(strPanelKey, "Compute A O", _frmDatasheet.btnComputeAO_Click);
            btnComputeAO.LargeImage = Properties.Resources.EPAComputeAO;
            btnComputeAO.GroupCaption = grpManipulate;
            btnComputeAO.Enabled = false;
            App.HeaderControl.Add(btnComputeAO);

            btnManipulate = new SimpleActionItem(strPanelKey, "Manipulate", _frmDatasheet.btnManipulate_Click);
            btnManipulate.LargeImage = Properties.Resources.EPAmanipulate;
            btnManipulate.GroupCaption = grpManipulate;
            btnManipulate.Enabled = false;
            App.HeaderControl.Add(btnManipulate);

            btnTransform = new SimpleActionItem(strPanelKey, "Transform", _frmDatasheet.btnTransform_Click);
            btnTransform.LargeImage = Properties.Resources.EPAtransform;
            btnTransform.GroupCaption = grpManipulate;
            btnTransform.Enabled = false;
            App.HeaderControl.Add(btnTransform);

            btnGoToModeling = new SimpleActionItem(strPanelKey, "Go To Model", btnGoToModeling_Click);
            btnGoToModeling.LargeImage = Properties.Resources.GoToModeling;
            btnGoToModeling.GroupCaption = grpManipulate;
            btnGoToModeling.Enabled = false;
            App.HeaderControl.Add(btnGoToModeling);
        }

        //add a datasheet panel
        public void AddPanel()
        {
            var dp = new DockablePanel(strPanelKey, strPanelCaption, _frmDatasheet, DockStyle.Fill);
            dp.DefaultSortOrder = (short)pluginType;
            App.DockManager.Add(dp);
        }


        //This function imports the signaller from the VBProjectManager
        [System.ComponentModel.Composition.Import("Signalling.GetSignaller", AllowDefault = true)]
        public Func<VBCommon.Signaller> GetSignaller
        {
            get;
            set;
        }


        public void OnImportsSatisfied()
        {
            //If we've successfully imported a Signaller, then connect its events to our handlers.
            signaller = GetSignaller();
            signaller.BroadcastState += new VBCommon.Signaller.BroadcastEventHandler<VBCommon.PluginSupport.BroadcastEventArgs>(BroadcastStateListener);
            signaller.ProjectSaved += new VBCommon.Signaller.SerializationEventHandler<VBCommon.PluginSupport.SerializationEventArgs>(ProjectSavedListener);
            signaller.ProjectOpened += new VBCommon.Signaller.SerializationEventHandler<VBCommon.PluginSupport.SerializationEventArgs>(ProjectOpenedListener);
            this.MessageSent += new MessageHandler<VBCommon.PluginSupport.MessageArgs>(signaller.HandleMessage);
        }



        //listen to Model's complete status
        private void BroadcastStateListener(object sender, VBCommon.PluginSupport.BroadcastEventArgs e)
        {
            if (((IPlugin)sender).PluginType == Globals.PluginType.Modeling)
                if ((bool)e.PackedPluginState["Complete"])
                    boolModelComplete = true;
        }


        public void NotifiableDataEventHandler(object sender, EventArgs e)
        {
            //We are handling a re-raised NotifiableDataEvent. The plugin's state is not considered complete b/c the user hasn't pressed the "Go To Modeling" button.
            boolComplete = false;

            if (boolModelComplete)
                boolChangesMadeDS = true;
                
            Broadcast();
        }


        public void Broadcast()
        {
            IDictionary<string, object> packedState = new Dictionary<string, object>();
            packedState = _frmDatasheet.PackState();

            if (packedState == null)
                return;
                
            packedState.Add("ChangesMadeDS", boolChangesMadeDS);
            packedState.Add("Complete", boolComplete);
            packedState.Add("Visible", boolVisible);
            signaller.RaiseBroadcastRequest(this, packedState);
        }


        private void BroadcastStateListener(object sender, VBCommon.PluginSupport.BroadCastEventArgs e)
        {
            if (((IPlugin)sender).PluginType == Globals.PluginType.ProjectManager)
            {
                //Test whether this plugin is meant to restore the packed state.
                //if (e.PackedPluginState.Keys == this.strPanelKey)
                //{

            }
        }


        private void ProjectSavedListener(object sender, VBCommon.PluginSupport.SerializationEventArgs e)
        {
            IDictionary<string, object> dictPackedState = _frmDatasheet.PackState();

            dictPackedState.Add("Complete", boolComplete);
            dictPackedState.Add("Visible", boolVisible);
            dictPackedState.Add("InitialPass", boolInitialPass);

            e.PackedPluginStates.Add(strPanelKey, dictPackedState);
        }


        private void ProjectOpenedListener(object sender, VBCommon.PluginSupport.SerializationEventArgs e)
        {
            if (e.PackedPluginStates.ContainsKey(strPanelKey))
            {
                IDictionary<string, object> dictPlugin = e.PackedPluginStates[strPanelKey];

                boolComplete = (bool)dictPlugin["Complete"];
                boolInitialPass = (bool)dictPlugin["InitialPass"];

                //check to see if there already is a datasheet open, if so, close it before opening a saved project
                if (this.Visible)
                    this.Hide();

                IDictionary<string, object> dictPlugin = e.PackedPluginStates[strPanelKey];
                _frmDatasheet.UnpackState(dictPlugin);               

                boolVisible = (bool)dictPlugin["Visible"];
                //when opening a saved project that has a datasheet, it will be complete.
                boolComplete = boolVisible; 

                if (boolVisible)
                {
                    this.Show();

                    if (boolComplete)
                    {
                        btnComputeAO.Enabled = true;
                        btnGoToModeling.Enabled = true;
                        btnManipulate.Enabled = true;
                        btnTransform.Enabled = true;
                    }
                }
                _frmDatasheet.UnpackState(dictPlugin);
            }
            else
            {
                Activate();
            }
        }


        private void SendMessage(string message)
        {
            if (MessageSent != null)
            {
                VBCommon.PluginSupport.MessageArgs e = new VBCommon.PluginSupport.MessageArgs(message);
                MessageSent(this, e);
            }
        }


        void DockManager_ActivePanelChanged(object sender, DotSpatial.Controls.Docking.DockablePanelEventArgs e)
        {
            if (e.ActivePanelKey == strPanelKey)
            {
                App.DockManager.SelectPanel(strPanelKey);
                App.HeaderControl.SelectRoot(strPanelKey);
            } 
        }


        void btnImport_Click(object sender, EventArgs e)
        {
            _frmDatasheet.btnImportData_Click(sender, e);
            btnValidate.Enabled = true;
        }


        void btnValidate_Click(object sender, EventArgs e)
        {
            _frmDatasheet.btnValidateData_Click(sender, e);
            btnComputeAO.Enabled = true;
            btnManipulate.Enabled = true;
            btnTransform.Enabled = true;
            btnGoToModeling.Enabled = true;
        }


        void btnGoToModeling_Click(object sender, EventArgs e)
        {
            if (!boolInitialPass)
            {
                //Ask whether the user wants to clobber the modeling and prediction tabs by modifying the data.
                DialogResult dlgr = MessageBox.Show("Changes in data and/or data attributes have occurred.\nPrevious modeling results will be erased. Proceed?", "Proceed to Modeling.", MessageBoxButtons.OKCancel);
                if (dlgr == DialogResult.Cancel)
                {
                    return;
                }
            }

            //Datasheet is complete when go to modeling is clicked
            boolComplete = true;
            boolInitialPass = false;
            Broadcast();
            
            //once you leave here, changes made to ds clear for next time here
            boolChangesMadeDS = false;
        }  
    }
}