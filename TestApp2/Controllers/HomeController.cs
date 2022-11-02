using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Net;
using System.Xml;
using TestApp2.Models;
using System.ServiceModel.Syndication;
using System.Xml.Linq;

namespace TestApp2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Чтение rss
        /// </summary>
        /// <param name="url">Сайт ленты</param>
        /// <returns></returns>
        public SyndicationFeed getFeed(string url)
        {
            XmlReader reader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(reader);
            return feed;
        }

        /// <summary>
        /// Чтение rss через прокси
        /// </summary>
        /// <param name="url">Сайт ленты</param>
        /// <param name="proxyAddress">Адрес прокси</param>
        /// <param name="isLocal"></param>
        /// <param name="login">Логин прокси</param>
        /// <param name="password">Пароль прокси</param>
        /// <returns></returns>
        public SyndicationFeed getFeed(string url, string proxyAddress, bool isLocal, string login, string password)
        {
            var proxy = new WebProxy(proxyAddress, isLocal);
            if (login != null && password != null)
                proxy.Credentials = new NetworkCredential(login, password);
            WebClient client = new WebClient();
            client.Proxy = proxy;
            Stream stream = client.OpenRead(url);
            XmlReader readxml = XmlReader.Create(stream);
            var feed = SyndicationFeed.Load(readxml);
            return feed;
        }

        public IActionResult Page()
        {
            Settings settings = new Settings();
            getSettings(settings);
            if (!String.IsNullOrEmpty(settings.proxyAddress))
            ViewBag.node = getFeed(
                settings.url, settings.proxyAddress, settings.isLocal, settings.login, settings.password);
            else
            ViewBag.node = getFeed(settings.url);
            ViewData["RefreshTime"] = settings.refreshTime * 1000;
            return View();
        }

        public class Settings
        {
            //Сайт ленты
            public string url { get; set; }
            //Адрес прокси
            public string proxyAddress { get; set; }
            public bool isLocal { get; set; }
            //Логин прокси
            public string login { get; set; }
            //Пароль прокси
            public string password { get; set; }
            //Время обновления данных
            public int refreshTime { get; set; }
        }

        public void getSettings(Settings sets)
        {
            XDocument xDoc = XDocument.Load("Settings//settings.xml");
            XElement? settings = xDoc.Element("settings");
            if (settings is not null)
            {
                // проходим по всем элементам person
                foreach (XElement set in settings.Elements("feed"))
                {

                    //XAttribute? name = set.Attribute("name");
                    XElement? feedUrl = set.Element("feedUrl");
                    XElement? refreshTime = set.Element("refreshTime");
                    sets.url = feedUrl.Value;
                    sets.refreshTime = (int)refreshTime;
                }
                foreach (XElement set in settings.Elements("proxy"))
                {
                    XAttribute? address = set.Attribute("address");
                    XAttribute? isLocal = set.Attribute("isLocal");
                    XElement? login = set.Element("login");
                    XElement? password = set.Element("password");
                    sets.proxyAddress = address.Value;
                    sets.isLocal = Convert.ToBoolean(isLocal.Value);
                    sets.login = login.Value;
                    sets.password = password.Value;
                }
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}