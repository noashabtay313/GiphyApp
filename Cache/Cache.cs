using System;
using System.Collections.Generic;
using System.Text;
using static Entities.JsonEntity;
using static GiphyApp.Enums;

namespace Cache
{
    public static class Cache
    {
        private static Dictionary<DateTime, List<Datum>> trendingCache = null;
        private static Dictionary<string, List<Datum>> searchResults = null;

        /// <summary>
        /// Set today's trending cache data to the dictionary
        /// </summary>
        /// <param name="today"></param>
        /// <param name="data"></param>
        public static void SetTrendingCacheData(DateTime today, List<Datum> data)
        {
            if (trendingCache == null)
            {
                trendingCache = new Dictionary<DateTime, List<Datum>>();
            }
            trendingCache.Add(today, data);
        }

        /// <summary>
        /// Retreive today's trending cache
        /// </summary>
        /// <param name="today"></param>
        /// <returns></returns>
        public static List<Datum> GetTrendingCache(DateTime today)
        {
            if (trendingCache == null || !trendingCache.ContainsKey(today))
                return null;
            else
                return trendingCache[today];
        }

        /// <summary>
        /// Set search results in the cache dictionary
        /// </summary>
        /// <param name="searchKeyWord"></param>
        /// <param name="data"></param>
        public static void SetSearchResults(string searchKeyWord, List<Datum> data)
        {
            searchKeyWord = searchKeyWord.ToLower();
            if (searchResults == null)
            {
                searchResults = new Dictionary<string, List<Datum>>();
            }
            if (!searchResults.ContainsKey(searchKeyWord))
            {
                searchResults.Add(searchKeyWord, data);
            }
        }

        /// <summary>
        /// Retreive data according the the search key words
        /// </summary>
        /// <param name="searchKeyWord"></param>
        /// <returns></returns>
        public static List<Datum> GetSearchResultsByKeyWord(string searchKeyWord)
        {
            searchKeyWord = searchKeyWord.ToLower();
            if (searchResults == null || !searchResults.ContainsKey(searchKeyWord))
                return null;
            else
                return searchResults[searchKeyWord];
        }

    }
}
