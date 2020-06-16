using Microsoft.AspNetCore.Http;
using OpenDailyPlanner.Models;

namespace OpenDailyPlanner.ViewModels
{
    public class DalilyPlannerViewModel : BaseDalilyPlannerViewModel
    {
        /// <summary>
        /// Режим работы записей
        /// </summary>
        public bool IsEditMode { get; private set; } = false;

        public DalilyPlannerViewModel(HttpContext httpContext, string userName, bool? isEditMode = null, NoteDailyField? sortField = null, bool? isReverse = null) :
            base(httpContext, userName)
        {
            IsEditMode = isEditMode != null && (bool)isEditMode;
            DailyPlanner?.NotesSortBy(sortField, isReverse);
        }
    }
}
