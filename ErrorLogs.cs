using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pro_createrecords_addin
{

    #region Class Documentation

    // Language: C#
    // Name:     ErrorLogs.cs								     	     
    // Author:   John W. Fell									     	
    // Dept:     GIS/Applications 								     
    // Date:     12/20/2018 07:16:00 AM
    // Location: \\DCAD.org\GIS\
    //**************************************************************************
    //*                                                                        *
    //*  PURPOSE:      The following subroutines are implemented               *
    //*                as the portion of this project for error                *
    //*                logging in the windows event viewer.                    *
    //*                To learn more about the windows                         *
    //*                event viewer go to this link:                           *
    //*                <https://bit.ly/2RbhKLA>                                *
    //*                                                                        *
    //*                                                                        *
    //*                Techniques for error logging are                        *
    //*                gleaned from the MSDN Article:                          *
    //*                EventLog.WriteEntry Method.                             *
    //*                <https://bit.ly/2By9mM8>                                *
    //*                                                                        *
    //*                Also, the technical article                             *
    //*                "EventLog Class" was helpful.                           *
    //*                <https://bit.ly/2rNDAXi>                                *
    //*                                                                        *
    //*  PUBLIC                                                                *
    //*  CONSTANTS:    eventLogSourceName - Name of the application log        *
    //*                source.                                                 *
    //*                                                                        *
    //*                                                                        *
    //*  METHODS:                                                              *
    //*                                                                        *
    //*                WriteLogEntry - Inserts information from a              *
    //*                try catch block exception (passed in as                 *
    //*                parameters to the method) into the event log.           *
    //*                The Write Entry method allows you to enter the          *
    //*                log entry and the type of message. Log entry            * 
    //*                types include:                                          *
    //*                Error, Failure/Success Audit,                           *
    //*                Information and Warning.                                *
    //*                There is an option for additional parameters            *
    //*                to be passed into the write entry method of             *
    //*                the EventLog class. See documentation for               *
    //*                more options.                                           *
    //*                                                                        *
    //*                IN DEVELOPMENT                                          *
    //*                SendEventLogEmail - For the purpose of error            *
    //*                notification any HIGH PRIORITY labeled errors           *
    //*                will produce an email message to application            *
    //*                maintenance staff.                                      *
    //*                                                                        *
    //*                GetCurrentUser - Identify the current logged            *
    //*                in user.                                                *
    //*                                                                        *
    //*                                                                        *
    //*  PROGRAMMER:   John W. Fell                                            *
    //*                                                                        *
    //*  SYSTEM:       Visual C# running on an Intel machine with              *
    //*                the Windows 10 Professional operating system.           *
    //*                                                                        *
    //*  MODIFICATIONS (1)                                                     *
    //*   TO DATE:     (2)                                                     *
    //*                (3)                                                     *
    //*                (4)                                                     *
    //*                                                                        *
    //*                                                                        *
    //*  GENERAL       Each routine applies the necessary commands             *
    //*   APPROACH:    for logging application level errors in the             *
    //*                windows event log for the os.                           *
    //*                                                                        *
    //*                                                                        *
    //*                                                                        *
    //**************************************************************************
    //**************************************************************************


    #endregion



    public static class ErrorLogs
    {

        #region Public Constants

        public const string EventLogSourceName = "Create Records Add-In";  // Name of event log source.

        public const string EventLogName = "Application"; // Name of event viewer Application log.

        public const string SmtpServer = "dcadex13.dcad.org";    // Host name of exchange server.



        #endregion

        #region Event Logging Procedures and Functions

        #region GetCurrentUser (In Development)
        //public static void GetCurrentUser()
        //{


        //    [System.Runtime.InteropServices.ComVisible(false)]

        //    public static System.Security.Principal.TokenImpersonationLevel ImpersonationLevel { get; }


        //    if (ImpersonationLevel === 'None'
        //public bool isImpersonating = System.Security.Principal.WindowsIdentity.ImpersonationLevel;

        //    public static string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

        //}


        // This subroutine writes an entry to an existing event log on the local machine.
        #endregion

        #region Write Message to Event Log

        public static void WriteLogEntry(String appSourceName, String entry, EventLogEntryType entryType)
        {


            try

            {

                // Check if event log exists

                if (!EventLog.SourceExists(appSourceName))

                {

                    // Create the event log source

                    EventLog.CreateEventSource(appSourceName, EventLogName);

                    System.Threading.Thread.Sleep(2000);


                }

                if (EventLog.SourceExists(appSourceName))

                {

                    EventLog eventLog = new EventLog();   // Reference event log.

                    eventLog.Source = appSourceName;                        // Define app source.

                    eventLog.WriteEntry(entry, entryType);                  // Write log entry. 

                }






            }

            catch (Exception)

            {

                throw;

            }

            #endregion


        }


        #region SendEventLogEmail (In Development)
        //public static void SendEventLogEmail(String procName, String exception)
        //{

        //    try
        //    {

        //        // Required for the application to assign the username to My.User.Name properly
        //        //this.User.InitializeWithWindowsUser()


        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}
        #endregion

        #endregion

    }
}
