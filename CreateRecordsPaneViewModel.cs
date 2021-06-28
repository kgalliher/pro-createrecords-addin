#region "CLASS DOCUMENTATION"
/*
 * ***************************************************************************************************************************************************************
 * Project:         Name of the Project
 * Class:           Name of the Class in the Project
 * Version:         1.0
 * Author:          Name of the person(s) who wrote the script
 * Date Created:    Date Project Created (mm/dd/yyyy)
 * Date Launched:   Date Project launched for general use (mm/dd/yyyy)
 * Dept:            GIS Division
 * Location:        Project file location (...\ArcAdmin\ProjectFolder...)
 * Revisions:       mm/dd/yyyy -programmer-: Summary of modifications to code or Docked Window
 * ***************************************************************************************************************************************************************
 * 
 * CLASS
 * PURPOSE:     A brief explanation for, why this class is needed in the project.
 *             (example- This class contains methods used for querying data in GPUB.)
 *
 * CLASS
 * DESCRIPTION: Describe the functionality or controls contained in the class
 *              (example- This class accepts parameters from AnotherClass.cs to query GPUB and populate a ListArray, which is used by YetAnotherClass.cs.)
 *
 * CLASS
 * PROPERTIES:   Describe the properties (protected or otherwise) for this class.
 *              (example- layer internal variable from AnotherClass.cs)
 *
 * CLASS 
 * METHODS:     Provide a list of the Methods available in this class and a brief explanation of their function.
 *              (example- GetSomeQuery() method accepts parameters from layer variable & accesses xyz feature class in GPUB and returns results in a ListArray.)
 *              (example- ReturnResults() method cycles through the ListArray and populates the message box for the user to review.)
 *
 * CLASS
 * EVENTS:      Provide a list of the Events available in this class and a brief explanation of the actions they represent.
 *              (example- DatabaseConnected event is triggered when a database connection is attempted along with the result.)
 *              (example- RecordRetrieved event is triggered when a database record was returned from a retrieve operation.)
 *
 * CLASS
 * USER
 * INTERFACE:   If the class provides a user interface, describe what the user can do and should expect 
 *              from the interface, otherwise if no user interface provided, leave blank.
 *              (example- This class provides a button control that, when activated, initiates the query.  Results returned are provided in a message box.)
 *
 * SUPPORTING
 * CLASSES
 * AND
 * INTERFACES:  If this class is dependent on other classes or interfaces, list those classes with a brief explanation of their dependency.
 *              (example- a). DCADUtils.cs ==> General functions & methods.)
 *              (example- b). ErrorLog.cs ==> User Event Log controls.)
 *
 * SOURCE
 * DATA
 * CONDITIONS:  Describe if there are specific conditions to be considered for internal/external data access or data formatting
 *              (example- xyz feature class must have Address field populated for the query to return successful results.)
 *
 *
 * SUPPORTING
 * ONLINE
 * DOCUMENTATION: If online documentation was used to create code in this file, then list them with a brief description here. Use https://bit.ly/ to minimize the URL. 
 *                 (example- (1)) List<double> - https://bit.ly/2wFEESu. A system.collections.generic list object of type double.
 *                 (example- (2)) foreach - https://bit.ly/2T16AZT. An iterator for any object type.
 *
 *
 * APPLICABLE
 * STANDARDS: If standards were considered as part of the development they should be listed here with a link if available.
 *            (example- (1) C# Coding Standards - https://bit.ly/r398u779. DCAD standards for C# development.
 *
 *
 * ***************************************************************************************************************************************************************
 * ***************************************************************************************************************************************************************
 */
#endregion

using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Core.Events;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace pro_createrecords_addin
{

    internal class CreateRecordsPaneViewModel : DockPane, INotifyPropertyChanged
    {

        #region Constants

        private const string _dockPaneID  = "pro_createrecords_addin_CreateRecordsPane";
        private const string _instance = "DCADSQLDV";
        private const string _database = "GEDT";
        private const AuthenticationMode _authentication = AuthenticationMode.OSA;
        private const string _version = "dbo.DEFAULT";
        private const string _afcView = "ADM.AFC_LOG_VW";
        private const string _yes = "Y";
        private const string _blank = "";
        private ObservableCollection<AFCLog> _afclogs = new ObservableCollection<AFCLog>();
        private ReadOnlyObservableCollection<AFCLog> _afclogsRO;
        private Object _lockObj = new object();
        


        #endregion

        protected CreateRecordsPaneViewModel() 
        {
            //TODO: Check to ensure that a parcel fabric is
            // included in the current map and that the AFC Log View exists
            // in the geodatabase

            _afclogsRO = new ReadOnlyObservableCollection<AFCLog>(_afclogs);
            BindingOperations.EnableCollectionSynchronization(_afclogsRO, _lockObj);


            // Call SearchForAFCLogs
            SearchForAFCLogs();

            // Update control
            //NotifyPropertyChanged("AFCLogs");

        }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
            if (pane == null)
                return;

            pane.Activate();

            
        }

        /// <summary>
        /// A duplicate property for the SearchUtilities
        /// that can be referenced in the user control
        /// dockpane.
        /// </summary>
        

        public ReadOnlyObservableCollection<AFCLog> AFCLogs
        {

            get { return _afclogsRO; }


        }

        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Create a Records from an Existing AFC Log";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        /// <summary>
        /// Override the default behavior when the dockpane's help icon is clicked
        /// or the F1 key is pressed.
        /// </summary>
        protected override void OnHelpRequested()
        {
            System.Diagnostics.Process.Start(@"http://dcadwiki.dcad.org/dcadwiki/ArcGISPro-CreateAFCRecords");
        }

        /// <summary>
        /// Search string used to limit the returned symbols.
        /// </summary>
        private string _searchString = "";
        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                SetProperty(ref _searchString, value, () => SearchString);

                //Call SearchForAFCLogs
                SearchForAFCLogs(_searchString);


            }
        }




        private AFCLog _selectedAFCLog;

        public AFCLog SelectedAFCLog
        {
            get { return _selectedAFCLog; }
            set { _selectedAFCLog = value; }
            // TODO: Create a record for the selected AFC Log
            // CreateRecordForSelectedAFCLog();
        }

        #region Methods

        #region Clear AFC Logs Collections

        /// <summary>
        /// Remove all items
        /// from the AFCLogs 
        /// collection.
        /// </summary>
        private void ClearAFCLogsCollection()
        {
            _afclogs.Clear();
        }

        #endregion

        #region Search for AFC Logs

        /// <summary>
        /// Update the list of AFC Logs given the current search text.
        /// </summary>
        public async Task SearchForAFCLogs(string _searchString = _blank)
        {
            if (AFCLogs.Count > 0)
            {
                ClearAFCLogsCollection();

            }

            await QueuedTask.Run(() =>
            {
                // Get a list of AFC Logs
                SearchingATable(_searchString);

                // Search for AFC Logs
                // and apply search string
                // if provided
                IEnumerable<AFCLog> linqResults;

                if (_searchString != _blank)
                {
                    linqResults = _afclogs.Where(afc => afc.DOC_NUM.Contains(_searchString));

                }
                else
                {
                    linqResults = _afclogs.Where(afc => afc.AFC_LOG_ID > 0);
                }

                    // Create a temporary observable collection
                    // for filtering
                    ObservableCollection<AFCLog>_tempafclogs;

                    // Filter the items in the existing observable collection
                    
                    _tempafclogs = new ObservableCollection<AFCLog>(linqResults);

                    // Compare temporary collection with the original.
                    // Remove any items from the original collection
                    // that do not appear in the temporary collection.
                    for (int i = _afclogs.Count - 1; i >= 0; i--)
                    {
                        var item = _afclogs[i];
                        if (!_tempafclogs.Contains(item))
                        {
                            lock (_lockObj)
                            {
                                _afclogs.Remove(item);
                            }

                        }
                    }

                    // Now add any items that are included in
                    // the temporary collection that are not in
                    // the original collection in the case of a
                    // backspace
                    foreach (var item in _tempafclogs)
                    {
                        if (!_afclogs.Contains(item))
                        {
                            lock (_lockObj)
                            {
                                _afclogs.Add(item);
                            }

                        }
                    }

                 // Remove temporary observable collection
                 _tempafclogs = null;
                


            });

            // Call NotifyPropertyChanged and pass in the AFCLogs property
            NotifyPropertyChanged(() => AFCLogs);


        }

        #endregion

        #region Searching a Table using QueryFilter and Populates AFCLogs List

        public async Task SearchingATable(string _searchString)
        {
            // Define columns to be included in
            // query filter
            string _instNum = "INSTRUMENT_NUM";
            string _seqNum = "SEQ_NUM";
            string _afcLogID = "AFC_LOG_ID";

            // Multi-threaded synchronization
            //private Object _lockObj = new object();
            //BindingOperations.EnableCollectionSynchronization(_afclogsRO, _lockObj);

            try
            {
                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => {

                    // Opening a Non-Versioned SQL Server instance.
                    DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.SQLServer)
                    {
                        AuthenticationMode = _authentication,

                        // Where testMachine is the machine where the instance is running and testInstance is the name of the SqlServer instance.
                        Instance = _instance,

                        // Provided that a database called LocalGovernment has been created on the testInstance and geodatabase has been enabled on the database.
                        Database = _database,

                        // Provided that a login called gdb has been created and corresponding schema has been created with the required permissions.
                        //User = "gdb",
                        //Password = "password",
                        Version = _version
                    };


                    using (Geodatabase geodatabase = new Geodatabase(connectionProperties))
                    using (Table table = geodatabase.OpenDataset<Table>(_afcView))
                    {

                        QueryFilter queryFilter = new QueryFilter
                        {
                            WhereClause = String.Format("{0} LIKE '%{1}%' OR {2} LIKE '%{1}%'", _instNum, _searchString, _seqNum),
                            SubFields = "*",
                            PostfixClause = String.Format("ORDER BY {0} ASC", _afcLogID)
                        };

                        using (RowCursor rowCursor = table.Search(queryFilter, false))
                        {
                            while (rowCursor.MoveNext())
                            {
                                using (Row row = rowCursor.Current)
                                {
                                    AFCLog afcLog = new AFCLog();
                                    afcLog.AFC_LOG_ID = Convert.ToInt32(row["AFC_LOG_ID"]);
                                    afcLog.AFC_STATUS_CD = Convert.ToInt32(row["AFC_STATUS_CD"]);
                                    afcLog.AFC_TYPE_CD = Convert.ToInt32(row["AFC_TYPE_CD"]);
                                    afcLog.AFC_YEAR = Convert.ToInt32(row["AFC_YEAR"]);
                                    afcLog.TILE_NO = Convert.ToInt32(row["TILE_NO"]);
                                    afcLog.DRAFTER_EMPL_ID = Convert.ToString(row["DRAFTER_EMPL_ID"]);
                                    afcLog.DRAFTER_COMP_DT = Convert.ToDateTime(row["DRAFTER_COMP_DT"]);
                                    afcLog.EFFECTIVE_DT = Convert.ToDateTime(row["EFFECTIVE_DT"]);
                                    afcLog.INSTRUMENT_NUM = Convert.ToString(row["INSTRUMENT_NUM"]);
                                    afcLog.SEQ_NUM = Convert.ToString(row["SEQ_NUM"]);
                                    afcLog.RUSH_IND = Convert.ToString(row["RUSH_IND"]) == _yes ? true : false;
                                    afcLog.ACCOUNT_NUM = Convert.ToString(row["ACCOUNT_NUM"]);
                                    afcLog.SetImageSource();    // Method sets the image source for the afc log type
                                    afcLog.SetDocumentNumber(); // Method sets the document number for the afc log type
                                    
                                    // Reads and Writes should be made from within the lock
                                    lock (_lockObj)
                                    {
                                        _afclogs.Add(afcLog);
                                    }
                                }
                            }
                        }
                    }
                });
            }
            catch (GeodatabaseFieldException fieldException)
            {
                // One of the fields in the where clause might not exist. There are multiple ways this can be handled:
                // Handle error appropriately
            }
            catch (Exception exception)
            {
                // logger.Error(exception.Message);
            }
        }



        #endregion

        #region Delegates


        #endregion

        #endregion


    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class CreateRecordsPane_ShowButton : Button
    {
        protected override void OnClick()
        {
            CreateRecordsPaneViewModel.Show();
        }
    }
}
