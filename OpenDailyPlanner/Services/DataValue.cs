using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

using OpenDailyPlanner.Models;

using Newtonsoft.Json;

namespace OpenDailyPlanner.Services
{
    public class DataValue
    {
        private static DataValue current;
        public static DataValue Current
        {
            get
            {
                if (current == null)
                    current = new DataValue();

                return current;
            }
        }

        /// <summary>
        /// Активный словарь ежидневников пользователей
        /// </summary>
        private readonly Dictionary<string, DailyPlannerBook> usersDailyPlanner;

        public DataValue()
        {
            usersDailyPlanner = new Dictionary<string, DailyPlannerBook>();
        }

        /// <summary>
        /// Получить ежидневник пользователя, если такого не будет будет создан новый
        /// </summary>
        /// <param name="httpContext">Текущий контекст</param>
        /// <returns>Eжидневник</returns>
        public DailyPlannerBook GetDailyPlanner(HttpContext httpContext) {
            var userName = Session.Current.GetUserName(httpContext.Session);
            return GetDailyPlanner(httpContext, userName);
        }

        /// <summary>
        /// Получить ежидневник пользователя, если такого не будет будет создан новый
        /// </summary>
        /// <param name="userName">имя пользователя</param>
        /// <returns>Eжидневник</returns>">
        public DailyPlannerBook GetDailyPlanner(HttpContext httpContext, string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return null;
            if (!usersDailyPlanner.ContainsKey(userName))
            {
                var dailyPlanner = GetDailyPlannerFromServer(httpContext, userName);

                if(dailyPlanner == null )
                    dailyPlanner = new DailyPlannerBook();

                usersDailyPlanner.Add(userName, dailyPlanner);
            }
            return usersDailyPlanner[userName]; ;
        }

        /// <summary>
        /// Создании демо записей в пользователю
        /// </summary>
        /// <param name="userName">имя пользователя</param>
        public void LoadDemoDataToCurrentDallyPlanner(string userName)
        {
            if (usersDailyPlanner == null || userName == null)
                return;
            if (!(usersDailyPlanner[userName] is DailyPlannerBook dailyPlanner))
                return;

            var newNotes = new NoteDaily[]
            {
                new NoteDaily(DateTime.Now, "Моя новая запись", "Очень интересная запись.", "Дом", NoteDailyStatus.Normal),
                new NoteDaily(DateTime.Now, "Моя новая тревожная запись", "Очень интересная запись и она очень важная.", "Дом", NoteDailyStatus.Important),
                new NoteDaily(DateTime.Now.AddDays(1), "Запись на завтра", "Что-то должен сделать завтра. Но не очень помню.", "Работа", NoteDailyStatus.Normal),
                new NoteDaily(DateTime.Now.AddDays(7), "Перезед", "В новый офис", "Переезд в новый офис.", NoteDailyStatus.Block)
            };

            dailyPlanner.AddNotes(newNotes);
        }

        /// <summary>
        /// Заменить текущий ежедневник на новый
        /// </summary>
        /// <param name="dailyPlanner">Новый ежедневнику</param>
        /// <param name="userName">имя пользователя</param>
        public void ReplaceDailyPlannerOnServer(DailyPlannerBook dailyPlanner, string userName) {
            try
            {
                if (dailyPlanner == null || userName == null)
                    return;
                if (!(usersDailyPlanner[userName] is DailyPlannerBook))
                    return;

                usersDailyPlanner[userName] = dailyPlanner;
                usersDailyPlanner[userName].RecalculateCount();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
        }

        /// <summary>
        /// Добавляем записи к тещему ежедневнику
        /// </summary>
        /// <param name="newDailyPlannerBook">Новый ежедневник</param>
        /// <param name="userName">имя пользователя</param>
        /// <param name="dateFrom">с даты</param>
        /// <param name="dateTo">по какую в ключительно</param>
        public void AddNoteFromNewToOldDailyPlannerOnServer(DailyPlannerBook newDailyPlannerBook, string userName, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                if (newDailyPlannerBook == null || userName == null)
                    return;
                if (!(usersDailyPlanner[userName] is DailyPlannerBook))
                    return;

                newDailyPlannerBook.RecalculateCount();
                var countnewDailyPlannerBook = newDailyPlannerBook.GetNoteCount();
                var newNotes = new NoteDaily[newDailyPlannerBook.GetNoteCount()];
                var indexNewNote = 0;

                for (uint i = 0; i < newDailyPlannerBook.GetNoteCount(); i++)
                {
                    var item = newDailyPlannerBook.GetNoteDally(i);

                    if ((dateFrom.HasValue && item.Date.Date < dateFrom.Value.Date) ||
                        (dateTo.HasValue && item.Date.Date > dateTo.Value.Date))
                    {
                        continue;
                    }

                    newNotes[indexNewNote] = item;
                    indexNewNote++;
                }
                usersDailyPlanner[userName].AddNotes(newNotes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
        }

        /// <summary>
        /// Сохраняем ежидневник на стороне сервера
        /// </summary>
        /// <param name="httpContext">Текущий контекст</param>
        /// <param name="dailyPlanner">Ежедневник</param>
        /// <param name="userName">Логин пользователя</param>
        /// <returns>Результат</returns>
        public bool SaveDailyPlannerOnServer(HttpContext httpContext, DailyPlannerBook dailyPlanner, string userName)
        {
            try
            {
                if (dailyPlanner == null || userName == null)
                    return false;
                if (!(usersDailyPlanner[userName] is DailyPlannerBook newdailyPlanner))
                    return false;
                var data = JsonConvert.SerializeObject(newdailyPlanner);
                if (string.IsNullOrEmpty(data))
                    return false;

                var pathFile = GetServerPath(httpContext, userName);

                using (var stream = new StreamWriter(pathFile))
                {
                    stream.Write(data);
                    stream.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        /// Получение данных о ежидневнике пользователя с сервера
        /// </summary>
        /// <param name="httpContext">Текущий контекст</param>
        /// <param name="userName">Логин пользователя</param>
        /// <returns>Ежедневник</returns>
        public DailyPlannerBook GetDailyPlannerFromServer(HttpContext httpContext, string userName)
        {
            try
            {
                if (userName == null)
                    return null;

                var pathFile = GetServerPath(httpContext, userName);
                if (!File.Exists(pathFile))
                    return null;

                using var stream = new StreamReader(pathFile);
                var data = stream.ReadToEnd();
                var dailyPlanner = JsonConvert.DeserializeObject<DailyPlannerBook>(data);

                dailyPlanner.RecalculateCount();

                return dailyPlanner;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// Вытасчиваем ежидневник из полученый от пользователя файла
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public (DailyPlannerBook dailyPlanner, string tempNameFile) GetDailyPlannerFromIFormFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length <= 0)
                    return (null, null);
                
                using var memoryStream = new MemoryStream();
                file.CopyTo(memoryStream);
                var data = System.Text.Encoding.Default.GetString(memoryStream.ToArray());
                var dailyPlanner = JsonConvert.DeserializeObject<DailyPlannerBook>(data);
                if (dailyPlanner == null)
                    return (null, null); ;

                var tempNameFile = Path.GetTempFileName();
                var tempPath = Path.GetTempPath();
                var pathFile = Path.Combine(tempPath, tempNameFile);

                using (var stream = new StreamWriter(pathFile))
                {
                    stream.Write(data);
                    stream.Close();
                }

                return (dailyPlanner, tempNameFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return (null, null);
            }
        }

        /// <summary>
        /// Получить ежедневний по верменому имени
        /// </summary>
        /// <param name="tempNameFile"></param>
        /// <returns></returns>
        public DailyPlannerBook GetDailyPlannerFromtempName(string tempNameFile)
        {
            try
            {
                if (string.IsNullOrEmpty(tempNameFile))
                    return null;

                var tempPath = Path.GetTempPath();
                var pathFile = Path.Combine(tempPath, tempNameFile);

                using var stream = new StreamReader(pathFile);
                var data = stream.ReadToEnd();
                stream.Close();
                var dailyPlanner = JsonConvert.DeserializeObject<DailyPlannerBook>(data);

                return dailyPlanner;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// Получить путь до файла ежидневника пользователя
        /// </summary>
        /// <param name="httpContext">Текущий контекст</param>
        /// <param name="userName">Логин пользователя</param>
        /// <returns>Путь</returns>
        public string GetPathServerDailyPlannerBookFile(HttpContext httpContext, string userName)
        {
            try
            {
                if (userName == null)
                    return null;
                return GetServerPath(httpContext, userName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// Получаем путь до файла ежидневника пользователя
        /// </summary>
        /// <param name="httpContext">Текущий контекст</param>
        /// <param name="userName">Логин пользователя</param>
        /// <returns>Путь</returns>
        private string GetServerPath(HttpContext httpContext, string userName)
        {
            if (!(httpContext.RequestServices.GetService(typeof(IWebHostEnvironment)) is IWebHostEnvironment environment))
                return null;

            var serverPath = environment.ContentRootPath;
            //var pathDirectory = Path.Combine(serverPath, "/UserDailyPlanner");
            // TODO: баг не делает нормальную комбинацию, результат путь только "/UserDailyPlanner"
            var pathDirectory = $"{serverPath}/UserDailyPlanner";

            if (!Directory.Exists(pathDirectory))
            {
                Directory.CreateDirectory(pathDirectory);
            }

            return Path.Combine(pathDirectory, $"{userName}.odp");
        }

    }
}
