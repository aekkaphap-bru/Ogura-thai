
CREATE TABLE [dbo].[Emp_MasterTypeLayoff](
	[Id]           INT NOT NULL PRIMARY KEY IDENTITY (1, 1),
    [TypeLayoff]   VARCHAR (100) NULL,
);

INSERT INTO [dbo].[Emp_MasterTypeLayoff](TypeLayoff)
SELECT lf.Type_Layoff
FROM [dbo].[Emp_TypeLayoff] AS lf
;