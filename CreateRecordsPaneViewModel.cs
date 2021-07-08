#region "CLASS DOCUMENTATION"
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
 *               _afclogs - The ObservableCollection object that is manipulated
 *                          in the SearchForAFCLogs() method. The database view
 *                          GEDT.ADM.AFC_LOG_VW is read using a QueryFilter and
 *                          the resulting RowCursor object populates AFC Log class
 *                          objects and adds them to this collection. This property
 *                          is manipulated in the business logic but is not the ultimate
 *                          data source. This property is bounds to the _afclogsRO property
 *                          which is returned by the AFCLogs property bound to the dock pane
 *                          control in the xaml.
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
using System.Windows;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Editing;

namespace pro_createrecords_addin
{

    internal class CreateRecordsPaneViewModel : DockPane, INotifyPropertyChanged
    {

        #region Constants

        private const string _dockPaneID  = "pro_createrecords_addin_CreateRecordsPane";
        private const string _instance = "DCADSQLVM02";
        private const string _database = "GEDT";
        private const AuthenticationMode _authentication = AuthenticationMode.OSA;
        private const string _version = "dbo.DEFAULT";
        private const string _afcView = "ADM.AFC_LOG_VW";
        private const string _yes = "Y";
        private const string _blank = "";
        private ObservableCollection<AFCLog> _afclogs = new ObservableCollection<AFCLog>();
        private ReadOnlyObservableCollection<AFCLog> _afclogsRO;
        private Object _lockObj = new object();

        /**************************************************************************************************
         * Public ICommand Implementations for Custom WPF Buttons. This allows the application to call    *
         * existing methods in the ViewModel from the button using AsyncRelayCommand.                     *
         * (1) RefreshListCommand - Refreshes the afc log list.                                           *
         * (2) CreateRecordCommand - Creates a new record based on selected AFC Log information.          *
         * (3) CreateCleanupRecordCommand - Creates a new parcel fabric records of the cleanup type.      *
         *     This is a custom record with specific attributes applied automatically when the workflow   *
         *     involves  cleaning up GIS data only and no legal document is triggering a parcel change.   *
         *************************************************************************************************/
        public ICommand RefreshListCommand { get; set; }
        public ICommand CreateRecordCommand { get; set; }
        public ICommand CreateCleanupRecordCommand { get; set; }


        #endregion

        public CreateRecordsPaneViewModel() 
        {
            //TODO: Check to ensure that a parcel fabric is
            // included in the current map and that the AFC Log View exists
            // in the geodatabase

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

            _afclogsRO = new ReadOnlyObservableCollection<AFCLog>(_afclogs);
            BindingOperations.EnableCollectionSynchronization(_afclogsRO, _lockObj);


            // Call SearchForAFCLogs
            AsyncSearchForAFCLogs();
            //DisplayTestMessage();

            /*******************************************************************************
             * Hook RefreshList and CreateCleanupRecord commands                           *
             * The AsyncRelayCommand is part of the Microsoft.Toolkit.Mvvm.Input namespace *
             * and allows developers to pass class methods to ICommand implementations to  *
             * be called from custom button controls on the xaml UI.                       *
             * *****************************************************************************/

            RefreshListCommand = new AsyncRelayCommand( func => AsyncSearchForAFCLogs());
            CreateRecordCommand = new AsyncRelayCommand( func => AsyncDisplayTestMessage());
            //CreateCleanupRecord = new AsyncRelayCommand(func => AsyncCreateCleanupRecord());


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

        #region Properties


        /// <summary>
        /// Property containing list of AFC logs
        /// that is bounds to the MVVM xaml dock pane.
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
            // TODO: Create a record for the selected AFC Log
            // CreateRecordForSelectedAFCLog();
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
        public async Task AsyncSearchForAFCLogs(string _searchString = _blank)
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
            int _afcCount = 0;
            string _whereClause = _blank;
            bool _acctNumBlank = false;

            // Define where clause based
            // on search string contents
            if (_searchString != _blank)
            {
                _whereClause = String.Format("{0} LIKE '%{1}%' OR {2} LIKE '%{1}%'", _instNum, _searchString, _seqNum);
            }

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
                                    afcLog.RUSH_IND = Convert.ToString(row["RUSH_IND"]) == _yes ? true : false;

                                    // Determine if Account Number is provided
                                    if (Convert.ToString(row["ACCOUNT_NUM"]).Equals(_blank)) _acctNumBlank = true;
                                    if (!_acctNumBlank) afcLog.ACCOUNT_NUM = Convert.ToString(row["ACCOUNT_NUM"]);

                                    //afcLog.ACCT_LIST = Convert.ToString(row["ACCT_LIST"]);
                                    afcLog.DOC_TYPE = Convert.ToString(row["DOC_TYPE"]);
                                    afcLog.SetImageSource();    // Method sets the image source for the afc log type
                                    afcLog.SetDocumentNumber(); // Method sets the document number for the afc log type
                                    afcLog.SetRecordType();     // Method sets the record type for the afc log
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

                    // If completed, add psuedo afc log if count is zero
                    AddEmptyListAFCLog(_afcCount);
                });
            }
            catch (GeodatabaseFieldException fieldException)
            {
                // One of the fields in the where clause might not exist. 
                // There are multiple ways this can be handled:
                // Handle error appropriately
                ErrorLogs.WriteLogEntry("Create Records Add-In", fieldException.Message, System.Diagnostics.EventLogEntryType.Error);
            }
            catch (Exception exception)
            {
                // logger.Error(exception.Message);
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
                    * the AFCLog class.                           *
                    **********************************************/
        
                    if (_afcCount == 0)
                    {
                        AFCLog afcLog = new AFCLog();
                        afcLog.AFC_LOG_ID = 1;
                        afcLog.AFC_STATUS_CD = 99;
                        afcLog.SetImageSource();
                        afcLog.SetDocumentNumber();
                        lock (_lockObj)
                        {
                            _afclogs.Add(afcLog);
                        }
                    }
        
        
                }
        #endregion



        #endregion

        #region Parcel Fabric Methods

        #region Create a New Record
            
            /// <summary>
            /// This asynchronous method creates
            /// a new record within the parcel fabric
            /// found in the active map. If a parcel
            /// fabric is not found it will display
            /// a message indicating the problem.
            /// </summary>
            /// <param name="_name"></param>
            /// <param name="_recordType"></param>
            /// <param name="_afcType"></param>
            /// <param name="_recordedDate"></param>
            /// <param name="_effectiveDate"></param>
            /// <param name="_recordStatus"></param>
            /// <returns></returns>
            public async Task AsyncCreateNewRecord(string _name, int _recordType, int _afcType, DateTime _recordedDate, DateTime _effectiveDate, int _recordStatus)
            {

                // TODO: Pass in record name, record type, afctype, 
                // recorded date, effective date, and record status

                string errorMessage = await QueuedTask.Run(async () =>
                {
                    Dictionary<string, object> RecordAttributes = new Dictionary<string, object>();
                    // TODO REMOVE: string sNewRecord = _name;

                    try
                    {
                        var myParcelFabricLayer = MapView.Active.Map.GetLayersAsFlattenedList().OfType<ParcelLayer>().FirstOrDefault();
                        //if there is no fabric in the map then bail
                        if (myParcelFabricLayer == null)
                            return "There is no fabric in the map.";
                        var recordsLayer = await myParcelFabricLayer.GetRecordsLayerAsync();
                        var editOper = new EditOperation()
                        {
                            Name = "Create Parcel Fabric Record",
                            ProgressMessage = "Create Parcel Fabric Record...",
                            ShowModalMessageAfterFailure = true,
                            SelectNewFeatures = false,
                            SelectModifiedFeatures = false
                        };
                        RecordAttributes.Add("Name", _name);
                        RecordAttributes.Add("RecordType", _recordType);
                        RecordAttributes.Add("RecordedDate", _recordedDate);
                        RecordAttributes.Add("EffectiveDate", _effectiveDate);
                        RecordAttributes.Add("AFCType", _afcType);
                        // TODO: Include additional record attributes here
                        // RecordAttributes.Add("Attribute01", sAttribute01);
                        // RecordAttributes.Add("Attribute02", sAttribute02)
                        // Etc...

                        var editRowToken = editOper.CreateEx(recordsLayer.FirstOrDefault(), RecordAttributes);
                        if (!editOper.Execute())
                            return editOper.ErrorMessage;
                
                        var defOID = -1;
                        var lOid = editRowToken.ObjectID.HasValue ? editRowToken.ObjectID.Value : defOID;
                        await myParcelFabricLayer.SetActiveRecordAsync(lOid);
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                    return "";
                });
                if (!string.IsNullOrEmpty(errorMessage))
                    MessageBox.Show(errorMessage, "Create New Record.");

        }



        #endregion

        #region Display Test Message

            public async Task AsyncDisplayTestMessage()
        {
            MessageBox.Show(String.Format("User: {0} clicked the image!", AFCLog.GetCurrentUser()));
        }

        #endregion


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
