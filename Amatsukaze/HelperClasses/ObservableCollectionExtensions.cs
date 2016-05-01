using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amatsukaze.ViewModel;

namespace Amatsukaze.HelperClasses
{
    public static class ObservableCollectionExtensions
    {
        public static void AlphabetSort<T>(this ObservableCollection<T> observable) where T : IComparable<T>, IEquatable<T>
        {
            List<T> sorted = observable.OrderBy(x => GetName(x)).ToList();
            for (int i = 0; i < sorted.Count(); i++)
                observable.Move(observable.IndexOf(sorted[i]), i);
        }

        private static string GetName<T>(T source) where T : IComparable<T>, IEquatable<T>
        {
            //Cast source back into animeentryobject
            AnimeEntryObject temp = source as AnimeEntryObject;

            if (temp.english.Length != 0 && temp.english != null) return temp.english;
            else return temp.title;
        }

            
    }
}
