using System;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace OpenDailyPlanner.Helper
{
    public static class SessionExtension
    {
        /// <summary>
        /// Получить данные по ключу из сессии виде строки
        /// </summary>
        /// <param name="session">Сессия</param>
        /// <param name="key">Ключь</param>
        /// <returns>Строка по ключу</returns>
        public static string GetString(this ISession session, string key)
        {
            var data = session.Get(key);
            if (data == null)
            {
                return null;
            }
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Задать данные по ключи из сесии из строки
        /// </summary>
        /// <param name="session"Сессия></param>
        /// <param name="key">Клюсь</param>
        /// <param name="value">Занчение</param>
        public static void SetString(this ISession session, string key, string value)
        {
            session.Set(key, Encoding.UTF8.GetBytes(value));
        }
    }
}
