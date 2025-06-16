CREATE TABLE [dbo].[WS_Training_Section] (
    [Id]          INT NOT NULL PRIMARY KEY IDENTITY (1, 1),
    [SectionCode] VARCHAR (20) NOT NULL,
	[SectionName] VARCHAR (100) NULL,
 
);

INSERT INTO [dbo].[WS_Training_Section] ([SectionCode], [SectionName])
values ('IT','IT:Information Technology')
      ,('QA','QA:Quality Assuarance')
	  ,('PP','PP:Process Press')
	  ,('BR','BR:Process Broaching')
	  ,('FC','FC:FCC Assy (Bobbin Type)')
	  ,('AC','AC:Casting Fac.2')
	  ,('HR','HR:Human Resource')
	  ,('PE','PE:Production Engineer')
	  ,('QC','QC:Quality Control')
	  ,('GS','GS:Process Griding')
	  ,('AB','AB:Assy Fac.1')
	  ,('CA','CA:Coil Assy')
	  ,('P2','P2:Production Fac.2')
	  ,('AD','AD:Administraton')
	  ,('EN','EN:Sales Engineering')
	  ,('RA','RA:Rotor Assy')
	  ,('L1','L1:Process Late 1')
	  ,('AT','AT:Armature Assy')
	  ,('AF','AF:Fcc Finishing (Epoxy Type)')
	  ,('MC','MC:Micro Clutch')
	  ,('PU','PU:BOI/Purchase')
	  ,('PC','PC:Production Control')
	  ,('L3','L3:Process L3,Oven,Sanding,Pre Oven L')
	  ,('L2','L2:Process Late 2')
	  ,('GN','GN:GKN Semi,GKN Casting ,GKN Finishing')
	  ,('AS','AS:Semi Assy Fac.2')
	  ,('SE','SE:Safety')