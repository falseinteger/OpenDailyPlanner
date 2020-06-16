using System;
namespace OpenDailyPlanner.Models
{
    public class NoteDaily
    {
        /// <summary>
        /// Дата записи
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// Заголовок
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Запись
        /// </summary>
        public string Body { get; private set; }

        /// <summary>
        /// Место
        /// </summary>
        public string Place { get; private set; }

        /// <summary>
        /// Статус
        /// </summary>
        public NoteDailyStatus Status { get; private set; }


        /// <summary>
        /// Список заголовков для записей
        /// </summary>
        public static NoteDailyDisplay[] Display
         => new NoteDailyDisplay[] {
                    new NoteDailyDisplay { Title = "Дата", Field = NoteDailyField.Date },
                    new NoteDailyDisplay { Title = "Заголовок", Field = NoteDailyField.Title },
                    new NoteDailyDisplay { Title = "Запись", Field = NoteDailyField.Body },
                    new NoteDailyDisplay { Title = "Место", Field = NoteDailyField.Place },
                    new NoteDailyDisplay { Title = "Статус", Field = NoteDailyField.Status }
                };

        /// <summary>
        /// Список зачений
        /// </summary>
        public string[] Values
        {
            get
            {
                return new string[] {
                    Date.ToString("dd.MM.yyyy"),
                    Title, Body, Place,
                    GetTitleDailyStatus(Status)
                };
            }
        }

        public NoteDaily(DateTime date, string title, string body, string place, NoteDailyStatus status = NoteDailyStatus.Normal)
        {
            Date = date;
            Title = title;
            Body = body;
            Place = place;
            Status = status;
        }

        public static NoteDaily Empty =>
            new NoteDaily(DateTime.Now, string.Empty, string.Empty, string.Empty, NoteDailyStatus.Normal);

        #region Status

        /// <summary>
        /// Получить список статусов для отображения
        /// </summary>
        /// <returns>наименования статусов</returns>
        private static string[] GetArrayDailyStatus()
        {
            var statusArray = Enum.GetValues(typeof(NoteDailyStatus));
            var statusDisplayStrings = new string[statusArray.Length];

            for (int i = 0; i < statusArray.Length; i++)
            {
                statusDisplayStrings[i] = GetTitleDailyStatus((NoteDailyStatus)statusArray.GetValue(i));
            }

            return statusDisplayStrings;
        }


        /// <summary>
        /// Получить наименования статуса для отображения
        /// </summary>
        /// <param name="status">статус</param>
        /// <returns>наименования статуса</returns>
        public static string GetTitleDailyStatus(NoteDailyStatus status)
        {
            switch (status)
            {
                case NoteDailyStatus.Normal: return "Нормальный";
                case NoteDailyStatus.Important: return "Важный";
                case NoteDailyStatus.Block: return "Приоритетный";
                default: return "Статус не найден";
            }
        }

        #endregion

    }

    public struct NoteDailyDisplay {

        public string Title { get; set; }
        public NoteDailyField Field { get; set; }

    }

    public enum NoteDailyStatus { Normal, Important, Block }
    public enum NoteDailyField { Date, Title, Body, Place,  Status }
}
