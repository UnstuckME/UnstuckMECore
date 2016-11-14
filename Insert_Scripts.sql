/**********************************************************
***********************************************************
***********INSERTION STORED PROCEDURE SCRIPTS**************
***********************************************************
**********************************************************/

USE UnstuckME_DB;
GO

/******DROP PROCEDURE STATEMENTS*************************/
DROP PROC [dbo].[CreateNewUser];
GO
DROP PROC [dbo].[InsertStudentIntoClass];
GO
DROP PROC [dbo].[CreateNewClass];
GO
DROP PROC [dbo].[CreateSticker];
GO
DROP PROC [dbo].[CreateOfficialMentor];
GO
DROP PROC [dbo].[CreateReview];
GO
DROP PROC [dbo].[CreateReport];
GO
DROP PROC [dbo].[InsertUserIntoMentorProgram];
GO
DROP PROC [dbo].[CreateServer];
GO
DROP PROC [dbo].[ChangeProfilePicture];
GO
/****************BEGIN CREATION SCRIPTS******************/
--CREATE NEW USER
CREATE PROC [dbo].[CreateNewUser]
    (
    @FirstName VARCHAR(30),
	@LastName VARCHAR(30),
	@EmailAddress VARCHAR(50),
	@Password NVARCHAR(32),
	@Privileges NVARCHAR(32)
    )
AS
    BEGIN
        if  (Exists(Select EmailAddress from UserProfile where EmailAddress = @EmailAddress))
            return 1;
        else
            BEGIN
                INSERT INTO UserProfile
				VALUES(@FirstName, @LastName, @EmailAddress, @Password, @Privileges)
            END

    END
GO

/* INSERTS A STUDENT AND A CLASS INTO UserToClass */
CREATE PROC [dbo].[InsertStudentIntoClass]
    (
    @UserID		INT,
	@ClassID	INT
    )
AS
    BEGIN
        if  (Exists(Select UserID, ClassID from UserToClass 
					where UserID = @UserID AND ClassID = @ClassID))
            return 1;
        else
            BEGIN
                INSERT INTO UserToClass
				VALUES(@UserID, @ClassID)
            END

    END
GO

--Create New Class
CREATE PROC [dbo].[CreateNewClass]
    (
    @CourseName VARCHAR(50),
	@CourseCode VARCHAR(5),
	@CourseNumber SMALLINT,
	@TermOffered TINYINT
    )
AS
    BEGIN
        if  (Exists(Select CourseCode, CourseNumber from Classes 
					where CourseCode = @CourseCode AND CourseNumber = @CourseNumber))
            return 1;
        else
            BEGIN
                INSERT INTO Classes
				VALUES(@CourseName, @CourseCode, @CourseNumber, @TermOffered)
            END

    END
GO

--Create Sticker
CREATE PROC [dbo].[CreateSticker]
    (
    @ProblemDescription		NVARCHAR(500),
	@ClassID				INT,
	@StudentID				INT,
	@MinimumStarRanking		FLOAT,
	@Timeout				INT
    )
AS
    BEGIN
        if  (Exists(Select StudentID, ClassID from Sticker 
					where StudentID = @StudentID AND ClassID = @ClassID))
            return 1;
        else
            BEGIN
                INSERT INTO Sticker (ProblemDescription, ClassID, StudentID, MinimumStarRanking, SubmitTime, [Timeout])
				VALUES(@ProblemDescription, @ClassID, @StudentID, @MinimumStarRanking, GETDATE(), DATEADD(second, @Timeout, GETDATE()))
            END

    END
GO

--CREATE AN OFFICIAL MENTOR
CREATE PROC [dbo].[CreateOfficialMentor]
    (
    @OrganizationName	NVARCHAR(50)
    )
AS
    BEGIN
        if  (Exists(Select OrganizationName from OfficialMentor 
					where OrganizationName = @OrganizationName))
            return 1;
        else
            BEGIN
                INSERT INTO OfficialMentor
				VALUES(@OrganizationName)
            END

    END
GO

CREATE PROC [dbo].[CreateReview]
    (
	@StickerID		INT,
	@ReviewerID		INT,
    @StarRanking	FLOAT,
	@Description	NVARCHAR(250)
    )
AS
    BEGIN
        if  (Exists(Select StickerID from Review 
					where StickerID = @StickerID AND ReviewerID = @ReviewerID))
            return 1;
        else
            BEGIN
                INSERT INTO Review
				VALUES(@StickerID, @ReviewerID, @StarRanking, @Description)
            END

    END
GO

CREATE PROC [dbo].[CreateReport]
    (
	@ReportDescription		NVARCHAR(200),
	@FlaggerID		INT,
    @ReviewID		FLOAT
    )
AS
    BEGIN
        if  (Exists(Select ReportID from Report 
					where ReviewID = @ReviewID AND FlaggerID = @FlaggerID))
            return 1;
        else
            BEGIN
                INSERT INTO Report
				VALUES(@ReportDescription, @FlaggerID, @ReviewID)
            END

    END
GO

CREATE PROC [dbo].[InsertUserIntoMentorProgram]
    (
	@UserID		INT,
	@MentorID	INT
    )
AS
    BEGIN
        if  (Exists(Select * from OmToUser 
					WHERE UserID = @UserID AND MentorID = @MentorID))
            return 1;
        else
            BEGIN
                INSERT INTO OmToUser
				VALUES(@UserID, @MentorID)
            END

    END
GO

CREATE PROC [dbo].[CreateServer]
    (
	@ServerName			VARCHAR(75),
	@ServerIP			VARCHAR(15),
	@ServerDomain		VARCHAR(50),
	@SchoolName			VARCHAR(75),
	@AdminUsername		VARCHAR(30),
	@AdminPassword		NVARCHAR(32),
	@EmailCredentials	NVARCHAR(50)
    )
AS
    BEGIN
        if  (Exists(Select ServerName from Server 
					WHERE ServerName = @ServerName))
            return 1;
        else
            BEGIN
                INSERT INTO [Server]
				VALUES(@ServerName, @ServerIP, @ServerDomain, @SchoolName,
						 @AdminUsername, @AdminPassword, @EmailCredentials)
            END

    END
GO

CREATE PROC [dbo].[ChangeProfilePicture]
    (
	@UserID		INT,
	@Photo		VARBINARY(MAX)
    )
AS
    BEGIN
        if  (NOT Exists(Select UserID from UserProfile 
					WHERE UserID = @UserID))
            return 1;
        else
            BEGIN
                INSERT INTO Picture
				VALUES(@UserID, @Photo)
            END

    END
GO


