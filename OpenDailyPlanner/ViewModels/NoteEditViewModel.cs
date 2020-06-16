using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OpenDailyPlanner.Models;
using OpenDailyPlanner.Services;

namespace OpenDailyPlanner.ViewModels
{
    public class NoteEditViewModel : BasePageViewModel
    {
        /// <summary>
        /// Индекс записи
        /// </summary>
        public uint? Id { get; set; }

        private NoteDaily NoteDaily { get; set; }
        private DailyPlannerBook DailyPlanner { get; set; }

        /// <summary>
        /// Дата записи
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Требуется выбрать дату записи")]
        public string DateDisplay {
            get
            {
                var date = Date.HasValue? (DateTime)Date : DateTime.Now;
                return date.ToString("dd.MM.yyyy");
            }
            set
            {
                DateTime.TryParseExact(value, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var newDate);
                Date = newDate;
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Требуется выбрать дату записи")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd.MM.yyyy}")]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Заголовок
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Требуется наименование записи")]
        public string Title { get; set; }

        /// <summary>
        /// Запись
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Требуется текст записи")]
        public string Body { get; set; }

        /// <summary>
        /// Место
        /// </summary>
        public string Place { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Требуется выбрать статус записи")]
        public NoteDailyStatus Status { get; set; }

        private SelectListItem _StatusSelect { get; set; }
        public SelectListItem StatusSelect {
            get => _StatusSelect;
            set
            {
                var statusArray = Enum.GetValues(typeof(NoteDailyStatus));
                for (int i = 0; i < statusArray.Length; i++)
                {
                    var item = (NoteDailyStatus)statusArray.GetValue(i);
                    if (value.Value == item.ToString()) {
                        Status = item;
                        return;
                    }
                }
            }
        }

        private SelectListItem[] _StatusList;
        public SelectListItem[] StatusList
        {
            get
            {
                if (_StatusList == null)
                {
                    var statusArray = Enum.GetValues(typeof(NoteDailyStatus));
                    _StatusList = new SelectListItem[statusArray.Length];
                    for (int i = 0; i < statusArray.Length; i++)
                    {
                        var item = (NoteDailyStatus)statusArray.GetValue(i);
                        StatusList[i] = new SelectListItem(NoteDaily.GetTitleDailyStatus(item), item.ToString(), item == Status);
                    }
                }
                return _StatusList;
            }
        }

        public NoteEditViewModel()
        {
            Id = null;
            Title = null;
            Body = null;
            Date = DateTime.Now;
            Place = null;
            Status = NoteDailyStatus.Normal;
        }

        public NoteEditViewModel(HttpContext httpContext, uint? id)
        {
            Id = id;
            DailyPlanner = DataValue.Current.GetDailyPlanner(httpContext);
            if (DailyPlanner == null)
                return;
            NoteDaily =
                id != null ? DailyPlanner.GetNoteDally((uint)Id) : NoteDaily.Empty ?? NoteDaily.Empty;
            Title = NoteDaily.Title;
            Body = NoteDaily.Body;
            Date = NoteDaily.Date;
            Place = NoteDaily.Place;
            Status = NoteDaily.Status;
        }
    }
}
