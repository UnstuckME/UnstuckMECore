﻿using System;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using UnstuckMEInterfaces;
using UnstuckME_Classes;
using UnstuckMeLoggers;

namespace UnstuckMEUserGUI
{
	[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	class ClientCallback : IClient
	{
		/// <summary>
		/// Adds a class to the user's GUI.
		/// </summary>
		/// <param name="inClass">The class to add to the user's GUI.</param>
		public void AddClasses(UserClass inClass)
		{
			try
			{
				Application.Current.Windows.OfType<UnstuckMEWindow>().SingleOrDefault().RecieveAddedClass(inClass);
			}
			catch(InvalidOperationException ex)
			{
			    UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_INTERACTION_ERROR, ex.Message, ex.Source);
			}
        }

		/// <summary>
		/// Forces the client to close with a messagebox popup.
		/// </summary>
		public void ForceClose()
		{
			UnstuckMEMessageBox messageBox = new UnstuckMEMessageBox(UnstuckMEBox.Shutdown, "UnstuckME Server has shutdown. Please Contact Your Server Administrator", "Server Shutdown", UnstuckMEBoxImage.Error);
			messageBox.ShowDialog();
		}

		/// <summary>
		/// Gets a message from the server to show to the client.
		/// </summary>
		/// <param name="message">The message to send to the client.</param>
		public void GetMessageFromServer(string message)
		{
			UnstuckMEMessageBox messagebox = new UnstuckMEMessageBox(UnstuckMEBox.OK, message, "Message from server", UnstuckMEBoxImage.Information);
			messagebox.ShowDialog();
		}

		/// <summary>
		/// Updates a user's conversation if they are online and another user sends them a message.
		/// </summary>
		/// <param name="message">The message being sent to the user.</param>
		public void GetMessage(UnstuckMEMessage message)
		{
		    try
		    {
		        Application.Current.Windows.OfType<UnstuckMEWindow>().SingleOrDefault().RecieveChatMessage(message);
		    }
		    catch (Exception ex)
		    {
		        UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_INTERACTION_ERROR, ex.Message, ex.Source);
            }
        }

		/// <summary>
		/// Removes a sticker from any online, qualified user's GUI.
		/// </summary>
		/// <param name="stickerID">The sticker to be removed.</param>
		public void RemoveGUISticker(int stickerID)
		{
			try
			{
				Application.Current.Windows.OfType<UnstuckMEWindow>().SingleOrDefault().RemoveStickerFromAvailableStickers(stickerID);
			}
			catch (InvalidOperationException ex)
			{
			    UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_INTERACTION_ERROR, ex.Message, ex.Source);
			}
        }

		/// <summary>
		/// Updates a user's list of stickers if they are online and meet the qualifications to see the newly submitted sticker.
		/// </summary>
		/// <param name="inSticker">The sticker being sent to the users.</param>
		public void RecieveNewSticker(UnstuckMEAvailableSticker inSticker)
		{
			try
			{
				Application.Current.Windows.OfType<UnstuckMEWindow>().SingleOrDefault().RecieveNewAvailableSticker(inSticker);
			}
			catch (InvalidOperationException ex)
			{
			    UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_INTERACTION_ERROR, ex.Message, ex.Source);
			}
        }

        /// <summary>
        /// Updates a chat message if a user in the conversation has edited it.
        /// </summary>
        /// <param name="message">The message that has been edited.</param>
        public void UpdateChatMessage(UnstuckMEMessage message)
        {
            try
            {
                Application.Current.Windows.OfType<UnstuckMEWindow>().SingleOrDefault().UpdateChatMessage(message);
            }
            catch (InvalidOperationException ex)
            {
                UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_INTERACTION_ERROR, ex.Message, ex.Source);
            }
        }

        /// <summary>
        /// Removes a chat message if a user in the conversation has deleted it.
        /// </summary>
        /// <param name="message">The message that has been removed.</param>
        public void DeleteChatMessage(UnstuckMEMessage message)
        {
            try
            {
                Application.Current.Windows.OfType<UnstuckMEWindow>().SingleOrDefault().DeleteChatMessage(message);
            }
            catch (InvalidOperationException ex)
            {
                UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_INTERACTION_ERROR, ex.Message, ex.Source);
            }
        }

        /// <summary>
        /// Opens a CreateTutorReview window for the submission of a review on
        /// the sticker identified by <paramref name="stickerID"/>.
        /// </summary>
        /// <param name="stickerID">The unique identifier of the sticker to submit a review on.</param>
        public void CreateReviewAsTutor(int stickerID)
        {
            try
            {
                UnstuckME.Pages.UserProfilePage.UpdateRatings();
                Window window = new SubWindows.AddTutorReviewWindow(stickerID);
                window.Show();
                window.Focus();
            }
            catch (Exception ex)
            {
                UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_INTERACTION_ERROR, ex.Message, ex.Source);
                UnstuckMEMessageBox messagebox = new UnstuckMEMessageBox(UnstuckMEBox.OK, "You need to logout and log back in to submit a review on " + stickerID +
                                                                         ". If this does not work, please contact an UnstuckME administrator to resolve this issue.",
                                                                         "Could not create a new review", UnstuckMEBoxImage.Error);
                messagebox.ShowDialog();
            }
        }

        /// <summary>
        /// Opens a CreateStudentReview window for the submission of a review on
        /// the sticker identified by <paramref name="stickerID"/>.
        /// </summary>
        /// <param name="stickerID">The unique identifier of the sticker to submit a review on.</param>
        public void CreateReviewAsStudent(int stickerID)
        {
            try
            {
                UnstuckME.Pages.UserProfilePage.UpdateRatings();
                Window window = new SubWindows.AddStudentReviewWindow(stickerID);
                window.Show();
                window.Focus();
            }
            catch (Exception ex)
            {
                UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_INTERACTION_ERROR, ex.Message, ex.Source);
                UnstuckMEMessageBox messagebox = new UnstuckMEMessageBox(UnstuckMEBox.OK, "You need to logout and log back in to submit a review on " + stickerID +
                                                                                          ". If this does not work, please contact an UnstuckME administrator to resolve this issue.",
                                                                         "Could not create a new review", UnstuckMEBoxImage.Error);
                messagebox.ShowDialog();
            }
        }

        /// <summary>
        /// When a tutor drops a sticker rather than submitting a review, this will find the sticker of the
        /// student who submitted it and reactivates the completed and delete buttons.
        /// </summary>
        /// <param name="stickerID">The unique identifier of the sticker to make active.</param>
        public void UpdateStickerStatus(int stickerID)
        {
            try
            {
                UnstuckME.Pages.StickerPage.MakeStickerActive(stickerID);
            }
            catch (Exception ex)
            {
                UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_INTERACTION_ERROR, ex.Message, "Could not update status of sticker " + stickerID + ", Source = " + ex.Source);
            }
        }

	    /// <summary>
	    /// When a review is submitted of a user, adds it to the list of reviews that have been
	    /// submitted on that user.
	    /// </summary>
	    /// <param name="review">The review to add to the user's profile page.</param>
	    public void RecieveReview(UnstuckMEReview review)
	    {
	        try
	        {
	            UnstuckME.Pages.UserProfilePage.AddReview(review);
	        }
	        catch (Exception ex)
	        {
	            UnstuckMEUserEndMasterErrLogger.GetInstance().WriteError(ERR_TYPES.USER_GUI_INTERACTION_ERROR, ex.Message, "Could not add review " + review.ReviewID + " to interface, Source = " + ex.Source);
	        }
	    }
    }
}
