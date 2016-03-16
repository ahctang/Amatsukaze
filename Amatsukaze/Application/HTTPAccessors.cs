using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Amatsukaze.ViewModel
{
    class MALAccessor
    {
        //These are the URL strings that are taken from the optionsobject when the accessor is initialized
        //For the time being I am taking only the query api url, but maybe in the future we will use the add/list/favorite http apis as well.
        public MALAccessor(OptionsObject optionsObject)
        {
            initializeURLstrings(optionsObject);
        }


        private string AnimeXMLQueryURL;
        private string MALLogin;
        private string MALPassword;
        private string cacheFolderPath;

        public void GetAnimeXML(string AnimeName)
        {
            //Construct the URL
            string url = constructQueryURL(AnimeName);

            //Construct the local path
            string localpath = cacheFolderPath + "MAL-" + AnimeName + ".xml";

            //Build login credentials
            var webClient = new WebClient();

            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(MALLogin + ":" + MALPassword));

            // Inject this string as the Authorization header
            webClient.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);

            //Fetch the actual resource
            try
            {
                webClient.DownloadFile(new Uri(url), localpath );
            }
            catch (Exception e)
            {
                //Handle webclient access exceptions here
                MessageBox.Show(e.Message) ;                     
            }

            //Free the resouce here
            webClient.Dispose();
        }

        private void initializeURLstrings(OptionsObject optionsObject)
        {
            this.AnimeXMLQueryURL = optionsObject.MALXMLQueryURL;
            this.MALLogin = optionsObject.MALLogin;
            this.MALPassword = optionsObject.MALPassword;
            this.cacheFolderPath = optionsObject.CacheFolderpath;
        }

        private string constructQueryURL(string AnimeName)
        {
            return AnimeXMLQueryURL + AnimeName;
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

        private void initializeURLstrings()
        {

        }
    }
}
