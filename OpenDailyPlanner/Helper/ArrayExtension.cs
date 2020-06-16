using System;
namespace OpenDailyPlanner.Helper
{
    public static class ArrayExtension
    {
        /// <summary>
        /// Удаляем элем в списке
        /// </summary>
        /// <typeparam name="T">Какой то массив</typeparam>
        /// <param name="source">Массив</param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T[] RemoveAt<T>(this T[] source, uint index)
        {
            if (index > source.Length)
                return source;

            var newArray = new T[source.Length - 1];

            if (index > 0)
                Array.Copy(source, 0, newArray, 0, index);
            if (index <= source.Length)
                Array.Copy(source, index + 1, newArray, index, newArray.Length - index);
            
            return newArray;
        }

    }
}
