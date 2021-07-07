﻿/***QUERY DOCUMENTATION ***********************************************************************************************************************************************
   == Author:             John W. Fell
   == Date:               06/28/21
   == File Name:          AN_CreateAFCLog_View.sql
   == Environment:        Development
   == Exec Location:      
   == Code Location:      https://github.com/dcadgis/pro-createrecords-addin
   == Purpose:            To generate a database view for current AFC Logs assigned to the authenticated user.
   == Algorithm:          Presents a database view for uncompleted and assigned AFC Logs. The SYSTEM_USER keyword
==                     helps to present only those assigned logs to the authenticated user.
   == Usage:              The database view can be queried using an IQueryFilter or ArcGIS Pro SDK Snippet.
   == Dependencies:       There must be a table named AFC_LOG in the mars database.
   == Permissions:        The authenticated user must have read permission. 
   == Resources:          (1) SYSTEM_USER  - https://bit.ly/3y0Zfv1
   ==                     (2) 
   == Revision History:
   ==                      06/28/21 - Created sql script.  -- jwf --
 *********************************************************************************************************************************************************************/


USE GEDT
GO

/* View Name: ADM.AFC_LOG_VW */

DECLARE @NOINSTNUM      VARCHAR(15)          =     'NO_INST_NUM'        -- For research forms
DECLARE @AFCSTACTIVE    SMALLINT             =      1                   -- Active AFC Status
DECLARE @AFCSTACTIVE    SMALLINT             =      1                   -- Active AFC Status
DECLARE @AFCSTCERTHLD   SMALLINT             =      4                   -- Cert Hold AFC Status
DECLARE @AFCSTQC        SMALLINT             =      6                   -- Cert Hold AFC Status
DECLARE @AFCSTCERTHLD   SMALLINT             =      4                   -- Cert Hold AFC Status

SELECT  AFC.AFC_LOG_ID
       ,AFC.AFC_YEAR
	   ,AFC.AFC_TYPE_CD
	   ,AFC.AFC_STATUS_CD
	   ,AFC.AFC_NOTE
	   ,AFC.RUSH_IND
	   ,AFC.TILE_NO
	   ,AFC.ACCOUNT_NUM
	   ,AFC.INSTRUMENT_NUM
	   ,AFC.SEQ_NUM
	   ,AFC.FILE_DATE
	   ,AFC.EFFECTIVE_DT
	   ,AFC.DRAFTER_EMPL_ID
	   ,AFC.DRAFTER_COMP_DT
	   ,AFC.ACCT_LIST
	   ,CASE AFC.AFC_TYPE_CD
	       WHEN 3 THEN @NOINSTNUM
		   ELSE DEED.DOC_TYPE
		END AS DOC_TYPE
	   
	    /**************************************
	    *   Make sure the appropriate path    *
		*   to the mars database is supplied  *
	    **************************************/
	   FROM DBPROD.DBO.AFC_LOG AFC  

	   INNER JOIN

	   DBPROD.DBO.DEED_MAIN DEED

	   ON AFC.INSTRUMENT_NUM = DEED.INSTRUMENT_NUM
	   
	   
	   
	   WHERE AFC.DRAFTER_EMPL_ID = CONVERT(CHAR(8), RIGHT(UPPER(RTRIM(SYSTEM_USER)), LEN(SYSTEM_USER) - 5))
	     AND AFC.AFC_YEAR IN (YEAR(GETDATE()) - 1, YEAR(GETDATE()))
		 AND AFC.DRAFTER_COMP_DT = '1900-01-01 00:00:00.000'
		 AND AFC.AFC_STATUS_CD IN (1, 3, 4)