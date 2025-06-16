
CREATE TABLE [dbo].[Emp_MasterNationality] (
    [Id]          INT NOT NULL PRIMARY KEY IDENTITY (1, 1),
    [Nationality] VARCHAR (250) NULL,
 
);

INSERT INTO [dbo].[Emp_MasterNationality] ([Nationality])
values ('Thai'),('Japanese')
