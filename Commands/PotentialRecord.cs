using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace pro_createrecords_addin.Commands
{
    public class PotentialRecord
    {

        #region Constants

        private const string NOT_COMPLETED_DATE = "1900-01-01 00:00:00.000";
        private const char BACK_SLASH = '\\';
        private const string BLANK = "";
        private const string NO_ACCT_NUM = "NO ACCOUNT NUMBER";
        private const int NOT_A_LEGAL_CHANGE = 4;


        #endregion

        #region Fields
        /// <summary>
        /// This class will use private variables as
        /// backing fields and get or set values via
        /// property getters and setters.
        /// </summary>
        private bool _recordCreated;      // If the record has been created for this potential record
        private string _recordDesc;       // Record description
        private DateTime _recordedDt;     // Recorded or filed date of the instrument
        private DateTime _recordEffDt;    // Record Effective date of the instrument
        private string _recordAcctNum;    // The parent account number
        private int _tileNum;             // Tile Number (e.g., 174)
        private string _recordImage;      // Image to symbol image for potential record type
        private string _recordName;       // Instrument or sequence number or cleanup tile record based on the potential record type
        private int _recordType;          // Variable that holds the record type based on the doc type
        private int _recordStatus;        // Determines if the record's parcels should be published.
        private bool _validRecord;        // Boolean value that determines if the potential record is valid.
        private Color _recordNameColor;   // Color object foreground for the listbox item's record name text property .
        private Color _recordAcctColor;   // Color object to color the foreground for the listbox item's record account num text property.
        public enum PotentialRecordType { AFC, Tile}; // Enum that defines the type of potential record.

        #endregion


        #region Constructor
        public PotentialRecord()
        {


            _recordCreated     = false;
            _recordDesc        = BLANK;
            _recordedDt        = DateTime.Now;
            _recordEffDt       = DateTime.Now;
            _recordAcctNum     = BLANK;
            _tileNum           = 0;
            _recordImage       = BLANK;
            _recordName        = BLANK;
            _recordType        = 0;
            _validRecord       = true;
            _recordNameColor   = Color.FromRgb(0, 0, 0);
            _recordAcctColor   = Color.FromRgb(128, 128, 128);



        }

        #endregion

        #region Properties

        /// <summary>
        /// Boolean property stating
        /// if a record has been created
        /// for the potential record.
        /// </summary>
        public bool RecordCreated
        {
            get { return _recordCreated; }
            set { _recordCreated = value; }
        }

        /// <summary>
        /// String property defining a
        /// description of the potential
        /// record.
        /// </summary>
        public string RecordDescription { 
        
            get { return _recordDesc; }
            set { _recordDesc = value; }

        }

        /// <summary>
        /// DateTime property providing
        /// the date the potential record
        /// was recorded.
        /// </summary>
        public DateTime RecordedDate
        {

            get { return _recordedDt; }
            set { _recordedDt = value; }

        }


        /// <summary>
        /// DateTime property that contains
        /// the effective date of the potential
        /// record.
        /// </summary>
        public DateTime RecordEffectiveDate
        {

            get { return _recordEffDt; }
            set { _recordEffDt = value; }

        }

        /// <summary>
        /// String property that stores
        /// the account number for the
        /// potential record.
        /// </summary>
        public string RecordAccountNumber
        {

            get { return _recordAcctNum; }
            set { _recordAcctNum = value; }

        }

        /// <summary>
        /// Integer prperty that stores
        /// the tile number for the
        /// potential record.
        /// </summary>
        public int TileNumber
        {
           
            get { return _tileNum; }
            set { _tileNum = value; }

        }

        /// <summary>
        /// String property that stores
        /// the path to the icon image 
        /// for the potential record.
        /// </summary>
        public string RecordImage
        {

            get { return _recordImage; }
            set { _recordImage = value; }

        }

        /// <summary>
        /// String property that stores
        /// the name of the potential
        /// record.
        /// </summary>
        public string RecordName
        {

            get { return _recordName; }
            set { _recordName = value; }

        }

        /// <summary>
        /// Integer property that stores
        /// the value for the record
        /// type.
        /// </summary>
        public int RecordType
        {

            get { return _recordType; }
            set { _recordType = value; }

        }

        /// <summary>
        /// Integer property that stores
        /// the value for the record 
        /// status. This determines if
        /// the record will be published.
        /// </summary>
        public int RecordStatus
        {

            get { return _recordStatus; }
            set { _recordStatus = value; }

        }

        /// <summary>
        /// Boolean property that determines
        /// if the record is valid or if it
        /// is just a placeholder for 
        /// displaying information a message
        /// to the user.
        /// </summary>
        public bool ValidRecord
        {

            get { return _validRecord; }
            set { _validRecord = value; }

        }

        /// <summary>
        /// Color property that identifies
        /// the color of the text that will
        /// display as the potential record's
        /// name in the dock pane.
        /// </summary>
        public Color RecordNameColor
        {

            get { return _recordNameColor; }
            set { _recordNameColor = value; }

        }

        /// <summary>
        /// Color property that identifies
        /// the color of the text that will
        /// display as the potential record's
        /// account number in the dock pane.
        /// </summary>
        public Color RecordAcctColor
        {

            get { return _recordAcctColor; }
            set { _recordAcctColor = value; }

        }


        #endregion

    }
}
