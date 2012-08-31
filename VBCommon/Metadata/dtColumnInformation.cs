﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace VBCommon.Metadata
{
    // class to carry datatable column enable/disable information
    // largely superceded by use of table column extended properties
    // but still used by the column plots enable/disable functions
    public class dtColumnInformation
    {
        //table to operate with
        private DataTable _dt = null;
        //dictionary to hold column info
        private Dictionary<string, bool> dictColstatus = null;
       

        // method initializes a datatable cols information structure to all enabled
        public dtColumnInformation(DataTable dt)
        {
            if (dt != null)
            {
                _dt = dt.Copy();
                dictColstatus = new Dictionary<string, bool>();

                for (int c = 0; c < _dt.Columns.Count; c++)
                {
                    //if this col has enabled flag, set that as the value in dictColstatus, else set it to true
                    if (_dt.Columns[c].ExtendedProperties.ContainsKey(VBCommon.Globals.ENABLED))
                    {
                        bool boolcoStatus;
                        string strEnabledStatus = _dt.Columns[c].ExtendedProperties["enabled"].ToString();
                        
                        if (strEnabledStatus == "False") boolcoStatus = false;
                        else boolcoStatus = true;
                        
                        dictColstatus.Add(_dt.Columns[c].ColumnName.ToString(), boolcoStatus);                    
                    }                            
                    else
                    {
                        dictColstatus.Add(_dt.Columns[c].ColumnName.ToString(), true);
                    }
                }
            }
        }


        /// <summary>
        /// constructor optionally calls method to init the column information structure 
        /// and return the itself - singleton
        /// </summary>
        /// <param name="dt">table</param>
        /// <param name="init">if true, initialize; for example, on import</param>
        /// <returns></returns>
//        public static dtColumnInformation getdtCI(DataTable dt, bool init)
//        {
//            //pass null after initialization to access the DTcolInfo property
//            //or pass init=true (after import) to initialize
//            if (dtCI == null || init) dtCI = new dtColumnInformation(dt);
//            return dtCI;
//        }


        // method returns the enable/disable status of the column name (key)
        //the table column name to check, true if enable, false if disable
        public bool getColStatus(string key)
        {
            //returns the status of a row
            bool boolStatus;
            dictColstatus.TryGetValue(key, out boolStatus);
            return boolStatus;
        }


        /// <summary>
        /// method to set a table column to enable/disable
        /// </summary>
        /// <param name="key">table column name</param>
        /// <param name="val">true to enable, false to disable</param>
        public void setColStatus(string key, bool val)
        {
            //sets the status of a column
            dictColstatus[key] = val;
        }


        /// <summary>
        /// property - structure to return column status of all columns in table...
        /// </summary>
        public Dictionary<string, bool> DTColInfo
        {
            //returns a col-status dictionary for all cols in the datatable
            set { dictColstatus = value; }
            get { return dictColstatus; }
        }


        /// <summary>
        /// method adds a new column to the column information structure, used when new columns are added to the table
        /// </summary>
        /// <param name="colname">column name to add</param>
        /// <returns>true iff sucessful, false if it exists already</returns>
        public bool addColumnNameToDic(string colname)
        {
            bool boolRetval = true;
            try
            {
                dictColstatus.Add(colname, true);
            }
            catch (Exception e)
            {
                boolRetval = false;
            }
            return boolRetval;
        }


        /// <summary>
        /// method removes a column from the table column information structure
        /// </summary>
        /// <param name="colname">col name to remove</param>
        /// <returns>true if successful, false iff not found</returns>
        public bool removeColumnFromDic(string colname)
        {
            bool boolRetval = true;
            try
            {
                dictColstatus.Remove(colname);
            }
            catch (Exception e)
            {
                return false;
            }
            return boolRetval;
        }

    }
}
