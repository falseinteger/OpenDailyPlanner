using Microsoft.AspNetCore.Http;

namespace OpenDailyPlanner.Services
{
    public class Session
    {
        private static Session current;
        public static Session Current
        {
            get
            {
                if (current == null)
                    current = new Session();

                return current;
            }
        }

        #region Работа с именем пользователя

        /// <summary>
        /// Постоянный ключ для имени пользователя
        /// </summary>
        private const string UserNameKey = "username";

        /// <summary>
        /// Проверить наличия пользователя
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public bool IsExistUserName(ISession session)
        {
            var username = GetUserName(session);

            return !string.IsNullOrEmpty(username) && !string.IsNullOrWhiteSpace(username);
        }

        /// <summary>
        /// Удалить имя пользователя
        /// </summary>
        /// <param name="session"></param>
        public void RemoveUserName(ISession session)
        {
            session.Remove(UserNameKey);
        }

        /// <summary>
        /// Задать имя пользователя
        /// </summary>
        /// <param name="session"></param>
        /// <param name="username"></param>
        public void SetUserName(ISession session, string username)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username))
                return;
           session.SetString(UserNameKey, username);
        }

        /// <summary>
        /// Получить имя пользователя
        /// </summary>
        /// <param name="session"></param>
        /// <returns>логин</returns>
        public string GetUserName(ISession session)
        {
            return session.GetString(UserNameKey);
        }

        #endregion

    }
}
