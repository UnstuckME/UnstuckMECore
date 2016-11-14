--UnstuckME Update/Delete Scripts
--********Delete********
USE UnstuckME_DB
GO

/***************************************************
DROP STORED PROCEDURES
***************************************************/
DROP PROC [dbo].[DeleteServerInformationByServerID];
GO
DROP PROC [dbo].[DeleteUserProfileByUserID];
GO
DROP PROC [dbo].[DeleteUserPictureByUserID];
GO
DROP PROC [dbo].[DeleteFileByFileID];
GO
DROP PROC [dbo].[DeleteMessageByMessageID];
GO
DROP PROC [dbo].[DeleteMentorOrganizationByMentorID];
GO
DROP PROC [dbo].[DeleteClassByClassID];
GO
DROP PROC [dbo].[DeleteStickerByStickerID];
GO
DROP PROC [dbo].[DeleteReviewByReviewID];
GO
DROP PROC [dbo].[ClearReviewDescriptionByReviewID];
GO
DROP PROC [dbo].[DeleteReportByReportID];
GO
DROP PROC [dbo].[UpdateServerNameByServerID];
GO
DROP PROC [dbo].[UpdateServerIPByServerID];
GO
DROP PROC [dbo].[UpdateServerDomainByServerID];
GO
DROP PROC [dbo].[UpdateSchoolNameByServerID];
GO
DROP PROC [dbo].[UpdateAdminPasswordByServerID];
GO
DROP PROC [dbo].[UpdateAdminUsernameByServerID];
GO
DROP PROC [dbo].[UpdateEmailCredentialsByServerID];
GO

--Photo
/********************************NEED MORE INFO ON HOW WE ARE STORING PHOTOS*******************************/


DROP PROC [dbo].[UpdateMentorNameByMentorID];
GO
DROP PROC [dbo].[UpdateDisplayFNameByUserID];
GO
DROP PROC [dbo].[UpdateDisplayLNameByUserID];
GO
DROP PROC [dbo].[UpdateEmailAddressByUserID];
GO
DROP PROC [dbo].[UpdateUserPasswordByUserID];
GO
DROP PROC [dbo].[UpdatePrivilegesByUserID];
GO
DROP PROC [dbo].[UpdateStickerProblemDescriptionByStickerID];
GO
DROP PROC [dbo].[UpdateTimeoutByStickerIDAndSeconds];
GO
DROP PROC [dbo].[UpdateCourseNameByClassID];
GO
DROP PROC [dbo].[UpdateCourseCodeByClassID];
GO
DROP PROC [dbo].[UpdateCourseNumberByClassID];
GO
DROP PROC [dbo].[UpdateTermsOfferedByClassID];
GO
DROP PROC [dbo].[UpdateStarRankingByReviewID];
GO
DROP PROC [dbo].[UpdateReviewDescriptionByReviewID];
GO
DROP PROC [dbo].[UpdateMessageByMessageID];
GO
DROP PROC [dbo].[UpdateTutorIDByTutorIDAndStickerID];
GO

--START CREATION SCRIPTS
/*********************************************************/
CREATE PROC [dbo].[DeleteServerInformationByServerID]
    (
    @ServerID INT
    )
AS
    BEGIN
        if  (NOT Exists(Select ServerID from [Server] where ServerID = @ServerID))
            return 1;
        else
            BEGIN
                DELETE FROM [Server]
				WHERE ServerID = @ServerID;
                return 0;
            END

    END
GO

/*********************************************************
--Delete User Profile Procedure Creation Script
*********************************************************/
CREATE PROC [dbo].[DeleteUserProfileByUserID]
    (
    @UserID INT
    )
AS
    BEGIN
        if  (NOT Exists(Select UserID from UserProfile where UserID = @UserID))
            return 1;
        else
            BEGIN
                DELETE FROM UserProfile
				WHERE UserID = @UserID;
                return 0;
            END

    END
GO

/*********************************************************
--Delete User Profile Picture Procedure Creation Script
*********************************************************/
CREATE PROC [dbo].[DeleteUserPictureByUserID]
    (
    @UserID INT
    )
AS
    BEGIN
        if  (NOT Exists(Select UserID from Picture where UserID = @UserID))
            return 1;
        else
            BEGIN
                DELETE Picture
				WHERE UserID = @UserID;
                return 0;
            END

    END
GO

/*********************************************************
--Delete File Procedure Creation Script
*********************************************************/
CREATE PROC [dbo].[DeleteFileByFileID]
    (
    @FileID INT
    )
AS
    BEGIN
        if  (NOT Exists(Select FileID from Files where FileID = @FileID))
            return 1;
        else
            BEGIN
                DELETE Files
				WHERE FileID = @fileID;
                return 0;
            END

    END
GO

/*********************************************************
--Delete Message Procedure Creation Script
*********************************************************/
CREATE PROC [dbo].[DeleteMessageByMessageID]
    (
    @MessageID INT
    )
AS
    BEGIN
        if  (NOT Exists(Select MessageID from [Messages] where MessageID = @MessageID))
            return 1;
        else
            BEGIN
                DELETE [Messages]
				WHERE MessageID = @messageID;
                return 0;
            END

    END
GO

/*********************************************************
--Delete Mentor Organization Procedure Creation Script
*********************************************************/
CREATE PROC [dbo].[DeleteMentorOrganizationByMentorID]
    (
    @MentorID INT
    )
AS
    BEGIN
        if  (NOT Exists(Select MentorID from OfficialMentor where MentorID = @MentorID))
            return 1;
        else
            BEGIN
				DELETE OfficialMentor
				WHERE MentorID = @mentorID;
                return 0;
            END

    END
GO

/*********************************************************
--Delete Class Procedure Creation Script
*********************************************************/
CREATE PROC [dbo].[DeleteClassByClassID]
    (
    @ClassID INT
    )
AS
    BEGIN
        if  (NOT Exists(Select ClassID from Classes where ClassID = @ClassID))
            return 1;
        else
            BEGIN
				DELETE Classes
				WHERE ClassID = @classID;
                return 0;
            END

    END
GO

/*********************************************************
--Delete Sticker Procedure Creation Script
*********************************************************/
CREATE PROC [dbo].[DeleteStickerByStickerID]
    (
    @StickerID INT
    )
AS
    BEGIN
        if  (NOT Exists(Select StickerID from Sticker where StickerID = @StickerID))
            return 1;
        else
            BEGIN
				DELETE Sticker
				WHERE StickerID = @stickerID;
                return 0;
            END

    END
GO

/*********************************************************
--Delete Review Procedure Creation Script
*********************************************************/
CREATE PROC [dbo].[DeleteReviewByReviewID]
    (
    @ReviewID INT
    )
AS
    BEGIN
        if  (NOT Exists(Select ReviewID from Review where ReviewID = @ReviewID))
            return 1;
        else
            BEGIN
				DELETE Review
				WHERE ReviewID = @reviewID;
                return 0;
            END

    END
GO

/*********************************************************
--Delete Review Description Procedure Creation Script
*********************************************************/
CREATE PROC [dbo].[ClearReviewDescriptionByReviewID]
    (
    @ReviewID INT
    )
AS
    BEGIN
        if  (NOT Exists(Select ReviewID from Review where ReviewID = @ReviewID))
            return 1;
        else
            BEGIN
				UPDATE Review
				SET [Description] = ''
				WHERE ReviewID = @reviewID;
                return 0;
            END

    END
GO

/*********************************************************
--Delete Report Procedure Creation Script
*********************************************************/
CREATE PROC [dbo].[DeleteReportByReportID]
    (
    @ReportID INT
    )
AS
    BEGIN
        if  (NOT Exists(Select ReportID from Report where ReportID = @ReportID))
            return 1;
        else
            BEGIN
				DELETE Report
				WHERE ReportID = @reportID;
                return 0;
            END

    END
GO

--********Update********
/*********************************************************
--Update Server Name
*********************************************************/
CREATE PROC [dbo].[UpdateServerNameByServerID]
    (
    @ServerID INT,
	@ServerName VARCHAR(75)
    )
AS
    BEGIN
        if  (NOT Exists(Select ServerID from [Server] where ServerID = @ServerID))
            return 1;
        else
            BEGIN
				UPDATE [Server]
				SET ServerName = @ServerName
				WHERE ServerID = @ServerID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Server IP Address
*********************************************************/
CREATE PROC [dbo].[UpdateServerIPByServerID]
    (
    @ServerID INT,
	@ServerIP VARCHAR(15)
    )
AS
    BEGIN
        if  (NOT Exists(Select ServerID from [Server] where ServerID = @ServerID))
            return 1;
        else
            BEGIN
				UPDATE [Server]
				SET ServerIP = @ServerIP
				WHERE ServerID = @ServerID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Server Domain Name
*********************************************************/
CREATE PROC [dbo].[UpdateServerDomainByServerID]
    (
    @ServerID INT,
	@ServerDomain VARCHAR(50)
    )
AS
    BEGIN
        if  (NOT Exists(Select ServerID from [Server] where ServerID = @ServerID))
            return 1;
        else
            BEGIN
				UPDATE [Server]
				SET ServerDomain = @ServerDomain
				WHERE ServerID = @ServerID;
                return 0;
            END

    END
GO

/*********************************************************
--Update School Name
*********************************************************/
CREATE PROC [dbo].[UpdateSchoolNameByServerID]
    (
    @ServerID INT,
	@SchoolName VARCHAR(75)
    )
AS
    BEGIN
        if  (NOT Exists(Select ServerID from [Server] where ServerID = @ServerID))
            return 1;
        else
            BEGIN
				UPDATE [Server]
				SET SchoolName = @SchoolName
				WHERE ServerID = @ServerID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Admin Password
*********************************************************/
CREATE PROC [dbo].[UpdateAdminPasswordByServerID]
    (
    @ServerID INT,
	@AdminPassword NVARCHAR(32)
    )
AS
    BEGIN
        if  (NOT Exists(Select ServerID from [Server] where ServerID = @ServerID))
            return 1;
        else
            BEGIN
				UPDATE [Server]
				SET AdminPassword = @AdminPassword
				WHERE ServerID = @ServerID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Admin Username
*********************************************************/
CREATE PROC [dbo].[UpdateAdminUsernameByServerID]
    (
    @ServerID INT,
	@AdminUsername VARCHAR(30)
    )
AS
    BEGIN
        if  (NOT Exists(Select ServerID from [Server] where ServerID = @ServerID))
            return 1;
        else
            BEGIN
				UPDATE [Server]
				SET AdminUsername = @AdminUsername
				WHERE ServerID = @ServerID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Server Email Credentials
*********************************************************/
CREATE PROC [dbo].[UpdateEmailCredentialsByServerID]
    (
    @ServerID INT,
	@EmailCredentials NVARCHAR(50)
    )
AS
    BEGIN
        if  (NOT Exists(Select ServerID from [Server] where ServerID = @ServerID))
            return 1;
        else
            BEGIN
				UPDATE [Server]
				SET EmailCredentials = @EmailCredentials
				WHERE ServerID = @ServerID;
                return 0;
            END

    END
GO

--Photo
/********************************NEED MORE INFO ON HOW WE ARE STORING PHOTOS*******************************/

/*********************************************************
--Update Mentor Organization Name
*********************************************************/
CREATE PROC [dbo].[UpdateMentorNameByMentorID]
    (
    @MentorID INT,
	@OrganizationName NVARCHAR(50)
    )
AS
    BEGIN
        if  (NOT Exists(Select MentorID from OfficialMentor where MentorID = @MentorID))
            return 1;
        else
            BEGIN
				UPDATE OfficialMentor
				SET OrganizationName = @OrganizationName
				WHERE MentorID = @MentorID;
                return 0;
            END

    END
GO

/*********************************************************
--Update User First Name
*********************************************************/
CREATE PROC [dbo].[UpdateDisplayFNameByUserID]
    (
    @UserID INT,
	@DisplayFName VARCHAR(30)
    )
AS
    BEGIN
        if  (NOT Exists(Select UserID from UserProfile where UserID = @UserID))
            return 1;
        else
            BEGIN
				UPDATE UserProfile
				SET DisplayFName = @DisplayFName
				WHERE UserID = @userID;
                return 0;
            END

    END
GO

/*********************************************************
--Update User Last Name
*********************************************************/
CREATE PROC [dbo].[UpdateDisplayLNameByUserID]
    (
    @UserID INT,
	@DisplayLName VARCHAR(30)
    )
AS
    BEGIN
        if  (NOT Exists(Select UserID from UserProfile where UserID = @UserID))
            return 1;
        else
            BEGIN
				UPDATE UserProfile
				SET DisplayLName = @DisplayLName
				WHERE UserID = @userID;
                return 0;
            END

    END
GO

/*********************************************************
--Update User Email Address
*********************************************************/
CREATE PROC [dbo].[UpdateEmailAddressByUserID]
    (
    @UserID INT,
	@EmailAddress VARCHAR(50)
    )
AS
    BEGIN
        if  (NOT Exists(Select UserID from UserProfile where UserID = @UserID))
            return 1;
        else
            BEGIN
				UPDATE UserProfile
				SET EmailAddress = @EmailAddress
				WHERE UserID = @userID;
                return 0;
            END

    END
GO

/*********************************************************
--Update User Password
*********************************************************/
CREATE PROC [dbo].[UpdateUserPasswordByUserID]
    (
    @UserID INT,
	@UserPassword NVARCHAR(32)
    )
AS
    BEGIN
        if  (NOT Exists(Select UserID from UserProfile where UserID = @UserID))
            return 1;
        else
            BEGIN
				UPDATE UserProfile
				SET UserPassword = @UserPassword
				WHERE UserID = @userID;
                return 0;
            END

    END
GO

/*********************************************************
--Update User Privileges
*********************************************************/
CREATE PROC [dbo].[UpdatePrivilegesByUserID]
    (
    @UserID INT,
	@Privileges BINARY(4)
    )
AS
    BEGIN
        if  (NOT Exists(Select UserID from UserProfile where UserID = @UserID))
            return 1;
        else
            BEGIN
				UPDATE UserProfile
				SET Privileges = @Privileges
				WHERE UserID = @userID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Sticker Problem Description
*********************************************************/
CREATE PROC [dbo].[UpdateStickerProblemDescriptionByStickerID]
    (
    @StickerID INT,
	@ProblemDescription NVARCHAR(500)
    )
AS
    BEGIN
        if  (NOT Exists(Select StickerID from Sticker where StickerID = @StickerID))
            return 1;
        else
            BEGIN
				UPDATE Sticker
				SET ProblemDescription = @ProblemDescription
				WHERE StickerID = @StickerID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Timeout Time
*********************************************************/
CREATE PROC [dbo].[UpdateTimeoutByStickerIDAndSeconds]
    (
    @StickerID INT,
	@Seconds INT
    )
AS
    BEGIN
        if  (NOT Exists(Select StickerID from Sticker where StickerID = @StickerID))
            return 1;
        else
            BEGIN
				UPDATE Sticker
				SET [Timeout] = DATEADD(second, @Seconds, GETDATE())
				WHERE StickerID = @StickerID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Common Course Name
*********************************************************/
CREATE PROC [dbo].[UpdateCourseNameByClassID]
    (
    @ClassID INT,
	@CourseName VARCHAR(50)
    )
AS
    BEGIN
        if  (NOT Exists(Select ClassID from Classes where ClassID = @ClassID))
            return 1;
        else
            BEGIN
				UPDATE Classes
				SET CourseName = @CourseName
				WHERE ClassID = @ClassID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Course Code (WRI, MATH, CST)
*********************************************************/
CREATE PROC [dbo].[UpdateCourseCodeByClassID]
    (
    @ClassID INT,
	@CourseCode VARCHAR(5)
    )
AS
    BEGIN
        if  (NOT Exists(Select ClassID from Classes where ClassID = @ClassID))
            return 1;
        else
            BEGIN
				UPDATE Classes
				SET CourseCode = @CourseCode
				WHERE ClassID = @ClassID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Course Number
*********************************************************/
CREATE PROC [dbo].[UpdateCourseNumberByClassID]
    (
    @ClassID INT,
	@CourseNumber SMALLINT
    )
AS
    BEGIN
        if  (NOT Exists(Select ClassID from Classes where ClassID = @ClassID))
            return 1;
        else
            BEGIN
				UPDATE Classes
				SET CourseNumber = @CourseNumber
				WHERE ClassID = @ClassID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Terms Offered
*********************************************************/
CREATE PROC [dbo].[UpdateTermsOfferedByClassID]
    (
    @ClassID INT,
	@TermOffered TINYINT
    )
AS
    BEGIN
        if  (NOT Exists(Select ClassID from Classes where ClassID = @ClassID))
            return 1;
        else
            BEGIN
				UPDATE Classes
				SET TermOffered = @TermOffered
				WHERE ClassID = @ClassID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Review Star Ranking
*********************************************************/
CREATE PROC [dbo].[UpdateStarRankingByReviewID]
    (
    @ReviewID INT,
	@StarRanking TINYINT
    )
AS
    BEGIN
        if  (NOT Exists(Select ReviewID from Review where ReviewID = @ReviewID))
            return 1;
        else
            BEGIN
				UPDATE Review
				SET StarRanking = @StarRanking
				WHERE ReviewID = @ReviewID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Review Description
*********************************************************/
CREATE PROC [dbo].[UpdateReviewDescriptionByReviewID]
    (
    @ReviewID INT,
	@Description NVARCHAR(250)
    )
AS
    BEGIN
        if  (NOT Exists(Select ReviewID from Review where ReviewID = @ReviewID))
            return 1;
        else
            BEGIN
				UPDATE Review
				SET [Description] = @Description
				WHERE ReviewID = @ReviewID;
                return 0;
            END

    END
GO

/*********************************************************
--Update Chat Message
*********************************************************/
CREATE PROC [dbo].[UpdateMessageByMessageID]
    (
    @MessageID INT,
	@MessageData NVARCHAR(500)
    )
AS
    BEGIN
        if  (NOT Exists(Select MessageID from [Messages] where MessageID = @MessageID))
            return 1;
        else
            BEGIN
				UPDATE [Messages]
				SET MessageData = @MessageData
				WHERE MessageID = @MessageID;
                return 0;
            END

    END
GO

/******************************************************************
--Update TutorID by TutorID and StickerID Procedure Creation Script
******************************************************************/
CREATE PROC [dbo].[UpdateTutorIDByTutorIDAndStickerID]
    (
	@TutorID	INT,
    @StickerID	INT
    )
AS
    BEGIN
        if  (NOT Exists(Select StickerID from Sticker where StickerID = @StickerID))
            return 1;
        else
            BEGIN
				UPDATE Sticker
				SET TutorID = @TutorID
				WHERE StickerID = @StickeriD;
                return 0;
            END

    END
GO

