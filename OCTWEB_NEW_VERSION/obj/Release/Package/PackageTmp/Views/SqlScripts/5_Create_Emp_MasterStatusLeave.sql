

CREATE TABLE [dbo].[Emp_MasterStatusLeave] (
    [Id]          INT NOT NULL PRIMARY KEY IDENTITY (1, 1),
	[StatusLeaveCode] INT NOT NULL,
    [StatusLeave] VARCHAR (250) NOT NULL,
 
);

INSERT INTO [dbo].[Emp_MasterStatusLeave] ([StatusLeaveCode], [StatusLeave])
VALUES (0,'Post leave application (แจ้งลาภายหลัง)')
      ,(1,'Early leave application (แจ้งลาล่วงหน้า)')