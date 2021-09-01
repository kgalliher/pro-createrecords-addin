#region "CLASS DOCUMENTATION"
/*
 * ***************************************************************************************************************************************************************
 * Project:         pro-createrecords-addin
 * Class:           CleanupRecord.cs
 * Version:         1.0
 * Author:          John W. Fell
 * Date Created:    06/24/2021
 * Date Launched:   Date Project launched for general use (mm/dd/yyyy)
 * Dept:            GIS Division
 * Location:        https://github.com/dcadgis/pro-createrecords-addin/
 * Revisions:       
 * ***************************************************************************************************************************************************************
 * 
 * CLASS
 * PURPOSE:     Represents an AFC Log with properties and methods for
 *              creating records. 
 *
 * CLASS
 * DESCRIPTION: An AFC log translates fairly seamlessly
 *              with esri's definition of a parcel fabrci "record." This
 *              class acts as a container to organize properties and 
 *              methods that will convert AFC logs into records within the
 *              ArcGIS Pro SDK framework.There are a number of properties that
 *              are specific to an AFC log such as instrument or sequence number,
 *              file and effective dates, tile number, etc that unique identify the
 *              AFC log. Not only are properties stored within this class, but course
 *              and fine-grained methods help to represent AFC log information to users
 *              within an ArcGIS Pro SDK dockpane with a customize panel. The binding
 *              of the AFC Logs observable collection views each item as an AFC Log,
 *              object represented by this class, and helper methods faciliate creation
 *              of records, raising events, and disabling methods when certain criteria
 *              are met.
 *
 * CLASS
 * PROPERTIES:  
 *              AFC_LOG_ID     - The ID number of the AFC Log represented in Mars.
 *              AFC_YEAR       - The year the AFC log was created.
 *              ACCOUNT_NUM    - The parent account number for the AFC log.
 *              AFC_TYPE_CD    - The type of AFC log (1 - Addition, 2 - Split/Deed, or 3 - Research Form).
 *              AFC_STATUS_CD  - The status of the AFC log:
 *                                1 - Active:           The AFC log is able to be processed by GIS
 *                                2 - Completed:        The AFC log has been completed and sent to PRE for processing
 *                                3 - Peinding:         The AFC log is unable to be completed for some reason
 *                                4 - Cert-Hold:        The AFC log is waiting for completion of the certification cycle
 *                                5 - Deleted:          The AFC log was deleted and is no longer available
 *                                6 - Quality Control:  The AFC log is currently under review by GIS staff
 *                                7 - Corrections:      The AFC log is in need of corrections before sending to PRE
 *              FILE_DATE      - The date that the document triggering the AFC log was recorded or filed.
 *              AFC_NOTE       - The description of the AFC log supplied by the creator.
 *              DOC_IMAGE      - The path to the image displayed in the dock pane and defined by AFC_TYPE_CD.
 *              DOC_TYPE       - The type of recorded document (e.g., Warranty Deed or Plat).
 *              INSTRUMENT_NUM - The recorder's instrument number for the plat or deed.
 *              SEQ_NUM        - The DCAD supplied sequence number in MMYY-SS format where SS is the sequence of the 
 *                               research form (e.g., 01, 02, 03, etc.).
 *              DOC_NUM        - The number (instrument number or sequence number + AFC year) that will be used
 *                               as the derived record name for the AFC log.
 *              RUSH_IND       - Determines if the AFC log needs to be expedited.
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


using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace pro_createrecords_addin
{


    public class CleanupRecord
    {


        #region Constants

        private const int CleanupRecordType = 28;
        private const string NOT_COMPLETED_DATE = "1900-01-01 00:00:00.000";
        private const char BACK_SLASH = '\\';
        private const string BLANK = "";
        private const string NO_ACCT_NUM = "NO ACCOUNT NUMBER";
        private const int PublishRecordStatus = 1;
        private const int NotALegalChangeAFCType = 4;


        #endregion

        #region Fields
        /// <summary>
        /// This class will use private variables as
        /// backing fields and get or set values via
        /// property getters and setters.
        /// </summary>
        private bool _recordCreated;      // If the record has been created for this AFC
        private int _afcTypeCd;           // AFC Type Code (1-Addition, 2-Split, 3-Research, 4-Not a legal change)
        private int _tileNo;              // Tile Number (e.g., 174)
        private string _docImage;         // Image to symbol image for AFC log type
        private string _recordName;       // Record name based on Tile number and date
        private DateTime _cleanupDt;      // Date the cleanup record was created
        private int _recordType;          // Variable that holds the record type based on the doc type
        private int _recordStatus;        // Determines if the record's parcels should be published.
        private string _accountNum;       // Although no account number for this type of record, this will 
                                          // act as a place holder
        private Color _msgClrDocNum;      // Color object foreground for the listbox item's doc num text property .
        private Color _msgClrAcctNum;     // Color object to color the foreground for the listbox item's account num text property.

        #endregion


        #region Constructor

        public CleanupRecord()
        {



            /// Initialize Fields
            /// Many of these fields are initialized
            /// with meaningless default values.
            /// True values will be populated later.
            _recordCreated = false;
            _afcTypeCd = NotALegalChangeAFCType;
            _tileNo = 0;
            _docImage = BLANK;
            _recordName = BLANK;
            _cleanupDt = DateTime.Now;
            _recordType = 0;
            _recordStatus = 0;
            _accountNum = NO_ACCT_NUM;







        }

        #endregion






        #region Properties



        /// <summary>
        /// Boolean property stating
        /// if a record has been created
        /// for the AFC LOG.
        /// </summary>
        public bool RECORD_CREATED
        {
            get { return _recordCreated; }
            set { _recordCreated = value; }
        }


        /// <summary>
        /// String property containing
        /// the record name.
        /// </summary>
        public string RECORD_NAME
        {
            get { return _recordName; }
            set { _recordName = value; }
        }


        /// <summary>
        /// AFC Type determines if an 
        /// AFC is Addition, Split, Research or
        /// No legal document. This particular
        /// type does not create an AFC but is meant
        /// to populate the "no legal document"
        /// descriptor.
        /// </summary>
        public int AFC_TYPE_CD
        {
            get { return _afcTypeCd; }
            set { _afcTypeCd = value; }
        }

        /// <summary>
        /// Date when the cleanup
        /// record was created.
        /// </summary>
        public DateTime CLEANUP_DT
        {
            get { return _cleanupDt; }
            set { _cleanupDt = value; }
        }

        /// <summary>
        /// Tile number where parent account is located.
        /// </summary>
        public int TILE_NO
        {
            get { return _tileNo; }
            set { _tileNo = value; }
        }

        /// <summary>
        /// The path for the document image
        /// that will display in the list box
        /// for AFC logs.
        /// </summary>
        public string DOC_IMAGE
        {
            get { return _docImage; }
            set { _docImage = value; }
        }

        /// <summary>
        /// Stores the integer value
        /// of the record type. Based
        /// on the record type domain. 
        /// For cleanup records, the
        /// record type is cleanup and
        /// is not a legal document.
        /// The land records team calls
        /// these types of operations
        /// "Quality-Driven Work Flows."
        /// </summary>
        public int RECORD_TYPE
        {
            get { return _recordType; }
            set { _recordType = value; }
        }


        /// <summary>
        /// Determines whether the Record's 
        /// parcel's should be published.
        /// 1 - Publish
        /// 2 - Publish after certification
        /// 3 - Pending
        /// </summary>
        public int RECORD_STATUS
        {
            get { return _recordStatus; }
            set { _recordStatus = value; }
        }

        /// <summary>
        /// Returns the color object
        /// for the text foreground property
        /// of the text block in the MVVM.
        /// </summary>
        public Color MSG_COLOR_DOC_NUM
        {
            get { return _msgClrDocNum; }
            set { _msgClrDocNum = value; }
        }




        #endregion

        #region Methods


        #region Get Current User
        /// <summary>
        /// Identifies the authenticated user.
        /// </summary>
        /// <returns>The string containing the user name without the domain.</returns>
        public static string GetCurrentUser()
        {
            // Get the logged in user
#pragma warning disable CA1416 // Disable platform compatibility
            string authenticatedUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
#pragma warning restore CA1416 // Re-enable platform compatibility

            return authenticatedUser.Split(BACK_SLASH)[1];

        }
        #endregion

        #region Set Image Source
        /// <summary>
        /// Determines the document image
        /// based on the type and status of 
        /// AFC
        /// </summary>
        public void SetImageSource()
        {


            try
            {

                // Not a legal document
                _docImage = "Images/no_document_64px.png";

            }
            catch (Exception ex)
            {

                ErrorLogs.WriteLogEntry("Create Records Add-In: Set Image Source", ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }



        }
        #endregion

        #region Set Record Name
        /// <summary>
        /// Generates the record name
        /// based on the selection
        /// in the dockpane.
        /// </summary>
        public void SetRecordName()
        {
            //TODO: Complete this method

        }
        #endregion

        #region Set Cleanup Record Type
        /// <summary>
        /// Sets the record type. 
        /// For this class there is only one
        /// record type (Cleanup Records). 
        /// </summary>
        public void SetCleanupRecordType()
        {
            try
            {
                // Cleanup record type

                _recordType = CleanupRecordType;
            }
            catch (Exception ex)
            {

                ErrorLogs.WriteLogEntry("Create Records Add-In: Set Cleanup Record Type", ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }

        }
        #endregion

        #region Set Record Status

        /// <summary>
        /// Applies a publish record status if
        /// the AFC Log is not marked as cert hold,
        /// otherwise, set the record status to 
        /// publish after certification.
        /// </summary>
        public void SetRecordStatus()
        {
            _recordStatus = PublishRecordStatus;
        }
        #endregion

        #region Set Foreground Color

        /// <summary>
        /// Returns a color object based on 
        /// the property type and if the
        /// AFC log is marked as a rush.
        /// </summary>
        /// <param name="_foregroundType"></param>
        /// <returns></returns>
        public void SetForegroundColor(int _foregroundType)
        {

            /**********************************************
            * Define colors for dock pane text         ****
            **********************************************/

            try
            {

                // Cleanup Record Name (Black)

                _msgClrDocNum = Color.FromRgb(0, 0, 0);

            }
            catch (Exception ex)
            {

                ErrorLogs.WriteLogEntry("Create New Record Add-In: Set Cleanup Record Foreground Color", ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }

        }


        #endregion




        #region Parcel Fabric Methods

        #region Create a New Cleanup Record

        /// <summary>
        /// This asynchronous method creates
        /// a new record within the parcel fabric
        /// found in the active map. If a parcel
        /// fabric is not found it will display
        /// a message indicating the problem.
        /// </summary>

        /// <returns></returns>
        public async Task AsyncCreateNewCleanupRecord()
        {

            try
            {
                MessageBox.Show("You clicked the button!");

                // Pass in record name, record type, afctype, 
                // recorded date, effective date, and record status
                string _name = this.RECORD_NAME;
                int _recordType = this.RECORD_TYPE;
                int _afcType = this.AFC_TYPE_CD;
                DateTime _recordedDate = this.CLEANUP_DT;
                DateTime _effectiveDate = this.CLEANUP_DT;
                int _recordStatus = this.RECORD_STATUS;


                string errorMessage = await QueuedTask.Run(async () =>
                {
                    Dictionary<string, object> RecordAttributes = new Dictionary<string, object>();

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

                        /**********************************************
                        * Assign local variables to record attributes *
                        * ********************************************/
                        RecordAttributes.Add("Name", _name);
                        RecordAttributes.Add("RecordType", _recordType);
                        RecordAttributes.Add("RecordedDate", _recordedDate);
                        RecordAttributes.Add("EffectiveDate", _effectiveDate);
                        RecordAttributes.Add("AFCType", _afcType);
                        RecordAttributes.Add("RecordStatus", _recordStatus);

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
                {
                    ErrorLogs.WriteLogEntry("Create New Record Add-In: Create New Record", errorMessage, System.Diagnostics.EventLogEntryType.Error);
                }
                else
                {
                    MessageBox.Show(String.Format("Created Cleanup Record: {0} in Tile: {1}.", _name, _tileNo));
                }


            }
            catch (Exception ex)
            {

                ErrorLogs.WriteLogEntry("Create New Record Add-In: Create New Record", ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }

            finally
            {
                /*************************************
                * The RecordCreatedEvent is          *
                * raised and notifies the View       *
                * Model to refresh the dock pane     *
                * and displaying AFC logs from the   *
                * updated database view.             *
                * ***********************************/
                RaiseCleanupRecordCreatedEvent(this);

            }



        }



        #endregion


        #endregion

        #region MapView Methods

        /// MapView.ZoomToSelectedAsync(Timespan?)
        /// <example>
        /// <code title="Zoom To Selected" description="Zoom to the map's selected features." region="Zoom To Selected Synchronous" source="..\..\ArcGIS\SharedArcGIS\SDK\Examples\ArcGIS.Desktop.Mapping\MapExploration\MapView_Examples.cs" lang="CS"/>
        /// </example>
        #region Zoom To Selected Synchronous
        public Task<bool> ZoomToSelectedAsync()
        {
            return QueuedTask.Run(() =>
            {
            //Get the active map view.
            var mapView = MapView.Active;
                if (mapView == null)
                    return false;

            //Zoom to the map's selected features.
            return mapView.ZoomToSelected();
            });
        }
        #endregion

        #endregion

        #region Events

        #region CleanupRecordCreated Event
        /// <summary>
        /// Used to identify when a record
        /// is created using the Create New
        /// Record method.
        /// </summary>
        public event EventHandler<CleanupRecordCreatedEventArgs> CleanupRecordCreatedEvent;
        #endregion

        #endregion

        #region Display Test Message
        /// <summary>
        /// Asynchronous method that
        /// displays a message to the
        /// user.
        /// </summary>
        /// <returns></returns>
        public async Task AsyncDisplayTestMessage()
        {
            try
            {

                await QueuedTask.Run(() =>
                {
                    var dialogResult = MessageBox.Show(String.Format("User: {0} clicked the image!", GetCurrentUser()));
                    if (dialogResult == System.Windows.MessageBoxResult.OK)
                    {
                        MessageBox.Show("Ok Clicked");
                    }
                    else
                    {
                        MessageBox.Show("Cancel Clicked");
                    }
                });

            }
            catch (Exception ex)
            {

                ErrorLogs.WriteLogEntry("Create Records Add-In", ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }


        }

        #endregion


        #region Raise Cleanup Record Created Event

        /// <summary>
        /// Raise the RecordCreated event.
        /// </summary>
        /// <param name="_afclog"></param>
        public void RaiseCleanupRecordCreatedEvent(CleanupRecord _record)
        {
            CleanupRecordCreatedEventArgs args = new CleanupRecordCreatedEventArgs();
            args.RecordName = _record.RECORD_NAME;
            args.DateCreated = DateTime.Now;
            CleanupRecordCreatedEvent?.Invoke(_record, args);
        }

        #endregion

        #endregion



    }

    public class CleanupRecordCreatedEventArgs
    {
        public string RecordName { get; set; }
        public DateTime DateCreated { get; set; }

    }

}











