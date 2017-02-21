﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using UnstuckMEServer;
using UnstuckME_Classes;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Security;
using System.Data.Objects;
using System.Drawing;
using System.Windows.Media;
using System.Threading;
using System.Net;
using System.ServiceModel.Channels;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.IO;

namespace UnstuckMEInterfaces
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    /// <summary>
    /// Implement any Operation Contracts from IUnstuckMEService.cs in this file.
    /// </summary>
    public class UnstuckMEService : IUnstuckMEService, IUnstuckMEServer, IUnstuckMEFileStream
    {
        public ConcurrentDictionary<int, ConnectedClient> _connectedClients = new ConcurrentDictionary<int, ConnectedClient>();
        public ConcurrentDictionary<int, ConnectedServerAdmin> _connectedServerAdmins = new ConcurrentDictionary<int, ConnectedServerAdmin>();

        //This function is for testing stored procedures. In program.cs replace:
        //Thread userStatusCheck = new Thread(_server.CheckStatus); with Thread userStatusCheck = new Thread(_server.SPTest); 
        public void SPTest()
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                try
                {
                    db.CreateChat(2);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void CheckUserStatus()
        {
            List<int> offlineUsers = new List<int>();
            try
            {
                while (true)
                {
                    foreach (KeyValuePair<int, ConnectedClient> client in _connectedClients)
                    {
                        if (client.Value.ChannelInfo.Channel.State != CommunicationState.Opened)
                        {
                            offlineUsers.Add(client.Key);
                        }
                    }
                    foreach (int user in offlineUsers)
                    {
                        ConnectedClient removedClient = new ConnectedClient();
                        _connectedClients.TryRemove(user, out removedClient);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(removedClient.User.EmailAddress + "'s socket is in a faulted state. They are now considered offline");
                        Console.ResetColor();
                    }
                    offlineUsers.Clear();
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ChangeUserName(string emailaddress, string newFirstName, string newLastName)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var users = GetUserInfo(null, emailaddress);

				users.FirstName = newFirstName;
                users.LastName = newLastName;
                db.SaveChanges();
            }
        }

        public bool CreateNewUser(string displayFName, string displayLName, string emailAddress, string userPassword)
        {
            bool success = false;
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                UnstuckMEPassword hashedUserPassword = new UnstuckMEPassword();
                hashedUserPassword = UnstuckMEHashing.GetHashedPassword(userPassword);
                int retVal = db.CreateNewUser(displayFName, displayLName, emailAddress, hashedUserPassword.Password, hashedUserPassword.Salt);
                if (retVal == 1)
                {
                    success = true;
                }
            }
            return success;
        }

        public int GetUserID(string emailAddress)
        {
            int userID = 0;
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var temp = db.GetUserID(emailAddress);
                userID = temp.First().Value;
            }
            return userID;
        }

        public UserInfo UserLoginAttempt(string emailAddress, string passWord)
        {
            ConnectedClient newClient = new ConnectedClient();
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                try
                {
                    UserInfo users = GetUserInfo(null, emailAddress);

                    string stringOfPassword = UnstuckMEHashing.RecreateHashedPassword(passWord, users.Salt);

                    if (stringOfPassword == users.UserPassword)
                    {
                        //If Client is already logged on return false (This may be removed later).
                        foreach (var client in _connectedClients)
                        {
                            if (client.Key == users.UserID)
                            {
                                return null;
                            }
                        }

                        //Stores Client into Logged in Users List
                        var establishedUserConnection = OperationContext.Current.GetCallbackChannel<IClient>();
                        newClient.ChannelInfo = OperationContext.Current;
                        newClient.connection = establishedUserConnection;
                        newClient.User = GetUserInfo(users.UserID, users.EmailAddress);
                        newClient.returnAddress = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                        _connectedClients.TryAdd(newClient.User.UserID, newClient);
                        //Login Success, Print to console window.
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Client Login: {0} at {1}", newClient.User.EmailAddress, System.DateTime.Now);
                        Console.ResetColor();
                        foreach (var admin in _connectedServerAdmins)
                        {
                            admin.Value.connection.GetUpdate(0, newClient.User);
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return newClient.User;
        }

        public List<UserClass> GetUserClasses(int UserID)
        {
            try
            {
                List<UserClass> Rlist = new List<UserClass>();

                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    var classes = db.GetUserClasses(UserID);
                    //This might work, if not let me know and i'll figure out something else.
                    foreach (var c in classes)
                    {
                        UserClass temp = new UserClass();
                        temp.CourseCode = c.CourseCode;
                        temp.CourseName = c.CourseName;
                        temp.CourseNumber = c.CourseNumber;
                        Rlist.Add(temp);
                    }
                }
                return Rlist;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public void InsertStudentIntoClass(int UserID, int ClassID)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                db.InsertStudentIntoClass(UserID, ClassID);
            }
        }

        public UserInfo GetUserInfo(Nullable<int> userID, string emailAddress)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var users = db.GetUserInfo(userID, emailAddress).First();

                UserInfo newClient = new UserInfo();
                newClient.UserID = users.UserID;
                newClient.FirstName = users.DisplayFName;
                newClient.LastName = users.DisplayLName;
                newClient.EmailAddress = users.EmailAddress;
                newClient.Privileges = users.Privileges;
                newClient.AverageStudentRank = (float)users.AverageStudentRank;
                newClient.AverageTutorRank = (float)users.AverageTutorRank;
                newClient.TotalStudentReviews = users.TotalStudentReviews;
                newClient.TotalTutorReviews = users.TotalTutorReviews;
                newClient.UserPassword = users.UserPassword;
                newClient.Salt = users.Salt;
                newClient.UserProfilePictureBytes = GetProfilePicture(newClient.UserID);
                return newClient;
            }
        }

        //Checks to see if email address exists on the database.
        public bool IsValidUser(string emailAddress)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                try
                {
                    var users = GetUserInfo(null, emailAddress);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void RegisterServerAdmin(AdminInfo LoggingInAdmin)
        {
            try
            {
                //Stores Server Admin into Logged in _ConnectedServerAdmins List
                IServer establishedUserConnection = OperationContext.Current.GetCallbackChannel<IServer>();
                bool oldConnection = false;
                foreach (var onlineAdmin in _connectedServerAdmins)
                {
                    if (onlineAdmin.Key == LoggingInAdmin.ServerAdminID)
                    {
                        oldConnection = true;
                        onlineAdmin.Value.connection = establishedUserConnection;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Server Admin Re-Login: {0} at {1}", onlineAdmin.Value.Admin.EmailAddress, System.DateTime.Now);
                        Console.ResetColor();
                    }
                }
                if (!oldConnection)
                {
                    ConnectedServerAdmin newAdmin = new ConnectedServerAdmin();
                    newAdmin.connection = establishedUserConnection;
                    newAdmin.Admin = LoggingInAdmin;
                    _connectedServerAdmins.TryAdd(newAdmin.Admin.ServerAdminID, newAdmin);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Server Admin Login: {0} at {1}", newAdmin.Admin.EmailAddress, System.DateTime.Now);
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR WHILE REGISTERING SERVER ADMIN!\nMESSAGE: " + ex.Message);
                Console.ResetColor();
            }
        }

        public void Logout()
        {
            ConnectedClient client = GetMyClient();
            if (client != null)
            {
                ConnectedClient removedClient;
                _connectedClients.TryRemove(client.User.UserID, out removedClient);
                foreach (var admin in _connectedServerAdmins)
                {
                    admin.Value.connection.GetUpdate(1, removedClient.User);
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Client Loggoff: {0} at {1}", removedClient.User.EmailAddress, System.DateTime.Now);
                Console.ResetColor();
            }
        }

        public ConnectedClient GetMyClient()
        {
            IClient establishedUserConnection = OperationContext.Current.GetCallbackChannel<IClient>();
            foreach (var client in _connectedClients)
            {
                if (client.Value.connection == establishedUserConnection)
                {
                    return client.Value;
                }
            }
            return null;
        }

        public void AdminLogout()
        {
            ConnectedServerAdmin connectedAdmin = GetMyAdmin();
            if (connectedAdmin != null)
            {
                ConnectedServerAdmin removedAdmin;
                _connectedServerAdmins.TryRemove(connectedAdmin.Admin.ServerAdminID, out removedAdmin);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Server Admin Logoff: {0} at {1}", removedAdmin.Admin.EmailAddress, System.DateTime.Now);
                Console.ResetColor();
            }
        }

        public ConnectedServerAdmin GetMyAdmin()
        {
            IServer establishedAdminConnection = OperationContext.Current.GetCallbackChannel<IServer>();
            foreach (var admin in _connectedServerAdmins)
            {
                if (admin.Value.connection == establishedAdminConnection)
                {
                    return admin.Value;
                }
            }
            return null;
        }

        public void AdminLogMessage(string message)
        {
            ConnectedServerAdmin currentAdmin = GetMyAdmin();
            //Future Will Log to Log File.
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Message: {0} Sent by: {1} at {2}", message, currentAdmin.Admin.EmailAddress, System.DateTime.Now);
            Console.ResetColor();
        }

        public void ChangePassword(UserInfo User, string newPassword)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                UnstuckMEPassword newHashedPassword = UnstuckMEHashing.GetHashedPassword(newPassword);
                var users = GetUserInfo(User.UserID, User.EmailAddress);

				users.UserPassword = newHashedPassword.Password;
                users.Salt = newHashedPassword.Salt;
                db.SaveChanges();
            }
        }

        public void DeleteUserAccount(int userID)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                db.DeleteUserProfileByUserID(userID);
            }
        }

		#region GetReviews Functions
		public List<UnstuckMEReview> GetUserStudentReviewsASC(int userID, short firstrow = 0, short lastrow = 50, float minstarrank = 0)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var studentReviews = db.GetUserStudentReviews_RankASC(userID, firstrow, lastrow, minstarrank);

                List<UnstuckMEReview> studentReviewList = new List<UnstuckMEReview>();
                foreach (var review in studentReviews)
                {
                    UnstuckMEReview usReview = new UnstuckMEReview();
                    usReview.ReviewID = review.ReviewID;
                    usReview.StickerID = review.StickerID;
                    usReview.ReviewerID = review.ReviewerID;
                    usReview.StarRanking = (float)review.StarRanking;
                    usReview.Description = review.Description;
                    studentReviewList.Add(usReview);
                }

                return studentReviewList;
            }
        }

        public List<UnstuckMEReview> GetUserStudentReviewsDESC(int userID, short firstrow = 0, short lastrow = 50, float minstarrank = 0)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var studentReviews = db.GetUserStudentReviews_RankDESC(userID, firstrow, lastrow, minstarrank);

                List<UnstuckMEReview> studentReviewList = new List<UnstuckMEReview>();
                foreach (var review in studentReviews)
                {
                    UnstuckMEReview usReview = new UnstuckMEReview();
                    usReview.ReviewID = review.ReviewID;
                    usReview.StickerID = review.StickerID;
                    usReview.ReviewerID = review.ReviewerID;
                    usReview.StarRanking = (float)review.StarRanking;
                    usReview.Description = review.Description;
                    studentReviewList.Add(usReview);
                }

                return studentReviewList;
            }
        }

        public List<UnstuckMEReview> GetUserTutorReviewsASC(int userID, short firstrow = 0, short lastrow = 50, float minstarrank = 0)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var tutorReviews = db.GetUserTutorReviews_RankASC(userID, firstrow, lastrow, minstarrank);

                List<UnstuckMEReview> tutorReviewList = new List<UnstuckMEReview>();
                foreach (var review in tutorReviews)
                {
                    UnstuckMEReview usReview = new UnstuckMEReview();
                    usReview.ReviewID = review.ReviewID;
                    usReview.StickerID = review.StickerID;
                    usReview.ReviewerID = review.ReviewerID;
                    usReview.StarRanking = (review.StarRanking.HasValue) ? (float)review.StarRanking.Value : 0;
                    usReview.Description = review.Description;
                    tutorReviewList.Add(usReview);
                }
                return tutorReviewList;
            }
        }

        public List<UnstuckMEReview> GetUserTutorReviewsDESC(int userID, short firstrow = 0, short lastrow = 50, float minstarrank = 0)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var tutorReviews = db.GetUserTutorReviews_RankDESC(userID, firstrow, lastrow, minstarrank);

                List<UnstuckMEReview> tutorReviewList = new List<UnstuckMEReview>();
                foreach (var review in tutorReviews)
                {
                    UnstuckMEReview usReview = new UnstuckMEReview();
                    usReview.ReviewID = review.ReviewID;
                    usReview.StickerID = review.StickerID;
                    usReview.ReviewerID = review.ReviewerID;
                    usReview.StarRanking = (review.StarRanking.HasValue) ? (float)review.StarRanking.Value : 0;
                    usReview.Description = review.Description;
                    tutorReviewList.Add(usReview);
                }
                return tutorReviewList;
            }
        }
#endregion
		#region GetSticker Functions
		public List<UnstuckMESticker> GetUserSubmittedStickersASC(int userID, short firstrow = 0, short lastrow = 0, float minstarrank = 0, Nullable<int> classID = null)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var userStickers = db.GetUserSubmittedStickers_ClassASC(userID, firstrow, lastrow, minstarrank, classID);

                List<UnstuckMESticker> stickerList = new List<UnstuckMESticker>();
                foreach (var sticker in userStickers)
                {
                    UnstuckMESticker usSticker = new UnstuckMESticker();
                    usSticker.StickerID = sticker.StickerID;
                    usSticker.ProblemDescription = sticker.ProblemDescription;
                    usSticker.ClassID = sticker.ClassID;
                    usSticker.StudentID = sticker.StudentID;
                    usSticker.TutorID = (sticker.TutorID.HasValue) ? sticker.TutorID.Value : 1;
                    usSticker.MinimumStarRanking = (sticker.MinimumStarRanking.HasValue) ? (float)sticker.MinimumStarRanking : 0;
                    usSticker.SubmitTime = sticker.SubmitTime;
                    usSticker.Timeout = (int)(sticker.Timeout - DateTime.Now).TotalSeconds;
                    stickerList.Add(usSticker);
                }
                return stickerList;
            }
        }

        public List<UnstuckMESticker> GetUserSubmittedStickersDESC(int userID, short firstrow = 0, short lastrow = 0, float minstarrank = 0, Nullable<int> classID = null)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var userStickers = db.GetUserSubmittedStickers_ClassDESC(userID, firstrow, lastrow, minstarrank, classID);

                List<UnstuckMESticker> stickerList = new List<UnstuckMESticker>();
                foreach (var sticker in userStickers)
                {
                    UnstuckMESticker usSticker = new UnstuckMESticker();
                    usSticker.StickerID = sticker.StickerID;
                    usSticker.ProblemDescription = sticker.ProblemDescription;
                    usSticker.ClassID = sticker.ClassID;
                    usSticker.StudentID = sticker.StudentID;
                    usSticker.TutorID = (sticker.TutorID.HasValue) ? sticker.TutorID.Value : 1;
                    usSticker.MinimumStarRanking = (sticker.MinimumStarRanking.HasValue) ? (float)sticker.MinimumStarRanking : 0;
                    usSticker.SubmitTime = sticker.SubmitTime;
                    usSticker.Timeout = (int)(sticker.Timeout - DateTime.Now).TotalSeconds;
                    stickerList.Add(usSticker);
                }
                return stickerList;
            }
        }

        public List<UnstuckMESticker> GetUserTutoredStickersASC(int userID, short firstrow = 0, short lastrow = 0, float minstarrank = 0, Nullable<int> classID = null)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var userStickers = db.GetUserTutoredStickers_ClassASC(userID, firstrow, lastrow, minstarrank, classID);

                List<UnstuckMESticker> stickerList = new List<UnstuckMESticker>();
                foreach (var sticker in userStickers)
                {
                    UnstuckMESticker usSticker = new UnstuckMESticker();
                    usSticker.StickerID = sticker.StickerID;
                    usSticker.ProblemDescription = sticker.ProblemDescription;
                    usSticker.ClassID = sticker.ClassID;
                    usSticker.StudentID = sticker.StudentID;
                    usSticker.TutorID = (sticker.TutorID.HasValue) ? sticker.TutorID.Value : 1;
                    usSticker.MinimumStarRanking = (sticker.MinimumStarRanking.HasValue) ? (float)sticker.MinimumStarRanking : 0;
                    usSticker.SubmitTime = sticker.SubmitTime;
                    usSticker.Timeout = (int)(sticker.Timeout - DateTime.Now).TotalSeconds;
                    stickerList.Add(usSticker);
                }

                return stickerList;
            }
        }

        public List<UnstuckMESticker> GetUserTutoredStickersDESC(int userID, short firstrow = 0, short lastrow = 0, float minstarrank = 0, Nullable<int> classID = null)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var userStickers = db.GetUserTutoredStickers_ClassDESC(userID, firstrow, lastrow, minstarrank, classID);

                List<UnstuckMESticker> stickerList = new List<UnstuckMESticker>();
                foreach (var sticker in userStickers)
                {
                    UnstuckMESticker usSticker = new UnstuckMESticker();
                    usSticker.StickerID = sticker.StickerID;
                    usSticker.ProblemDescription = sticker.ProblemDescription;
                    usSticker.ClassID = sticker.ClassID;
                    usSticker.StudentID = sticker.StudentID;
                    usSticker.TutorID = (sticker.TutorID.HasValue) ? sticker.TutorID.Value : 1;
                    usSticker.MinimumStarRanking = (sticker.MinimumStarRanking.HasValue) ? (float)sticker.MinimumStarRanking : 0;
                    usSticker.SubmitTime = sticker.SubmitTime;
                    usSticker.Timeout = (int)(sticker.Timeout - DateTime.Now).TotalSeconds;
                    stickerList.Add(usSticker);
                }

                return stickerList;
            }
        }

        public List<UnstuckMEAvailableSticker> GetActiveStickersASC(float minstarrank = 0, short firstrow = 0, short lastrow = 50, Nullable<int> userID = null, Nullable<int> classID = null)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var userStickers = db.GetActiveStickers_ClassASC(minstarrank, firstrow, lastrow, userID, classID);

                List<UnstuckMEAvailableSticker> stickerList = new List<UnstuckMEAvailableSticker>();
                foreach (var sticker in userStickers)
                {
					UnstuckMEAvailableSticker usSticker = new UnstuckMEAvailableSticker();
					usSticker.StickerID = sticker.StickerID;
					usSticker.ProblemDescription = sticker.ProblemDescription;
					usSticker.ClassID = sticker.ClassID;
					usSticker.CourseCode = sticker.CourseCode;
					usSticker.CourseName = sticker.CourseName;
					usSticker.CourseNumber = sticker.CourseNumber;
					usSticker.StudentID = sticker.StudentID;
					//usSticker.TutorID = (sticker.TutorID.HasValue) ? sticker.TutorID.Value : 1;
					usSticker.StudentRanking = (sticker.MinimumStarRanking.HasValue) ? (float)sticker.MinimumStarRanking : 0;
					//usSticker.SubmitTime = sticker.SubmitTime;
					usSticker.Timeout = sticker.Timeout;
					stickerList.Add(usSticker);
				}

				return stickerList;
            }
        }

        public List<UnstuckMEAvailableSticker> GetActiveStickersDESC(float minstarrank = 0, short firstrow = 0, short lastrow = 50, Nullable<int> userID = null, Nullable<int> classID = null)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var userStickers = db.GetActiveStickers_ClassDESC(minstarrank, firstrow, lastrow, userID, classID);

                List<UnstuckMEAvailableSticker> stickerList = new List<UnstuckMEAvailableSticker>();
                foreach (var sticker in userStickers)
                {
					UnstuckMEAvailableSticker usSticker = new UnstuckMEAvailableSticker();
					usSticker.StickerID = sticker.StickerID;
					usSticker.ProblemDescription = sticker.ProblemDescription;
					usSticker.ClassID = sticker.ClassID;
					usSticker.CourseCode = sticker.CourseCode;
					usSticker.CourseName = sticker.CourseName;
					usSticker.CourseNumber = sticker.CourseNumber;
					usSticker.StudentID = sticker.StudentID;
					//usSticker.TutorID = (sticker.TutorID.HasValue) ? sticker.TutorID.Value : 1;
					usSticker.StudentRanking = (sticker.MinimumStarRanking.HasValue) ? (float)sticker.MinimumStarRanking : 0;
					//usSticker.SubmitTime = sticker.SubmitTime;
					usSticker.Timeout = sticker.Timeout;
					stickerList.Add(usSticker);
				}

				return stickerList;
            }
        }

        public List<UnstuckMEAvailableSticker> GetActiveStickersWithOrg_OrgClassASC(int orgID, float minstarrank = 0, short firstrow = 0, short lastrow = 50, Nullable<int> userID = null, Nullable<int> classID = null)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var userStickers = db.GetActiveStickersWithOrganization_OrgClassASC(orgID, minstarrank, firstrow, lastrow, userID, classID);

                List<UnstuckMEAvailableSticker> stickerList = new List<UnstuckMEAvailableSticker>();
                foreach (var sticker in userStickers)
                {
					UnstuckMEAvailableSticker usSticker = new UnstuckMEAvailableSticker();
					usSticker.StickerID = sticker.StickerID;
					usSticker.ProblemDescription = sticker.ProblemDescription;
					usSticker.ClassID = sticker.ClassID;
					usSticker.CourseCode = sticker.CourseCode;
					usSticker.CourseName = sticker.CourseName;
					usSticker.CourseNumber = sticker.CourseNumber;
					usSticker.StudentID = sticker.StudentID;
					//usSticker.TutorID = (sticker.TutorID.HasValue) ? sticker.TutorID.Value : 1;
					usSticker.StudentRanking = (sticker.MinimumStarRanking.HasValue) ? (float)sticker.MinimumStarRanking : 0;
					//usSticker.SubmitTime = sticker.SubmitTime;
					usSticker.Timeout = sticker.Timeout;
					stickerList.Add(usSticker);
				}

				return stickerList;
            }
        }

        public List<UnstuckMEAvailableSticker> GetActiveStickersWithOrg_OrgDESC(int orgID, float minstarrank = 0, short firstrow = 0, short lastrow = 50, Nullable<int> userID = null, Nullable<int> classID = null)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var userStickers = db.GetActiveStickersWithOrganization_OrgDESC(orgID, minstarrank, firstrow, lastrow, userID, classID);

                List<UnstuckMEAvailableSticker> stickerList = new List<UnstuckMEAvailableSticker>();
                foreach (var sticker in userStickers)
                {
					UnstuckMEAvailableSticker usSticker = new UnstuckMEAvailableSticker();
					usSticker.StickerID = sticker.StickerID;
					usSticker.ProblemDescription = sticker.ProblemDescription;
					usSticker.ClassID = sticker.ClassID;
					usSticker.CourseCode = sticker.CourseCode;
					usSticker.CourseName = sticker.CourseName;
					usSticker.CourseNumber = sticker.CourseNumber;
					usSticker.StudentID = sticker.StudentID;
					//usSticker.TutorID = (sticker.TutorID.HasValue) ? sticker.TutorID.Value : 1;
					usSticker.StudentRanking = (sticker.MinimumStarRanking.HasValue) ? (float)sticker.MinimumStarRanking : 0;
					//usSticker.SubmitTime = sticker.SubmitTime;
					usSticker.Timeout = sticker.Timeout;
					stickerList.Add(usSticker);
				}

				return stickerList;
            }
        }

        public List<UnstuckMEAvailableSticker> GetActiveStickersWithOrg_ClassDESC(int orgID, float minstarrank = 0, short firstrow = 0, short lastrow = 50, Nullable<int> userID = null, Nullable<int> classID = null)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var userStickers = db.GetActiveStickersWithOrganization_ClassDESC(orgID, minstarrank, firstrow, lastrow, userID, classID);

                List<UnstuckMEAvailableSticker> stickerList = new List<UnstuckMEAvailableSticker>();
                foreach (var sticker in userStickers)
                {
					UnstuckMEAvailableSticker usSticker = new UnstuckMEAvailableSticker();
					usSticker.StickerID = sticker.StickerID;
					usSticker.ProblemDescription = sticker.ProblemDescription;
					usSticker.ClassID = sticker.ClassID;
					usSticker.CourseCode = sticker.CourseCode;
					usSticker.CourseName = sticker.CourseName;
					usSticker.CourseNumber = sticker.CourseNumber;
					usSticker.StudentID = sticker.StudentID;
					//usSticker.TutorID = (sticker.TutorID.HasValue) ? sticker.TutorID.Value : 1;
					usSticker.StudentRanking = (sticker.MinimumStarRanking.HasValue) ? (float)sticker.MinimumStarRanking : 0;
					//usSticker.SubmitTime = sticker.SubmitTime;
					usSticker.Timeout = sticker.Timeout;
					stickerList.Add(usSticker);
				}

				return stickerList;
            }
        }

        public List<UnstuckMEAvailableSticker> GetActiveStickersWithOrg_OrgClassDESC(int orgID, float minstarrank = 0, short firstrow = 0, short lastrow = 50, Nullable<int> userID = null, Nullable<int> classID = null)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var userStickers = db.GetActiveStickersWithOrganization_OrgClassDESC(orgID, minstarrank, firstrow, lastrow, userID, classID);

                List<UnstuckMEAvailableSticker> stickerList = new List<UnstuckMEAvailableSticker>();
                foreach (var sticker in userStickers)
                {
					UnstuckMEAvailableSticker usSticker = new UnstuckMEAvailableSticker();
					usSticker.StickerID = sticker.StickerID;
                    usSticker.ProblemDescription = sticker.ProblemDescription;
                    usSticker.ClassID = sticker.ClassID;
					usSticker.CourseCode = sticker.CourseCode;
					usSticker.CourseName = sticker.CourseName;
					usSticker.CourseNumber = sticker.CourseNumber;
                    usSticker.StudentID = sticker.StudentID;
                    //usSticker.TutorID = (sticker.TutorID.HasValue) ? sticker.TutorID.Value : 1;
                    usSticker.StudentRanking = (sticker.MinimumStarRanking.HasValue) ? (float)sticker.MinimumStarRanking : 0;
					//usSticker.SubmitTime = sticker.SubmitTime;
					usSticker.Timeout = sticker.Timeout;
                    stickerList.Add(usSticker);
                }

                return stickerList;
            }
        }
#endregion

		public void AddUserToTutoringOrganization(int userID, int organizationID)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                db.InsertUserIntoMentorProgram(userID, organizationID);
            }
        }

        public void SubmitSticker(UnstuckMESticker newSticker)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                db.CreateSticker(newSticker.ProblemDescription, newSticker.ClassID, newSticker.StudentID, newSticker.MinimumStarRanking, newSticker.Timeout);

                foreach (int orgID in newSticker.AttachedOrganizations)
                {
                    db.AddOrgToSticker(newSticker.StickerID, orgID);
                    Console.WriteLine("OrganizationID: " + orgID);
                }
            }
        }

        public byte[] GetProfilePicture(int userID)
        {
            byte[] imgByte = null;
			//string filepath = string.Empty;
			
			//using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
			//{
			//	filepath = db.GetProfilePicture(userID).First();
			//}

			//FileStream profilepic = File.OpenRead(filepath);
			//using (MemoryStream ms = new MemoryStream())
			//{
			//	profilepic.CopyToAsync(ms);
			//	imgByte = ms.ToArray();
			//}

			using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
			{
				imgByte = db.GetProfilePicture(userID).First();
			}

            return imgByte;
        }

		public void SetProfilePicture(int userID, byte[] image)
		{
			using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
			{
				db.UpdateProfilePicture(userID, image);
			}
		}

		//public void SetProfilePicture(int userID, System.IO.FileStream image)
		//{
		/* Don't use this, but don't erase it as I may want it later - Matthew
		* 
		* string[] directories = Environment.CurrentDirectory.Split('\\');
		* string filepath = string.Empty;
		* 
		* for (int i = 0; i < directories.Length - 2; i++)
		* filepath += directories[i];
		* 
		* filepath += "\\Files\\Users\\" + userID.ToString();
		* filepath = Directory.GetFiles(filepath, @"ProfilePicture.*").FirstOrDefault();
		*/

		//	string filepath = Environment.CurrentDirectory + "..\\Files\\" + userID.ToString() + "_" + image.Name.Split('\\').LastOrDefault();
		//	byte[] bytes = null, buffer = null;

		//	using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
		//	{
		//		int read = 0;
		//		while ((read = image.Read(buffer, 0, (int)image.Length)) > 0)
		//			ms.Write(buffer, 0, read);

		//		bytes = ms.ToArray();
		//	}

		//	using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
		//	{
		//		db.UpdateProfilePicture(userID, bytes);
		//	}
		//}

		public List<UserInfo> AdminGetAllOnlineUsers()
        {
            List<UserInfo> userList = new List<UserInfo>();

            foreach (var user in _connectedClients)
            {
                userList.Add(user.Value.User);
            }
            return userList;
        }

        public void RemoveUserFromClass(int UserID, int ClassID)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                db.DeleteUserFromClass(UserID, ClassID);
            }
        }

        public void AdminServerShuttingDown()
        {
            try
            {
                foreach (var client in _connectedClients)
                {
                    client.Value.connection.ForceClose();
                }
            }
            catch (Exception)
            { }
        }

        public List<string> GetCourseCodes()
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var codes = db.GetCourseCodes();

                List<string> rlist = new List<string>();
                List<string> rlist2 = new List<string>();
                foreach (var code in codes)
                {
                    rlist.Add(code.ToString());
                }

                IEnumerable<string> list = rlist.Distinct();
                foreach (string classcode in list)
                {
                    rlist2.Add(classcode);
                }

                return rlist2;
            }
        }

        public int GetCourseIdNumberByCodeAndNumber(string code, string number)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                int num = Convert.ToInt32(number);
                var ID = db.GetCourseIDByCodeAndNumber(code, (short)num).First();
                //(from u in db.Classes
                //  where u.CourseCode == code && u.CourseNumber == num
                //  select new { ClassID = u }).First();
                return ID.Value;
            }
        }

        public string GetCourseNameByCodeAndNumber(string code, string number)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                int num = Convert.ToInt32(number);
                var name = db.GetCourseNameByCodeAndNumber(code, (short)num).First();
                //(from u in db.Classes
                //where u.CourseCode == code && u.CourseNumber == num
                //select new { CourseName = u }).First();

                return name;
            }
        }

        public List<string> GetCourseNumbersByCourseCode(string CourseCode)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var codes = db.GetCourseNumberByCourseCode(CourseCode);
                //from u in db.Classes
                //where u.CourseCode == CourseCode
                //select new { CourseNum = u };

                List<string> rlist = new List<string>();
                List<string> rlist2 = new List<string>();
                foreach (var code in codes)
                {
                    rlist.Add(code.Value.ToString());
                }

                IEnumerable<string> list = rlist.Distinct();
                foreach (string classcode in list)
                {
                    rlist2.Add(classcode);
                }

                return rlist2;
            }
        }

        public int AddFriend(int userId, int friendUserID)
        {
            try
            {
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    db.AddFriend(userId, friendUserID);
                }

                return friendUserID; //On success return friendID
            }
            catch (Exception)
            {
                return -1; //If Failure to add friend
            }
        }

        public int CreateChat(int userId)
        {
            try
            {
                int chatID = -1;
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    int result = db.CreateChat(userId).FirstOrDefault().Value;
                    chatID = result;
                }

                return chatID; //On success return chatID failure -1
            }
            catch (Exception)
            {
                return -1; //If Failure to create chat
            }
        }

        public int AdminCreateMentoringOrganization(string organizationName)
        {
            try
            {
                int retVal = -1;
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    retVal = db.CreateMentorOrganization(organizationName);
                }

                return retVal;
            }
            catch (Exception)
            {
                return -1; //If Failure to create organization
            }
        }

        public int AdminCreateClass(string courseName, string courseCode, int courseNumber)
        {
            try
            {
                int retVal = -1;
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    retVal = db.CreateNewClass(courseName, courseCode, (short)courseNumber);
                }

                return retVal;
            }
            catch (Exception)
            {
                return -1; //If Failure to create class
            }
        }

        public int CreateReport(string reportDescription, int flaggerID, int reviewID)
        {
            try
            {
                int retVal = -1;
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    retVal = db.CreateReport(reportDescription, flaggerID, reviewID);
                }

                return retVal;
            }
            catch (Exception)
            {
                return -1; //If Failure to create report
            }
        }

        public int CreateReview(int stickerID, int reviewerID, double starRanking, string description)
        {
            try
            {
                int retVal = -1;
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    retVal = db.CreateReview(stickerID, reviewerID, starRanking, description);
                }

                return retVal;
            }
            catch (Exception)
            {
                return -1; //If Failure to create review
            }
        }

        public int AdminDeleteClass(int classID)
        {
            try
            {
                int retVal = -1;
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    retVal = db.DeleteClassByClassID(classID);
                }

                return retVal;
            }
            catch (Exception)
            {
                return -1; //If Failure to delete class
            }
        }

        public int DeleteFriend(int userID, int fileID)
        {
            try
            {
                int retVal = -1;
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    retVal = db.DeleteFriend(userID, fileID);
                }

                return retVal;
            }
            catch (Exception)
            {
                return -1; //If Failure to remove friend
            }
        }

        public int AdminDeleteMentoringOrganization(int organizationID)
        {
            try
            {
                int retVal = -1;
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    retVal = db.DeleteMentorOrganizationByMentorID(organizationID);
                }

                return retVal;
            }
            catch (Exception)
            {
                return -1; //If Failure to delete organization
            }
        }

        public int DeleteMessage(int messageID)
        {
            try
            {
                int retVal = -1;
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    retVal = db.DeleteMessageByMessageID(messageID);
                }

                return retVal;
            }
            catch (Exception)
            {
                return -1; //If Failure to create chat.
            }
        }

        // <summary>
        // This function will only allow a user to delete their own report.
        // </summary>
        // <param name="userID"></param>
        // <param name="reportID"></param>
        // <returns></returns>
        public int DeleteReportByUser(int userID, int reportID)
        {
            try
            {
                int retVal = -1;
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    //var report = from u in db.Reports
                    //where reportID == u.ReportID
                    //select new { ReportID = u.ReportID, ReporterID = u.FlaggerID };

                    //if (report.First().ReporterID == userID)
                    //{
                    //	db.DeleteReportByReportID(reportID);
                    //}
                    //else
                    //{
                    //	throw new Exception();
                    //}

                    //not sure if this works
                    var report = db.GetReportsSubmittedByUser(userID);

                    for (int i = 0; report.ElementAt(i).ReportID == reportID; i++)
                    {
                        if (report.ElementAt(i).ReportID == reportID)
                            db.DeleteReportByReportID(reportID);
                    }
                }

                return retVal;
            }
            catch (Exception)
            {
                return -1; //failure to find or delete report
            }
        }
        public int AdminDeleteReport(int reportID)
        {
            try
            {
                int retVal = -1;
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    retVal = db.DeleteReportByReportID(reportID);
                }

                return retVal;
            }
            catch (Exception)
            {
                return -1; //If Failure to delete report
            }
        }

        public int InsertMessage(int chatID, string message, int userID)
        {
            try
            {
                int retVal = -1;
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    retVal = db.InsertMessage(chatID, message, null, null, userID);
                }

                return retVal;
            }
            catch (Exception)
            {
                return -1; //If Failure to insert message
            }
        }

        public int InsertUserIntoChat(int userID, int chatID)
        {
            try
            {
                int retVal = -1;
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    retVal = db.InsertUserIntoChat(userID, chatID);
                }

                return retVal;
            }
            catch (Exception)
            {
                return -1; //If Failure to cinsert user into chat
            }
        }

        public UserClass GetCourseCode_Name_NumberByID(int ClassID)
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var _class = db.ViewClasses(ClassID);
                //from u in db.Classes
                //where u.ClassID == ClassID
                //select new { u.CourseCode, u.CourseName, u.CourseNumber};

                UserClass classinfo = new UserClass();
                classinfo.ClassID = ClassID;
                classinfo.CourseCode = _class.First().CourseCode;
                classinfo.CourseName = _class.First().CourseName;
                classinfo.CourseNumber = _class.First().CourseNumber;

                return classinfo;
            }
        }

        public List<Organization> GetAllOrganizations()
        {
            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
            {
                var organizations = db.GetAllOrganizations();

                List<Organization> orgs = new List<Organization>();

                foreach (var org in organizations)
                {
                    Organization new_org = new Organization();
                    new_org.MentorID = org.MentorID;
                    new_org.OrganizationName = org.OrganizationName;
                    orgs.Add(new_org);
                }

                return orgs;
            }
        }

        public bool TestNewConfig()
        {
            return true;
        }

        public UnstuckMEFile UploadDocument()
        {
			UnstuckMEFile file = new UnstuckMEFile();
            file.Content = System.IO.File.ReadAllBytes(@"C:\Data\Introduction to WCF.ppt");
            file.Name = "Introduction to WCF.ppt";

            return file;
        }

		public System.IO.Stream Test(System.IO.Stream stream)
        {
            Console.WriteLine("Hello World");
			return stream;
        }

        private List<UnstuckMEChat> GetChatIDs(int userID)
        {
            try
            {
                List<UnstuckMEChat> chatIDList = new List<UnstuckMEChat>();
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    var dbChats = db.GetAllChatsAUserIsPartOF(userID);

                    foreach (var chatItem in dbChats)
                    {
                        UnstuckMEChat temp = new UnstuckMEChat();
                        temp.ChatID = chatItem.Value;
                        chatIDList.Add(temp);
                    }
                }
                return chatIDList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private List<UnstuckMEChatUser> GetChatMembers(int chatID)
        {
            try
            {
                List<UnstuckMEChatUser> UserList = new List<UnstuckMEChatUser>();
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    var chatMembers = db.GetAllMembersOfAChat(chatID);
                    foreach (var member in chatMembers)
                    {
                        UnstuckMEChatUser temp = new UnstuckMEChatUser();
                        temp.UserID = member.UserID;
                        temp.UserName = member.DisplayFName;
                        temp.ProfilePicture = null;
                        UserList.Add(temp);
                    }
                }
                return UserList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        private List<UnstuckMEMessage> GetChatMessages(int chatID)
        {
            try
            {
                List<UnstuckMEMessage> MessageList = new List<UnstuckMEMessage>();
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
					var messages = db.GetChatMessages(chatID);

                    foreach (var message in messages)
                    {
                        UnstuckMEMessage temp = new UnstuckMEMessage();
                        temp.Message = message.MessageData;
                        temp.MessageID = message.MessageID;
                        temp.Time = message.SentTime;
                        //temp.IsFile = message.IsFile;
                        temp.FilePath = message.FilePath;
                        temp.ChatID = chatID;
                        temp.SenderID = message.SentBy;
                        temp.Username = message.DisplayFName + " " + message.DisplayLName;
                        MessageList.Add(temp);
                    }
                }
                return MessageList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public List<UnstuckMEChat> GetUserChats(int userID)
        {
            try
            {
                List<UnstuckMEChat> chatList = GetChatIDs(userID);
                if (chatList == null) { throw new Exception("ChatID List Retrieval Failure"); }
                foreach (UnstuckMEChat chat in chatList)
                {
                    chat.Users = GetChatMembers(chat.ChatID);
                    chat.Messages = GetChatMessages(chat.ChatID);
                }
                return chatList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public void SendMessage(UnstuckMEMessage message)
        {
            try
            {
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    db.InsertMessage(message.ChatID, message.Message, null, false, message.SenderID);
                }
                foreach (int client in message.UsersInConvo)
                {
                    if (client != message.SenderID)
                    {
                        if (_connectedClients.ContainsKey(client)) //Checks to see if client is online.
                        {
                            _connectedClients[client].connection.GetMessage(message);
                        }
                    }
                }
                //foreach (var user in _connectedClients)
                //{
                //    if (user.Key != message.SenderID)
                //    {
                //        string test = user.Value.connection.GetMessage(message);
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

		public void UploadFile(UnstuckMEMessage message, UnstuckMEFile file)
		{
			try
			{
				using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
				{
					//db.InsertMessage(message.ChatID, message.Message, message.FilePath, message.IsFile, message.SenderID);
					//user uploads a file contained in a message
					//server receives message
					//server saves bytes locally and updates filepath
					//server updates database with new message
					//server sends message to users in the chat

				}

				foreach (int client in message.UsersInConvo)
				{
					if (client != message.SenderID && _connectedClients.ContainsKey(client))
						_connectedClients[client].connection.GetFile(message, file);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

        public List<UnstuckMEAvailableSticker> InitialAvailableStickerPull(int userID)
        {
            try
            {
                List<UnstuckMEAvailableSticker> stickerList = new List<UnstuckMEAvailableSticker>();
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    var dbStickers = from a in db.Stickers
                                     where (a.StudentID != userID) && (a.MinimumStarRanking <= (from b in db.UserProfiles where b.UserID == userID select b.AverageTutorRank).FirstOrDefault()) && a.TutorID == null
                                     join c in db.Classes on a.ClassID equals c.ClassID
                                     join d in db.UserProfiles on a.StudentID equals d.UserID
                                     select new
                                     {
                                         StickerID = a.StickerID,
                                         StickerTimeout = a.Timeout,
                                         ProblemDescription = a.ProblemDescription,
                                         StudentID = a.StudentID,
                                         StudentRanking = d.AverageStudentRank,
                                         ClassID = c.ClassID,
                                         CourseCode = c.CourseCode,
                                         CourseName = c.CourseName,
                                         CourseNumber = c.CourseNumber,
                                     };

                    foreach (var sticker in dbStickers)
                    {
                        UnstuckMEAvailableSticker temp = new UnstuckMEAvailableSticker();
                        temp.ClassID = sticker.ClassID;
                        temp.CourseCode = sticker.CourseCode;
                        temp.CourseName = sticker.CourseName;
                        temp.CourseNumber = sticker.CourseNumber;
                        temp.ProblemDescription = sticker.ProblemDescription;
                        temp.StickerID = sticker.StickerID;
                        temp.StudentID = sticker.StudentID;
                        temp.StudentRanking = sticker.StudentRanking;
                        temp.Timeout = sticker.StickerTimeout;
                        stickerList.Add(temp);
                    }

                }
                return stickerList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        public UnstuckMEChat GetSingleChat(int chatID)
        {
            try
            {
                UnstuckMEChat returnedChat = new UnstuckMEChat();
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    //var dbChat = //from a in db.Chats
                                 //where a.ChatID == chatID
                                 //select a;
                    returnedChat.ChatID = chatID;
                    returnedChat.Messages = GetChatMessages(chatID);
                    returnedChat.Users = GetChatMembers(chatID);
                }

                return returnedChat;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public UserClass GetSingleClass(int classID)
        {
            try
            {
                UserClass temp = new UserClass();
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
					var classDB = db.ViewClasses(classID).First();
								  //from a in db.Classes
                                  //where a.ClassID == classID
                                  //select new { ClassID = a.ClassID, CourseCode = a.CourseCode, CourseName = a.CourseName, CourseNumber = a.CourseNumber };

                    temp.ClassID = classDB.ClassID;
                    temp.CourseNumber = classDB.CourseNumber;
                    temp.CourseName = classDB.CourseName;
                    temp.CourseCode = classDB.CourseCode;
                }
                return temp;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public void AddClassesToClient(int inClass, int userID)
        {
            try
            {
                UserClass temp = new UserClass();

                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
					var c = db.ViewClasses(inClass).First();
							//from a in db.Classes
                            //where a.ClassID == inClass
                            //select new { a.ClassID, a.CourseCode, a.CourseName, a.CourseNumber };

                    temp.CourseNumber = c.CourseNumber;
                    temp.CourseName = c.CourseName;
                    temp.CourseCode = c.CourseCode;
                    temp.ClassID = c.ClassID;
                }

                foreach (var client in _connectedClients)
                {
                    if (client.Key == userID)
                    {
                        client.Value.connection.AddClasses(temp);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void AcceptSticker(int tutorID, int stickerID)
        {
            try
            {
                using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
                {
                    db.UpdateTutorIDByTutorIDAndStickerID(tutorID, stickerID);
                }
                foreach (var client in _connectedClients)
                {
                    if(client.Key != tutorID)
                    {
                        client.Value.connection.RemoveGUISticker(stickerID);
                    }  
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}