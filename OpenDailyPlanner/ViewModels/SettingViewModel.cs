using System;
using Microsoft.AspNetCore.Http;

namespace OpenDailyPlanner.ViewModels
{
    public class SettingViewModel : BaseDalilyPlannerViewModel
    {
        public long NoteCount => DailyPlanner?.GetNoteCount() ?? 0;

        public SettingViewModel(HttpContext httpContext, string userName) : base(httpContext, userName)
        {
        }
    }
}
