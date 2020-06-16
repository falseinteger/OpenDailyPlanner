using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OpenDailyPlanner.Models;
using OpenDailyPlanner.Services;
using OpenDailyPlanner.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenDailyPlanner.Controllers
{
    public class DalilyPlannerController : Controller
    {
        private DailyPlannerBook DailyPlanner => DataValue.Current.GetDailyPlanner(HttpContext);
        private InfoLabelViewModel CheckDailyPlanner {
            get
            {
                if(DailyPlanner != null)
                    new InfoLabelViewModel { Type = InfoLabelType.Warning, Message = $"Не удалось найти ежидневник!" };
                return null;
            }
        }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        private string _CurrentUserName;
        private string CurrentUserName
        {
            get
            {
                if (string.IsNullOrEmpty(_CurrentUserName))
                {
                    _CurrentUserName = Services.Session.Current.GetUserName(HttpContext.Session);
                }
                return _CurrentUserName;
            }
        }

        #region Index

        /// <summary>
        /// Главная страница ежидневника
        /// </summary>
        /// <param name="isEditMode">Режим просмотра</param>
        /// <returns></returns>
        public IActionResult Index(bool? isEditMode = null, NoteDailyField? sortField = null, bool? isReverse = null)
        {
            if (!Services.Session.Current.IsExistUserName(HttpContext.Session))
                return RedirectToAction("Index", "Home");

            var newModel = new DalilyPlannerViewModel(HttpContext, CurrentUserName, isEditMode, sortField, isReverse);
            return View(newModel);
        }

        /// <summary>
        /// Удаляем запись из ежидневника
        /// </summary>
        /// <param name="id">индекс записи</param>
        /// <returns>Результат</returns>
        public IActionResult Remove(uint id)
        {
            var infoLabelViewModel = CheckDailyPlanner;
            if (infoLabelViewModel == null) {

                if (!DailyPlanner.RemoveNote(id))
                {
                    infoLabelViewModel = new InfoLabelViewModel {
                        Type = InfoLabelType.Error,
                        Message = $"Не удалось удалить запись под номером \"{id}\""
                    };
                } else
                {
                    //Сохраняем изменения
                    DataValue.Current.SaveDailyPlannerOnServer(HttpContext, DailyPlanner, CurrentUserName);
                }

            }
            if (infoLabelViewModel != null)
            {
                var newModel = new DalilyPlannerViewModel(HttpContext, CurrentUserName, true)
                {
                    InfoLabel = infoLabelViewModel
                };
                return View(newModel);
            }
            return RedirectToAction("Index", "DalilyPlanner", new { isEditMode = true });
        }

        /// <summary>
        /// Открываем запись для редактирования в ежидневнике
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Edit(uint? id)
        {
            return View(new NoteEditViewModel(HttpContext, id));
        }


        /// <summary>
        /// Создать демо база данных
        /// </summary>
        /// <returns></returns>
        public IActionResult CreateDemo()
        {
            DataValue.Current.LoadDemoDataToCurrentDallyPlanner(CurrentUserName);

            // Сохраняем демо записи
            DataValue.Current.SaveDailyPlannerOnServer(HttpContext, DailyPlanner, CurrentUserName);
            return RedirectToAction("Index", "DalilyPlanner");
        }

        #endregion

        #region Edit

        /// <summary>
        /// Сохранения записи в ежидневник
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(NoteEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var infoLabelViewModel = CheckDailyPlanner;
            if (infoLabelViewModel == null) {

                var note = new NoteDaily(model.Date ?? System.DateTime.Now, model.Title, model.Body, model.Place, model.Status);

                if (model.Id == null)
                {
                    DailyPlanner.AddNote(note);   
                }
                else if (!DailyPlanner.UpdateNote((uint)model.Id, note))
                {
                    infoLabelViewModel = new InfoLabelViewModel
                    {
                        Type = InfoLabelType.Error,
                        Message = $"Не удалось обновить запись под номером \"{model.Id}\""
                    };
                }

                //Сохраняем новую запись или изменение
                if (infoLabelViewModel == null)
                    DataValue.Current.SaveDailyPlannerOnServer(HttpContext, DailyPlanner, CurrentUserName);
            }

            if (infoLabelViewModel != null)
            {
                model.InfoLabel = infoLabelViewModel;
                return View(model);
            }
            return RedirectToAction("Index", "DalilyPlanner", new { isEditMode = false });
        }

        #endregion

        #region Setting

        /// <summary>
        /// Настройки ежидневника
        /// </summary>
        /// <returns></returns>
        public IActionResult Setting()
        {
            var newModel = new SettingViewModel(HttpContext, CurrentUserName);
            return View(newModel);
        }

        /// <summary>
        /// Отрпавить файл словаря пользователю
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult DownloadFile()
        {
            var PathFile = DataValue.Current.GetPathServerDailyPlannerBookFile(HttpContext, CurrentUserName);
            if(string.IsNullOrEmpty(PathFile) || !System.IO.File.Exists(PathFile))
            {
                var model = new SettingViewModel(HttpContext, CurrentUserName);
                model.InfoLabel = new InfoLabelViewModel
                {
                    Type = InfoLabelType.Error,
                    Message = $"Не удалось найти ежедневник пользователя: \"{model.UserName}\"!"
                };
                 
                return View("Setting", model);
            }
            var fileName = $"{CurrentUserName}.odp";
            var fileStream = new System.IO.FileStream(PathFile, System.IO.FileMode.Open);
            return File(fileStream, System.Net.Mime.MediaTypeNames.Application.Json, fileName);
        }

        /// <summary>
        /// Загрузка файла
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult UploadFile(IFormFile[] files)
        {
            try
            {
                if (files == null || files.Length < 1)
                {
                    return ShowErrorUploadFile();
                }

                var file = files[0];
                var (dailyPlanner, tempNameFile) = DataValue.Current.GetDailyPlannerFromIFormFile(file);
                if (dailyPlanner == null)
                {
                    var InfoLabel = new InfoLabelViewModel
                    {
                        Type = InfoLabelType.Error,
                        Message = $"Архив пустой или содержит ошибки! Требуется ручная проверка файла."
                    };
                    ShowErrorUploadFile(InfoLabel);
                }

                var newModel = new UploadFileViewModel { TempDailyPlannerName = tempNameFile };
                var (dateMin, dateMax) = dailyPlanner.GetDateMinAndMaxOfNotes();

                newModel.DateFrom = dateMin;
                newModel.DateTo = dateMax;

                return View(newModel);
            }
            catch
            {
                return ShowErrorUploadFile();
            }
        }

        private IActionResult ShowErrorUploadFile(InfoLabelViewModel InfoLabel = null)
        {
            if (InfoLabel == null)
            {
                InfoLabel = new InfoLabelViewModel
                {
                    Type = InfoLabelType.Error,
                    Message = $"Не удалось загрузить файл. Попробуйте повторно."
                };
            }
            var newModel = new SettingViewModel(HttpContext, CurrentUserName)
            {
                InfoLabel = InfoLabel
            };
            return View("Setting", newModel);
        }

        [HttpPost]
        public IActionResult SetNewDalily(UploadFileViewModel model)
        {
            var fileName = model.TempDailyPlannerName;
            var newDailyPlanner = DataValue.Current.GetDailyPlannerFromtempName(fileName);
            if(newDailyPlanner == null)
            {
                return ShowErrorUploadFile();
            }

            DataValue.Current.ReplaceDailyPlannerOnServer(newDailyPlanner, CurrentUserName);

            // Сохраняем записи
            DataValue.Current.SaveDailyPlannerOnServer(HttpContext, DailyPlanner, CurrentUserName);
            return RedirectToAction("Index", "DalilyPlanner");
        }

        [HttpPost]
        public IActionResult AddNewDalily(UploadFileViewModel model)
        {
            var fileName = model.TempDailyPlannerName;
            var newDailyPlanner = DataValue.Current.GetDailyPlannerFromtempName(fileName);
            if (newDailyPlanner == null)
            {
                return ShowErrorUploadFile();
            }

            DataValue.Current.AddNoteFromNewToOldDailyPlannerOnServer(newDailyPlanner, CurrentUserName);

            // Сохраняем записи
            DataValue.Current.SaveDailyPlannerOnServer(HttpContext, DailyPlanner, CurrentUserName);
            return RedirectToAction("Index", "DalilyPlanner");
        }

        [HttpPost]
        public IActionResult AddNewDalilyByDates(UploadFileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("UploadFile", model);
            }

            var fileName = model.TempDailyPlannerName;
            var newDailyPlanner = DataValue.Current.GetDailyPlannerFromtempName(fileName);
            if (newDailyPlanner == null)
            {
                return ShowErrorUploadFile();
            }

            DataValue.Current.AddNoteFromNewToOldDailyPlannerOnServer(newDailyPlanner, CurrentUserName, model.DateFrom, model.DateTo);

            // Сохраняем записи
            DataValue.Current.SaveDailyPlannerOnServer(HttpContext, DailyPlanner, CurrentUserName);
            return RedirectToAction("Index", "DalilyPlanner");
        }


        #endregion
    }

}
