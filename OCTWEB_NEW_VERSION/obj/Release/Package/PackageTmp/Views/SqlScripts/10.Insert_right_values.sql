
INSERT INTO [dbo].[Systems]
      ([SYS_Name],[SYS_Description])
VALUES ('System Support','Use for System Others'),
       ('Dashboard','Use for Dashboard')
;

INSERT INTO [dbo].[RightGroup]
     ([RIG_Name],[RIG_Description],[SYS_Id])
VALUES ('Billing Note', 'Use for print bill',8),
       ('Dashboard', 'Use for Dashboard',9)
;

INSERT INTO [dbo].[Rights]
		([RIH_Name],[RIH_Description],[RIH_Pages],[RIG_Id])
VALUES ('Leave Setup','Use for leave setup','',13)
	  ,('Leave','Use for leave','',16)
	  ,('AP Billing Note','Use for print bill','',17)
	  ,('AR Billing Note','Use for print bill','',17)
	  ,('Sales Dashboard','Use for sales dashboard','',18)
	  ,('Purchase Dashboard','Use for purchase dashboard','',18)
