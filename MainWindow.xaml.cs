using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cache;
using Entities;
using XamlAnimatedGif;

namespace GiphyApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /* If we wish to expand the users' control over what they receive, we can change this variable in such a way
         * so it would be received from the user (selected from a combobox for example) */
        const string baseApiUrl = "https://api.giphy.com/v1/gifs/";
        const string apiKey = "mRUuBQMihBpYA8B4cgvH1B9is5rLTJVS";
        const string rating = "g";
        const string limit = "25";
        const string offset = "0";
        const string language = "en";

        int maxResults = 0;
        int readerIndex = 0;
        public int selectedRadioButton { get; set; }
       

        public MainWindow()
        {
            InitializeComponent();
            int.TryParse(limit, out maxResults);
            selectedRadioButton = (int)Enums.enumSearchType.Trending;
            rdbTrendingGif.IsChecked = true;
        }

        /// <summary>
        /// Button click event - Receive gifs according to the selected radiobutton and search words (if selected)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetGifs(object sender, RoutedEventArgs e)
        {
            if (selectedRadioButton == (int)Enums.enumSearchType.SearchByText)
            {
                GetGifsByKeyWords();
            }
            else if (selectedRadioButton == (int)Enums.enumSearchType.Trending)
            {
                GetTrendingGifs();
            }

        }

        /// <summary>
        /// Receive today's trending gifs
        /// </summary>
        private void GetTrendingGifs()
        {
            if (Cache.Cache.GetTrendingCache(DateTime.Now.Date) == null)
            {
                string url = baseApiUrl + "trending?api_key=" + apiKey + "&limit=" + limit + "& rating=" + rating;
                JsonEntity.Root rootData = ReceiveDataFromApi(url);
                if (rootData != null)
                    Cache.Cache.SetTrendingCacheData(DateTime.Now.Date, rootData.data);
            }
            readerIndex = 0;
            string uri = string.Empty;
            List<Entities.JsonEntity.Datum> data = Cache.Cache.GetTrendingCache(DateTime.Now.Date);
            DisplayData(data);
        }

        /// <summary>
        /// Receive gifs by key words
        /// </summary>
        private void GetGifsByKeyWords()
        {
            string searchKeyWords = txtSearchKeyWords.Text;
            if (!string.IsNullOrEmpty(searchKeyWords))
            {
                if (Cache.Cache.GetSearchResultsByKeyWord(searchKeyWords) == null)
                {
                    string url = baseApiUrl + "search?api_key=" + apiKey + "&q=" + searchKeyWords + "&limit=" + limit + "&offset=" + offset + "&rating=" + rating + "&lang=" + language;
                    JsonEntity.Root rootData = ReceiveDataFromApi(url);
                    if (rootData != null)
                        Cache.Cache.SetSearchResults(searchKeyWords, rootData.data);
                }
                readerIndex = 0;
                List<Entities.JsonEntity.Datum> data = Cache.Cache.GetSearchResultsByKeyWord(searchKeyWords);
                DisplayData(data);
            }
            else
            {
                MessageBox.Show(this, "Please enter your search words", "Error");
            }
        }

        /// <summary>
        /// Display the gifs received according to the user's search request
        /// </summary>
        /// <param name="data">Deserielized json data</param>
        private void DisplayData(List<Entities.JsonEntity.Datum> data)
        {
            if (data != null)
            {
                string uri = data[readerIndex].images.original.url;
                AnimationBehavior.SetSourceUri(gifDisplay, new Uri(uri));
            }
        }

        /// <summary>
        /// Receive data from the giphy servers using their API
        /// </summary>
        /// <param name="url">Url to access the API</param>
        /// <returns></returns>
        private JsonEntity.Root ReceiveDataFromApi(string url)
        {
            WebRequest request = HttpWebRequest.Create(url);
            WebResponse response = null;
            JsonEntity.Root rootData = null;
            try
            {
                response = request.GetResponse();
            }
            catch (Exception ex)
            {
                string error = "There seems to be an issue with the connection to the Giphy API. Please try again later.";
                if (ex.Message.Contains("429"))
                    error = "You have reached today's gif limit from Giphy";
                MessageBox.Show(this, error, "Error");
            }

            if (response != null)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());

                string responseJson = reader.ReadToEnd();

                rootData = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonEntity.Root>(responseJson);
            }
            return rootData;
        }

        /// <summary>
        /// Button click event - view next gif in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_Next(object sender, RoutedEventArgs e)
        {
            readerIndex++;
            if (readerIndex == maxResults)
                readerIndex = 0;
            DisplayGif();
        }

        /// <summary>
        /// Button click event - view previous gif in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_Prev(object sender, RoutedEventArgs e)
        {
            readerIndex--;
            if (readerIndex < 0)
                readerIndex = maxResults-1;
            DisplayGif();
        }

        /// <summary>
        /// Display gif according to the selected radiobutton
        /// </summary>
        private void DisplayGif()
        {
            if (selectedRadioButton == (int)Enums.enumSearchType.Trending)
            {
                if (Cache.Cache.GetTrendingCache(DateTime.Now.Date) == null)
                {
                    MessageBox.Show(this, "No trending gifs where retreived today", "Attention");
                }
                else
                    AnimationBehavior.SetSourceUri(gifDisplay, new Uri(Cache.Cache.GetTrendingCache(DateTime.Now.Date)[readerIndex].images.original.url));
            }
            else if (selectedRadioButton == (int)Enums.enumSearchType.SearchByText)
            {
                string searchKeyWords = txtSearchKeyWords.Text;
                if (Cache.Cache.GetSearchResultsByKeyWord(searchKeyWords) == null)
                {
                    MessageBox.Show(this, "Gifs by the keywords '" + searchKeyWords + "' where not retreived", "Attention");
                }
                else
                    AnimationBehavior.SetSourceUri(gifDisplay, new Uri(Cache.Cache.GetSearchResultsByKeyWord(searchKeyWords)[readerIndex].images.original.url));
            }
        }

        /// <summary>
        /// Radiobutton checked event - update the current selected radiobutton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rdbChecked(object sender, RoutedEventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            if (rdb.Tag.ToString() == Enums.enumSearchType.Trending.ToString())
                selectedRadioButton = (int)Enums.enumSearchType.Trending;
            else if (rdb.Tag.ToString() == Enums.enumSearchType.SearchByText.ToString())
                selectedRadioButton = (int)Enums.enumSearchType.SearchByText;
        }
    }
}
