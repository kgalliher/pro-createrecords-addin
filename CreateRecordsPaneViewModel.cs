﻿#region "CLASS DOCUMENTATION"
/*
 * ***************************************************************************************************************************************************************
 * Project:         Create AFC Records
 * Class:           CreateRecordsPaneViewModel.cs
 * Version:         0.1.0
 * Author:          John W. Fell
 * Date Created:    06/21/2021
 * Date Launched:   TBD
 * Dept:            GIS Deparment
 * Location:        https://github.com/dcadgis/pro-createrecords-addin/
 * Revisions:       
 * ***************************************************************************************************************************************************************
 * 
 * CLASS
 * PURPOSE:     Business logic for MVVM pattern dockpane used in ArcGIS Pro.
 *              
 *
 * CLASS
 * DESCRIPTION: This class generates the list of AFC logs to display in the 
 *              dock pane as well as other features such as onclick button
 *              events.
 * CLASS
 * PROPERTIES:   AFCLogs  - The ReadOnlyObservableCollection object that is
 *                          bound to the XAML ListBox object in the 
 *                          CreateRecordsPane.xaml file.

 *                

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
 * DOCUMENTATION: ControlTemplate - https://bit.ly/2XOiCLw. A template for toggle button control.
 *
 *
 * APPLICABLE
 * STANDARDS: C# Coding Standards - https://bit.ly/r398u779. DCAD standards for C# development.
 *
 *
 * ***************************************************************************************************************************************************************
 * ***************************************************************************************************************************************************************
 */
#endregion

using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using ArcGIS.Desktop.Mapping;
using System.Windows;

namespace pro_createrecords_addin
{

    internal class CreateRecordsPaneViewModel : DockPane, INotifyPropertyChanged
    {

        #region Constants

        private const string DockPaneID  = "pro_createrecords_addin_CreateRecordsPane";

        private const string EventLogSourceName = "Create Records Add-In";

        private static readonly string Instance = "DCADSQLVM02";

        private static readonly string Database = "GEDT";

        private const AuthenticationMode Authentication = AuthenticationMode.OSA;

        private static readonly string Version = "dbo.DEFAULT";

        private static readonly string AFCView = "ADM.AFC_LOG_VW";

        private const string Yes = "Y";

        private const string Blank = "";

        private static readonly string TileLayerName = "DCAD Tiles";

        private static readonly int CleanupRecordType = 28;


        /*********************************************************************************
        * The _afclogs variable is an ObservableCollection object that is manipulated    *
        * in the SearchForAFCLogs() method. The database view GEDT.ADM.AFC_LOG_VW is     *
        * read using a QueryFilter and the resulting RowCursor object populates AFC Log  *
        * class objects and adds them to this collection. This property is manipulated   *
        * in the business logic but is not the ultimate data source. This property is    *
        * bound to the _afclogsRO property which is returned by the AFCLogs property     *
        * bound to the dock pane  control in the xaml.                                   *
        * *******************************************************************************/


        private ObservableCollection<AFCLog> _afclogs = new ObservableCollection<AFCLog>();

        private ReadOnlyObservableCollection<AFCLog> _afcLogsRO;

        private MapView _mapView;

        private Object _lockObj = new object();

         /*************************************************************************************************
         * Public ICommand Implementations for Custom WPF Buttons. This allows the application to call    *
         * existing methods in the ViewModel from the button using AsyncRelayCommand.                     *
         * (1) RefreshListCommand - Refreshes the afc log list.                                           *
         * (2) CreateCleanupRecordCommand - Creates a new parcel fabric records of the cleanup type.      *
         *     This is a custom record with specific attributes applied automatically when the workflow   *
         *     involves  cleaning up GIS data only and no legal document is triggering a parcel change.   *
         *************************************************************************************************/

        /// <summary>
        /// Represents a wrapper
        /// for the refresh list
        /// command.
        /// </summary>
        public ICommand RefreshListCommand { get; set; }



        /// <summary>
        /// Represents a wrapper
        /// for the create cleanup
        /// record command.
        /// </summary>
        public ICommand CreateCleanupRecordCommand { get; set; }


        #endregion

        public CreateRecordsPaneViewModel() 
        {
            // Ensure that a parcel fabric is
            // included in the current map
            


            // and that the AFC Log View exists
            // in the geodatabase
            // Check to ensure that the DCAD tile feature class is in the 
            // current map



        /******************************************************************
         * ReadOnlyObservableCollection for AFC Logs binding:
         * This variable is assigned a new ReadOnlyObservableCollection
         * bound to the public ObservableCollection object _afclogs.
         * The _afclogs variable is a collection of AFCLog objects and 
         * is manipulated based on the contents of the ADM.AFC_LOG_VW
         * database view. To update the list of AFC logs in the
         * wrap panel properly, a lock object must be used to add
         * items to the _afclogs list. However, the AFC logs list only
         * updates as changes occur to the database view when bound to
         * a ReadOnlyObservableCollection, hence this approach is used.
         ******************************************************************/

            _afcLogsRO = new ReadOnlyObservableCollection<AFCLog>(_afclogs);

            BindingOperations.EnableCollectionSynchronization(_afcLogsRO, _lockObj);


            
            // Call SearchForAFCLogs
            
            AsyncSearchForAFCLogs();


            /*******************************************************************************
             * Hook RefreshList and CreateCleanupRecord commands                           *
             * The AsyncRelayCommand is part of the Microsoft.Toolkit.Mvvm.Input namespace *
             * and allows developers to pass class methods to ICommand implementations to  *
             * be called from custom button controls on the xaml UI.                       *
             * *****************************************************************************/

            RefreshListCommand = new AsyncRelayCommand( func => AsyncSearchForAFCLogs());

            CreateCleanupRecordCommand = new AsyncRelayCommand(func => AsyncSearchDCADTiles());

            /**********************************************************************************
             * Sets the active MapView and determines if it is null                           *
             * If null, then an error message is sent to the event log.                       *
             * *******************************************************************************/

            try
            {
                if (!VerifyParcelFabricInMap())
                {

                    MessageBox.Show("A parcel fabric layer does not exist in the map. Please add a parcel fabric layer and try again. ", "No Parcel Fabric in the Map", MessageBoxButton.OK);

                    ErrorLogs.WriteLogEntry(EventLogSourceName, "There was no parcel fabric layer in the map. Please add a parcel fabric and try again.", System.Diagnostics.EventLogEntryType.Error);

                }

                _mapView = MapView.Active;

                if (_mapView is null)
                {

                    MessageBox.Show("There was a problem accessing the active map view.", "Active Map View Cannot Be Accessed", MessageBoxButton.OK);

                    ErrorLogs.WriteLogEntry(EventLogSourceName, "There was a problem accessing the active map view.", System.Diagnostics.EventLogEntryType.Error);

                    

                }

            }

            catch (Exception ex)

            {

                ErrorLogs.WriteLogEntry(EventLogSourceName, ex.Message, System.Diagnostics.EventLogEntryType.Error);

            }
            
        }





        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            DockPane pane = FrameworkApplication.DockPaneManager.Find(DockPaneID);
            if (pane == null)
                return;

            pane.Activate();

            
        }

        #region Properties


        /// <summary>
        /// Property containing list of AFC logs
        /// that is bounds to the MVVM xaml dock pane.
        /// </summary>
        public ReadOnlyObservableCollection<AFCLog> AFCLogs
        {

            get { return _afcLogsRO; }


        }


        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Create a Record from an Existing AFC Log";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
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
                
                AsyncSearchForAFCLogs(_searchString);


            }
        }




        private AFCLog _selectedAFCLog;

        public AFCLog SelectedAFCLog
        {
            get { return _selectedAFCLog; }
            set { _selectedAFCLog = value; }

        }

        #endregion

        #region Methods

        #region Help Button Override

        /// <summary>
        /// Override the default behavior when the dockpane's help icon is clicked
        /// or the F1 key is pressed.
        /// </summary>
        protected override void OnHelpRequested()
        {
            System.Diagnostics.Process.Start(@"http://dcadwiki.dcad.org/dcadwiki/ArcGISPro-CreateAFCRecords");
        }

        #endregion

        #region Verify Parcel Fabric Exists in the Map

        /// <summary>
        /// This method gets a flattended list
        /// of map layers that are of ParcelLayer
        /// types. If the result is not null,
        /// a parcel fabric exists in the map.
        /// </summary>
        private bool VerifyParcelFabricInMap()
        {
            bool _parcelFabricExists = false;

            try
            {

                ParcelLayer myParcelFabricLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<ParcelLayer>().FirstOrDefault();

                //if there is no fabric in the map then bail

                if (myParcelFabricLayer == null)
                {

                    _parcelFabricExists = false;

                }

                else
                {
                    _parcelFabricExists = true;
                }


            }
            catch (Exception ex)
            {

                ErrorLogs.WriteLogEntry(EventLogSourceName, ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }


            return _parcelFabricExists;


        }

        #endregion

        #region Verify Parcel Fabric Exists in the Map

        /// <summary>
        /// This method gets a flattended list
        /// of map layers that are of ParcelLayer
        /// types. If the result is not null,
        /// a parcel fabric exists in the map.
        /// </summary>
        private bool VerifyAFCLogViewExistsInGDB()
        {
            bool _parcelFabricExists = false;
            //TODO: Complete this method
            try
            {

                ParcelLayer myParcelFabricLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<ParcelLayer>().FirstOrDefault();

                //if there is no fabric in the map then bail

                if (myParcelFabricLayer == null)
                {

                    _parcelFabricExists = false;

                }

                else
                {
                    _parcelFabricExists = true;
                }


            }
            catch (Exception ex)
            {

                ErrorLogs.WriteLogEntry(EventLogSourceName, ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }


            return _parcelFabricExists;


        }

        #endregion

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

        #region Search for DCAD Tiles
        /// <summary>
        /// Performs an intersecting
        /// spatial query on the map
        /// extent for DCAD tile
        /// numbers and adds these
        /// to the panel items
        /// read only observable
        /// collection.
        /// IMPORTANT: The tile layer
        /// must exist in the current
        /// map.
        /// </summary>
        /// <returns></returns>
        public async Task AsyncSearchDCADTiles()
        {
            /******************************
             *  Clear AFCLogs and add     *
             *  Tile Numbers in current   *
             *  extent to dockpane        *
             *****************************/
            await AsyncSearchForAFCLogs(_searchString, true);

            



        }

        #endregion

        #region Search for AFC Logs

        /// <summary>
        /// Update the list of AFC Logs given the current search text.
        /// If the AFC Log has already had a record created during the 
        /// current session, then skip this AFC log and do not add it
        /// to the Create Records Pane collection. 
        /// </summary>
        public async Task AsyncSearchForAFCLogs(string _searchString = Blank, bool _cleanupRecord = false)
        {
            if (AFCLogs.Count > 0)
            {

                ClearAFCLogsCollection();

            }

            await QueuedTask.Run(() =>
            {
                /************************************************************************
                 * Get a list of AFC Logs                                               *
                 * **********************************************************************
                 * If a cleanup record is requested, this will return tiles that could  *
                 * be used to create a cleanup record instead of an AFC log.            *
                 * *********************************************************************/

                if (_cleanupRecord)
                {

                    SearchingAFeatureLayer(_mapView);

                }

                else
                {

                    PopulateAFCLogCollection (_searchString);

                }


                // Search for AFC Logs
                
                // and apply search string
                
                // if provided

                // TODO: Fix the search capability (duplicates entries when backspacing?)
                
                IEnumerable<AFCLog> linqResults;

                if (_searchString != Blank)
                {
                    linqResults = _afclogs.Where(afc => afc.DOC_NUM.Contains(_searchString) || afc.SEQ_NUM.Contains(_searchString));

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

                /**********************************************
                 * Remove any items that are included in
                 * the records collection because these
                 * have already had a record created
                 * during this session.
                 * *******************************************/

                // Remove temporary observable collection

                _tempafclogs = null;
                


            });

            // Call NotifyPropertyChanged and pass in the AFCLogs property
            
            NotifyPropertyChanged(() => AFCLogs);


        }

        #endregion

        #region Populates AFCLog Collection

        /// <summary>
        /// Pulls data from the view retrieving
        /// AFC logs from the Mars database and
        /// puts them into an AFC Log collection
        /// for binding to the dock pane.
        /// </summary>
        /// <param name="_searchString">A set of characters used to filter
        /// the current AFC logs assigned to the authenticated user.</param>
        /// <returns></returns>
        public async Task PopulateAFCLogCollection(string _searchString)
        {
            // Define columns to be included in
            
            // query filter
            
            string _instNum = "INSTRUMENT_NUM";
            
            string _seqNum = "SEQ_NUM";
            
            string _afcLogID = "AFC_LOG_ID";
            
            int _afcCount = 0;
            
            string _whereClause = Blank;
            
            bool _acctNumBlank = false;
            
            int _docNumType = 1;         // Helps the SetForegroundColor method know the color type for DOC_NUM.
            
            int _acctNumTYpe = 2;        // Helps the SetForegroundColor method know the color type for ACCOUNT_NUM.

            // Define where clause based
            
            // on search string contents
            
            if (_searchString != Blank)
            {
                _whereClause = String.Format("{0} LIKE '%{1}%' OR {2} LIKE '%{1}%'", _instNum, _searchString, _seqNum);
            }

            // Multi-threaded synchronization
            
            //private Object _lockObj = new object();
            
            //BindingOperations.EnableCollectionSynchronization(_afclogsRO, _lockObj);

            try
            {
                await QueuedTask.Run(() => {

                    // Opening a Non-Versioned SQL Server instance.
                    DatabaseConnectionProperties connectionProperties =
                    CreateNewConnectionPropObj(EnterpriseDatabaseType.SQLServer, 
                    Authentication, Database, Instance, Version);

                    using (Geodatabase geodatabase = new Geodatabase(
                        connectionProperties))
                    {
                        using (Table table = geodatabase.OpenDataset<Table>(AFCView))
                        {

                            QueryFilter queryFilter = new QueryFilter
                            {
                                WhereClause = _whereClause,

                                SubFields = "*",

                                PostfixClause = String.Format("ORDER BY {0} ASC", _afcLogID)
                            };

                            using (RowCursor rowCursor = table.Search(queryFilter, false))
                            {


                                /* ***********************************
                                 * Search through returned rows from *
                                 * query filter and create a new     *
                                 * AFC Log object and bind to the    *
                                 * AFCLogs observable collection.    *
                                 * ***********************************/
                                while (rowCursor.MoveNext())
                                {
                                    using (Row row = rowCursor.Current)
                                    {
                                        AFCLog afcLog = new AFCLog();

                                        // Determine AFC NOTE length and truncate if longer than 35 chars

                                        int _afcNoteLength = Convert.ToString(row["AFC_NOTE"]).Length;

                                        if (_afcNoteLength > 35) _afcNoteLength = 35;

                                        afcLog.AFC_LOG_ID = Convert.ToInt32(row["AFC_LOG_ID"]);

                                        afcLog.AFC_STATUS_CD = Convert.ToInt32(row["AFC_STATUS_CD"]);

                                        afcLog.AFC_TYPE_CD = Convert.ToInt32(row["AFC_TYPE_CD"]);

                                        afcLog.AFC_YEAR = Convert.ToInt32(row["AFC_YEAR"]);

                                        afcLog.AFC_NOTE = Convert.ToString(row["AFC_NOTE"]).Substring(0, _afcNoteLength);

                                        afcLog.TILE_NO = Convert.ToInt32(row["TILE_NO"]);

                                        afcLog.DRAFTER_EMPL_ID = Convert.ToString(row["DRAFTER_EMPL_ID"]);

                                        afcLog.DRAFTER_COMP_DT = Convert.ToDateTime(row["DRAFTER_COMP_DT"]);

                                        afcLog.FILE_DATE = Convert.ToDateTime(row["FILE_DATE"]);

                                        afcLog.EFFECTIVE_DT = Convert.ToDateTime(row["EFFECTIVE_DT"]);

                                        afcLog.INSTRUMENT_NUM = Convert.ToString(row["INSTRUMENT_NUM"]);

                                        afcLog.SEQ_NUM = Convert.ToString(row["SEQ_NUM"]);

                                        afcLog.RUSH_IND = Convert.ToString(row["RUSH_IND"]) == Yes ? true : false;

                                        // Determine if Account Number is provided

                                        if (Convert.ToString(row["ACCOUNT_NUM"]).Equals(Blank)) _acctNumBlank = true;

                                        if (!_acctNumBlank) afcLog.ACCOUNT_NUM = Convert.ToString(row["ACCOUNT_NUM"]);

                                        //afcLog.ACCT_LIST = Convert.ToString(row["ACCT_LIST"]);

                                        afcLog.DOC_TYPE = Convert.ToString(row["DOC_TYPE"]);

                                        afcLog.SetImageSource();    // Method sets the image source for the afc log type

                                        afcLog.SetDocumentNumber(); // Method sets the document number for the afc log type

                                        afcLog.SetRecordType();     // Method sets the record type for the afc log

                                        // Set the record status based on

                                        // the AFC status code

                                        afcLog.SetRecordStatus();   // Method that sets the record status for the afc log

                                        /***************************************
                                        * Subscribe to AFCRecordCreated Event  *
                                        * in the AFCRecord class.              *
                                        * *********************************** */

                                        afcLog.AFCRecordCreatedEvent += OnAFCRecordCreated;

                                        /***********************************************
                                        * Set the foreground color for the document    *
                                        * and account number properties based on the   *
                                        * RUSH_IND (if yes == RED else Black/Gray      *
                                        * *********************************************/

                                        afcLog.SetForegroundColor(_docNumType);

                                        afcLog.SetForegroundColor(_acctNumTYpe);

                                        _afcCount += 1;             // Increment afc count variable

                                        // Reads and Writes should be made from within the lock

                                        lock (_lockObj)
                                        {
                                            _afclogs.Add(afcLog);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // If completed, add psuedo afc log if count is zero
                    
                    AddEmptyListAFCLog(_afcCount);
                });
            }
            catch (GeodatabaseFieldException fieldException)
            {
                // One of the fields in the where clause might not exist. 
                
                // There are multiple ways this can be handled:
                
                // Handle error appropriately
                
                ErrorLogs.WriteLogEntry(EventLogSourceName, fieldException.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            catch (Exception exception)
            {
                ErrorLogs.WriteLogEntry(EventLogSourceName, exception.Message, System.Diagnostics.EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// When a record is created, the dock pane should
        /// refresh and remove the newly created AFC log
        /// from the observable collection based on
        /// the database view. The database view involves
        /// a join on the records feature class and removes
        /// any AFC Logs whose document number matches an
        /// existing record in the feature class.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAFCRecordCreated(object sender, AFCRecordCreatedEventArgs e)
        {
            try
            {
                if (e.RecordType == CleanupRecordType)
                {
                    SearchingAFeatureLayer(_mapView);
                }

                else
                {

                    AsyncSearchForAFCLogs();

                }


            }
            catch (Exception exception)
            {

                ErrorLogs.WriteLogEntry(EventLogSourceName, exception.Message, System.Diagnostics.EventLogEntryType.Error);
            }

        }



        #region Add Empty Result List AFC Log
        /// <summary>
        /// Adds a psuedo afc log entry to the
        /// observable collection to send a message
        /// to the user to assign and afc log.
        /// </summary>
        /// <param name="_afcCount"></param>
        private void AddEmptyListAFCLog(int _afcCount)
                {
        
                    /***********************************************
                    * DEFAULT BEHAVIOR WHEN NO AFC LOG ASSIGNED   *
                    * *********************************************
                    * Displays a default afc log row in the       *
                    * observable collection. See details in       *
                    * the CleanupRecord class.                           *
                    * This type of afc log is not valid, but just *
                    * displays an empty marker with message.      *
                    **********************************************/
        
                    if (_afcCount == 0)
                    {
                        AFCLog afcLog = new AFCLog();
                        
                        afcLog.AFC_LOG_ID = 1;
                        
                        afcLog.AFC_STATUS_CD = 99;
                        
                        afcLog.VALID_AFC_LOG = false;

                        afcLog.SetImageSource();

                        afcLog.SetDocumentNumber();


                lock (_lockObj)
                        {
                            _afclogs.Add(afcLog);
                        }

                
                    }
        
        
                }
        #endregion

        #region Create New Connection Properties Object

        /// <summary>
        /// Creates a new connection
        /// properties object
        /// from defined parameters
        /// for the enterprise
        /// database connection.
        /// </summary>
        /// <param name="dbms">The RDBMS type (e.g., SQL Server, PostgresSQL, etc.).</param>
        /// <param name="auth">The authentication mode (e.g., OSA or Database).</param>
        /// <param name="db">The name of the database. </param>
        /// <param name="instance">The name of the database server instance.</param>
        /// <param name="version">The name of the geodatabase version.</param>
        /// <returns></returns>
        public DatabaseConnectionProperties CreateNewConnectionPropObj(
                        EnterpriseDatabaseType dbms,
                        AuthenticationMode auth,
                        string db,
                        string instance,
                        string version)
        {
            DatabaseConnectionProperties connectionProperties =
                new DatabaseConnectionProperties(dbms)
                {
                    AuthenticationMode = auth,

                    Database = db,

                    Instance = instance,

                    Version = version
                };

            return connectionProperties;
        }

        #endregion

        #endregion

        #region Searching a Feature Layer using SpatialQueryFilter

        /// <summary>
        /// Searches for DCAD tiles within
        /// the active MapView's extent and
        /// displays them in the dockpane.
        /// </summary>
        /// <param name="mapView">The active MapView.</param>
        /// <returns></returns>
        public async Task SearchingAFeatureLayer(MapView mapView)
        {

            // Feature Layer Variables
            string _tileNumField = "Number";                 // Tile Number field name
            string _cleanupRecordType = "CLEANUP RECORD";    // Defines the record type
            int _afcTypeCd = 4;                              // Not a legal change AFC Type code
            int _afcCount = 0;                               // Counter for total tiles added to pane
            bool _lyrExists;                                 // Boolean variable showing the Tile layer exists


            // This returns a collection of layers of the "name" specified.

            // You can use any Linq expression to query the collection.  

            _lyrExists = MapManagement.ConfirmTOCLayer(TileLayerName);


            // If the tile layer exists
            // then create a spatial filter
            // to query the layer
            if (_lyrExists)
            {

                await QueuedTask.Run(() => {

                    // Access tile layer
                    FeatureLayer dcadTileLayer = MapView.Active.Map.GetLayersAsFlattenedList()
                               .OfType<FeatureLayer>().Where(f => f.Name == TileLayerName).FirstOrDefault();

                        // Using a spatial query filter to find all features

                        // which intersect the current map extent.

                        SpatialQueryFilter spatialQueryFilter = new SpatialQueryFilter
                        {
                            
                            FilterGeometry = mapView.Extent,

                            SpatialRelationship = SpatialRelationship.Intersects

                        };

                        using (RowCursor tileCursor = dcadTileLayer.GetFeatureClass().Search(spatialQueryFilter, false))
                        {
                            
                        while (tileCursor.MoveNext())
                            {
                        
                            using (Feature feature = (Feature)tileCursor.Current)
                                {
                                    // Check if cleanup record exists

                                    // Add the tile name to the AFC logs
                                    
                                    // observable collection
                                    
                                    AFCLog afcLog = new AFCLog();

                                    afcLog.AFC_TYPE_CD = _afcTypeCd;
                                    
                                    afcLog.TILE_NO = Convert.ToInt32(feature[_tileNumField]);

                                    afcLog.DRAFTER_EMPL_ID = AFCLog.GetCurrentUser();

                                    afcLog.DOC_TYPE = _cleanupRecordType;
                                    
                                    afcLog.SetImageSource();    // Method sets the image source for the afc log type
                                    
                                    afcLog.SetDocumentNumber(); // Method sets the document number for the afc log type
                                    
                                    afcLog.SetRecordType();     // Method sets the record type for the afc log
                                    
                                    // Set the record status based on
                                    
                                    // the AFC status code
                                    
                                    afcLog.SetRecordStatus();   // Method that sets the record status for the afc log
                                    
                                    /***************************************
                                     * If a cleanup record already exists  *
                                     * for this tile and was created today *
                                     * having the same record name, then   *
                                     * color the text red and assign the   *
                                     * message to account number property  *
                                     * ************************************/



                                    /***************************************
                                    * Subscribe to AFCRecordCreated Event  *
                                    * in the AFCRecord class.              *
                                    * *********************************** */
                                    
                                    afcLog.AFCRecordCreatedEvent += OnAFCRecordCreated;
                                    
                                    _afcCount += 1;             // Increment afc count variable
                                    
                                    // Reads and Writes should be made from within the lock
                                    
                                    lock (_lockObj)
                                    {
                                        _afclogs.Add(afcLog);
                                    }
                                }
                            }
                        }
                });
            }
            else
            {
                MessageBox.Show($"The layer {TileLayerName} does not exist in the map. " +
                                $"Please check your layers and try again.");
            }
        }


        #endregion Searching a Feature Layer using SpatialQueryFilter




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
