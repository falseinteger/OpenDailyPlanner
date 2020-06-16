using System;
using System.Collections;
using Newtonsoft.Json;
using OpenDailyPlanner.Helper;

namespace OpenDailyPlanner.Models
{
    public class DailyPlannerBook
    {
        /// <summary>
        /// Инверсирвная сортировка
        /// </summary>
        [JsonProperty(nameof(IsReverse))]
        public bool IsReverse { get; private set; } = true;

        /// <summary>
        /// Сортировка по параметру
        /// </summary>
        [JsonProperty(nameof(SortField))]
        public NoteDailyField SortField { get; private set; } = NoteDailyField.Date;

        /// <summary>
        /// Записи
        /// </summary>
        [JsonProperty(nameof(Notes))]
        private NoteDaily[] Notes;

        /// <summary>
        /// Послений активный индекс
        /// </summary>
        private uint LastIndex = 0;

        public DailyPlannerBook()
        {
            Notes = new NoteDaily[0];
        }

        /// <summary>
        /// Получить запись по индексу
        /// </summary>
        /// <param name="index">Индекс записи</param>
        /// <returns>Запись</returns>
        public NoteDaily GetNoteDally(uint index)
        {
            if (index > LastIndex)
                return null;
            return Notes[index];
        }

        /// <summary>
        /// Записываем новую запись
        /// </summary>
        /// <param name="newNote">Новая запись</param>
        public void AddNote(NoteDaily newNote)
        {
            if(LastIndex >= Notes.Length)
                ResizeNotes(1);
            Notes.SetValue(newNote, LastIndex);
            LastIndex++;
        }

        /// <summary>
        /// Записывам новые записи
        /// </summary>
        /// <param name="newNote"></param>
        public void AddNotes(NoteDaily[] newNote)
        {
            if (newNote == null || newNote.Length <= 0)
                return;

            var countEmpty = Notes.Length - LastIndex;
            if (countEmpty < newNote.Length)
            {
                var newCount = newNote.Length - countEmpty;
                ResizeNotes((uint)newCount);
            }
            
            for (uint i = 0; i < newNote.Length; i++)
            {
                var item = newNote[i];
                if (item == null)
                    continue;
                Notes.SetValue(newNote[i], LastIndex);
                LastIndex++;
            }
        }

        /// <summary>
        /// Удаляем запись в списках
        /// </summary>
        /// <param name="index">Индекс записи</param>
        /// <returns>Успех добавление записи</returns>
        public bool RemoveNote(uint index)
        {
            if (Notes.Length < index)
                return false;
            Notes = Notes.RemoveAt(index);
            LastIndex--;
            return true;
        }

        /// <summary>
        /// Обновить запись по индексу
        /// </summary>
        /// <param name="index">Индекс записи</param>
        /// <param name="updateNote">Новая запись</param>
        /// <returns>Успех добавление записи</returns>
        public bool UpdateNote(uint index, NoteDaily updateNote)
        {
            if (index >= Notes.Length)
                return false;
            Notes[index] = updateNote;
            return true;
        }

        /// <summary>
        /// Получить число имеющихся записей в ежидневнике
        /// </summary>
        /// <returns></returns>
        public uint GetNoteCount()
        {
            return LastIndex;
        }

        /// <summary>
        /// Пересчитать значения инедкса
        /// </summary>
        public void RecalculateCount()
        {
            for (uint i = 0; i < Notes.Length; i++)
            {
                LastIndex = i;
                if (Notes[i] == null)
                    continue;
            }
        }

        /// <summary>
        /// Получить минимальную и максимальную дату записей
        /// </summary>
        /// <returns></returns>
        public (DateTime dateMin, DateTime dateMax) GetDateMinAndMaxOfNotes()
        {
            var dateMin = DateTime.Now;
            var dateMax = DateTime.Now;

            for (uint i = 0; i < Notes.Length; i++)
            {
                if (Notes[i] == null ||
                    Notes[i].Date == null)
                    continue;

                if (dateMin > Notes[i].Date)
                    dateMin = Notes[i].Date;

                if (dateMax < Notes[i].Date)
                    dateMax = Notes[i].Date;
            }

            return (dateMin, dateMax);
        }

        /// <summary>
        /// Отсортировать записи по параметру 
        /// </summary>
        /// <param name="noteDailyField">Параметр сортировки</param>
        /// <param name="isReverse">Обратная сортировка</param>
        public void NotesSortBy(NoteDailyField? sortField = null, bool? isReverse = null)
        {
            if (sortField != null)
                SortField = (NoteDailyField)sortField;
            if (isReverse != null)
                IsReverse = (bool)isReverse;

            var caseInsensitiveComparer = new CaseInsensitiveComparer();
            Array.Sort(Notes, Comparison(caseInsensitiveComparer, SortField, IsReverse));
        }

        /// <summary>
        /// Сортировка
        /// </summary>
        /// <param name="caseInsensitiveComparer"></param>
        /// <param name="noteDailyField">Параметр сортировки</param>
        /// <param name="isReverse">Обратная сортировка</param>
        /// <returns></returns>
        private Comparison<NoteDaily> Comparison(CaseInsensitiveComparer caseInsensitiveComparer, NoteDailyField noteDailyField, bool isReverse = true)
        {
            return new Comparison<NoteDaily>((x, y) => {

                if (x == null || y == null)
                    return 0;

                switch (noteDailyField)
                {
                    case NoteDailyField.Status:
                        return isReverse ?
                            caseInsensitiveComparer.Compare(x.Status, y.Status):
                            caseInsensitiveComparer.Compare(y.Status, x.Status);
                    case NoteDailyField.Place:
                        return isReverse?
                            caseInsensitiveComparer.Compare(x.Place, y.Place):
                            caseInsensitiveComparer.Compare(y.Place, x.Place);
                    case NoteDailyField.Body:
                        return isReverse ?
                            caseInsensitiveComparer.Compare(x.Body, y.Body):
                            caseInsensitiveComparer.Compare(y.Body, x.Body);
                    case NoteDailyField.Title:
                        return isReverse ?
                            caseInsensitiveComparer.Compare(x.Title, y.Title):
                            caseInsensitiveComparer.Compare(y.Title, x.Title);
                    case NoteDailyField.Date:
                    default:
                        return isReverse ?
                            caseInsensitiveComparer.Compare(x.Date, y.Date):
                            caseInsensitiveComparer.Compare(y.Date, x.Date);
                }
            });
        }

        /// <summary>
        /// Изменить размер количество записей
        /// </summary>
        /// <param name="count">Сколько требуется добавить пунктов</param>
        private void ResizeNotes(uint count)
        {
            if (count <= 0)
                return;
            Array.Resize(ref Notes, Notes.Length + (int)count);
        }
    }
}
