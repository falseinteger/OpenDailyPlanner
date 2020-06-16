using System;
using System.ComponentModel.DataAnnotations;

namespace OpenDailyPlanner.ViewModels
{
    public class HomeViewModel
    {
        [Display(Name = "Ваше имя")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Логин требуется для вход в приложение ежидневник")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Только буквы и цифры")]
        public string UserName { get; set; }
    }
}
