using Telegram.Bot.Types;


namespace BotTelegramLib
{   
    /// <summary>
    /// Список чата пользователя с его данными.
    /// </summary>
    public class ChatUserContain
    {
        public int Index { get; set; }
        public long ChatId { get; set; }
        public bool SortL { get; set; }
        public int AddBF { get; set; }
        public int CorrectBF { get; set; }
        public bool DelBF { get; set; }
        public bool DownloadBF { get; set; }
        public bool UploadBF { get; set; }
        public string ChatUserTitle { get; set; }
        public string ChatUserAuthor { get; set; }
        public string ChatUserDescription { get; set; }
        public string ChatUserGenre { get; set; }



        public ChatUserContain(int index, long chatId, bool sortL, int addBF, int correctBF, bool delBF, bool downloadBF, bool uploadBF,
                               string chatutitle, string chatuauthor, string chatudescription, string chatugenre)
        {
            Index = index;
            ChatId = chatId;
            SortL = sortL;
            AddBF = addBF;
            CorrectBF = correctBF;
            DelBF = delBF;
            DownloadBF = downloadBF;
            UploadBF = uploadBF;
            ChatUserTitle = chatutitle;
            ChatUserAuthor = chatuauthor;
            ChatUserDescription = chatudescription;
            ChatUserGenre = chatugenre;
        }
    }
}
