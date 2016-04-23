using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.IO.Compression;
using System.Xml;
using Amatsukaze.ViewModel;


namespace Amatsukaze.HelperClasses
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

        //Gets an anime XML. Returns true if animeXML is downloaded successfully, but return false if it can't find the anime or if an exception is thrown. 
        //Error handling should be done in the viewmodel
        public bool GetAnimeXML(string AnimeName)
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
                string returnstring = webClient.DownloadString(new Uri(url));
                Console.WriteLine(returnstring);
                File.WriteAllText(localpath, returnstring);

                //Check here for 0 length string (i.e. anime is not found)
                if (returnstring.Length == 0)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                //Handle webclient access exceptions here
                Console.WriteLine(e.Message);
                webClient.Dispose();
                return false;
            }

            //Normal termination
            //Free the resouce here
            webClient.Dispose();

            return true;
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

        public AniDBAccessor(OptionsObject optionsObject)
        {
            initializeURLstrings(optionsObject);
        }

        private string aniDBIDURL;
        private string aniDBXMLQueryURL;
        private string cacheFolderPath;
        private AnimeList Animes = new AnimeList();

        /// <summary>
        /// Same as above. Returns true on normal termination and false on error. Error handling should be done in the view model
        /// </summary>
        /// <param name="AnimeName"></param>
        /// <returns></returns>
        public bool GetAnimeXML(string AnimeName)
        {
            //We have to get the ID first
            int AnimeID = 0;
            string IDQueryString = constructIDQueryURL(AnimeName);


            using (var client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                try
                {
                    //Download xml file from ID website
                    string buffer = client.DownloadString(IDQueryString);
                    Console.WriteLine(buffer);

                    //Parse xml return
                    ParseAniDBID(buffer);

                    //Get the actual ID
                    AnimeID = GetAniDBID(AnimeName);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }

            //Now go to aniDB to go get the XML with the parsed AniID
            if (AnimeID != 0)
            {
                using (var client = new WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    try
                    {
                        //Construct the URL
                        string url = constructAniDBXMLQueryURL(AnimeID.ToString());

                        //Construct the local path
                        string localpath = cacheFolderPath + "AniDB-" + AnimeName + ".xml";

                        //AniDB returns the fucking file as a gzip.... decompress here... 
                        byte[] gzipstream = client.DownloadData(new Uri(url));

                        using (GZipStream stream = new GZipStream(new MemoryStream(gzipstream), CompressionMode.Decompress))
                        {
                            const int size = 4096;
                            byte[] buffer = new byte[size];
                            using (MemoryStream memory = new MemoryStream())
                            {
                                int count = 0;
                                do
                                {
                                    count = stream.Read(buffer, 0, size);
                                    if (count > 0)
                                    {
                                        memory.Write(buffer, 0, count);
                                    }
                                }
                                while (count > 0);

                                File.WriteAllBytes(localpath, memory.ToArray());
                            }
                        }


                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return false;
                    }
                }
            }
            else
            {
                //AniID was not found
                return false;
            }


            return true;
        }

        private int GetAniDBID(string AnimeName)
        {
            int AniID = 0;
            int MinStringDistance = 10; //Just some placeholder value. Too lazy to rewrite this as a for loop
            

            foreach (Anime anime in Animes.animeList)
            {
                foreach (string name in anime.Names)
                {
                    //Match is done by string distance as opposed to simple matches as provided by list.contains and or list.findindex
                    int temp = calculateStringDistance(AnimeName.ToUpper(), name.ToUpper());
                    if (temp  < MinStringDistance)
                    {
                        MinStringDistance = temp;
                        AniID = anime.AniID;
                    }
                }
            }
            return AniID;
        }

        private void initializeURLstrings(OptionsObject optionsobject)
        {
            this.aniDBIDURL = optionsobject.AniDBIDRetrieverURL;
            this.aniDBXMLQueryURL = optionsobject.AniDBXMLQueryURL;
            this.cacheFolderPath = optionsobject.CacheFolderpath;
        }       

        //The while loop is different in this than the anidb parser because the elements are not separated by \n
        private bool ParseAniDBID(string input)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(new StringReader(input)))
                {
                    while (reader.ReadToFollowing("anime"))
                    {
                        Anime Animebuffer = new Anime();
                        Animebuffer.AniID = Convert.ToInt16(reader.GetAttribute("aid"));

                        reader.ReadToDescendant("title");
                        Animebuffer.Names.Add(reader.ReadElementContentAsString());

                        while (reader.NodeType != XmlNodeType.EndElement)
                        {
                            string buffer = reader.ReadElementContentAsString();
                            Animebuffer.Names.Add(buffer);
                        }
                        Animes.animeList.Add(Animebuffer);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        private string constructIDQueryURL(string AnimeName)
        {
            return aniDBIDURL + AnimeName;
        }

        private string constructAniDBXMLQueryURL(string AniID)
        {
            return aniDBXMLQueryURL + AniID;
        }

        //Borrowed from Rossetta Code
        private int calculateStringDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            for (int i = 0; i <= n; i++)
                d[i, 0] = i;
            for (int j = 0; j <= m; j++)
                d[0, j] = j;

            for (int j = 1; j <= m; j++)
                for (int i = 1; i <= n; i++)
                    if (s[i - 1] == t[j - 1])
                        d[i, j] = d[i - 1, j - 1];  //no operation
                    else
                        d[i, j] = Math.Min(Math.Min(
                            d[i - 1, j] + 1,    //a deletion
                            d[i, j - 1] + 1),   //an insertion
                            d[i - 1, j - 1] + 1 //a substitution
                            );
            return d[n, m];
        }
    }

    class AnimeList
    {
        public List<Anime> animeList = new List<Anime>();
    }

    class Anime
    {
        public int AniID;
        public List<string> Names = new List<string>();
    }
}
