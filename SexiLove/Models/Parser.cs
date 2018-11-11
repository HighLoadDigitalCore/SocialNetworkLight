using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using SexiLove.Extensions;

namespace SexiLove.Models
{
    public class ParserResult
    {
        public int CatCount { get; set; }
        public int PageCountCreate { get; set; }
        public int PageCountUpdate { get; set; }
    }
    public class Parser
    {
        public List<string> TownUrls = new List<string>()
        {
            "spb",
            "msk",
            "nsk",
            "ekb",
            "nnv",
            "kzn",
            "vbg",
            "smr",
            "tlt",
            "krd",
            "sochi",
            "ryazan",
            "krasnoyarsk",
        };

        public List<string> TownNames  = new List<string>()
        {
            "Санкт-Петербург",
            "Москва",
            "Новосибирск",
            "Екатеринбург",
            "Нижний Новгород",
            "Казань",
            "Выборг",
            "Самара",
            "Тольятти",
            "Краснодар",
            "Сочи",
            "Рязань",
            "Красноярск",
        };

        private string GetPageContent(string url)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Encoding = Encoding.UTF8;
                
                return
                    wc.DownloadString("http://kudago.com" + url)
                        .Replace("\t", "")
                        .Replace("\n", "")
                        .Replace("\r", "")
                        .Replace("  ", " ")
                        .Replace("  ", " ")
                        .Replace("  ", " ");
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public ParserResult Parse()
        {
            var result = new ParserResult();
            var db = new xDBDataContext();
            for (int i = 0; i < TownUrls.Count; i++)
            {
                var url = TownUrls[i];
                var mainPage = GetPageContent("/" + url + "/");
                //<a class=".+?site-nav-link.+?" href="(.)+?"><span>(.)+?</span></a>

                //<div\s*class="centered-container">([а-яА-Я\s\w\d<>="-/▼…:]+)<\!\-\-
                var m1 = new Regex("<div\\s*class=\"centered-container\">([а-яА-Я\\s\\w\\d\\<\\=\\\"\\/\\-\\>\\,\\!\\#]+?)more-", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                //<ul\s+id="more-events"[^>]+>([а-яА-Я\s\w\d<>="-/▼…:(?!)]+?)<\/ul>
                var m2 = new Regex("<ul\\s+id=\"more-events\"[^>]+>([а-яА-Я\\s\\w\\d<>=\"-/▼…:(?!)]+?)<\\/ul>", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                //<ul\s+id="more-places"[^>]+>([а-яА-Я\s\w\d<>="-/▼…:(?!)]+?)<\/ul>
                var m3 = new Regex("<ul\\s+id=\"more-places\"[^>]+>([а-яА-Я\\s\\w\\d<>=\"-/▼…:(?!)]+?)<\\/ul>", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                var match1 = m1.Matches(mainPage);
                var match2 = m2.Matches(mainPage);
                var match3 = m3.Matches(mainPage);

                string links = "";
                try
                {
                    foreach (Match m in match1)
                    {
                        links += m.Groups[0].Captures[0].Value;
                    }
                    foreach (Match m in match2)
                    {
                        links += m.Groups[0].Captures[0].Value;
                    }
                    foreach (Match m in match3)
                    {
                        links += m.Groups[0].Captures[0].Value;
                    }
                }
                catch
                {
                    continue;
                    
                }
                var rxl = new Regex("<a href=\"([^\\\"]+)\"[^>]+><span>([^<]+)</span></a>",
                    RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var cats = rxl.Matches(links);

                foreach (Match cat in cats)
                {
                    string caturl = "";
                    try
                    {
                        caturl = cat.Groups[1].Captures[0].Value.Split<string>("/").ElementAt(1);
                    }
                    catch
                    {
                        continue;
                        
                    }
                    var exist = db.EventCats.FirstOrDefault(x => x.ExportURL == caturl);
                    if (exist == null)
                    {
                        try
                        {
                            exist = new EventCat()
                            {
                                ExportURL = caturl,
                                Name = cat.Groups[2].Captures[0].Value
                            };
                        }
                        catch
                        {
                            continue;
                            
                        }
                        db.EventCats.InsertOnSubmit(exist);
                        db.SubmitChanges();
                    }
                    var rel =
                        db.EventCatsForTowns.FirstOrDefault(x => x.Town.Name == TownNames[i] && x.CatID == exist.ID);
                    if (rel == null)
                    {
                        var tid = db.Towns.FirstOrDefault(x => x.Name == TownNames[i]);
                        if (tid == null)
                        {
                            tid = new Town(){Name = TownNames[i]};
                            db.Towns.InsertOnSubmit(tid);
                            db.SubmitChanges();
                        }
                        rel = new EventCatsForTown()
                        {
                            CatID = exist.ID,
                            TownID =  tid.ID
                        };
                        db.EventCatsForTowns.InsertOnSubmit(rel);
                        db.SubmitChanges();
                    }

                    var prx = new Regex("<a[^>]+data-page=\"([^\\\"]+)\"[^>]*>", 
                        RegexOptions.Multiline | RegexOptions.IgnoreCase);

                    var catPageContent = GetPageContent("/" + url + "/" + exist.ExportURL + "/");
                    var pages = prx.Matches(catPageContent);
                    List<int> pager = null;
                    try
                    {
                        pager = (from Match page in pages select page.Groups[1].Captures[0].Value.ToInt()).ToList();
                        pager = pager.Distinct().Where(x => x > 0).OrderBy(x => x).ToList();
                    }
                    catch
                    {
                        pager = new List<int>(){1};
                    }

                    for (int j = 0; j < pager.Count; j++)
                    {
                        var curl = "/" + url + "/" + exist.ExportURL+"/";
                        if (pager[j] > 1)
                            curl += "?page=" + pager[j];

                        var pageContent = GetPageContent(curl);
                        var rart = new Regex("<article[^>]+class=\"post[^\"]+\"[^>]*>(.+?)<\\/article>",RegexOptions.Multiline | RegexOptions.IgnoreCase);
                        var articles = rart.Matches(pageContent);
                        foreach (Match article in articles)
                        {
                            string artText = "";
                            try
                            {
                                artText = article.Groups[1].Captures[0].Value;
                            }
                            catch
                            {
                                continue;
                                
                            }

                            var imageRx = new Regex("<img[^\"]+src=\"(.+?)\"[^\"]+class=\"post-image-first\"[^>]+>",
                                RegexOptions.Multiline | RegexOptions.IgnoreCase);
                            var linkRx = new Regex("<a href=\"([^\"]+?)\"[^>]class=\"post-title-link\"[^>]+><span>([^<]+?)<\\/span><\\/a>",
                                RegexOptions.Multiline | RegexOptions.IgnoreCase);
                            var shortRx = new Regex("<div[^>]+class=\"post-description\">(.+?)<\\/div>",
                                RegexOptions.Multiline | RegexOptions.IgnoreCase);

                            var link = linkRx.Match(artText);
                            string detailurl = "";
                            try
                            {
                                detailurl = link.Groups[1].Captures[0].Value;
                            }
                            catch
                            {
                                continue;
                                
                            }
                            var artUID = detailurl.Split<string>("/").Last();
                            var existEvent =
                                db.Events.FirstOrDefault(
                                    x =>
                                        x.ExportLink == artUID && x.TownCatID == rel.ID);
                            if (existEvent == null)
                            {
                                try
                                {
                                    existEvent = new Event()
                                    {
                                        ExportLink = artUID,
                                        Name = link.Groups[2].Captures[0].Value,
                                        Photo = DownLoadPhoto(imageRx.Match(artText).Groups[1].Captures[0].Value),
                                        ShortDescr = shortRx.Match(artText).Groups[1].Captures[0].Value.ClearHTML(),
                                        TownCatID = rel.ID,
                                    };
                                }
                                catch
                                {
                                    continue;
                                    
                                }
                                var detailContent = GetPageContent(detailurl);
                                var descrRx =
                                    new Regex("<div[^\"]class=\"[^\"]+post-big-banner-content[^>]+>.*<\\/div>(.*)<p class=\"typo-help\">");
                                var dateRx = new Regex("<span[^>]+class=\"[^\"]*dtstart[^\"]*\"[^>]+>(.*?)<\\/span>");
                                try
                                {
                                    existEvent.FullDescr =
                                        descrRx.Match(detailContent).Groups[1].Captures[0].Value.ClearHTML();
                                }
                                catch
                                {
                                    existEvent.FullDescr = "";
                                }
                                try
                                {
                                    existEvent.StartDate =
                                        DateTime.Parse(dateRx.Match(detailContent).Groups[1].Captures[0].Value);
                                }
                                catch
                                {
                                    existEvent.StartDate = null;
                                }

                                db.Events.InsertOnSubmit(existEvent);
                                db.SubmitChanges();
                            }
                        }

                    }



                }


            }


            return result;
        }

        private string DownLoadPhoto(string url)
        {
            var path = "/Content/Events/" + Guid.NewGuid() + Path.GetExtension(url);
            WebClient wc = new WebClient();
            wc.DownloadFile("http://kudago.com"+url, HttpContext.Current.Server.MapPath(path));
            return path;
        }
    }
}