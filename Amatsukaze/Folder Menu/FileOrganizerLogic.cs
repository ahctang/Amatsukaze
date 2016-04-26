using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Amatsukaze.ViewModel.FolderReadingLogic;

namespace Amatsukaze.ViewModel
{
    /// <summary>
    /// Logic for getting the episode information of each file
    /// </summary>
    public static class FileOrganizerLogic
    {
        /// <summary>True if you want the logic result to be printed to console</summary>
        private static readonly bool PRINT_TO_CONSOLE = true;
        /// <summary>List of accepted video formats. File is not parsed if not in these</summary>
        private static readonly string[] VIDEO_FORMATS = {
            "mkv", "avi", "mp4,", "mov", "flv", "ogg", "wmv",
            "rm", "rmvb", "m4p ", "m4v", "mpg", "mpeg", "vob",
            "ogv", "qt", "mp2", "mpe", "mpv", "f4v"
        };

        /// <summary>
        /// Scan a directory and all its subfolders recursively to return a collection of Series/Anime (and their episodes).
        /// </summary>
        /// <param name="rootFolderAbsolutePath">Absolute path to the root folder to scan</param>
        /// <returns>A collection of all Series/Anime and their found episode in the argument root folder</returns>
        public static ObservableCollection<Series> parseRootFolderAsSeries(string rootFolderAbsolutePath)
        {
            HashSet<string> allFiles = recursiveGetAllFilePaths(rootFolderAbsolutePath);
            return parseAsSeries(allFiles);
        }

        // TODO : performance check. This method might take a while to execute.
        /// <summary>
        /// Scan all folders saved in cache recursively to return a collection of Series/Anime (and their episodes).
        /// </summary>
        /// <returns>A collection of all Series/Anime and their found episode in the saved folder cache</returns>
        public static ObservableCollection<Series> parseAllSavedFoldersAsSeries()
        {
            // Get all folders saved in cache
            ObservableCollection<FolderEntity> folders = FolderReadingLogic.readFoldersCache();
            // Will store all the files without duplicates
            HashSet<string> allFilesInSavedFolders = new HashSet<string>();

            // For each of these folders, get all theirs files, and add them to allFilesInSavedFolders
            foreach (FolderEntity folder in folders)
            {
                HashSet<string> allFiles = recursiveGetAllFilePaths(folder.path);
                foreach (string file in allFiles)
                {
                    allFilesInSavedFolders.Add(file);
                }
            }

            // Return the result of all these files parsed as Series
            return parseAsSeries(allFilesInSavedFolders);
        }

        /// <summary>
        /// Scan file paths to return a collection of Series/Anime (and their episodes).
        /// </summary>
        /// <param name="allFilePaths">HashSet containing absolute path of all the files to scan</param>
        /// <returns>A collection of all Series/Anime and their found episode in the argument file paths set</returns>
        private static ObservableCollection<Series> parseAsSeries(HashSet<string> allFilePaths)
        {
            ObservableCollection<Series> result = new ObservableCollection<Series>();

            foreach (string filePath in allFilePaths)
            {
                // Name of the file, will get little by little stripped of each information found in it when found
                string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);

                // If file extension not video file format, skip to next file
                if (fileName.LastIndexOf(".") == -1 || !VIDEO_FORMATS.Contains(fileName.Substring(fileName.LastIndexOf(".") + 1)))
                {
                    continue;
                }

                // Make a new episode object
                SeriesEpisode episode = new SeriesEpisode();
                episode.filePath = filePath;

                List<string> surroundedStrings = getSurroundedParts(fileName);

                // Extract file extension
                episode.extension = fileName.Substring(fileName.LastIndexOf("."));
                fileName = fileName.Replace(episode.extension, "");

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
                            fileName = fileName.Replace(surroundedString, "");
                        }
                    }
                    else if (Regex.IsMatch(surroundedString, "\\d+x\\d+") || Regex.IsMatch(surroundedString, "\\d+p")
                      || Regex.IsMatch(surroundedString, "[Bb][Dd]") || Regex.IsMatch(surroundedString, "[Dd][Vv][Dd]")
                      || Regex.IsMatch(surroundedString, "Hi\\d+P"))
                    {
                        episode.quality += surroundedString;
                        fileName = fileName.Replace(surroundedString, "");
                    }
                    else if (surroundedString.Equals(fileName.Substring(0, surroundedString.Length)))
                    {
                        if (episode.subGroup != null)
                        {
                            throw new Exception("wtf man");
                        }
                        else
                        {
                            episode.subGroup = surroundedString;
                            fileName = fileName.Replace(surroundedString, "");
                        }
                    }
                }

                // Extract episode number
                fileName = fileName.Replace("_", " ").Trim();
                episode.episodeNumber = Regex.Match(fileName, "[ ]([eE][pP][iI][sS][oO][dD][eE][. ]?)?([eE][pP][. ]?)?(Vol[. ][ ]?)?[#]?\\d\\d?([ ]?v\\d([.]?\\d)?)?([ ]?v\\d([.]?\\d)?)?([ ]?[&][&]?[ ]?(\\d\\d?))?([.]\\d\\d)?",
                    RegexOptions.RightToLeft).Value.Trim();
                if (episode.episodeNumber.Length > 0)
                {
                    fileName = fileName.Replace(episode.episodeNumber, "").Trim();
                }

                // What's left is the anime name, removing useless characters
                if (fileName.EndsWith("-"))
                {
                    fileName = fileName.Remove(fileName.LastIndexOf("-")).Trim();
                }
                episode.seriesName = fileName.Replace("  ", " ").Trim();

                bool existingSeries = false;
                foreach (Series series in result)
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
                    result.Add(newSeries);
                }
            }

            // Print result to console
            if (PRINT_TO_CONSOLE)
            {
                foreach (Series series in result)
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
            }

            return result;
        }

        /// <summary>
        /// Recursively gets the name of all file paths in the rootFolder directory and all of its subfolders for all depth
        /// </summary>
        /// <param name="rootFolderAbsolutePath">path of the root folder to scan</param>
        /// <returns>An Hashset of all file paths contained in the root folder and its subfolders (recursive)</returns>
        private static HashSet<string> recursiveGetAllFilePaths(string rootFolderAbsolutePath)
        {
            HashSet<string> results = new HashSet<string>();

            DirectoryInfo directoryInfo = new DirectoryInfo(rootFolderAbsolutePath);

            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                results.Add(fileInfo.FullName);
            }

            foreach (DirectoryInfo subDirectoryInfo in directoryInfo.GetDirectories())
            {
                foreach (string fileInSubdirectory in recursiveGetAllFilePaths(subDirectoryInfo.FullName))
                {
                    results.Add(fileInSubdirectory);
                }
            }

            return results;
        }

        /// <summary>
        /// Gets all parts between () or [] of a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns>A list of all substrings of input that is between () or []</returns>
        private static List<string> getSurroundedParts(string input)
        {
            List<string> result = new List<string>();

            while (input.IndexOf("[") != -1)
            {
                string surrounded = input.Substring(input.IndexOf("["), input.IndexOf("]") - input.IndexOf("[") + 1);
                result.Add(surrounded);
                input = input.Remove(input.IndexOf("["), input.IndexOf("]") - input.IndexOf("[") + 1);
            }

            while (input.IndexOf("(") != -1)
            {
                string surrounded = input.Substring(input.IndexOf("("), input.IndexOf(")") - input.IndexOf("(") + 1);
                result.Add(surrounded);
                input = input.Remove(input.IndexOf("("), input.IndexOf(")") - input.IndexOf("(") + 1);
            }

            return result;
        }

    }

    /// <summary>
    /// Represents a series/anime and all its found episodes
    /// </summary>
    public class Series
    {
        /// <summary>Name of the series/anime</summary>
        public string name { get; set; }
        /// <summary>List of episodes found for that series</summary>
        public List<SeriesEpisode> episodes { get; set; } = new List<SeriesEpisode>();
    }

    /// <summary>
    /// Represents an episode of a series/anime, and all information that can be taken from its filename
    /// </summary>
    public class SeriesEpisode
    {
        /// <summary>Name of the series/anime this episode is in</summary>
        public string seriesName { get; set; }
        /// <summary>Name of the sub group that translated this episode</summary>
        public string subGroup { get; set; }
        /// <summary>Number of the episode</summary>
        public string episodeNumber { get; set; }
        /// <summary>Encoding quality</summary>
        public string quality { get; set; }
        /// <summary>Hashcode of the episode</summary>
        public string hash { get; set; }
        /// <summary>Path to the actual file</summary>
        public string filePath { get; set; }
        /// <summary>File extension</summary>
        public string extension { get; set; }
    }

}
