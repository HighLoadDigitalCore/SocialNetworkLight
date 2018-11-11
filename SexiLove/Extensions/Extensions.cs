using System.Collections;
using System.ComponentModel;
using System.Data.Linq;
using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
using SexiLove.Extensions.Helpers;
using SexiLove.Models;


namespace SexiLove.Extensions
{
    public enum WordKind
    {
        Man,
        Woman,
        Middle,
        Day,
        Minute
    }

    public static class Extensions
    {
        public static long? ToIPInt(this string ip)
        {
            Int64? ipInteger = null;
            try
            {
                ipInteger = (long)(uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(IPAddress.Parse(ip).GetAddressBytes(), 0));
            }
            catch (Exception)
            { }

            return ipInteger;

        }
        public static string ToIPString(this long? value)
        {
            if (!value.HasValue || value.Value < 0)
                return "";
            else if (value.Value < 4294967295)
                return IPAddress.Parse(value.Value.ToString()).ToString();
            else
                return "255.255.255.255";
        }
        public static byte[] GetFileContent(this string value)
        {
            if (value.IsNullOrEmpty()) return null;
            var path = HttpContext.Current.Server.MapPath(value);
            if (!File.Exists(path)) return null;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int) fs.Length);
                return buffer;
            }
        }
        public static string LastSegment(this string value)
        {
            var spl = value.Split<string>("/", "\\");
            if (spl.Any())
                return spl.Last().Trim();
            return "";
        }
        public static string PreLastSegment(this string value)
        {
            var spl = value.Split<string>("/");
            if (spl.Any() && spl.Count()>1)
                return spl.ElementAt(spl.Count()-2).Trim();
            return "";
        }

        public static string ReplaceEmpty(this string input, string repl)
        {
            if (input.IsNullOrEmpty()) return repl;
            return input;
        }
        public static string ToIntString(this bool val)
        {
            return val ? "1" : "0";
        }

        public static bool HasImageExtension(this string ext)
        {
            return ext != null &&
                   (ext.EndsWith(".jpg") || ext.EndsWith(".jpeg") || ext.EndsWith(".gif") || ext.EndsWith(".png") || ext.EndsWith("ico"));
        }

        public static IEnumerable<IEnumerable<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> source)
        {
            var enumerators = source.Select(e => e.GetEnumerator()).ToArray();
            try
            {
                while (enumerators.All(e => e.MoveNext()))
                {
                    yield return enumerators.Select(e => e.Current).ToArray();
                }
            }
            finally
            {
                Array.ForEach(enumerators, e => e.Dispose());
            }
        }

        public static IEnumerable<T> GetHalfOf<T>(this IEnumerable<object> list, bool left, int limit = 100)
        {
            var all = list.Take(limit).ToList();
            int leftCount = 0;
            int rem = 0;
            leftCount = Math.DivRem(all.Count(), 2, out rem);
            if (rem > 0)
                leftCount++;
            if (left)
                return all.Take(leftCount).Select(x => (T)x);
            return all.Skip(leftCount).Select(x => (T)x);

        }
   
        public static Binary ToBinary(this Stream stream)
        {
            var buf = new byte[stream.Length];
            stream.Read(buf, 0, buf.Length);
            return new Binary(buf);
        }

        public static bool CreateFolderIfNeeded(this string path)
        {
            bool result = true;
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception)
                {
                    /*TODO: Need to process this exception.*/
                    result = false;
                }
            }
            return result;
        }

        public static Guid? ToGuid(this string inp)
        {
            try
            {
                return new Guid(inp);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void Detach(this object entity)
        {
            entity.SetPropertyValue("PropertyChanged", null);
            entity.SetPropertyValue("PropertyChanging", null);

            foreach (FieldInfo fi in entity.GetType().
                GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fi.FieldType.ToString().Contains("EntityRef"))
                {
                    var value = fi.GetValue(entity);
                    if (value != null)
                    {
                        fi.SetValue(entity, null);
                    }
                }
                if (fi.FieldType.ToString().Contains("EntitySet"))
                {
                    var value = fi.GetValue(entity);
                    if (value != null)
                    {
                        MethodInfo mi = value.GetType().GetMethod("Clear");
                        if (mi != null)
                        {
                            mi.Invoke(value, null);
                        }

                        fi.SetValue(entity, value);
                    }
                }
            }
        }

        public static string ToNiceDate(this DateTime dt)
        {
            if (DateTime.Now.Subtract(dt).TotalHours <= 24)
            {
                return "Сегодня<br>" + dt.ToString("HH:mm");
            }
            return dt.ToString("dd.MM.yy");
        }

        public static Binary ToBinary(this FileInfo fi, bool deleteAfter = false)
        {
            var fs = fi.OpenRead();
            var buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();
            if (deleteAfter)
            {
                try
                {
                    fi.Delete();
                }
                catch
                {

                }
            }
            return buffer;
        }

        public static T SafeElementAt<T>(this IEnumerable<T> list, int index)
        {
            if (index < list.Count())
                return list.ElementAt(index);
            return default(T);
        }

        public static bool IsFilled(this Binary input)
        {
            return input != null && input.Length > 0;
        }
        public static object ToSuitableType(this object inp)
        {
            int ret;
            if (int.TryParse((string)inp, out ret))
                return (object)ret;
            return (object)inp;
        }

      
        public static DateTime? ToDate(this string value, string format = "dd.MM.yyyy")
        {
            DateTime res;
            if (DateTime.TryParseExact(value, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out res))
                return res;
            return null;
        }
        public static MvcHtmlString TwoStateValidationMessageFor<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression)
        {
            return MvcHtmlString.Create("");
            var defMsg = helper.ValidationMessageFor(expression).ToString();
            return MvcHtmlString.Create(string.IsNullOrEmpty(defMsg) ? "<i class=\"icon-good\"></i>" : "<i class=\"icon-bad\"></i>");
        }

        public static string GetArgFromQuery(this string query, string key)
        {
            var parts = query.Split<string>("?").ToArray();
            var pairs = parts[1].Split<string>("&")
                    .Select(x => x.Split<string>("=").ToArray())
                    .Select(x => new KeyValuePair<string, string>(x[0], x[1]));
            return pairs.Any(x => x.Key == key) ? pairs.First(x => x.Key == key).Value : "";
        }

        public static Guid GetCurrentUserUID(this HttpContext context)
        {
            return context.GetCurrentUserUID("");
        }
     
        public static Guid GetCurrentUserUID(this HttpContext context, string userName = "")
        {
            var user = userName.IsNullOrEmpty() ? Membership.GetUser() : Membership.GetUser(userName);
            if (user == null) return new Guid();
            return (Guid)user.ProviderUserKey;
        }
        public static Guid GetCurrentUserUID(this HttpContextBase context)
        {
            return context.GetCurrentUserUID("");
        }
        public static Guid GetCurrentUserUID(this HttpContextBase context, string userName = "")
        {
            var user = userName.IsNullOrEmpty() ? Membership.GetUser() : Membership.GetUser(userName);
            if (user == null) return new Guid();
            return (Guid)user.ProviderUserKey;
        }

        public static string GenerateURL(this HttpRequestBase request, List<KeyValuePair<string, string>> newPairs, bool onlyNewPairs = false)
        {

            var url = request.RawUrl;
            if (url.Contains("?"))
                url = url.Substring(0, url.IndexOf("?"));

            List<KeyValuePair<string, string>> oldPairs = new List<KeyValuePair<string, string>>();
            if (request.RawUrl.Contains("?"))
                oldPairs =
                    request.RawUrl.Substring(request.RawUrl.IndexOf("?") + 1)
                           .Split<string>("&")
                           .Select(x => x.Split<string>("="))
                           .Select(x => new KeyValuePair<string, string>(x.ElementAt(0), x.ElementAt(1)))
                           .ToList();
            oldPairs = oldPairs.Where(x => !newPairs.Any(z => z.Key == x.Key)).ToList().Concat(newPairs).ToList();

            if (onlyNewPairs)
            {
                oldPairs = newPairs;
            }

            return url + "?" + oldPairs.Where(x => x.Value.IsFilled()).Select(x => string.Format("{0}={1}", x.Key, x.Value)).JoinToString("&");

        }

        public static string GenerateURL(this HttpRequest request, List<KeyValuePair<string, string>> newPairs)
        {

            var url = request.RawUrl;
            if (url.Contains("?"))
                url = url.Substring(0, url.IndexOf("?"));

            List<KeyValuePair<string, string>> oldPairs = new List<KeyValuePair<string, string>>();
            if (request.RawUrl.Contains("?"))
                oldPairs =
                    request.RawUrl.Substring(request.RawUrl.IndexOf("?") + 1)
                           .Split<string>("&")
                           .Select(x => x.Split<string>("="))
                           .Select(x => new KeyValuePair<string, string>(x.ElementAt(0), x.ElementAt(1)))
                           .ToList();
            oldPairs = oldPairs.Where(x => !newPairs.Any(z => z.Key == x.Key)).ToList().Concat(newPairs).ToList();

            return url + "?" + oldPairs.Where(x => x.Value.IsFilled()).Select(x => string.Format("{0}={1}", x.Key, x.Value)).JoinToString("&");

        }

        public static object ExecuteMethod(this object obj, string methodName, params object[] argList)
        {
            var type = obj.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName);
            object result = null;
            if (methodInfo != null)
            {
                result = methodInfo.Invoke(obj, argList);
            }
            return result;
        }

        public static ITable GetTableByName(this DataContext context, string tableName)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context is empty");
            }
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName is empty");
            }
            return (ITable)context.GetType().GetProperty(tableName).GetValue(context, null);
        }

   
        public static string GetRequestIP(this HttpRequestBase request)
        {
            var sIpAddress =
                request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            return String.IsNullOrEmpty(sIpAddress)
                       ? request.ServerVariables["REMOTE_ADDR"]
                       : sIpAddress.Split<string>(", ").ToArray()[0];
        }
        public static string GetRequestIP(this HttpRequest request)
        {
            var sIpAddress =
                request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            return String.IsNullOrEmpty(sIpAddress)
                       ? request.ServerVariables["REMOTE_ADDR"]
                       : sIpAddress.Split<string>(", ").ToArray()[0];
        }


        public static string Clear(this string inp, IEnumerable<string> trash, string replacer = "")
        {
            return trash.Aggregate(inp, (current, tr) => current.Replace(tr, replacer));
        }
        public static string Clear(this string inp, IEnumerable<char> trash, char replacer)
        {
            return trash.Aggregate(inp, (current, tr) => current.Replace(tr, replacer));
        }

        public static string JoinToURL(this IEnumerable source)
        {
            return "/" +
                   source.Cast<object>()
                         .Where(x => x != null)
                         .Select(x => x.ToString().Trim('\\', '/'))
                         .JoinToString("/");
        }
        public static string JoinToString(this IEnumerable source, string delimiter = "")
        {

            return string.Join(delimiter, source.Cast<object>().Where(x => x != null).Select(x => x.ToString()));
        }

        private static readonly DateTime MaxDate = new DateTime(0x270f, 12, 0x1f, 0x17, 0x3b, 0x3b, 0x3e7);
        private static readonly DateTime MinDate = new DateTime(0x76c, 1, 1);
        private static readonly Regex WebUrlExpression = new Regex(@"((http|https)://)?([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.Singleline | RegexOptions.Compiled);

        public static int LoadPossibleProperties(this object target, object source)
        {
            return LoadPossibleProperties(target, source, new string[0]);
        }
        public static int LoadPossibleProperties(this object target, object source, IEnumerable<string> excludeList)
        {
            var avail = target.GetPropertyNameList().Intersect(source.GetPropertyNameList());
            int counter = 0;
            var fields = source.GetType().
                    GetFields(BindingFlags.NonPublic | BindingFlags.Instance);


            foreach (var propName in avail)
            {

                if (excludeList.Contains(propName)) continue;

                var fi = fields.FirstOrDefault(x => x.Name == ("_" + propName));
                if (fi != null)
                {

                    if (fi.FieldType.ToString().Contains("EntityRef"))
                    {
                        continue;
                    }
                    if (fi.FieldType.ToString().Contains("EntitySet"))
                    {
                        continue;
                    }
                }

                try
                {
                    target.SetPropertyValue(propName, source.GetPropertyValue(propName));
                    counter++;
                }
                catch
                {

                }
            }
            return counter;
        }

        public static Type GetPropertyType(this object source, string propName)
        {
            var prop = source.GetType().GetProperties().FirstOrDefault(x => x.Name == propName);
            return prop == null ? typeof(string) : prop.PropertyType;
        }

        public static int LoadPossibleProperties(this object target, IEnumerable<string> excludeList, FormCollection collection)
        {

            var avail = target.GetPropertyNameList().Intersect(collection.AllKeys);
            int counter = 0;

            foreach (var propName in avail)
            {
                if (excludeList.Contains(propName)) continue;
                try
                {
                    target.SetPropertyValue(propName,
                                            collection.GetValue(propName).ConvertTo(target.GetPropertyType(propName)));
                    counter++;
                }
                catch
                {

                }
            }
            return counter;
        }

        public static bool IsFilled(this decimal? val)
        {
            return val.HasValue && val.Value != 0;
        }

        public static IEnumerable<T> Split<T>(this string target, params string[] args)
        {
            if (target == null) return new List<T>();
            return target.Split(args.Any() ? args : new[] { ";", ".", "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Select(x => x.ConvertFromString<T>());
        }


        public static bool IsWebUrl(this string target)
        {
            return (!string.IsNullOrEmpty(target) && WebUrlExpression.IsMatch(target));
        }
        public static bool IsValid(this DateTime target)
        {
            return ((target >= MinDate) && (target <= MaxDate));
        }
        public static RouteValueDictionary ToRouteValues(this HttpRequestBase request)
        {
            var routes = new RouteValueDictionary();
            foreach (string key in request.QueryString.Keys)
            {
                routes.Add(key, request.QueryString[key]);
            }
            return routes;
        }


        public static bool IsArchiveExt(this string fileName)
        {
            return fileName.EndsWith(".rar") || fileName.EndsWith(".zip") || fileName.EndsWith(".gz");
        }
        public static IEnumerable<T> WrapWithEnumerable<T>(this T obj)
        {
            yield return obj;
        }

        public static List<string> GetPropertyNameList(this object data)
        {
         
            var infos = data.GetType().GetProperties();
            var excludeTypes = new[] { "EntityRef", "EntitySet" };
            var i = infos.Where(x => x.CanRead && x.CanWrite && !excludeTypes.Any(z => x.PropertyType.FullName.Contains(z))).Select(x => x.Name).ToList();
            return i;
        }

        public static object GetPropertyValue(this object data, string field)
        {
            var infos = data.GetType().GetProperties();
            var prop = infos.FirstOrDefault(x => x.Name == field);
            if (prop == null) return null;
            return prop.GetValue(data, null);
        }

        public static void SetPropertyValueByString(this object data, string name, string field)
        {
            var infos = data.GetType().GetProperties();
            var prop = infos.FirstOrDefault(x => x.Name == name);
            if (prop == null) return;
            prop.SetValue(data, Convert.ChangeType(field, prop.PropertyType), null);
        }

        public static void SetPropertyValue(this object data, string name, object field)
        {
            var infos = data.GetType().GetProperties();
            var prop = infos.FirstOrDefault(x => x.Name == name);
            if (prop == null) return;
            try
            {
                prop.SetValue(data, field, null);
            }
            catch
            {
                try
                {
                    prop.SetValue(data, field.ToString(), null);
                }
                catch
                {

                }
            }

        }

        public static string GetPropertyAttribute<T>(this object data, string propName, string attrName)
        {
            var infos = data.GetType().GetProperties();
            var prop = infos.FirstOrDefault(x => x.Name == propName);
            if (prop == null) return "";

            var attrList = prop.GetCustomAttributes(true);
            foreach (var a in attrList)
            {
                if (a is T)
                    return (a.GetPropertyValue(attrName) ?? "").ToString();

            }
            return "";

        }


        public static string GetStringPostfix(this int value, WordKind kind)
        {
            var rest = value % 10;
            if (kind == WordKind.Man)
            {
                if (value > 10 && value < 20)
                {
                    return "ов";
                }
                else if (rest == 0 || rest >= 5)
                {
                    return "ов";
                }
                else if (rest == 1)
                {
                    return "";
                }
                else return "а";
            }
            if (kind == WordKind.Woman)
            {
                if (value > 10 && value < 20)
                {
                    return "";
                }
                else if (rest == 0 || rest >= 5)
                {
                    return "";
                }
                else if (rest == 1)
                {
                    return "а";
                }
                else return "ы";
            }
            if (kind == WordKind.Day)
            {
                if (value > 10 && value < 20)
                {
                    return "дней";
                }
                else if (rest == 0 || rest >= 5)
                {
                    return "дней";
                }
                else if (rest == 1)
                {
                    return "день";
                }
                else return "дня";
            }
            if (kind == WordKind.Minute)
            {
                if (value > 10 && value < 20)
                {
                    return "минут";
                }
                else if (rest == 0 || rest >= 5)
                {
                    return "минут";
                }
                else if (rest == 1)
                {
                    return "минута";
                }
                else return "минуты";
            }

            return "";
        }

        public static bool IsGuid(this string guid)
        {
            try
            {
                var g = new Guid(guid);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static MvcHtmlString DisplayNameFor<TModel, TProperty>(this HtmlHelper<IEnumerable<TModel>> helper, Expression<Func<TModel, TProperty>> expression)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            name = helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            var metadata = ModelMetadataProviders.Current.GetMetadataForProperty(() => Activator.CreateInstance<TModel>(), typeof(TModel), name);
            return new MvcHtmlString(metadata.DisplayName);
        }

        public static string ReduceSpaces(this string value)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[ ]{2,}", options);
            return regex.Replace(value, @" ");
        }

        public static bool IsMailAdress(this string value)
        {
            if (value.IsNullOrEmpty())
                return false;
            Regex regex =
                new Regex(
                    @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");

            return regex.IsMatch(value);
        }

   

        public static string TruncateToPoint(this string value, int len)
        {
            return TruncateToSymbol(value, len);
        }

        public static string TruncateToSymbol(this string value, int len, string symbol = ".")
        {
            if (len >= value.Length) return value;
            int index = value.IndexOf(symbol, len);
            if (index < 0) return value;
            return value.Substring(0, index + 1);
        }

        public static string PageUrl(this HttpRequest request)
        {

            return request.RawUrl.Split(new[] { '?' })[0];
        }

        public static string ToShowStatus(this bool? value)
        {

            return (value ?? false).ToShowStatus();
        }
        public static string ToShowStatus(this bool value)
        {
            return value ? "Отображается" : "Скрыто";
        }
        public static string ToYesNoStatus(this bool value)
        {
            return value ? "Да" : "Нет";
        }

        public static int GetFirstHalf(this int value)
        {
            int rem = value % 2;
            return (int)(value / 2) + rem;
        }

        public static string ClearBorders(this string value)
        {
            string val = value.Trim();
            val = val.Replace("..", ".");
            val = val.Replace("--", "-");
            val = val.Replace("  ", " ");
            if (val.StartsWith(".") || val.StartsWith("-")) val = val.Substring(1).Trim();
            if (val.StartsWith(".") || val.StartsWith("-")) val = val.Substring(1).Trim();
            if (val.EndsWith("-")) val = val.Substring(0, val.Length - 1);
            return val.Replace("- ", "-").Replace(" -", "").Trim();

        }
        public static bool ToBool(this string value)
        {
            bool parsed;
            if (string.IsNullOrEmpty(value)) return false;
            if (value == "1") return true;
            if (value.ToLower() == "да") return true;
            if (value == "0") return false;
            if (value.Contains(",")) value = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)[0];
            bool.TryParse(value.ToNiceForm(), out parsed);
            return parsed;
        }

        public static decimal ToDecimal(this string value)
        {
            if (value.IsNullOrEmpty()) return 0;
            decimal parsed;
            decimal.TryParse(value.Replace(".", ",").Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsed);
            return parsed;
        }
        public static decimal? ToNullableDecimal(this string value)
        {
            if (value.IsNullOrEmpty()) return null;
            decimal parsed;
            if (decimal.TryParse(value.Replace(".", ",").Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
                return parsed;
            return null;
        }
        public static int ToInt(this string value)
        {
            if (value.IsNullOrEmpty()) return 0;
            int parsed = 0;
            if (int.TryParse(value, out parsed))
                return parsed;
            return 0;
        }
        public static int? ToNullInt(this string value)
        {
            if (value.IsNullOrEmpty() || value == "null") return null;
            int parsed = 0;
            if (int.TryParse(value, out parsed))
                return parsed;
            return null;
        }

        public static bool IsNullInt(this string value)
        {
            if (value.IsNullOrEmpty() || value == "null") return true;
            int parsed = 0;
            return int.TryParse(value, out parsed);
        }

        public static bool IsInt(this string value)
        {
            int parsed = 0;
            return int.TryParse(value, out parsed);
        }

        public static string UnTranslit(this string value, TransliterationType type)
        {
            return Translitter.Back(value, type);
        }
        public static string Translit(this string value, TransliterationType type)
        {
            return Translitter.Front(value, type);
        }
        public static string Translit(this string value)
        {
            return Translitter.Front(value);
        }
        public static string UnTranslit(this string value)
        {
            return Translitter.Back(value);
        }

        public static IEnumerable<XElement> ReadElements(this XmlReader reader, XName elementName)
        {

            while (!reader.EOF)
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        {

                            if (reader.Name == elementName)
                            {
                                XElement el = (XElement)XElement.ReadFrom(reader);
                                if (el != null)
                                    yield return el;
                            }
                            else
                                reader.Read();
                        }
                        break;
                    default:
                        {
                            reader.Read();
                        }
                        break;
                }

            }
            /*
                        if (reader.Name == elementName.LocalName && reader.NamespaceURI == elementName.NamespaceName)
                            yield return (XElement)XElement.ReadFrom(reader);
            */

            /*
                        while (reader.ReadToNextSibling(elementName.LocalName, elementName.NamespaceName))
                            yield return (XElement)XElement.ReadFrom(reader);
            */
        }
        public static string ClearHTML(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }
            return
                Regex.Replace(value, @"<[^>]*>", " ")
                     .Trim()
                     .Replace("&nbsp;", "")
                     .Replace("&laquo;", "«")
                     .Replace("&raquo;", "»")
                     .Replace("&lt;", "<")
                     .Replace("&gt", ">").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
        }

        public static string CreateDir(this HttpServerUtility server, string path)
        {
            var mapped = server.MapPath(path);
            if (!Directory.Exists(mapped))
            {
                Directory.CreateDirectory(mapped);
            }
            return mapped;
        }

        public static string CreateDir(this HttpServerUtilityBase server, string path)
        {
            var mapped = server.MapPath(path);
            if (!Directory.Exists(mapped))
            {
                Directory.CreateDirectory(mapped);
            }
            return mapped;
        }

        public static MvcHtmlString ActionLinkQuery(this HtmlHelper html, string linkText, string actionName, string controllerName)
        {
            return html.ActionLinkQuery(linkText, actionName, controllerName, new List<string>());
        }

        public static MvcHtmlString ActionLinkQuery(this HtmlHelper html, string linkText, string actionName, string controllerName, IEnumerable<string> queryFilter)
        {
            return html.ActionLinkQuery(linkText, actionName, controllerName, queryFilter, new RouteValueDictionary());
        }

        public static MvcHtmlString ActionLinkQuery(this HtmlHelper html, string linkText, string actionName, string controllerName, IEnumerable<string> queryFilter, object routeValues)
        {
            return html.ActionLinkQuery(linkText, actionName, controllerName, queryFilter, new RouteValueDictionary(routeValues.ToDictionary()), null);
        }

        public static MvcHtmlString ActionLinkQuery(this HtmlHelper html, string linkText, string actionName, string controllerName, IEnumerable<string> queryFilter, RouteValueDictionary routeValues)
        {
            return html.ActionLinkQuery(linkText, actionName, controllerName, queryFilter, routeValues, null);
        }

        public static MvcHtmlString ActionLinkQuery(this HtmlHelper html, string linkText, string actionName, string controllerName, IEnumerable<string> queryFilter, object routeValues, object htmlAttributes)
        {
            return html.ActionLinkQuery(linkText, actionName, controllerName, queryFilter, new RouteValueDictionary(routeValues.ToDictionary()), htmlAttributes);
        }

        public static MvcHtmlString ActionLinkQuery(this HtmlHelper html, string linkText, string actionName, string controllerName, IEnumerable<string> queryFilter, RouteValueDictionary routeValues, object htmlAttributes)
        {
            RouteValueDictionary dictionary = new RouteValueDictionary();
            NameValueCollection query = html.ViewContext.HttpContext.Request.QueryString;
            foreach (string key in query.Keys)
            {
                if (queryFilter.Contains<string>(key) && !dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, query[key]);
                }
            }
            foreach (KeyValuePair<string, object> pair in routeValues)
            {
                if (!dictionary.ContainsKey(pair.Key))
                    dictionary.Add(pair.Key, pair.Value);
            }
            return html.ActionLink(linkText, actionName, controllerName, dictionary, (htmlAttributes ?? new object()).ToDictionary());
        }

        public static T ConvertFromString<T>(this string text)
        {
            if (typeof(Enum).IsAssignableFrom(typeof(T)))
            {
                try
                {
                    return (T)Enum.Parse(typeof(T), text);
                }
                catch
                {
                    return default(T);
                }
            }
            return (T)Convert.ChangeType(text, typeof(T));
        }

        public static Bitmap CreateThumbnail(this Bitmap loBMP, int lnWidth, int lnHeight, bool useCalc)
        {
            Bitmap bmpOut = null;
            try
            {
                decimal lnRatio;
                decimal lnTemp;
                int lnNewWidth = 0;
                int lnNewHeight = 0;
                if (useCalc)
                {
                    if ((loBMP.Width < lnWidth) && (loBMP.Height < lnHeight))
                    {
                        return loBMP;
                    }
                    if (loBMP.Width > loBMP.Height)
                    {
                        lnRatio = (decimal)lnWidth / (decimal)loBMP.Width;
                        lnNewWidth = lnWidth;
                        lnTemp = loBMP.Height * lnRatio;
                        lnNewHeight = (int)lnTemp;
                    }
                    else
                    {
                        lnRatio = (decimal)lnHeight / (decimal)loBMP.Height;
                        lnNewHeight = lnHeight;
                        lnTemp = loBMP.Width * lnRatio;
                        lnNewWidth = (int)lnTemp;
                    }
                }
                else
                {
                    lnNewHeight = lnHeight;
                    lnNewWidth = lnWidth;
                }
                bmpOut = new Bitmap(lnNewWidth, lnNewHeight);
                using (Graphics g = Graphics.FromImage(bmpOut))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(loBMP, 0, 0, lnNewWidth, lnNewHeight);
                }
                loBMP.Dispose();
            }
            catch
            {
                return null;
            }
            return bmpOut;
        }

        public static string ForDisplaing(this decimal value)
        {
            if (Math.Truncate(value) != value)
            {
                return value.ToString("f1");
            }
            return value.ToString("f0");
        }

        public static string FormatWith(this string str, params object[] args)
        {
            return string.Format(str, args);

        }
        public static string FormatWith(this string str, params string[] args)
        {
            return string.Format(str, (object[])args);
        }

        public static Color GenerateRandom(this Color color)
        {
            Random rnd = new Random((DateTime.Now.Millisecond * DateTime.Now.Second) * DateTime.Now.Minute);
            return Color.FromArgb(0xff, rnd.Next(1, 0x100), rnd.Next(1, 0x100), rnd.Next(1, 0x100));
        }

        public static bool IsEmpty(this Guid guid)
        {
            return (guid == new Guid());
        }

        public static bool IsEmpty(this Guid? guid)
        {

            return (!guid.HasValue || guid == new Guid());
        }
        public static bool IsDecimal(this string value)
        {
            if (value.IsNullOrEmpty()) return false;
            decimal parsed;
            return decimal.TryParse(value.Replace(".", ",").Trim(), NumberStyles.Any, CultureInfo.CurrentCulture,
                                    out parsed);
        }
        public static bool IsFilled(this string str)
        {
            return !str.IsNullOrEmpty();
        }
        public static bool IsNullOrEmpty(this string str)
        {
            return ((str == null) || string.IsNullOrEmpty(str.Trim()));
        }

        public static bool IsWeekEnd(this DateTime date)
        {
            return ((date.DayOfWeek == DayOfWeek.Sunday) || (date.DayOfWeek == DayOfWeek.Saturday));
        }

        public static RedirectToRouteResult RedirectToActionWithQuery(this Controller controller, string actionName, string controllerName, IEnumerable<string> defaultRoutes)
        {
            RouteValueDictionary dictionary = new RouteValueDictionary();
            NameValueCollection query = controller.ControllerContext.HttpContext.Request.QueryString;
            foreach (string key in query.Keys)
            {
                if (defaultRoutes.Contains(key))
                {
                    dictionary.Add(key, query[key]);
                }
            }
            return (typeof(Controller).GetMethod("RedirectToAction", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(string), typeof(RouteValueDictionary) }, null).Invoke(controller, new object[] { actionName, controllerName, dictionary }) as RedirectToRouteResult);
        }

    

        public static byte[] ToByteArray(this HttpPostedFileBase file)
        {
            byte[] buffer = new byte[0x8000];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while (true)
                {
                    read = file.InputStream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                    {
                        return ms.ToArray();
                    }
                    ms.Write(buffer, 0, read);

                }
                /*
                                bool CS$4$0001;
                                goto Label_0045;
                            Label_0015:
                                read = file.InputStream.Read(buffer, 0, buffer.Length);
                                if (read <= 0)
                                {
                                    return ms.ToArray();
                                }
                                ms.Write(buffer, 0, read);
                            Label_0045:
                                CS$4$0001 = true;
                                goto Label_0015;
                */
            }
        }

        public static IDictionary<string, object> ToDictionary(this object obj)
        {
            BindingFlags attr = BindingFlags.Public | BindingFlags.Instance;
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (PropertyInfo property in obj.GetType().GetProperties(attr))
            {
                if (property.CanRead)
                {
                    dict.Add(property.Name, property.GetValue(obj, null));
                }
            }
            return dict;
        }

        public static string ToHtml(this decimal value)
        {
            return value.ToString("f2").Replace(",", ".");
        }

        public static string ToUniString(this decimal value)
        {
            if (value - (int)value == 0)
                return value.ToString("f0");
            return value.ToString("f2").Replace(",", ".");
        }

        public static string ToNiceForm(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            if (str.Length == 1)
            {
                return str.ToUpper();
            }
            return (str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower());
        }

        public static string GeneratePassword(this Random rnd, int len = 6)
        {
            string pattern = "0123456789abcdefghijklmnopqrstuvwxyz";
            string pass = "";
            if (len == 0) len = 6;
            for (int i = 0; i < len; i++)
            {
                pass += pattern.Substring(rnd.Next(0, pattern.Length), 1);
            }
            return pass;
        }

        public static object ToNonAnonymousList<T>(this List<T> list, Type t)
        {
            object l = Activator.CreateInstance(typeof(List<>).MakeGenericType(new[] { t }));
            MethodInfo addMethod = l.GetType().GetMethod("Add");
            foreach (T item in list)
            {
                addMethod.Invoke(l, new[] { item.ToType(t) });
            }
            return l;
        }

        public static object ToType<T>(this object obj, T type)
        {
            object tmp = Activator.CreateInstance(Type.GetType(type.ToString()));
            foreach (PropertyInfo pi in obj.GetType().GetProperties())
            {
                try
                {
                    tmp.GetType().GetProperty(pi.Name).SetValue(tmp, pi.GetValue(obj, null), null);
                }
                catch
                {
                }
            }
            return tmp;
        }

        public static T ToTypedValue<T>(this object obj)
        {
            Type t = typeof(T);
            object tmp = Activator.CreateInstance(t);
            try
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                {

                    if (obj == null)
                    {
                        return (T)(object)null;
                    }
                    else
                    {
                        var nullableConverter = new NullableConverter(t);
                        t = nullableConverter.UnderlyingType;
                    }
                }
                tmp = Convert.ChangeType(obj, t);
            }
            catch (Exception e)
            {

            }

            return (T)tmp;
        }

        public static object ToTypedObject(this object text, Type type)
        {
            if (typeof(Enum).IsAssignableFrom(type))
            {
                try
                {
                    return Enum.Parse(type, text.ToString());
                }
                catch
                {
                    return null;
                }
            }
            try
            {
                return Convert.ChangeType(text, type);
            }
            catch
            {
                return null;
            }
        }

        public static DateTime WeekStarted(this DateTime date)
        {
            if (date.DayOfWeek != DayOfWeek.Monday)
            {
                DateTime current = date;
                while (current.DayOfWeek != DayOfWeek.Monday)
                {
                    current = current.AddDays(-1.0);
                }
                return current.Date;
            }
            return date.Date;
        }
    }
}

