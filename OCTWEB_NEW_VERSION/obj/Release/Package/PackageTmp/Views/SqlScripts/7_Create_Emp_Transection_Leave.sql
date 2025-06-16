
CREATE TABLE [dbo].[Emp_Transection_Leave] (
	   [Id]					INT NOT NULL PRIMARY KEY IDENTITY (1, 1)
      ,[Emp_Id]				INT NOT NULL
      ,[DateSt]				DATETIME NOT NULL
      ,[Leave_startdate]    DATETIME NOT NULL
      ,[Leave_enddate]      DATETIME NOT NULL
      ,[Leave_time]         VARCHAR(50) NOT NULL
      ,[Leave_timeend]		VARCHAR(50) NOT NULL
      ,[Shift]				VARCHAR(50) NOT NULL
      ,[Date_leave]			FLOAT NOT NULL
      ,[Date_Hour]			FLOAT NOT NULL
      ,[Reasons_for_leave]	VARCHAR(50) NULL
      ,[Type_ReasonId]		INT NOT NULL
      ,[Approved_emp]		VARCHAR(50) NULL
      ,[ReasonnotApEmp]		VARCHAR(100) NULL
      ,[Approved_hr]		VARCHAR(50) NULL
      ,[ReasonnotApHr]		VARCHAR(100) NULL
      ,[Status_pay]			VARCHAR(50) NULL
      ,[M_certificate]		VARCHAR(50) NULL
      ,[DT_UpEmpN]			DATETIME NULL
      ,[DT_UpHRN]			DATETIME NULL
      ,[AddbyEmp]			VARCHAR(50) NULL
      ,[AddAppbyHr]			VARCHAR(50) NULL
      ,[Status_SunSat]		INT  NULL
      ,[Note]				VARCHAR(50) NULL
      ,[Status_SendmailHR]	INT  NULL
      ,[StatusWork]			INT  NULL
      ,[StatusWorkDetail]	VARCHAR(1000) NULL
      ,[Status_hrchk]	    VARCHAR(3) NULL
      ,[Status_hrchkChar]	VARCHAR(20) NULL   
);


INSERT INTO [dbo].[Emp_Transection_Leave] 
(
       [Emp_Id]
      ,[DateSt]
      ,[Leave_startdate]
      ,[Leave_enddate]
      ,[Leave_time]
      ,[Leave_timeend]
      ,[Shift]
      ,[Date_leave]
      ,[Date_Hour]
      ,[Reasons_for_leave]
      ,[Type_ReasonId]
      ,[Approved_emp]
      ,[ReasonnotApEmp]
      ,[Approved_hr]
      ,[ReasonnotApHr]
      ,[Status_pay]
      ,[M_certificate]
      ,[DT_UpEmpN]
      ,[DT_UpHRN]
      ,[AddbyEmp]
      ,[AddAppbyHr]
      ,[Status_SunSat]
      ,[Note]
      ,[Status_SendmailHR]
      ,[StatusWork]
      ,[StatusWorkDetail]
      ,[Status_hrchk]
      ,[Status_hrchkChar]
)

SELECT	
       r.[EmpList_Id]
      ,CONVERT(DATETIME,r.[DateSt],105)
      ,CONVERT(DATETIME,CONVERT(DATETIME,r.[Leave_startdate],105)+CONVERT(DATETIME,r.[Leave_time],108) )
      ,CONVERT(DATETIME,CONVERT(DATETIME,r.[Leave_enddate],105)+CONVERT(DATETIME,r.[Leave_timeend],108))
      ,r.[Leave_time]
      ,r.[Leave_timeend]
      ,r.[Shift]
      ,CONVERT(FLOAT,r.[Date_leave])
      ,CONVERT(FLOAT,r.[Date_Hour])
      ,r.[Reasons_for_leave]
      ,r.[Type_ReasonId]
      ,r.[Approved_emp]
      ,r.[ReasonnotApEmp]
      ,r.[Approved_hr]
      ,r.[ReasonnotApHr]
      ,r.[Status_pay]
      ,r.[M_certificate]
      ,CONVERT(DATETIME,r.[DT_UpEmpN],20)
      ,CONVERT(DATETIME,r.[DT_UpHRN],20)
      ,r.[AddbyEmp]
      ,r.[AddAppbyHr]
      ,r.[Status_SunSat]
      ,r.[Note]
      ,r.[Status_SendmailHR]
      ,r.[StatusWork]
      ,r.[StatusWorkDetail]
      ,r.[Status_hrchk]
      ,r.[Status_hrchkChar]
FROM dbo.[Emp_SickLeave] as r