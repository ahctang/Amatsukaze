using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amatsukaze.ViewModel
{
    class MALAccessor
    {
        //These are the URL strings that are taken from the optionsobject when the accessor is initialized
        //For the time being I am taking only the query api url, but maybe in the future we will use the add/list/favorite http apis as well.
        private string AnimeXMLQueryURL;

        public void GetAnimeXML(string AnimeName)
        {
        }

        private void initializeURLstrings()
        {

        }
    }

    class AniDBAccessor
    {
        public void GetAnimeXML(string AnimeName)
        {
        }

        private int GetAniDBID(string AnimeName)
        {
            return 0;
        }
    }
}
