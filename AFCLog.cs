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


using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace pro_createrecords_addin
{


    public class AFCLog
    {


        #region Constants

        private const string NOT_COMPLETED_DATE = "1900-01-01 00:00:00.000";
        private const char BACK_SLASH = '\\';
        private const string BLANK = "";

        #region Deed Types
        /* DEED TYPES *****************************************************/
        private const string AOH = "AFFIDAVIT OF HEIRSHIP";
        private const string AMD = "AMENDMENT TO DECLARATION OF CONDOMINUM";
        private const string ALD = "ASSESSMENT LIEN DEED";
        private const string BOS = "BILL OF SALE";
        private const string COD = "CONDOMINIUM DECLARATION";
        private const string CND = "CONSTABLE DEED";
        private const string CFD = "CONTRACT FOR DEED";
        private const string COS = "CONTRACT OF SALE";
        private const string CON = "CONVEYANCE";
        private const string DCA = "DEDICATION";
        private const string DED = "DEED";
        private const string EAS = "EASEMENT";
        private const string GWD = "GENERAL WARRANTY DEED";
        private const string GFT = "GIFT DEED";
        private const string JDG = "JUDGEMENT";
        private const string ORD = "ORDINANCE";
        private const string PLT = "PLAT";
        private const string QCD = "QUIT CLAIM DEED";
        private const string ROW = "RIGHT OF WAY";
        private const string RWD = "RIGHT OF WAY DEED";
        private const string SHD = "SHERRIF'S DEED";
        private const string TRM = "TERMINATION";
        private const string TRD = "TRUSTEE DEED";
        private const string TRS = "TRUSTEE'S/SUBSTITUTE TRUSTEE'S DEED";
        private const string WAD = "WARRANTY DEED";
        private const string WD2 = "WD (AKA WARRANTY DEED)";
        #endregion

        #endregion

        #region Fields
        /// <summary>
        /// This class will use private variables as
        /// backing fields and get or set values via
        /// property getters and setters.
        /// </summary>
        private bool _recordCreated;      // If the record has been created for this AFC
        private int _afcLogID;            // AFC Log unique identifier (e.g., 321548)
        private int _afcYear;             // Year the AFC log was created
        private int _afcTypeCd;           // AFC Type Code (1-Addition, 2-Split, or 3-Research)
        private int _afcStatusCd;         // AFC Status Code (1-Active, 2-Completed, 
                                          //                  3-Pending, 4-Cert-Hold, 
                                          //                  5-Deleted, 6-Quality Control, or
                                          //                  7-Corrections)
        private string _afcNote;          // User defined AFC description
        private DateTime _fileDt;         // Recorded or filed date of the instrument
        private DateTime _effectiveDt;    // Effective date of the instrument
        private DateTime _specCompDt;     // Completion date of AFC Log AKA DRAFTER_COMP_DT
        private string _specialistID;     // AKA DRAFTER_EMPL_ID in AFC_LOG table or Specialist User ID
        private string _instrumentNum;    // Instrument Number (e.g., 202100012345)
        private string _accountNum;       // The parent account number
        private string _acctList;         // List of additional parent account numbers
        private int _tileNo;              // Tile Number (e.g., 174)
        private string _seqNum;           // Sequence Number (e.g., 0520-01)
        private bool _rush;               // Indicates if the AFC Log is critical
        private string _docImage;         // Image to symbol image for AFC log type
        private string _docNum;           // Instrument number or sequence number depending on the AFC Type
        private string _docType;          // Description of the deed type from DEED_MAIN table
        private int _recordType;          // Variable that holds the record type based on the doc type


        #endregion

        public AFCLog()
        {

            #region Constructor

            /// Initialize Fields
            /// Many of these fields are initialized
            /// with meaningless default values.
            /// True values will be populated later.
            _recordCreated = false;
            _afcLogID = 0;
            _afcTypeCd = 0;
            _afcStatusCd = 0;
            _afcNote = BLANK;
            _fileDt = DateTime.Now;
            _effectiveDt = DateTime.Now;
            _specCompDt = DateTime.Now;
            _specialistID = BLANK;
            _instrumentNum = BLANK;
            _accountNum = BLANK;
            _acctList = BLANK;
            _tileNo = 0;
            _seqNum = BLANK;
            _rush = false;
            _docImage = BLANK;
            _docNum = BLANK;
            _docType = BLANK;
            _recordType = 0;

            #endregion

        }

        #region Properties

        /// <summary>
        /// AFC Log Id that uniquely identifies the AFC log
        /// </summary>
        public int AFC_LOG_ID
        {

            get { return _afcLogID; }
            set { _afcLogID = value; }

        }

        /// <summary>
        /// Year the AFC log was created
        /// </summary>
        public int AFC_YEAR
        {
            get { return _afcYear; }
            set { _afcYear = value; }
        }

        /// <summary>
        /// The parent account number
        /// </summary>
        public string ACCOUNT_NUM
        {
            get { return _accountNum; }
            set { _accountNum = value; }
        }


        /// <summary>
        /// AFC Type determines if an AFC is Addition, Split, or Research.
        /// </summary>
        public int AFC_TYPE_CD
        {
            get { return _afcTypeCd; }
            set { _afcTypeCd = value; }
        }

        /// <summary>
        /// AFC Status code describing the state of the log.
        /// May be Active, Completed, Pending, Cert-Hold, Deleted, Quality Control, or Corrections.
        /// </summary>
        public int AFC_STATUS_CD
        {
            get { return _afcStatusCd; }
            set { _afcStatusCd = value; }
        }

        /// <summary>
        /// AFC Note is a general description
        /// of the AFC log.
        /// </summary>
        public string AFC_NOTE
        {
            get { return _afcNote; }
            set { _afcNote = value; }
        }

        /// <summary>
        /// File date of the instrument.
        /// </summary>
        public DateTime FILE_DATE
        {
            get { return _fileDt; }
            set { _fileDt = value; }
        }

        /// <summary>
        /// Date when the recorded document becomes effective.
        /// </summary>
        public DateTime EFFECTIVE_DT
        {
            get { return _effectiveDt; }
            set { _effectiveDt = value; }
        }

        /// <summary>
        /// Date when the specialist completed the AFC log.
        /// AKA DRAFTER_COMP_DT in AFC_LOG table.
        /// </summary>
        public DateTime DRAFTER_COMP_DT
        {
            get { return _specCompDt; }
            set { _specCompDt = value; }
        }

        /// <summary>
        /// Specialist ID or username.
        /// </summary>
        public string DRAFTER_EMPL_ID
        {
            get { return _specialistID; }
            set { _specialistID = value; }
        }


        /// <summary>
        /// The document or instrument number included in the log.
        /// </summary>
        public string INSTRUMENT_NUM
        {
            get { return _instrumentNum; }
            set { _instrumentNum = value; }
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
        /// AFC Log sequence number for Research Form types
        /// </summary>
        public string SEQ_NUM
        {
            get { return _seqNum; }
            set { _seqNum = value; }

        }

        /// <summary>
        /// Indicates if the AFC log is critical 
        /// and should be processed immediately
        /// </summary>
        public bool RUSH_IND
        {
            get { return _rush; }
            set { _rush = value; }
        }


        /// <summary>
        /// String list of additional parent 
        /// account numbers.
        /// </summary>
        public string ACCT_LIST
        {
            get { return _acctList; }
            set { _acctList = value; }
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
        /// The document number 
        /// later defined by the AFC Type.
        /// </summary>
        public string DOC_NUM
        {
            get { return _docNum; }
            set { _docNum = value; }
        }

        /// <summary>
        /// The type of deed. This
        /// will help to define 
        /// the record type.
        /// </summary>
        public string DOC_TYPE
        {
            get { return _docType; }
            set { _docType = value; }
        }

        /// <summary>
        /// Stores the integer value
        /// of the record type. Based
        /// on DOC_TYPE property and
        /// represents the coded value
        /// for the RecordType domain
        /// in the Records feature class.
        /// </summary>
        public int RECORD_TYPE
        {
            get { return _recordType; }
            set { _recordType = value; }
        }



        #endregion

        #region Methods
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

        /// <summary>
        /// Determines the document image
        /// based on the type and status of 
        /// AFC
        /// </summary>
        public void SetImageSource()
        {

            /* First check to see if the AFC Status is Active */

            if (_afcStatusCd == 1)
            {
                switch (_afcTypeCd)
                {
                    case 1:                                     // Addition
                        _docImage = "Images/addition_document_64px.png";
                        break;


                    case 2:                                     // Split
                        _docImage = "Images/split_document_64px.png";
                        break;

                    case 3:                                     // Research
                        _docImage = "Images/research_document_64px.png";
                        break;

                    default:                                     // Not Provided
                        _docImage = "Images/no_document_64px.png";
                        break;
                }
            }
            else /* If not, then the AFC status is Cert-Hold */
            {
                _docImage = "Images/no_document_64px.png";       // Cert Hold
            }
        }


        /// <summary>
        /// Determines the document number
        /// based on the type of AFC
        /// </summary>
        public void SetDocumentNumber()
        {

            switch (_afcTypeCd)
            {
                case 1:                                     // Addition
                    _docNum = _instrumentNum;
                    break;


                case 2:                                     // Split
                    _docNum = _instrumentNum;
                    break;

                case 3:                                     // Research
                    _docNum = String.Format("{0}-{1}",_afcYear.ToString(), _seqNum);
                    break;

                default:                                    // Not provided
                    _docNum = "Assign an AFC log...";
                    break;
            }
        }

        /// <summary>
        /// Sets the record type based 
        /// on the document type.
        /// </summary>
        public void SetRecordType()
        {
            switch (_docType)
            {
                case AOH:
                    _recordType = 1;
                    break;

                case AMD:
                    _recordType = 2;
                    break;

                case ALD:
                    _recordType = 3;
                    break;

                case BOS:
                    _recordType = 4;
                    break;

                case COD:
                    _recordType = 5;
                    break;

                case CND:
                    _recordType = 6;
                    break;

                case CFD:
                    _recordType = 7;
                    break;

                case COS:
                    _recordType = 8;
                    break;

                case CON:
                    _recordType = 9;
                    break;

                case DCA:
                    _recordType = 10;
                    break;

                case DED:
                    _recordType = 11;
                    break;

                case EAS:
                    _recordType = 12;
                    break;

                case GWD:
                    _recordType = 13;
                    break;

                case GFT:
                    _recordType = 14;
                    break;

                case JDG:
                    _recordType = 15;
                    break;

                case ORD:
                    _recordType = 16;
                    break;

                case PLT:
                    _recordType = 17;
                    break;

                case QCD:
                    _recordType = 18;
                    break;

                case ROW:
                    _recordType = 19;
                    break;

                case RWD:
                    _recordType = 20;
                    break;

                case SHD:
                    _recordType = 21;
                    break;

                case TRM:
                    _recordType = 22;
                    break;

                case TRD:
                    _recordType = 23;
                    break;

                case TRS:
                    _recordType = 24;
                    break;

                case WAD:
                    _recordType = 25;
                    break;

                case WD2:
                    _recordType = 26;
                    break;

                default:
                    break;
            }
        }

        #endregion


    }
}




