#region INTERFACE DOCUMENTATION
/*
 * ***************************************************************************************************************************************************************
 * Project:         Name of the Project
 * Interface:       Name of the Interface in the Project
 * Version:         1.0
 * Author:          Name of the person(s) who wrote the script
 * Date Created:    Date Project Created (mm/dd/yyyy)
 * Date Launched:   Date Project launched for general use (mm/dd/yyyy)
 * Dept:            GIS Division
 * Location:        Project file location (...\ArcAdmin\ProjectFolder...)
 * Revisions:       mm/dd/yyyy -programmer-: Summary of modifications to code or Docked Window
 * ***************************************************************************************************************************************************************
 * 
 * INTERFACE
 * PURPOSE:     A brief explanation for why this interface is needed in the project.
 *             (example- This interface includes required members for a derived readable 
 *                       repository class following a C# generic repository pattern.)
 *
 * INTERFACE
 * DESCRIPTION: Describe the functionality or controls contained in the interface
 *              (example- Essential members for a repository pattern are included here. They represent 
 *                        retrieval methods and events indicating results from such an operation.)
 *
 * INTERFACE
 * PROPERTIES:   Describe the properties (protected or otherwise) for this interface.
 *              (example- layer internal variable from AnotherClass.cs)
 *
 * INTERFACE 
 * METHODS:     Provide a list of the Methods available in this interface and a brief explanation of their expected function.
 *              (example- GetSomeQuery() method should accept parameters from layer variable & accesses xyz feature class in GPUB and returns results in a ListArray.)
 *              (example- ReturnResults() method should cycle through the ListArray and populates the message box for the user to review.)
 *
 * INTERFACE
 * EVENTS:      Provide a list of the Events available in this interface and a brief explanation of the actions they should represent.
 *              (example- DatabaseConnected event should be triggered when a database connection is attempted along with the result.)
 *              (example- RecordRetrieved event should be triggered when a database record was returned from a retrieve operation.)
 *
 *
 * SUPPORTING
 * CLASSES
 * AND
 * INTERFACES:  If this interface is dependent of other classes or interfaces, list those classes with a brief explanation of their dependency.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pro_createrecords_addin.Interfaces
{
    public interface IRecordable
    {

        void SetDocumentNumber(int potentialRecordStatusCd);

        void SetImageSource(int potentialRecordStatusCd);

        void SetRecordStatus(int potentialRecordStatusCd);

        void SetRecordType(int potentialRecordType);

        void SetForegroundColor(int foregroundType);




    }
}
