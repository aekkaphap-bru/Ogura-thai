
CREATE TABLE [dbo].[Emp_MasterRegion] (
    [Id]          INT NOT NULL PRIMARY KEY IDENTITY (1, 1),
    [province]	  VARCHAR (150) NULL,
    [district]	  VARCHAR (150) NULL,
    [parish]      VARCHAR (150) NULL,
    
);


INSERT INTO [dbo].[Emp_MasterRegion] (province,district,parish)
SELECT r.province,r.district,r.parish
FROM dbo.EmpRegion as r

