﻿using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using UnstuckMEServer;
using UnstuckME_Classes;

namespace UnstuckMEInterfaces
{
    /// <summary>
    /// Implement any Operation Contracts from IUnstuckMEService.cs in this file.
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
	public partial class UnstuckMEService : IUnstuckMEService, IUnstuckMEServer, IUnstuckMEFileStream
	{
		private readonly ConcurrentDictionary<int, ConnectedClient> _connectedClients = new ConcurrentDictionary<int, ConnectedClient>();
		private readonly ConcurrentDictionary<int, ConnectedServerAdmin> _connectedServerAdmins = new ConcurrentDictionary<int, ConnectedServerAdmin>();
		private static ConcurrentQueue<UnstuckMEBigSticker> _stickerList;
		private static ConcurrentQueue<UnstuckMEMessage> _messageList;
		private static readonly ConcurrentQueue<int> _reviewList = new ConcurrentQueue<int>();
		
		#region Thread Functions
        /// <summary>
        /// Checks to see if there are any new messages for the client. If there are, sends the message to that client
        /// as long as they are logged into the server.
        /// </summary>
        public async void CheckForNewMessages()
	    {
	        _messageList = new ConcurrentQueue<UnstuckMEMessage>();

	        while (true)
	        {
	            while (_messageList.Count != 0)
	            {
	                UnstuckMEMessage temp;
	                _messageList.TryDequeue(out temp);

	                try
	                {
	                    await Task.Factory.StartNew(() => AsyncMessageSendToUsers(temp));
	                }
	                catch (Exception ex)
	                {
	                    Console.WriteLine("Failed to send message to users, {0}", ex.Message);
	                }
	            }
                Thread.Sleep(500);
	        }
	    }

	    /// <summary>
	    /// Asyncronously sends a message to a user and logs it in the database.
	    /// </summary>
	    /// <param name="inMessage">The message to be sent and stored in the database.</param>
	    private void AsyncMessageSendToUsers(UnstuckMEMessage inMessage)
	    {
	        foreach (int client in inMessage.UsersInConvo)
	        {
	            if (client != inMessage.SenderID)
	            {
	                if (_connectedClients.ContainsKey(client)) //Checks to see if client is online.
	                    _connectedClients[client].Connection.GetMessage(inMessage);
	            }
	        }
	    }

	    /// <summary>
		/// Starts a new task that sends stickers to the clients who meet the criteria specified with the sticker.
		/// </summary>
		public async void CheckForNewStickers()
		{
			_stickerList = new ConcurrentQueue<UnstuckMEBigSticker>();
			while (true)
			{
				while (_stickerList.Count != 0)
				{
					UnstuckMEBigSticker temp;
					_stickerList.TryDequeue(out temp);

					await Task.Factory.StartNew(() => SendStickerToClients(temp));
				}
                Thread.Sleep(5000);
			}
		}

		/// <summary>
		/// Checks for online users that match the criteria specified with the sticker and sends it to those clients.
		/// </summary>
		/// <param name="inSticker">The sticker to be sent to qualified online users.</param>
		public void SendStickerToClients(UnstuckMEBigSticker inSticker)
		{
			UnstuckMEAvailableSticker s = new UnstuckMEAvailableSticker
			{
				ClassID = inSticker.Class.ClassID,
				ProblemDescription = inSticker.ProblemDescription,
				StudentID = inSticker.StudentID,
				StickerID = inSticker.StickerID,
				Timeout = inSticker.Timeout,
				CourseCode = inSticker.Class.CourseCode,
				CourseName = inSticker.Class.CourseName,
				CourseNumber = inSticker.Class.CourseNumber,
				StudentRanking = inSticker.StudentRanking
			};

			try
			{
				using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
				{
					var tutors = db.GetUsersThatCanTutorASticker(s.StickerID);

					foreach (var tutor in tutors)
					{
						if (tutor.HasValue && _connectedClients.ContainsKey(tutor.Value))
						    _connectedClients[tutor.Value].Connection.RecieveNewSticker(s);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("SendStickerToClients Function Error: " + ex.Message);
			}
		}
        #endregion

        /// <summary>
        /// Checks to see if the email address exists on the database.
        /// </summary>
        /// <param name="emailAddress">The email address of the user.</param>
        /// <returns>True if the email address is specified with an account, false if not.</returns>
        public bool IsValidUser(string emailAddress)
	    {
	        bool retVal = false;

	        try
	        {
	            if (GetUserInfo(null, emailAddress) != null)
    	            retVal = true;
	        }
	        catch (Exception ex)
	        {
	            Console.WriteLine(ex.Message);
	        }

            return retVal;
        }

	    /// <summary>
		/// Disconnects a user from the server.
		/// </summary>
		public void Logout()
		{
			ConnectedClient client = GetMyClient();
			if (client != null)
			{
				ConnectedClient removedClient;
				_connectedClients.TryRemove(client.User.UserID, out removedClient);

				foreach (var admin in _connectedServerAdmins)
				    admin.Value.Connection.GetUpdate(1, removedClient.User);

                Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Client Loggoff: {0} at {1}", removedClient.User.EmailAddress, DateTime.Now);
				Console.ResetColor();
			}
		}

		/// <summary>
		/// Checks the list of connected clients for the connection of the calling client.
		/// </summary>
		/// <returns>The client that is trying to disconnect.</returns>
		public ConnectedClient GetMyClient()
		{
			IClient establishedUserConnection = OperationContext.Current.GetCallbackChannel<IClient>();
			foreach (var client in _connectedClients)
			{
				if (client.Value.Connection == establishedUserConnection)
				    return client.Value;
			}

			return null;
		}

		/// <summary>
		/// Checks the list of connected server administrators for the connection of the calling client.
		/// </summary>
		/// <returns>The server administrator that is trying to disconnect.</returns>
		public ConnectedServerAdmin GetMyAdmin()
		{
			IServer establishedAdminConnection = OperationContext.Current.GetCallbackChannel<IServer>();
			foreach (var admin in _connectedServerAdmins)
			{
				if (admin.Value.Connection == establishedAdminConnection)
				    return admin.Value;
			}
			return null;
		}

		/// <summary>
		/// Changes the password of the specified user. Rehashes the password before storing on the database.
		/// </summary>
		/// <param name="User">All the necessary user information.</param>
		/// <param name="newPassword">The new password.</param>
		public void ChangePassword(UserInfo User, string newPassword)
		{
			using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
			{
				UnstuckMEPassword newHashedPassword = UnstuckMEHashing.GetHashedPassword(newPassword);
				var users = (from b in db.UserProfiles
							where b.UserID == User.UserID
							select b).First();
				
				users.UserPassword = newHashedPassword.Password;
				users.Salt = newHashedPassword.Salt;
				db.SaveChanges();
			}
		}

		/// <summary>
		/// Sends an account verification email containing a code in order to activiate the account. Uses email settings configured
		/// by the server administrator to send the email.
		/// </summary>
		/// <param name="emailtype">An enum specifying the purpose of the email being sent.</param>
		/// <param name="userEmailAddress">The email address of the new user.</param>
		/// <param name="username">The first name of the new user.</param>
		/// <returns>An 8-character code that must be entered in on the client in order to activate the account.</returns>
		public string SendEmail(EmailType emailtype, string userEmailAddress, string username)
		{
			string verificationCode = GenerateVerificationCode();  //generate a random 8-character verification code

			var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			var mailSettings = (SmtpSection)config.GetSection("system.net/mailSettings/smtp");
			MailAddress address = new MailAddress(mailSettings.Network.UserName, "UnstuckME");
			MailMessage email = new MailMessage(address, new MailAddress(userEmailAddress));

			switch (emailtype)
			{
				case EmailType.CreateAccount:
					{
						email.Subject = "Activating your UnstuckME account";
						//this body is temporary as recipient shouldn't reply to the email
						email.Body = "Thanks for joining UnstuckME " + username + "! Please activate your account by entering the verification code below into the prompt in the application.\n\n"
							+ "By creating an account, you agree to UnstuckME Terms of Service and your University's Student Code of Conduct\n\nYour verification code:\t" + verificationCode
							+ "\n\nIf something is not working, please reply to this email with your problem and we will attempt to solve your issue.";
						email.Priority = MailPriority.Normal;
						break;
					}
				case EmailType.ResetPassword:
					{
						email.Subject = "UnstuckME account password reset";
						//this body is temporary as recipient shouldn't reply to the email
						email.Body = "Hello, " + username + "!\n\nThis is to notify you of an attempt to change your UnstuckME account password. You may use this password to log in to your account"
							+ ", but please remember to change it again from within the application once you have successfully logged in. To do this, go to your User Profile and click on 'Edit Profile'"
							+ " button directly underneath your profile picture. To change your password from within the application, enter your new password in the box and click 'Save'.\n\nYour "
							+ "temporary password is:\t" + verificationCode + "\n\nIf you did not request to reset your UnstuckME account password or you are experiencing other problems, please "
							+ "reply to this email and we will attempt to solve your issue.";
						email.Priority = MailPriority.Normal;
						break;
					}
			}

			SmtpClient client = new SmtpClient
			{
				Credentials = new NetworkCredential(mailSettings.Network.UserName, mailSettings.Network.Password),
				DeliveryFormat = mailSettings.DeliveryFormat,
				DeliveryMethod = mailSettings.DeliveryMethod,
				EnableSsl = mailSettings.Network.EnableSsl,
				Timeout = 300000    //milliseconds = 300 seconds = 5 minutes
			};

			try
			{
				client.Send(email);		//send the email
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw;					  //throw the error back to the client
			}
			finally
			{
				email.Dispose();    //clean up memory
				client.Dispose();
			}

			return verificationCode;
		}

		/// <summary>
		/// Generates a random 8-character verification code for a user to activate their account.
		/// </summary>
		/// <returns>A randomly generated 8-character code.</returns>
		private static string GenerateVerificationCode()
		{
			string value = string.Empty;

			try
			{
				using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
				{
					byte[] tokenData = new byte[128];
					rng.GetBytes(tokenData);

					for (int i = 0, bytesSkipped = 0; i < tokenData.Length && value.Length < 8; i++)
					{
					    while ((tokenData[i + bytesSkipped] <= 48 || tokenData[i + bytesSkipped] >= 57) &&
								(tokenData[i + bytesSkipped] <= 65 || tokenData[i + bytesSkipped] >= 90) &&
								(tokenData[i + bytesSkipped] <= 97 || tokenData[i + bytesSkipped] >= 122))
					        bytesSkipped++;

					    value += Convert.ToChar(tokenData[i + bytesSkipped]);
					}
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex.Message);
				Console.ResetColor();
			}

			return value;
		}

	    /// <summary>
	    /// Gets the size in bytes of file specified by <paramref name="messageID"/>.
	    /// </summary>
	    /// <param name="messageID">The unique identifier of the message to get the filepath from.</param>
	    /// <returns>Returns the length in bytes of the file if the file exists, -1 if it doesn't.</returns>
	    public long GetFileSize(int messageID)
	    {
	        try
	        {
	            using (UnstuckME_DBEntities db = new UnstuckME_DBEntities())
	            {
	                string filepath = (from m in db.Messages
	                                where m.MessageID == messageID
	                                select m.FilePath).First();

	                FileInfo file = new FileInfo(filepath);
	                return file.Exists ? file.Length : -1;
	            }
	        }
	        catch (Exception)
	        {
	            return -1;
	        }
	    }
	}
}