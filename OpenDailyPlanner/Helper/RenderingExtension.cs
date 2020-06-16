using Microsoft.AspNetCore.Mvc.Rendering;

namespace OpenDailyPlanner.Helper
{
    public static class RenderingExtension
    {
        /// <summary>
        /// Определяем находимся ли мы в текущем ли контроллере и выполняем нужное нам действие
        /// </summary>
        /// <param name="viewContext">Контекст</param>
        /// <param name="controller">Контроллер</param>
        /// <param name="action">Действие</param>
        /// <returns>Результат</returns>
        public static bool CheckCurrentControllerAndActon(this ViewContext viewContext, string controller, string action)
        {
            var currentController = viewContext.RouteData.Values["controller"]?.ToString();
            var currentAction = viewContext.RouteData.Values["action"]?.ToString();
            return currentController == controller && currentAction == action;
        }
    }
}
