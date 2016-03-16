﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Amatsukaze.ViewModel
{
    class FileOrganizerLogic
    {

        private static string[] videoFormats = { ".mkv", ".avi", ".mp4,", ".mov", ".flv", ".ogg", ".wmv", ".rm", ".rmvb", ".m4p ", ".m4v", ".mpg", ".mpeg", ".vob", ".ogv", ".qt", ".mp2", ".mpe", ".mpv", ".f4v"};

        /**
        * From a root directory. Parses all sub-directories and files to make a list of series
        **/ 
        public static List<Series> parseAsSeries(string rootFolderAbsolutePath)
        {
            List<Series> seriesList = new List<Series>();

            // Read the file and display it line by line.
            //System.IO.StreamReader file = new System.IO.StreamReader(
            //    Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\fileNames.txt");

            string line;
            //while ((line = file.ReadLine()) != null)
            foreach (string file in getFileNamesOfAllSubdirectories(rootFolderAbsolutePath))
            {
                line = file;
                List<string> surroundedStrings = getSurroundedParts(line);

                SeriesEpisode episode = new SeriesEpisode();

                // If file extension not video file format, skip to next file
                if (!videoFormats.Contains(line.Substring(line.LastIndexOf("."))))
                {
                    continue;
                }

                // Extract file extension
                episode.extension = line.Substring(line.LastIndexOf("."));
                line = line.Replace(episode.extension, "");

                for (int i = 0; i < surroundedStrings.Count; i++)
                {
                    string surroundedString = surroundedStrings[i];

                    if (surroundedString.Length == 10 && Regex.IsMatch(surroundedString, "[[(][a-fA-F0-9]*[])]"))
                    {
                        if (episode.hash != null)
                        {
                            throw new Exception("wtf man");
                        }
                        else
                        {
                            episode.hash = surroundedString;
                            line = line.Replace(surroundedString, "");
                        }
                    }
                    else if (Regex.IsMatch(surroundedString, "\\d+x\\d+") || Regex.IsMatch(surroundedString, "\\d+p")
                      || Regex.IsMatch(surroundedString, "[Bb][Dd]") || Regex.IsMatch(surroundedString, "[Dd][Vv][Dd]")
                      || Regex.IsMatch(surroundedString, "Hi\\d+P"))
                    {
                        episode.quality += surroundedString;
                        line = line.Replace(surroundedString, "");
                    }
                    else if (surroundedString.Equals(line.Substring(0, surroundedString.Length)))
                    {
                        if (episode.subGroup != null)
                        {
                            throw new Exception("wtf man");
                        }
                        else
                        {
                            episode.subGroup = surroundedString;
                            line = line.Replace(surroundedString, "");
                        }
                    }
                }

                // Extract episode number
                line = line.Replace("_", " ").Trim();
                episode.episodeNumber = Regex.Match(line, "[ ]([eE][pP][iI][sS][oO][dD][eE][. ]?)?([eE][pP][. ]?)?(Vol[. ][ ]?)?[#]?\\d\\d?([ ]?v\\d([.]?\\d)?)?([ ]?v\\d([.]?\\d)?)?([ ]?[&][&]?[ ]?(\\d\\d?))?([.]\\d\\d)?",
                    RegexOptions.RightToLeft).Value.Trim();
                if (episode.episodeNumber.Length > 0)
                {
                    line = line.Replace(episode.episodeNumber, "").Trim();
                }

                // What's left is the anime name, removing useless characters
                if (line.EndsWith("-"))
                {
                    line = line.Remove(line.LastIndexOf("-")).Trim();
                }
                episode.seriesName = line.Replace("  ", " ").Trim();

                bool existingSeries = false;
                foreach (Series series in seriesList)
                {
                    if (series.name.Equals(episode.seriesName))
                    {
                        series.episodes.Add(episode);
                        existingSeries = true;
                    }
                }

                if (!existingSeries)
                {
                    Series newSeries = new Series();
                    newSeries.name = episode.seriesName;
                    newSeries.episodes.Add(episode);
                    seriesList.Add(newSeries);
                }
            }
            //file.Close();

            foreach (Series series in seriesList)
            {
                Console.WriteLine("SeriesName = " + series.name);
                foreach (SeriesEpisode episode in series.episodes)
                {
                    Console.WriteLine("    SeriesName = " + episode.seriesName);
                    Console.WriteLine("        Subgroup = " + episode.subGroup);
                    Console.WriteLine("        Extension = " + episode.extension);
                    Console.WriteLine("        Quality = " + episode.quality);
                    Console.WriteLine("        Hash = " + episode.hash);
                    Console.WriteLine("        Episode Number = " + episode.episodeNumber);
                }
            }

            return seriesList;
        }

        /**
        * Recursively gets the name of all files in the rootFolder directory and all of its subfolders for all depth
        **/
        private static List<string> getFileNamesOfAllSubdirectories(string rootFolderAbsolutePath)
        {
            List<string> results = new List<string>();
            DirectoryInfo directoryInfo = new DirectoryInfo(rootFolderAbsolutePath);

            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                results.Add(fileInfo.Name);
            }

            foreach (DirectoryInfo subDirectoryInfo in directoryInfo.GetDirectories())
            {
                foreach (string fileInSubdirectory in getFileNamesOfAllSubdirectories(subDirectoryInfo.FullName))
                {
                    results.Add(fileInSubdirectory);
                }
            }

            return results;
        }

        /**
        * Gets all parts between () or [] of a string
        **/
        private static List<string> getSurroundedParts(string line)
        {
            List<string> result = new List<string>();


            while (line.IndexOf("[") != -1)
            {
                string surrounded = line.Substring(line.IndexOf("["), line.IndexOf("]") - line.IndexOf("[") + 1);
                result.Add(surrounded);
                line = line.Remove(line.IndexOf("["), line.IndexOf("]") - line.IndexOf("[") + 1);
            }

            while (line.IndexOf("(") != -1)
            {
                string surrounded = line.Substring(line.IndexOf("("), line.IndexOf(")") - line.IndexOf("(") + 1);
                result.Add(surrounded);
                line = line.Remove(line.IndexOf("("), line.IndexOf(")") - line.IndexOf("(") + 1);
            }

            return result;
        }

    }

    public class Series
    {
        public string name { get; set; }
        public List<SeriesEpisode> episodes { get; set; } = new List<SeriesEpisode>();
    }

    public class SeriesEpisode
    {
        public string seriesName { get; set; }
        public string subGroup { get; set; }
        public string episodeNumber { get; set; }
        public string quality { get; set; }
        public string hash { get; set; }
        public string extension { get; set; }
    }

}
