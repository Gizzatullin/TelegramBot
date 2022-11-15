using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using System.Collections.Generic;
using File = System.IO.File;
using NLog;

namespace BotTelegramLib
{
    public class Program
    {
        static Library library = new Library();

        static ITelegramBotClient botClient = new TelegramBotClient("5528529213:AAH7isAlBorMXr7vKzNg2ETsUt9zuNwV9Gs");

        static bool sortList = false;

        static string mesList;

        static int addBookFlag = 0;
        static string title;
        static string author;
        static string description;
        static string genre;
        static string filenamebook;

        static int correctBookFlag = 0;
        static int idCorrect;

        static bool delBookFlag = false;

        static bool downloadBookFlag = false;

        static bool uploadBookFlag = false;
        static int idUpload = 0;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        static ChatUserContain chatUser;
        static List<ChatUserContain> ChatUserList = new List<ChatUserContain>();
        static int indexChat = 0;
        static int indexChatUse;


        static void Main(string[] args)
        {
            logger.Info("Запуск работы программы.");
            InterfaceBot();            
            logger.Info("Конец работы программы.");

        }


        /// <summary>
        /// Обновление информации с Телеграм-Ботом.
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message)
            {
                logger.Info($"Поступило сообщение от пользователя {update.Message.From.Username} - {update.Message.Text}.");
                await HandleMessage(botClient, update.Message);
                return;
            }
        }


        /// <summary>
        /// Обмен сообщениями с Телеграм-ботом.
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task HandleMessage(ITelegramBotClient botClient, Message message)
        {
            string fileNameUser = message.From.Id + ".txt";
            string pdir = Convert.ToString(message.From.Id);
            string pathBook = Path.Combine(Environment.CurrentDirectory + $@"\BookFile\{pdir}\");
                        
            if (!Directory.Exists(pathBook))
            {
                Directory.CreateDirectory(pathBook);
            }

            ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "Вывести все книги 📚", "Добавить книгу ➕" },
                new KeyboardButton[] { "Корректировать данные о книге ✍🏻", "Удалить книгу 🗑" },
                new KeyboardButton[] { "Скачать книгу 💾", "Загрузить книгу 📲" }
                })
            { ResizeKeyboard = true };



            //////////////////////////////////////////////////////////////////
            
            if (message.Text == "/start")
            {
                chatUser = new ChatUserContain(indexChat, Convert.ToInt64(message.Chat.Id), false, 0, 0, false, false, false, null, null, null, null);
                ChatUserList.Add(chatUser);
                indexChat++;
            }

            ChatUserContain chatForUse = ChatUserList.FirstOrDefault(u => u.ChatId == message.Chat.Id);
                    
            if (chatForUse != null)
            {
                sortList = chatUser.SortL;
                addBookFlag = chatUser.AddBF;
                correctBookFlag = chatUser.CorrectBF;
                delBookFlag = chatUser.DelBF;
                downloadBookFlag = chatUser.DownloadBF;
                uploadBookFlag = chatUser.UploadBF;
                title = chatUser.ChatUserTitle;
                author = chatUser.ChatUserAuthor;
                description = chatUser.ChatUserDescription;
                genre = chatUser.ChatUserGenre;
            }

            

            ///////////////////////////////////////////////////////////////////


            if (addBookFlag != 0)
            {
                sortList = false;
                delBookFlag = false;
                correctBookFlag = 0;
                downloadBookFlag = false;
                uploadBookFlag = false;

                switch (addBookFlag)
                {
                    case 1:
                        {
                            title = message.Text;
                            addBookFlag++;
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите автора:");
                            break;
                        }
                    case 2:
                        {
                            author = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите описание:");
                            addBookFlag++;
                            break;
                        }
                    case 3:
                        {
                            description = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите жанр:");
                            addBookFlag++;
                            break;
                        }
                    case 4:
                        {
                            genre = message.Text;
                            filenamebook = "Нет файла";
                            int id = 0;
                            Book newBook = new Book(id, title, author, description, genre, filenamebook);
                            bool FlagCoorect = false;
                            library.SavetoFile(newBook, FlagCoorect, fileNameUser, pathBook, pdir);

                            await botClient.SendTextMessageAsync(message.Chat.Id, "Добавление книги прошло успешно.");
                            addBookFlag = 0;
                            break;
                        }
                }
            }


            if (correctBookFlag != 0)
            {
                sortList = false;
                delBookFlag = false;
                downloadBookFlag = false;
                uploadBookFlag = false;
                addBookFlag = 0;

                switch (correctBookFlag)
                {
                    case 1:
                        {
                            var allBooks = library.ReadfromFile(fileNameUser, pathBook);
                            string idStr = message.Text;
                            int id = GetIntFromString(idStr);
                            idCorrect = id;

                            if (id == 0)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Нет такого номера.");
                                correctBookFlag = 0;
                            }
                            else
                            {
                                if (id > allBooks.Count)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "В библиотеке нет книги с данным номером.");
                                    correctBookFlag = 0;
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, "Введите название:");
                                    correctBookFlag++;
                                }
                            }
                            break;
                        }
                    case 2:
                        {
                            title = message.Text;
                            correctBookFlag++;
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите автора:");
                            break;
                        }
                    case 3:
                        {
                            author = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите описание:");
                            correctBookFlag++;
                            break;
                        }
                    case 4:
                        {
                            description = message.Text;
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Введите жанр:");
                            correctBookFlag++;
                            break;
                        }
                    case 5:
                        {
                            genre = message.Text;
                            filenamebook = null;
                            bool result = library.CorrectBookInfo(idCorrect, title, author, description, genre, filenamebook, fileNameUser, pathBook, pdir);

                            if (result) await botClient.SendTextMessageAsync(message.Chat.Id, "Корректировка данных о книге прошло успешно.");
                            else await botClient.SendTextMessageAsync(message.Chat.Id, "Ошибка.");

                            correctBookFlag = 0;

                            break;
                        }
                }
            }


            if (delBookFlag == true)
            {
                sortList = false;
                correctBookFlag = 0;
                downloadBookFlag = false;
                uploadBookFlag = false;
                addBookFlag = 0;

                var allBooks = library.ReadfromFile(fileNameUser, pathBook);
                if (allBooks.Count == 0) await botClient.SendTextMessageAsync(message.Chat.Id, "В библиотеке пока нет книг.");
                else
                {
                    string idStr = message.Text;
                    int id = GetIntFromString(idStr);

                    if (id == 0) await botClient.SendTextMessageAsync(message.Chat.Id, "Нет такого номреа.");
                    else
                    {
                        bool result = library.DeletefromFile(id, fileNameUser, pathBook, pdir);
                        if (result)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Удаление книги прошло успешно.");
                            logger.Info($"Пользователь {message.From.Username} удалил книгу c номером {id}.");
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Ошибка.");
                            logger.Error($"Пользователь {message.From.Username} не смог удалить книгу.");
                        }
                    }
                }
                delBookFlag = false;
            }


            if (downloadBookFlag == true)
            {
                sortList = false;
                correctBookFlag = 0;
                delBookFlag = false;
                uploadBookFlag = false;
                addBookFlag = 0;

                var allBooks = library.ReadfromFile(fileNameUser, pathBook);
                if (allBooks.Count == 0) await botClient.SendTextMessageAsync(message.Chat.Id, "В библиотеке пока нет книг.");
                else
                {
                    string idStr = message.Text;
                    int id = GetIntFromString(idStr);

                    if (id == 0) await botClient.SendTextMessageAsync(message.Chat.Id, "Нет такого номера.");
                    else
                    {
                        if (id <= allBooks.Count)
                        {
                            List<Book> allCurrentBooks = library.ReadfromFile(fileNameUser, pathBook);
                            Book bookForDownload = allCurrentBooks.FirstOrDefault(u => u.Id == id);
                            string fileNameDownload = bookForDownload.FileNameBook;
                            string filePath = Path.Combine(Environment.CurrentDirectory + $@"\BookFile\{pdir}", fileNameDownload);
                            int sExeption = 0;

                            try
                            {
                                var stream = File.Open(filePath, FileMode.Open);
                                stream.Close();
                            }
                            catch (Exception)
                            {
                                sExeption = 1;
                                await botClient.SendTextMessageAsync(message.Chat.Id, "Приносим извенение, но файла данной книги пока нет в библиотеке.");
                                logger.Error($"Пользователь {message.From.Username} не смог скачать файл книги c номером {id}.");
                            }
                            if (sExeption == 0)
                            {
                                var stream = File.Open(filePath, FileMode.Open);
                                await botClient.SendDocumentAsync(message.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, fileNameDownload));
                                stream.Close();
                                logger.Info($"Пользователь {message.From.Username} cкачал успешно файл книги c номером {id}.");
                                sExeption = 0;
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Ошибка, нет такого номером.");
                        }
                    }
                }
                downloadBookFlag = false;
            }


            if (uploadBookFlag == true)
            {
                sortList = false;
                correctBookFlag = 0;
                delBookFlag = false;
                downloadBookFlag = false;
                addBookFlag = 0;

                var allBooks = library.ReadfromFile(fileNameUser, pathBook);
                if (allBooks.Count == 0) await botClient.SendTextMessageAsync(message.Chat.Id, "В библиотеке пока нет книг.");
                else
                {
                    string idStr = message.Text;
                    int id = GetIntFromString(idStr);

                    if (id == 0) await botClient.SendTextMessageAsync(message.Chat.Id, "Нет такого номера.");
                    else
                    {
                        if (id > allBooks.Count)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "В библиотеке нет книги с данным номером.");
                            idUpload = 0;
                        }
                        else
                        {
                            idUpload = id;
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Отправьте файл книги сообщением.");
                        }
                    }
                }
                uploadBookFlag = false;
            }


            if (message.Document != null)
            {
                if (idUpload != 0)
                {
                    int id = idUpload;
                    var document = message.Document;
                    var file = await botClient.GetFileAsync(document.FileId);
                    string fP = Path.Combine(Environment.CurrentDirectory + $@"\BookFile\{pdir}", document.FileName);
                    var fs = new FileStream(fP, FileMode.Create);

                    await botClient.DownloadFileAsync(file.FilePath, fs);


                    List<Book> allCurrentBooks = library.ReadfromFile(fileNameUser, pathBook);
                    Book bookForUpload = allCurrentBooks.FirstOrDefault(u => u.Id == id);
                    bookForUpload.FileNameBook = document.FileName;

                    bool result = library.CorrectBookInfo(bookForUpload.Id, bookForUpload.Title, bookForUpload.Author,
                                                          bookForUpload.Description, bookForUpload.Genre, bookForUpload.FileNameBook, fileNameUser, pathBook, pdir);
                    if (result)
                    {
                        logger.Info($"Пользователь {message.From.Username} загрузил файл {document.FileName} в книгу с номером {id}.");
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Загрузка файла прошла успешно.");

                    }
                    else
                    {
                        logger.Error($"Пользователь {message.From.Username} не смог загрузить файл в книгу с номером {id}.");
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ошибка.");
                    }

                    fs.Close();
                    idUpload = 0;
                }
                else
                {
                    logger.Error($"Пользователь {message.From.Username} вместо сообщения направил телеграм-боту документ.");
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Для загрузки файла в библиотеку выберите сначала команды <Загрузить файл книги>.");
                }
            }

            switch (message.Text)
            {
                case "/start":
                    {
                        logger.Info("Программа работает по ветке </start>.");
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Для работы используйте дополнительную клавиатуру.", replyMarkup: keyboard);
                        break;
                    }

                case "Вывести все книги 📚":
                    {
                        logger.Info("Программа работает по ветке - Вывести все книги.");
                        var allBooks = library.ReadfromFile(fileNameUser, pathBook);
                        if (allBooks.Count == 0)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "В библиотеке пока нет книг.");
                        }
                        else
                        {
                            foreach (var b in allBooks)
                            {
                                mesList = mesListForOutput(b);
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"<i>{mesList}</i>", ParseMode.Html);
                            }

                            await botClient.SendTextMessageAsync(message.Chat.Id, "При необходимости вывода отсортированного списка выберите:" +
                                                                                  "\n1 - по названию, 2 - по автору, 3 - по жанру");
                            sortList = true;
                        }
                        break;
                    }
                case "1":
                    {
                        logger.Info("Пользователь выбрал команду сортировки книг по названию.");
                        if (sortList != false)
                        {
                            var allBooks = library.ReadfromFile(fileNameUser, pathBook);
                            var sortedBookTitle = allBooks.OrderBy(b => b.Title);
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Сортировка книг по названию:");

                            foreach (var b in sortedBookTitle)
                            {
                                mesList = mesListForOutput(b);
                                await botClient.SendTextMessageAsync(message.Chat.Id, mesList); ;
                            }
                            sortList = false;
                        }
                        break;
                    }
                case "2":
                    {
                        logger.Info("Пользователь выбрал команду сортировки книг по автору.");
                        if (sortList != false)
                        {
                            var allBooks = library.ReadfromFile(fileNameUser, pathBook);
                            var sortedBookTitle = allBooks.OrderBy(b => b.Author);
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Сортировка книг по автору:");

                            foreach (var b in sortedBookTitle)
                            {
                                mesList = mesListForOutput(b);
                                await botClient.SendTextMessageAsync(message.Chat.Id, mesList); ;
                            }
                            sortList = false;
                        }
                        break;
                    }
                case "3":
                    {
                        logger.Info("Пользователь выбрал команду сортировки книг по жанру.");
                        if (sortList != false)
                        {
                            var allBooks = library.ReadfromFile(fileNameUser, pathBook);
                            var sortedBookTitle = allBooks.OrderBy(b => b.Genre);
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Сортировка книг по жанру:");

                            foreach (var b in sortedBookTitle)
                            {
                                mesList = mesListForOutput(b);
                                await botClient.SendTextMessageAsync(message.Chat.Id, mesList); ;
                            }
                            sortList = false;
                        }
                        break;
                    }
                case "Добавить книгу ➕":
                    {
                        logger.Info("Программа работает по ветке - Добавить новую книгу.");
                        addBookFlag = 1;
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Введите название:");
                        break;
                    }

                case "Корректировать данные о книге ✍🏻":
                    {
                        logger.Info("Программа работает по ветке - Корректировать данные о книге.");
                        correctBookFlag = 1;
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Введите номер книги:");
                        break;
                    }

                case "Удалить книгу 🗑":
                    {
                        logger.Info("Программа работает по ветке - Удалить книгу.");
                        delBookFlag = true;
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Введите номер книги для удаления:");
                        break;
                    }
                case "Скачать книгу 💾":
                    {
                        logger.Info("Программа работает по ветке - Скачать файл книги.");
                        downloadBookFlag = true;
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Введите номер книги для скачивания:");
                        break;
                    }
                case "Загрузить книгу 📲":
                    {
                        logger.Info("Программа работает по ветке - Загрузить файл книги.");
                        uploadBookFlag = true;
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Введите номер книги для загрузки файла в библиотеку:");
                        break;
                    }
            }


            //////////////////////////////////////////////////////////////////////

            chatForUse = ChatUserList.FirstOrDefault(v => v.ChatId == message.Chat.Id);
            if (chatForUse != null)
            {
                chatUser.SortL = sortList;
                chatUser.AddBF = addBookFlag;
                chatUser.CorrectBF = correctBookFlag;
                chatUser.DelBF = delBookFlag;
                chatUser.DownloadBF = downloadBookFlag;
                chatUser.UploadBF = uploadBookFlag;
                chatUser.ChatUserTitle = title;
                chatUser.ChatUserAuthor = author;
                chatUser.ChatUserDescription = description;
                chatUser.ChatUserGenre = genre;
                
                indexChatUse = chatUser.Index;
                ChatUserList.RemoveAt(indexChatUse);
                ChatUserList.Insert(indexChatUse,chatForUse);
            }
                                

            //////////////////////////////////////////////////////////////////////

            return;
        }


        /// <summary>
        /// Вывод книги в чат телеграм-бота.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string mesListForOutput(Book b)
        {
            mesList = $"{b.Id}. \"{b.Title}\", автор - {b.Author}\n" +
                      $"(описание:{b.Description}, жанр:{b.Genre})";
            logger.Info("Выполнен метод <mesListForOutput>");
            return mesList;
        }


        /// <summary>
        /// Обработка ошибок при работе Телеграм-Бота.
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="exception"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception;
            switch (exception)
            {
                case ApiRequestException apiRequestException:
                    {
                        Console.WriteLine($"Ошибка телеграм API:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}");
                        break;
                    }
                default: exception.ToString(); break;
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }


        /// <summary>
        /// Взаимодейстиве с пользователем (ввод-вывод данных) через БОТ-ТЕЛЕГРАМ.
        /// </summary>
        static void InterfaceBot()
        {
            var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            
            botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken: cts.Token);

            Console.ReadLine();

            cts.Cancel();
        }
            

        /// <summary>
        /// Проверка на корректный ввод данных (int).
        /// </summary>
        /// <param name="inputStr"></param>
        /// <returns></returns>
        public static int GetIntFromString(string inputStr)
        {
            int input = 0;

            try
            {
                input = int.Parse(inputStr);
            }
            catch (FormatException)
            {
                logger.Error("Выполнен метод <GetIntFromString> при котором выявлен не верный ввод данных.");
            }
            logger.Info("Выполнен метод <GetIntFromString> для преобразования строкового значения в число тип <int>.");
            return input;
        }
    }
}
