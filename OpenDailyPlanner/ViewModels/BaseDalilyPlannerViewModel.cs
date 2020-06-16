using System;

using Microsoft.AspNetCore.Http;

using OpenDailyPlanner.Models;
using OpenDailyPlanner.Services;

namespace OpenDailyPlanner.ViewModels
{
    public class BaseDalilyPlannerViewModel : BasePageViewModel
    {
      
        /// <summary>
        /// Ежидненвник пользователя
        /// </summary>
        public DailyPlannerBook DailyPlanner { get; private set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; private set; } = "Noname";

        public BaseDalilyPlannerViewModel(HttpContext httpContext, string userName)
        {
            UserName = userName;
            DailyPlanner = DataValue.Current.GetDailyPlanner(httpContext, UserName);
        }
    }
}
