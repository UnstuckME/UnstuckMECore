﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestDataCreator
{



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double CreateStars(int WholePortion)
        {
            double star = 0;
            switch (WholePortion)
            {
                case 1:
                    star = .5;
                    break;
                case 2:
                    star = 1;
                    break;
                case 3:
                    star = 1.5;
                    break;
                case 4:
                    star = 2;
                    break;
                case 5:
                    star = 2.5;
                    break;
                case 6:
                    star = 3;
                    break;
                case 7:
                    star = 3.5;
                    break;
                case 8:
                    star = 4;
                    break;
                case 9:
                    star = 4.5;
                    break;
                case 10:
                    star = 5;
                    break;
                default:
                    break;
            }
            return star;
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            button.Visibility = Visibility.Hidden;
            // Sticker related numbers
            int StartNum = Convert.ToInt32(TextIn_StickerStartNum.Text);       // the sticker start number
            int NumToMake = Convert.ToInt32(TextIn_NumToCreate.Text);      // the number of new stickers you want to make
            int CurrentNum = StartNum;     // the sticker number that is currently being created
            int EndNum = StartNum + NumToMake;         // the last sticker to make

            int MinClassNumber = Convert.ToInt32(TextIn_MinClassNum.Text);
            int MaxClassNumber = Convert.ToInt32(TextIn_MaxClassNum.Text);

            int MinUserNumber = Convert.ToInt32(TextIn_MinUserNum.Text);
            int MaxUserNumber = Convert.ToInt32(TextIn_MaxUserNum.Text);

            int MinUnstuckifier = Convert.ToInt32(TextIn_MinUnstucker.Text);
            int MaxUnstuckifier = Convert.ToInt32(TextIn_MaxUnstucker.Text);

            //review stuff
            int ReviewStartNumber = Convert.ToInt32(TextIn_ReviewStartNumber.Text);

            double MinStarUnstuckifier = Convert.ToDouble(comboBox_LowestStarUnstucker.Text);
            double MaxStarUserUnstuckifier = Convert.ToDouble(comboBox_HighestStarUnstucker.Text);

            double MinStarUser = Convert.ToDouble(comboBox_LowestStarUser.Text);
            double MaxStarUser = Convert.ToDouble(comboBox_HighestStarUser.Text);
            // random number makers
            Random RandClass = new Random();

            List<String> StickerOutputList = new List<String>();
            List<String> ReviewOutputList = new List<String>();
            List<String> ChatOutputList = new List<String>();
            List<String> UserToChatOutputList = new List<String>();
            List<String> MessageOutputList = new List<String>();

            // Replace Exsisting Data
            bool Overwrite = IsOverWrite.IsChecked.GetValueOrDefault();

            int MessageID = 1;
            while(CurrentNum <= EndNum)
            {
                int ID = CurrentNum;
                int ClassId = RandClass.Next(MinClassNumber, MaxClassNumber + 1);
                String ProblemDesc = ("Test problem for class " + ClassId + " on Sticker " + ID);
                int Reporter = RandClass.Next(MinUserNumber, MaxUserNumber + 1);
                int Unstucker = RandClass.Next(MinUnstuckifier, MaxUnstuckifier + 1);
                while (Unstucker == Reporter)
                {
                    Unstucker = RandClass.Next(MinUnstuckifier, MaxUnstuckifier + 1);
                }
                int ReviewIDStudent = ReviewStartNumber++;
                int IDStudent = ReviewIDStudent;
                String StudentReviewDesc = ("Test Review Number " + IDStudent + " for the help requesting student " + Reporter + " on sticker " + ID);
                double StudentStars = CreateStars(RandClass.Next(1, 11));

                int ReviewIDMentor = ReviewStartNumber++;
                int IDMentor = ReviewIDMentor;
                String MentorReviewDesc = ("Test Review Number " + IDMentor + " for the Mentor " + Unstucker + " on sticker " + ID);
                double MentorStars = CreateStars(RandClass.Next(1, 11));

                String StickerOutput = (ID + "," + ClassId + "," + ProblemDesc + "," + Reporter + "," + Unstucker);

                String StudentReviewOutput = (IDStudent + "," + ID + "," + Reporter + "," + StudentStars + "," + StudentReviewDesc  );
                String MentorReviewOutput = (IDMentor + "," + ID + "," + Unstucker + "," + MentorStars + "," + MentorReviewDesc );

                String UserToChatStudentOutput = (Reporter + "," + CurrentNum);
                String UserToChatMentorOutput = (Unstucker + "," + CurrentNum);


                
                    StickerOutputList.Add(StickerOutput);
                
                
                    ReviewOutputList.Add(StudentReviewOutput);
                    ReviewOutputList.Add(MentorReviewOutput);
                
                
                    ChatOutputList.Add(CurrentNum.ToString());
                
                
                    UserToChatOutputList.Add(UserToChatStudentOutput);
                    UserToChatOutputList.Add(UserToChatMentorOutput);
                

                int randMessageNumber = RandClass.Next(5, 30);
                for (int i = 0; i < randMessageNumber; i++)
                {
                    int user = Reporter;
                    if (i % 2 == 0)
                        user = Unstucker;
                    String MessageOutput = (MessageID + "," + CurrentNum + "," + "Message " + i + "," + user);
                    
                        MessageOutputList.Add(MessageOutput);
                        MessageID++;
                }




                CurrentNum++;
            }


            new Thread(() =>
            {
                using (System.IO.StreamWriter StickerFile =
                   new System.IO.StreamWriter(@"..\..\..\..\FakeDataStickers.csv", !Overwrite))
                {
                    foreach (String item in StickerOutputList)
                    {
                        StickerFile.WriteLine(item);
                    }
                }
            }).Start();

            new Thread(() =>
            {
                using (System.IO.StreamWriter ReviewFile =
            new System.IO.StreamWriter(@"..\..\..\..\FakeDataReviews.csv", !Overwrite))
            {
                foreach (String item in ReviewOutputList)
                {
                    ReviewFile.WriteLine(item);
                }
            }
            }).Start();

            new Thread(() =>
            {
                using (System.IO.StreamWriter ChatFile =
            new System.IO.StreamWriter(@"..\..\..\..\FakeDataChat.csv", !Overwrite))
            {
                foreach (String item in ChatOutputList)
                {
                    ChatFile.WriteLine(item);
                }
            }
            }).Start();
            new Thread(() =>
                {
                    using (System.IO.StreamWriter UserToChatFile =
            new System.IO.StreamWriter(@"..\..\..\..\FakeDataUserToChat.csv", !Overwrite))
            {
                foreach (String item in UserToChatOutputList)
                {
                    UserToChatFile.WriteLine(item);
                }
            }
                }).Start();
            new Thread(() =>
                    {
                        using (System.IO.StreamWriter MessageFile =
            new System.IO.StreamWriter(@"..\..\..\..\FakeDataMessages.csv", !Overwrite))
            {
                foreach (String item in MessageOutputList)
                {
                    MessageFile.WriteLine(item);
                }
            }
                    }).Start();




            button.Visibility = Visibility.Visible;
            TextIn_StickerStartNum.Text = (EndNum + 1).ToString();
            TextIn_ReviewStartNumber.Text = (ReviewStartNumber).ToString();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            //if (IsOverWrite.IsChecked == (false | null))
            //{
            //    IsOverWrite.IsChecked = true;
            //}
            //else
            //{
            //    IsOverWrite.IsChecked = false;
            //}
        }
    }
}
