# pro-createrecords-addin

![Create Records AddIn](../Images/create_records_image.png)

## Table of Contents
* [Introduction](#introduction)
* [Architecture](#architecture)
* [Integration](#integration)
* [Deployment](#deployment)
* [Usage](#usage)
* [Revision History](#revision-history)

### Introduction
This project represents an ArcGIS Pro 2.8 SDK Add-In that reads a database view and 
displays view data within a custom list nested within a dockpane.

### Architecture
A combination of directories contain sql queries, c# files, images, and documentation files.
Their purpose and contents include:

 * DarkImages - Images for ArcGIS Pro's dark theme.
 * Images - Images for ArcGIS Pro's dark theme.
 * Docs - Contains this README.MD document.
 * SQL - Contains sql queries that identify the ADM.AFC_LOG_VW.
 * Root - C#, .daml, and .xaml files that makup the MVVM Addin.

### Integration
In order to synchronize newly digitized road centerline names into the 
DBPROD.DBO.STREET_INFROMATION table, two database views are created in the
GEDT geodatabase and python scripts use the arcpy.ArcSDESQLExecute() method
to execute a SQL insert statement. The ADM.SYNC_STREET_ADDR_VW database view
handles the formatting of columns to match the target table schema. The ADM.AGGR_ROADCENTERLINE_VW
aggregates road names together to handle an issue with displaying road names with different
jurisdictions.

### Deployment
In order to get this to function in an ArcGIS Pro project, the repository should be
downloaded to the **\\\dcad.org\GIS\Source\Toolboxes** network share. Once downloaded,
the attached toolbox can be added to any ArcGIS Pro Project.

### Usage
To use this geoprocessing tool in ArcGIS Pro, add the Address Data Management toolbox (i.e., Address Data Management.tbx)
to the toolbox folder in the catalog pane. 


Once added, the geoprocessing tool named *Synchronize Street Addresses 1.0.0* may be used 
either independently or within an ArcGIS Pro task. 


> #### Warning: 
> There is a limit of 25 or more rows that will be inserted at one time from the sync street address view into the street information table.
> 

If within a [task](https://bit.ly/3f1EpUC), there is no user input
required, so it can be a task hidden to the user. If the tool fails, it will report
warnings or errors to the display. If there are no rows in the view to 
be synchronized then a warning is displayed alerting the user. This can
be supressed when used in a task because the user does not need to know.







----
## Revision History

|*Date*|*Rev*|*Description*|*Author*|
|------|-----|-------------|--------|
|12/02/20|1.0 |Initial Release |John W. Fell |
|05/05/21|1.1 |Include Integration, Deployment, and Usage |John W. Fell |


----
##### Footnotes
