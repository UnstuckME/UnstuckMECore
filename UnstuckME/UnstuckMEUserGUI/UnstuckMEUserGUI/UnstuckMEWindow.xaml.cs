﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using UnstuckMeLoggers;
using UnstuckMEUserGUI.SubWindows;
using UnstuckME_Classes;

namespace UnstuckMEUserGUI
{
	/// <summary>
	/// Interaction logic for UnstuckMEWindow.xaml
	/// </summary>
	public partial class UnstuckMEWindow : Window
	{
		private static Privileges userPrivileges;

		public UnstuckMEWindow()
		{
			InitializeComponent();
			UnstuckME.MainWindow = this;

			userPrivileges = UnstuckME.User.Privileges;
			CheckAdminPrivledges(userPrivileges); //Disables/Enables Admin/Moderator Tab Depending on Privilege

			UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_LOGIN, UnstuckME.User.EmailAddress);
		}

		private async void Window_ContentRendered(object sender, EventArgs e)
		{
			UnstuckME.Red = StickerButton.Background;
			await Task.Factory.StartNew(() => InitializeStaticMembers());
			UnstuckME.Pages.StickerPage = new StickerPage();
			UnstuckME.Pages.SettingsPage = new SettingsPage();
			UnstuckME.Pages.ChatPage = new ChatPage();
			UnstuckME.Pages.UserProfilePage = new UserProfilePage();
			UnstuckME.Pages.ModeratorPage = new ModeratorPage();
			UnstuckME.Pages.AdminPage = new AdminPage();

			SwitchToStickerTab();
			try
			{
				await Task.Factory.StartNew(() => LoadStickerPageAsync());
				await Task.Factory.StartNew(() => LoadSideBarsAsync());
				await Task.Factory.StartNew(() => LoadChatPageAsync());
				await Task.Factory.StartNew(() => LoadUserProfilePageAsync());
				await Task.Factory.StartNew(() => LoadSettingsPageAsync());
				await Task.Factory.StartNew(() => LoadAdminPageAsync());
			}
			catch (InvalidOperationException ex)
			{
			    var trace = new StackTrace(ex, true).GetFrame(0).GetMethod();
                UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_INTERACTION_ERROR, ex.Message, trace.Name);
			}

			LoadingScreen.Visibility = Visibility.Collapsed;
			await Task.Factory.StartNew(() => CheckForReviews());
		}

		#region Asynchronous Loading Section
		private void InitializeStaticMembers()
		{
			UnstuckME.ImageConverter = new ImageSourceConverter();
			UnstuckME.Pages = new UnstuckMEPages();

			using (MemoryStream ms = new MemoryStream())
			{
				UnstuckME.FileStream.GetProfilePicture(UnstuckME.User.UserID).CopyTo(ms);
				UnstuckME.User.UserProfilePictureBytes = ms.ToArray();
				UnstuckME.UserProfilePicture = UnstuckME.ImageConverter.ConvertFrom(UnstuckME.User.UserProfilePictureBytes) as ImageSource;  //convert image so it can be displayed
			}
		}

		private void LoadStickerPageAsync()
		{
			try
			{
                Dispatcher.Invoke(() =>
				{
					UnstuckME.Pages.StickerPage.AvailableStickers = UnstuckME.Server.InitialAvailableStickerPull(UnstuckME.User.UserID);
					foreach (UnstuckMEAvailableSticker sticker in UnstuckME.Pages.StickerPage.AvailableStickers)
					    UnstuckME.Pages.StickerPage.StackPanelAvailableStickers.Children.Add(new AvailableSticker(sticker));

				    UnstuckME.Pages.StickerPage.MyStickersList = UnstuckME.Server.GetUserSubmittedStickers(UnstuckME.User.UserID);
					foreach (UnstuckMESticker sticker in UnstuckME.Pages.StickerPage.MyStickersList)
					    UnstuckME.Pages.StickerPage.StackPanelMyStickers.Children.Add(new MySticker(sticker));

				    UnstuckME.Pages.StickerPage.OpenStickers = UnstuckME.Server.GetUserTutoredStickers(UnstuckME.User.UserID);
					foreach (UnstuckMESticker sticker in UnstuckME.Pages.StickerPage.OpenStickers)
					    UnstuckME.Pages.StickerPage.StackPanelOpenStickers.Children.Add(new OpenSticker(sticker));

				    UnstuckME.Pages.StickerPage.RecentStickers = UnstuckME.Server.GetStickerHistory(UnstuckME.User.UserID);
					foreach (UnstuckMESticker sticker in UnstuckME.Pages.StickerPage.RecentStickers)
					    UnstuckME.Pages.StickerPage.StackPanelStickerHistory.Children.Add(new StickerHistory(sticker));
				});
			}
			catch (Exception ex)
			{
				UnstuckMEMessageBox error = new UnstuckMEMessageBox(UnstuckMEBox.OK, "Error Loading Stickers, Please Contact Your Server Administrator if Problem Persists.\n" + "Error Message: " + ex.Message, "Sticker Loading Error", UnstuckMEBoxImage.Error);
				error.ShowDialog();
			}
		}

		private void LoadChatPageAsync()
		{
			try
			{
                Dispatcher.Invoke(() =>
				{
					//// ===========================Ryan's optimized code=========================================
					//UnstuckME.ChatSessions = UnstuckME.Server.GetChatIDs(UnstuckME.User.UserID);
					//List<string> folders =  Directory.GetDirectories(UnstuckME.ProgramDir.ChatDir).ToList();
					//List<int?> oldFriends = UnstuckME.ProgramDir.GetAllFriends();
     //               List<int?> newFriends = new List<int?>();

     //               foreach (UnstuckMEChat chat in UnstuckME.ChatSessions)
					//{
					//	string path = UnstuckME.ProgramDir.ChatDir + @"\" + chat.ChatID;
					//	if (folders.Contains<string>(path))
					//	    folders.Remove(path);
					//	else
					//	    UnstuckME.ProgramDir.MakeChatDir(chat.ChatID.ToString());

					//    UnstuckME.ProgramDir.AddChatDatFile(chat.ChatID.ToString());
					//	UnstuckME.ProgramDir.AddNewMsgFile(chat.ChatID.ToString(), true);
     //                   List<int?>  tempfriends = UnstuckME.Server.GetMemberIDsFromChat(chat.ChatID); //Users from server
     //                   foreach (var user in tempfriends)
     //                   {
     //                       if (!oldFriends.Contains(user) && !newFriends.Contains(user))
     //                           newFriends.Add(user);
     //                   }
					//	//newFriends.AddRange(tempfriends.Where(id => !oldFriends.Contains(id) && !newFriends.Contains(id)));
					//}
     //               oldFriends.RemoveAll(i => newFriends.Contains(i));

     //               UnstuckMEChatUser newUser = new UnstuckMEChatUser();
     //               foreach (int? nf in newFriends)
     //               {
     //                   if (nf.HasValue)
     //                   {
     //                       newUser = UnstuckME.Server.GetFriendInfo(nf.Value);
     //                       newUser.UserID = nf.Value;

     //                       using (MemoryStream ms = new MemoryStream())
     //                       {
     //                           UnstuckME.FileStream.GetProfilePicture(newUser.UserID).CopyTo(ms);
     //                           newUser.UnProccessPhot = ms.ToArray();
     //                       }

     //                       UnstuckME.ProgramDir.AddNewFriend(newUser);
     //                   }
     //               }

     //               oldFriends.ForEach(i => UnstuckME.ProgramDir.DeleteFriend(i));
     //               folders.ForEach(i => Directory.Delete(i, true)); // Recursively delete contents of a directory

					// ~~~~~~~~~~~~~~~~~~~~AJ's Code that is currently getting re-factored~~~~~~~~~~~~~~~~~~~~~~~~~
					UnstuckME.ChatSessions = UnstuckME.Server.GetUserChats(UnstuckME.User.UserID);
					foreach (UnstuckMEChat chat in UnstuckME.ChatSessions)
					    UnstuckME.Pages.ChatPage.AddConversation(chat);
				    foreach (UnstuckMEChatUser user in UnstuckME.FriendsList)
				        UnstuckME.Pages.ChatPage.StackPanelAddContacts.Children.Add(new ContactCreateConversation(user));
				});
			}
			catch(Exception ex)
			{
				UnstuckMEMessageBox error = new UnstuckMEMessageBox(UnstuckMEBox.OK, "Error Loading Chat Page, Please Contact Your Server Administrator if Problem Persists.\n" + "Error Message: " + ex.Message, "Chat Loading Error", UnstuckMEBoxImage.Error);
				error.ShowDialog();
			}
		}

		private void LoadUserProfilePageAsync()
		{
			try
			{
                Dispatcher.Invoke(() =>
				{
					UnstuckME.Pages.UserProfilePage.ProfilePicture.Source = UnstuckME.UserProfilePicture;  //convert image so it can be displayed
					UnstuckME.Pages.UserProfilePage.ImageEditProfilePicture.Source = UnstuckME.UserProfilePicture;
					UnstuckME.Pages.UserProfilePage.UpdateRatings(UnstuckME.User.AverageStudentRank, UnstuckME.User.AverageTutorRank);
					UnstuckME.Pages.UserProfilePage.RepopulateClasses();
				    UnstuckME.Pages.UserProfilePage.PopulateReviews();
				});
			}
			catch (Exception ex)
			{
				UnstuckMEMessageBox error = new UnstuckMEMessageBox(UnstuckMEBox.OK, "Error Loading User Profile Page, Please Contact Your Server Administrator if Problem Persists.\n" + "Error Message: " + ex.Message, "Profile Loading Error", UnstuckMEBoxImage.Error);
				error.ShowDialog();
			}
		}

		private void LoadSettingsPageAsync()
		{
            Dispatcher.Invoke(() =>
			{
				// your asynchronous code here.
			});
		}

		private void LoadAdminPageAsync()
		{
            Dispatcher.Invoke(() =>
			{
				// your asynchronous code here.
			});
		}

		private void LoadSideBarsAsync()
		{
			try
			{
                Dispatcher.Invoke(() =>
				{
					UnstuckME.FriendsList = UnstuckME.Server.GetFriends(UnstuckME.User.UserID);
					foreach (UnstuckMEChatUser friend in UnstuckME.FriendsList)
					    OnlineUsersStack.Children.Add(new OnlineUser(friend));
				});
			}
			catch (Exception ex)
			{
				UnstuckMEMessageBox error = new UnstuckMEMessageBox(UnstuckMEBox.OK, "Error Right Side Panel, Please Contact Your Server Administrator if Problem Persists.\n" + "Error Message: " + ex.Message, "Contacts/Notifications Loading Error", UnstuckMEBoxImage.Error);
				error.ShowDialog();
			}
		}

		private void CheckForReviews()
		{
			Thread.Sleep(2000);    //wait 2 seconds so everything can be placed

			try
			{
                Dispatcher.Invoke(() =>
				{
					KeyValuePair<int, bool> needsreview = UnstuckME.Server.CheckForReviews(UnstuckME.User.UserID);

					if (needsreview.Key != 0)
					{
						Window win;
						
						if (needsreview.Value)
							win = new AddTutorReviewWindow(needsreview.Key);
						else
							win = new AddStudentReviewWindow(needsreview.Key);

						win.ShowDialog();
						//win.Focus();
					}
				});
			}
			catch (Exception ex)
			{
			    var trace = new StackTrace(ex, true).GetFrame(0).GetMethod();
                UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_SERVER_CONNECTION_ERROR, ex.Message, trace.Name);
				UnstuckMEMessageBox error = new UnstuckMEMessageBox(UnstuckMEBox.OK, "You need to submit a review for the stickers you have open.", "Review Needed", UnstuckMEBoxImage.Information);
				error.ShowDialog();
			}
		}

		#endregion

		public void AddUserToContactsStack(UnstuckMEChatUser inChatUser)
		{
			OnlineUsersStack.Children.Add(new OnlineUser(inChatUser));
		}

		public void CheckAdminPrivledges(Privileges inPrivleges)
		{
			switch (inPrivleges)
			{
				case Privileges.Admin: //Admin
					{
						AdminButton.Visibility = Visibility.Visible;
						AdminButton.IsEnabled = true;
						break;
					}
				case Privileges.Moderator: //Moderator
					{
						AdminButton.Visibility = Visibility.Visible;
						AdminButton.IsEnabled = true;
						break;
					}
				case Privileges.User: //User
					{
						AdminButton.Visibility = Visibility.Hidden;
						AdminButton.IsEnabled = false;
						break;
					}
				default:
					break;
			}
		}

		public void SwitchToChatTab()
		{
			MainFrame.NavigationService.RemoveBackEntry();
			MainFrame.Navigate(UnstuckME.Pages.ChatPage);
			ChatButton.Background = UnstuckME.Red;
			StickerButton.Background = UnstuckME.Blue;
			SettingButton.Background = UnstuckME.Blue;
			UserProfileButton.Background = UnstuckME.Blue;
			AdminButton.Background = UnstuckME.Blue;
			DisableStickerSubmit();
		}
		public void SwitchToStickerTab()
		{
			MainFrame.NavigationService.RemoveBackEntry();
			MainFrame.Navigate(UnstuckME.Pages.StickerPage);
			ChatButton.Background = UnstuckME.Blue;
			StickerButton.Background = UnstuckME.Red;
			SettingButton.Background = UnstuckME.Blue;
			UserProfileButton.Background = UnstuckME.Blue;
			AdminButton.Background = UnstuckME.Blue;
			EnableStickerSubmit();
		}
		public void SwitchToUserProfileTab()
		{
			MainFrame.NavigationService.RemoveBackEntry();
			MainFrame.Navigate(UnstuckME.Pages.UserProfilePage);
			ChatButton.Background = UnstuckME.Blue;
			StickerButton.Background = UnstuckME.Blue;
			SettingButton.Background = UnstuckME.Blue;
			UserProfileButton.Background = UnstuckME.Red;
			AdminButton.Background = UnstuckME.Blue;
			DisableStickerSubmit();
		}
		public void SwitchToSettingsTab()
		{
			MainFrame.NavigationService.RemoveBackEntry();
			MainFrame.Navigate(UnstuckME.Pages.SettingsPage);
			ChatButton.Background = UnstuckME.Blue;
			StickerButton.Background = UnstuckME.Blue;
			SettingButton.Background = UnstuckME.Red;
			UserProfileButton.Background = UnstuckME.Blue;
			AdminButton.Background = UnstuckME.Blue;
			DisableStickerSubmit();
		}
		public void SwitchToAdminTab(Privileges inPriviledges)
		{
			switch (inPriviledges)
			{
				case Privileges.Admin: //Admin
					{
						MainFrame.Navigate(UnstuckME.Pages.AdminPage);
						break;
					}
				case Privileges.Moderator: //Moderator
					{
						MainFrame.Navigate(UnstuckME.Pages.ModeratorPage);
						break;
					}
				default: //In case someone figures out a way to make admin button show
					{
                        UnstuckMEMessageBox error = new UnstuckMEMessageBox(UnstuckMEBox.OK, "You do not have access to this tab.", "Unauthorized Access", UnstuckMEBoxImage.Warning);
                        error.ShowDialog();
                        return;
					}
			}

			ChatButton.Background = UnstuckME.Blue;
			StickerButton.Background = UnstuckME.Blue;
			SettingButton.Background = UnstuckME.Blue;
			UserProfileButton.Background = UnstuckME.Blue;
			AdminButton.Background = UnstuckME.Red;
			DisableStickerSubmit();
		}

		private void DisableStickerSubmit()
		{
			CreateStickerButton.Visibility = Visibility.Hidden;
			CreateStickerButton.IsEnabled = false;
		}

		private void EnableStickerSubmit()
		{
			CreateStickerButton.Visibility = Visibility.Visible;
			CreateStickerButton.IsEnabled = true;
		}

		public void AddStickerToMyStickers(UnstuckMESticker inSticker)
		{
			UnstuckME.Pages.StickerPage.StackPanelMyStickers.Children.Add(new MySticker(inSticker));
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
            UnstuckME.UserExit = true;
			try
			{
                UnstuckME.ChannelFactory.Abort();
				//UnstuckME.Server.Logout();
			}
			catch (Exception)
			{ /*This is empty because we don't want the program to break but we also don't want to catch this exception*/ }
		    UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_LOGOUT, UnstuckME.User.EmailAddress);
        }

        public void RecieveChatMessage(UnstuckMEMessage message)
		{
			UnstuckME.Pages.ChatPage.AddMessage(message);
			if (UnstuckME.CurrentChatSession.ChatID != message.ChatID || MainFrame.Content != UnstuckME.Pages.ChatPage)
			{
				NewMessageNotification temp = null; 
				foreach (var notification in NotificationStack.Children.OfType<NewMessageNotification>())
				{
					if(notification.Message.ChatID == message.ChatID)
					    temp = notification;
				}
				if (temp == null)
				    NotificationStack.Children.Insert(0, new NewMessageNotification(message));
				else
				{
					NotificationStack.Children.Insert(0, new NewMessageNotification(message, temp.NotificationCount + 1));
					NotificationStack.Children.Remove(temp);
				}
			}
		}

		public void RecieveAddedClass(UserClass inClass)
		{
            Dispatcher.Invoke(() =>
			{
				try
				{
					ClassDisplay temp = new ClassDisplay(inClass);
					UnstuckME.Pages.UserProfilePage.BottomLeftStack.Children.Add(temp);
				}
				catch (Exception)
				{ }
			});
		}

		public void RecieveNewAvailableSticker(UnstuckMEAvailableSticker sticker)
		{
            Dispatcher.Invoke(() =>
			{
				try
				{
					UnstuckME.Pages.StickerPage.AvailableStickers.Add(sticker);
					UnstuckME.Pages.StickerPage.StackPanelAvailableStickers.Children.Add(new AvailableSticker(sticker));
					//NotificationStack.Children.Add(new AvailableStickerNotification(sticker));  This looks gross currently so I'm not going to use it.
				}
				catch (Exception)
				{
                    UnstuckMEMessageBox error = new UnstuckMEMessageBox(UnstuckMEBox.OK, "Sticker Update Failed", "Sticker Update Failure", UnstuckMEBoxImage.Warning);
                    error.ShowDialog();
                }
			});
		}

		public void RemoveStickerFromAvailableStickers(int stickerID)
		{
            Dispatcher.Invoke(() =>
			{
				try
				{
					AvailableSticker temp = null;
					foreach (var control in UnstuckME.Pages.StickerPage.StackPanelAvailableStickers.Children.OfType<AvailableSticker>())
					{
						if (control.Sticker.StickerID == stickerID)
						    temp = control;
					}
					if (temp != null)
					    UnstuckME.Pages.StickerPage.StackPanelAvailableStickers.Children.Remove(temp);
				}
				catch (Exception ex)
				{
				    var trace = new StackTrace(ex, true).GetFrame(0).GetMethod();
				    UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_INTERACTION_ERROR, "Failed to remove sticker from available stickers", trace.Name);
				}
			});
		}

		public void StickerAcceptedStartConversation(UnstuckMEAvailableSticker sticker, int tutorID)
		{
            Dispatcher.Invoke(() =>
			{
				int PreExistingChatID = -1;
				//Search For Pre-Existing Conversation
				foreach (UnstuckMEChat chat in UnstuckME.ChatSessions)
				{
					bool studentFound = false;
					bool tutorFound = false;
					if (chat.Users.Count == 2)
					{
						foreach (UnstuckMEChatUser user in chat.Users)
						{
							if (user.UserID == sticker.StudentID)
							    studentFound = true;
						    if (user.UserID == tutorID)
						        tutorFound = true;
						}
						if (studentFound && tutorFound)
						    PreExistingChatID = chat.ChatID;
					}
				}

				if (PreExistingChatID == -1)
				{
					//Chat Not Found. Creating a new one.
					PreExistingChatID = UnstuckME.Server.CreateChat(UnstuckME.User.UserID);
					UnstuckME.Server.InsertUserIntoChat(sticker.StudentID, PreExistingChatID);
				}

				UnstuckMEMessage temp = new UnstuckMEMessage
				{
					ChatID = PreExistingChatID,
					FilePath = string.Empty,
					Message = UnstuckME.User.FirstName + " " + UnstuckME.User.LastName + " has accepted a sticker you submitted!",
					MessageID = 0,
					SenderID = UnstuckME.User.UserID,
					Username = UnstuckME.User.FirstName,
					UsersInConvo = new List<int>()
				};

				temp.UsersInConvo.Add(UnstuckME.User.UserID);
				temp.UsersInConvo.Add(sticker.StudentID);
				temp.MessageID = UnstuckME.Server.SendMessage(temp);
				temp.Message = "Your have accepted a Sticker!";

				UnstuckMESticker tempSticker = new UnstuckMESticker
				{
					ChatID = PreExistingChatID,
					ClassID = sticker.ClassID,
					ProblemDescription = sticker.ProblemDescription,
					StickerID = sticker.StickerID,
					StudentID = sticker.StudentID,
					Timeout = sticker.Timeout,
					TutorID = UnstuckME.User.UserID
				};

				UnstuckME.Pages.StickerPage.StackPanelOpenStickers.Children.Add(new OpenSticker(tempSticker));
				UnstuckME.Pages.ChatPage.AddMessage(temp);
				SwitchToChatTab();
				UnstuckME.Pages.ChatPage.ButtonAddUserDone_Click(null, null);

				foreach (Conversation convo in UnstuckME.Pages.ChatPage.StackPanelConversations.Children.OfType<Conversation>())
				{
					if (convo.Chat.ChatID == PreExistingChatID)
					    convo.ConversationUserControl_MouseLeftButtonDown(null, null);
				}
			});
		}

		public void UpdateChatMessage(UnstuckMEMessage message)
		{
            Dispatcher.Invoke(() => 
			{
				UnstuckME.Pages.ChatPage.EditMessage(message);
			});
		}

		public void DeleteChatMessage(UnstuckMEMessage message)
		{
            Dispatcher.Invoke(() =>
			{
				UnstuckME.Pages.ChatPage.RemoveMessage(message);
			});
		}

		#region ButtonLogic
		private void ButtonLogout_MouseEnter(object sender, MouseEventArgs e)
		{
			ButtonLogout.Background = Brushes.IndianRed;
		}

		private void ButtonLogout_MouseLeave(object sender, MouseEventArgs e)
		{
			ButtonLogout.Background = UnstuckME.Red;
		}

		private void ButtonLogout_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
		    try
		    {
		        UnstuckME.Server.Logout();
                UnstuckME.UserExit = true;
		        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
		        config.AppSettings.Settings["RememberMe"].Value = "false";
		        config.Save();

		        Close();
		        Application.Current.MainWindow = new LoginWindow();
		        Application.Current.MainWindow.Show();
		    }
		    catch (Exception ex)
		    {
		        var trace = new StackTrace(ex, true).GetFrame(0).GetMethod();
                UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_SERVER_CONNECTION_ERROR, ex.Message,
                    string.Format("Server failed to log out user {0}, Source = {1}", UnstuckME.User.EmailAddress, trace.Name));
		        Close();
		    }
		}

		private void ButtonAddContact_MouseEnter(object sender, MouseEventArgs e)
		{
			ButtonAddContact.Background = Brushes.LightSteelBlue;
		}

		private void ButtonAddContact_MouseLeave(object sender, MouseEventArgs e)
		{
			ButtonAddContact.Background = null;
		}

		private void ButtonAddContact_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			AddFriendWindow friendWindow = new AddFriendWindow();
			friendWindow.Show();
		}

		private void CreateStickerButton_MouseEnter(object sender, MouseEventArgs e)
		{
			CreateStickerButton.Background = Brushes.IndianRed;
		}

		private void CreateStickerButton_MouseLeave(object sender, MouseEventArgs e)
		{
			CreateStickerButton.Background = UnstuckME.Red;
		}

		private void CreateStickerButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			StickerCreationWindow window = new StickerCreationWindow();
			window.ShowDialog();
		}

		private void AdminButton_MouseEnter(object sender, MouseEventArgs e)
		{
		    AdminButton.Background = AdminButton.Background == UnstuckME.Blue ? Brushes.SteelBlue : Brushes.IndianRed;
		}

		private void AdminButton_MouseLeave(object sender, MouseEventArgs e)
		{
		    AdminButton.Background = AdminButton.Background == Brushes.SteelBlue ? UnstuckME.Blue : UnstuckME.Red;
		}

		private void AdminButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			SwitchToAdminTab(userPrivileges);
		}

		private void SettingButton_MouseEnter(object sender, MouseEventArgs e)
		{
		    SettingButton.Background = SettingButton.Background == UnstuckME.Blue ? Brushes.SteelBlue : Brushes.IndianRed;
		}

		private void SettingButton_MouseLeave(object sender, MouseEventArgs e)
		{
		    SettingButton.Background = SettingButton.Background == Brushes.SteelBlue ? UnstuckME.Blue : UnstuckME.Red;
		}

		private void SettingButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			SwitchToSettingsTab();
		}

		private void UserProfileButton_MouseEnter(object sender, MouseEventArgs e)
		{
		    UserProfileButton.Background = UserProfileButton.Background == UnstuckME.Blue ? Brushes.SteelBlue : Brushes.IndianRed;
		}

		private void UserProfileButton_MouseLeave(object sender, MouseEventArgs e)
		{
		    UserProfileButton.Background = UserProfileButton.Background == Brushes.SteelBlue ? UnstuckME.Blue : UnstuckME.Red;
		}

		private void UserProfileButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			SwitchToUserProfileTab();
		}

		private void ChatButtonBorder_MouseEnter(object sender, MouseEventArgs e)
		{
		    ChatButton.Background = ChatButton.Background == UnstuckME.Blue ? Brushes.SteelBlue : Brushes.IndianRed;
		}

		private void ChatButtonBorder_MouseLeave(object sender, MouseEventArgs e)
		{
		    ChatButton.Background = ChatButton.Background == Brushes.SteelBlue ? UnstuckME.Blue : UnstuckME.Red;
		}

		private void ChatButtonBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			SwitchToChatTab();
		}

		private void StickerButton_MouseEnter(object sender, MouseEventArgs e)
		{
		    StickerButton.Background = StickerButton.Background == UnstuckME.Blue ? Brushes.SteelBlue : Brushes.IndianRed;
		}

		private void StickerButton_MouseLeave(object sender, MouseEventArgs e)
		{
		    StickerButton.Background = StickerButton.Background == Brushes.SteelBlue ? UnstuckME.Blue : UnstuckME.Red;
		}

		private void StickerButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			SwitchToStickerTab();
		}
        #endregion

        public void ConnectionLost_AttemptToReconnect()
        {
            Hide();
            ReconnectingWindow reconnect = new ReconnectingWindow();
            reconnect.Show();
        }
    }

    public class UnstuckMEPages
	{
		public StickerPage StickerPage { get; set; }
		public SettingsPage SettingsPage { get; set; }
		public UserProfilePage UserProfilePage { get; set; }
		public ChatPage ChatPage { get; set; }
		public AdminPage AdminPage { get; set; }
		public ModeratorPage ModeratorPage { get; set; }
	}
}
