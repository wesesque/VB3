﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using System.Windows.Forms;
using VBCommon;


namespace VBProjectManager
{
    public partial class VBProjectManager
    {
        /*public void Open(string projectFile)
        {
            SerializableDictionary<string, object> dictPackedState = new SerializableDictionary<string, object>();

            XmlDeserializer deserializer = new XmlDeserializer();

            dictPackedState = deserializer.Deserialize(projectFile) as SerializableDictionary<string, object>;
            
            //get the filename to come over right away
            FileInfo fi = new FileInfo(projectFile);
            //projectmanager.ProjectName = fi.Name;

            signaller.UnpackProjectState(dictPackedState);
        }


        public void Save(string projectFile)
        {
            SerializableDictionary<string, object> dictPackedStates = new SerializableDictionary<string, object>();

            signaller.RaiseSaveRequest(dictPackedStates);

            FileInfo _fi = new FileInfo(projectFile);
            //projectmanager.ProjectName = _fi.Name;

            XmlSerializer serializerDict = new XmlSerializer();
            serializerDict.Serialize(dictPackedStates, projectFile);
        }*/


        private void ProjectSavedListener(object sender, VBCommon.SerializationEventArgs e)
        {
            e.PackedPluginStates.Add(strPluginKey, PackState());
        }


        private void ProjectOpenedListener(object sender, VBCommon.SerializationEventArgs e)
        {
            if (e.PackedPluginStates.ContainsKey(strPluginKey))
            {
                this.UnpackState(e.PackedPluginStates[strPluginKey]);
            }
            else
            {
                //Set this plugin to an empty state.
            }
        }


        private void BroadcastListener(object sender, VBCommon.SerializationEventArgs e)
        {
            //listen to others broadcast..receiving something
            //e.PackedPluginState
        }


        public void Broadcast(IPlugin _pluginType, IDictionary<string, object> packedState)
        {
            //IDictionary<string, object> packedState = new Dictionary<string, object>();
            //packedState = PackState();
            signaller.RaiseBroadcastRequest(_pluginType, packedState);
        }


        public IDictionary<string, object> PackState()
        {
            IDictionary<string, object> dictLocalProjMngr = new Dictionary<string, object>();

            dictLocalProjMngr.Add("ProjectName", ProjectName);
            
            //another dictionary holding key/values of order of plugins to reopen
            //dictLocalProjMngr.Add("TabStates", TabStates); //what order (top/bottom to save plugins in
                        
            return dictLocalProjMngr;
        }

        
        public void UnpackState(IDictionary<string, object> dictPackedState)
        {           
            //Dictionary<string, object> dictLocalProjectMngr = new Dictionary<string, object>();
            //dictLocalProjectMngr = (Dictionary<string, object>)objPackedState;

            this.strPathName = (string)dictPackedState["ProjectName"];
            //this.TabStates = (Dictionary<string, bool>)dictLocalProjectMngr["TabStates"];
        }
    }
}
