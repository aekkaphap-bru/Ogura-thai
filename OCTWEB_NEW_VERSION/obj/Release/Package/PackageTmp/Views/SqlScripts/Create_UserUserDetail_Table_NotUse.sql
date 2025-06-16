CREATE TABLE [dbo].[UsersUserDetail](
    [Id]     INT  IDENTITY (1, 1) NOT NULL,
    [UserId] NVARCHAR (128) NOT NULL,
    [USE_Id] int NOT NULL,
    CONSTRAINT [PK_dbo.UsersUserDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_dbo.UserDetail_USE_Id] FOREIGN KEY ([USE_Id]) REFERENCES [dbo].[UserDetail]([USE_Id]),
	CONSTRAINT [FK_dbo.Users_UserId] FOREIGN KEY (UserId) REFERENCES [dbo].[Users]([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UserIdIndex]
    ON [dbo].[UsersUserDetail](UserId ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [USE_IdIndex]
    ON [dbo].[UsersUserDetail](USE_Id ASC);