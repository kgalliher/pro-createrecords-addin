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
using DCAD.GIS;
using pro_createrecords_addin.Commands;
using pro_createrecords_addin.DataProviders;

namespace pro_createrecords_addin
{

    internal class CreateRecordsPaneViewModel : DockPane, INotifyPropertyChanged
    {

        #region Constants

        private const string DockPaneID  = "pro_createrecords_addin_CreateRecordsPane";



        private static readonly string Instance = "DCADSQLVM02";

        private static readonly string Database = "GEDT";

        private const AuthenticationMode Authentication = AuthenticationMode.OSA;

        private static readonly string Version = "dbo.DEFAULT";

        private static readonly string AFCView = "ADM.AFC_LOG_VW";

        private const string Yes = "Y";

        private const string Blank = "";



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

        PotentialRecordProvider potentialRecs = new PotentialRecordProvider(PotentialRecord.PotentialRecordType.AFC);
        private ReadOnlyObservableCollection<PotentialRecord> _potentialRecordsRO;

        private MapView _mapView;

        private Object _lockObj = new object();

         /*************************************************************************************************
         * Public ICommand Implementations for Custom WPF Buttons. This allows the application to call    *
         * existing methods in the ViewModel from the button using AsyncRelayCommand.                     *
         * (1) RefreshListCommand - Refreshes the potential record list.                                  *
         * (2) CreateCleanupRecordCommand - Creates a new parcel fabric records of the cleanup type.      *
         *     This is a custom record with specific attributes applied automatically when the workflow   *
         *     involves cleaning up GIS data only and no legal document is triggering a parcel change.   *
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
            
            _potentialRecordsRO = new ReadOnlyObservableCollection<PotentialRecord>(potentialRecs.PotentialRecords);

            BindingOperations.EnableCollectionSynchronization(_potentialRecordsRO, _lockObj);


            
            // Call SearchForAFCLogs
            
            await SearchPotentialRecordsAsync();


            /*******************************************************************************
             * Hook RefreshList and CreateCleanupRecord commands                           *
             * The AsyncRelayCommand is part of the Microsoft.Toolkit.Mvvm.Input namespace *
             * and allows developers to pass class methods to ICommand implementations to  *
             * be called from custom button controls on the xaml UI.                       *
             * *****************************************************************************/

            RefreshListCommand = new AsyncRelayCommand( func => potentialRecs.SearchPotentialRecordsAsync());

            CreateCleanupRecordCommand = new AsyncRelayCommand(func => AsyncSearchDCADTiles());

            /**********************************************************************************
             * Sets the active MapView and determines if it is null                           *
             * If null, then an error message is sent to the event log.                       *
             * *******************************************************************************/

            try
            {
                if (!Layers.VerifyParcelFabricInMap())
                {

                    MessageBox.Show("A parcel fabric layer does not exist in the map. Please add a parcel fabric layer and try again. ", "No Parcel Fabric in the Map", MessageBoxButton.OK);

                    OS.WriteLogEntry(OS.EventLogSourceName, "There was no parcel fabric layer in the map. Please add a parcel fabric and try again.", System.Diagnostics.EventLogEntryType.Error);

                }

                _mapView = MapView.Active;

                if (_mapView is null)
                {

                    MessageBox.Show("There was a problem accessing the active map view.", "Active Map View Cannot Be Accessed", MessageBoxButton.OK);

                    OS.WriteLogEntry(OS.EventLogSourceName, "There was a problem accessing the active map view.", System.Diagnostics.EventLogEntryType.Error);

                    

                }

            }

            catch (Exception ex)

            {

                OS.WriteLogEntry(OS.EventLogSourceName, ex.Message, System.Diagnostics.EventLogEntryType.Error);

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
        /// Property containing list of potential records
        /// that is bounds to the MVVM xaml dock pane.
        /// </summary>
        public ReadOnlyObservableCollection<PotentialRecord> PotentialRecords
        {

            get { return _potentialRecordsRO; }


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
        /// Search string used to limit the returned AFC logs.
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

                // When the user enters values into
                // the search box, this method
                // will update the resulting list
                
                await PotentialRecordProvider.SearchPotentialRecordsAsync(_searchString);


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
            await SearchPotentialRecordsAsync(_searchString, true);

            



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
