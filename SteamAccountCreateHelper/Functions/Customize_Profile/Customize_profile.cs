﻿using SteamAccountCreateHelper.Utils;
using SteamAuth;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SteamAccountCreateHelper
{
    class Customize_profile
    {

        public static void Login_An_Customize(string path_to_save, string username, string pass)
        {
            try
            {
                UserLogin login = new UserLogin(username, pass);

                LoginResult response = LoginResult.BadCredentials;
                while ((response = login.DoLogin()) != LoginResult.LoginOkay)
                {
                    Log.info($"Login Response: {response} on Account: {username}");
                    switch (response)
                    {
                        
                        case LoginResult.NeedCaptcha:
                            System.Diagnostics.Process.Start(APIEndpoints.COMMUNITY_BASE + "/public/captcha.php?gid=" + login.CaptchaGID); //Open a web browser to the captcha image
                            Console.WriteLine("Please enter captcha text: ");
                            string captchaText = Console.ReadLine();
                            login.CaptchaText = captchaText;
                            break;

                        default:
                            break;
                    }
                }

                string Region = GetAccountRegion(login.Session);

                File.AppendAllText(path_to_save, $"\nSteamID: {login.Session.SteamID}\nProfileLink: https://steamcommunity.com/profiles/{login.Session.SteamID}\nCreation Date:{DateTime.Now.ToShortDateString()}\nRegion:{Region}");
                Log.info($"Save SteamID64: {login.Session.SteamID} on Account: {username}");


                if (Main._Form1.ck_Set_NickNameRandom.Checked == true)
                {
                    Enable_Profile_Default(login.Session, username);
                }

                if (Main._Form1.ck_use_custom_avatar.Checked == true)
                {
                    Set_Custom_Avatar(login.Session);
                }

                if (Main._Form1.ck_GameAndInventory_Public.Checked == true)
                {
                    Set_GameAndInventory_Public(login.Session);
                }
            }
            catch(Exception ex)
            {
                Log.error("Erro To Customize Account " + username);
                Log.error(ex.Message);
            }
        }

        public static void Enable_Profile_Default(SteamAuth.SessionData SessionData, string username)
        {
            RandomUtils.RandomLoginUsingNamefake();

            var Enable_Profile = new RequestBuilder("https://steamcommunity.com/profiles/" + SessionData.SteamID + "/edit?welcomed=1")
                    .GET()
                    .AddCookies(SessionData)
                    .Execute();

            string page_edit = "https://steamcommunity.com/profiles/" + SessionData.SteamID + "/edit";

            var edit = new RequestBuilder("https://steamcommunity.com/profiles/" + SessionData.SteamID + "/edit").POST()
                 .AddHeader(HttpRequestHeader.Referer, page_edit)
                .AddPOSTParam("sessionID", SessionData.SessionID)
                .AddPOSTParam("type", "profileSave")
                .AddPOSTParam("personaName", RandomUtils.LatesNameFakeRequest.maiden_name)
                .AddPOSTParam("real_name", RandomUtils.LatesNameFakeRequest.name)
                .AddPOSTParam("country", Main.countries[RandomUtils.GetRandomInt(0, Main.countries.Count)])
                .AddPOSTParam("customURL", username)
                .AddPOSTParam_int("favorite_badge_badgeid", 1)
                .AddCookies(SessionData)
                .Execute();
        }

        public static void Set_Custom_Avatar(SteamAuth.SessionData SessionData)
        {
            
            bool restart = false;
            do
            {
                restart = false;
                
                try                                                              
                {
                    string Random_IMG_URL = Main.Avatar_URL_List[RandomUtils.GetRandomInt(0, Main.Avatar_URL_List.Count)];

                    Image img = DownloadImage(Random_IMG_URL);

                    Image DownloadImage(string fromUrl)
                    {
                        using (System.Net.WebClient webClient = new System.Net.WebClient())
                        {
                            using (Stream stream = webClient.OpenRead(fromUrl))
                            {
                                return Image.FromStream(stream);
                            }
                        }
                    }

                    Image imagem_pronta = ResizeImage_Profile.ResizeImage(img, 184, 184);

                    byte[] imagem_byte = converterDemo(imagem_pronta);

                    string ContentType = "image/jpg";

                    var baseURL = "https://steamcommunity.com/actions/FileUploader?type=player_avatar_image&sId=" + SessionData.SteamID + "&bgColor=262627&fgColor=ffffff";

                    var response = new RequestBuilder(baseURL).POST()
                        .AddHeader(HttpRequestHeader.Referer, "https://steamcommunity.com/profiles/" + SessionData.SteamID + "/edit")
                        .AddPOSTParam_int("MAX_FILE_SIZE", 1048576)
                        .AddPOSTParam("type", "player_avatar_image")
                        .AddPOSTParam("sId", SessionData.SteamID.ToString())
                        .AddPOSTParam("sessionid", SessionData.SessionID)
                        .AddPOSTParam_int("doSub", 1)
                        .AddFile_Bytes("avatar", imagem_byte, ContentType)
                        .AddCookies(SessionData)
                        .Execute();

                }
                catch (IndexOutOfRangeException)
                {

                     restart = true;
                }

            } while (restart);

        }

        public static void Set_GameAndInventory_Public(SteamAuth.SessionData SessionData)
        {
            
            string page_edit = "https://steamcommunity.com/profiles/" + SessionData.SteamID + "/edit";

            var edit = new RequestBuilder("https://steamcommunity.com/profiles/" + SessionData.SteamID + "/ajaxsetprivacy").POST()
                 .AddHeader(HttpRequestHeader.Referer, page_edit)
                .AddPOSTParam("sessionid", SessionData.SessionID)
                .AddPOSTParam("Privacy", "{\"PrivacyProfile\":3,\"PrivacyInventory\":3,\"PrivacyInventoryGifts\":3,\"PrivacyOwnedGames\":3,\"PrivacyPlaytime\":3,\"PrivacyFriendsList\":2}")
                .AddPOSTParam("eCommentPermission", "0")
                .AddCookies(SessionData)
                .Execute();
        }

        public static string GetAccountRegion(SteamAuth.SessionData SessionData)
        {
            var Request = new RequestBuilder("https://store.steampowered.com/account/").GET()
                 .AddHeader(HttpRequestHeader.Referer, "https://store.steampowered.com/")
                .AddPOSTParam("sessionid", SessionData.SessionID)
                .AddCookies(SessionData)
                .Execute();

            var RegionRegex = new Regex("(?<=account_data_field\">).*(?=<\\/span>)");
            var RegionFinal = RegionRegex.Match(Request.Content);

            return Convert.ToString(RegionFinal.Value);
        }

        public static byte[] converterDemo(Image x)
        {
            ImageConverter _imageConverter = new ImageConverter();
            byte[] xByte = (byte[])_imageConverter.ConvertTo(x, typeof(byte[]));
            return xByte;
        }
    }
}
