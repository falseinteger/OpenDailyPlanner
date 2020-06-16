using System;
using System.ComponentModel.DataAnnotations;
using OpenDailyPlanner.Models;

namespace OpenDailyPlanner.ViewModels
{
    public class UploadFileViewModel : BasePageViewModel
    {
        
        /// <summary>
        /// Имя файла временного ежидненвник
        /// </summary>
        public string TempDailyPlannerName { get; set; }

        /// <summary>
        /// Дата записи с каково
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Требуется выбрать дату записи")]
        public string DateDisplayFrom
        {
            get
            {
                var date = DateFrom.HasValue ? (DateTime)DateFrom : DateTime.Now;
                return date.ToString("dd.MM.yyyy");
            }
            set
            {
                DateTime.TryParseExact(value, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var newDate);
                DateFrom = newDate;
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Требуется выбрать дату записи")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }


        /// <summary>
        /// Дата записи по какое включительно
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Требуется выбрать дату записи")]
        public string DateDisplayTo
        {
            get
            {
                var date = DateTo.HasValue ? (DateTime)DateTo : DateTime.Now;
                return date.ToString("dd.MM.yyyy");
            }
            set
            {
                DateTime.TryParseExact(value, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var newDate);
                DateTo = newDate;
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Требуется выбрать дату записи")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

    }
}
