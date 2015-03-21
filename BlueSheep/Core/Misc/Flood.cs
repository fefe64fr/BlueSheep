﻿using BlueSheep.Common.IO;
using BlueSheep.Common.Protocol.Messages;
using BlueSheep.Engine.Constants;
using BlueSheep.Engine.Types;
using BlueSheep.Interface;
using BlueSheep.Interface.Text;
using BlueSheep.Interface.Text.Chat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;

namespace BlueSheep.Core.Misc
{
    public class Flood
    {
        #region Fields
        AccountUC account;
        public bool stop;
        List<string> listOfPlayers;
#endregion
        
        #region Constructors
        public Flood(AccountUC Account)
        {
            account = Account;
           
        }
        public Flood(AccountUC Account, List<string> list)
        {
            account = Account;
            listOfPlayers = list;
        }
        #endregion

        #region Public Methods
        public void StartFlooding(int channel, bool useSmiley, bool useNumbers, string content, int interval)
        {
            stop = false;
            string ncontent = content;
            while (stop == false)
            {
                if (useSmiley == true)
                    ncontent = AddRandomSmiley(content);
                if (useNumbers == true)
                    ncontent = AddRandomNumber(content);
                SendMessage(channel, ncontent);
                account.Wait(interval * 1000, interval * 1000);
            }
        }

        public void SendMessage(int channel, string content)
        {
            using (BigEndianWriter writer = new BigEndianWriter())
            {
                ChatClientMultiMessage msg = new ChatClientMultiMessage(content, (sbyte)channel);
                msg.Serialize(writer);
                writer.Content = account.HumanCheck.hash_function(writer.Content);
                MessagePackaging pack = new MessagePackaging(writer);
                pack.Pack((int)msg.ProtocolID);
                account.SocketManager.Send(pack.Writer.Content);
                if (account.DebugMode.Checked)
                    account.Log(new BotTextInformation("[SND] 861 (ChatClientMultiMessage)"), 0);
            }
        }

        public void SendPrivateTo(string name)
        {

            string content = account.FloodContent;
            if (account.IsRandomingSmileyBox.Checked == true)
                content = AddRandomSmiley(content);
            if (account.IsRandomingNumberBox.Checked == true)
                content = AddRandomNumber(content);
            using (BigEndianWriter writer = new BigEndianWriter())
            {
                ChatClientPrivateMessage msg = new ChatClientPrivateMessage(content, name);
                msg.Serialize(writer);
                writer.Content = account.HumanCheck.hash_function(writer.Content);
                MessagePackaging pack = new MessagePackaging(writer);
                pack.Pack((int)msg.ProtocolID);
                account.SocketManager.Send(pack.Writer.Content);
                account.Log(new PrivateTextInformation("à " + name + " : " + content), 1);
                if (account.DebugMode.Checked)
                    account.Log(new BotTextInformation("[SND] 851 (ChatClientPrivateMessage)"), 0);
            }
        }
 

        public void SaveNameInMemory(string name)
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BlueSheep", "Accounts", account.AccountName, "Flood");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            try
            {
                if (listOfPlayers[0] != "null")
                {
                    foreach(string elem in listOfPlayers)
                    {
                        if (elem == name)
                        {
                            account.Log(new BotTextInformation("(ADVANCED FLOOD)Player déjà entré"), 5);
                            return;
                        }
                    }
                }
                else
                {
                    listOfPlayers.Clear();
                }
                var swriter = new StreamWriter(path + "\\Players.txt", true);
                swriter.WriteLine(name);
                swriter.Close();
                listOfPlayers.Add(name);
                account.PlayerListLb.Items.Add(name);
                account.Log(new BotTextInformation("(ADVANCED FLOOD)Player ajouté."), 5);
            }
            catch (Exception ex)
            {
                account.Log(new ErrorTextInformation("(ADVANCED FLOOD)Impossible d'ajouté le player"), 5);
                account.Log(new ErrorTextInformation(ex.ToString()), 5);
            }
           
        }
        #endregion

        #region Private Methods
        private string AddRandomSmiley(string content)
        {
            int randomIndex = new Random().Next(0, 8);
            string nCon = content + " " + smileys[randomIndex];
            return nCon;
        }

        private string AddRandomNumber(string content)
        {
            int randomIndex = new Random().Next(0, 500);
            string nCon = content + " " + randomIndex.ToString();
            return nCon;
        }
        #endregion

        #region Enums
        public static readonly IList<String> smileys = new ReadOnlyCollection<string>
        (new List<String> {":)",";)","=)",":D",":p","=p",":d","=d","=P"});
        #endregion
    }
}
