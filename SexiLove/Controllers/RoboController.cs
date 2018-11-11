using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SexiLove.Extensions.Mail;
using SexiLove.Models;

namespace sexivirt.Web.Areas.Default.Controllers
{
    public class RoboController : BaseController
    {


        //
        // GET: /Default/Robo/
        public ActionResult Result(string OutSum, int InvId, string SignatureValue)
        {
            
            var md5 = new MD5CryptoServiceProvider();
            var sCrcBase = string.Format("{0}:{1}:{2}", OutSum, InvId, Config.RoboPass2);
            var buf = md5.ComputeHash(Encoding.ASCII.GetBytes(sCrcBase));
            var actualSignature = buf.Select(b => string.Format("{0:x2}", b)).Aggregate((acc, el) => acc + el);


            // Если подписи совпадают...
            if (string.Equals(actualSignature, SignatureValue, StringComparison.InvariantCultureIgnoreCase))
            {
                var order = DB.MoneyTransactions.FirstOrDefault(x => x.ID == InvId);
                
                if (order == null)
                {
                    return new ContentResult() {Content = "FAIL" + InvId};
                }

                decimal payedTotal = Convert.ToDecimal(OutSum, new CultureInfo("en-US"));

                // если не совпадают суммы
                if (order.Sum != payedTotal)
                {
                    return new ContentResult() { Content = "FAIL" + InvId};
                }

                // всё ОК
                // Если обработчик OnResult почему-то не отработал и заказ ещё не отмечен как оплаченный
                order.Status = 1;
                DB.SubmitChanges();
                

                return new ContentResult() { Content = "OK" + InvId };
            }

            return new ContentResult()
            {
                Content =
                    "FAIL" + InvId
            };
        }
        public ActionResult Success(string OutSum, int InvId, string SignatureValue)
        {

            var md5 = new MD5CryptoServiceProvider();
            var sCrcBase = string.Format("{0}:{1}:{2}", OutSum, InvId, Config.RoboPass1);
            var buf = md5.ComputeHash(Encoding.ASCII.GetBytes(sCrcBase));
            var actualSignature = buf.Select(b => string.Format("{0:x2}", b)).Aggregate((acc, el) => acc + el);


            // Если подписи совпадают...
            if (string.Equals(actualSignature, SignatureValue, StringComparison.InvariantCultureIgnoreCase))
            {
                var order = DB.MoneyTransactions.FirstOrDefault(x => x.ID == InvId);
                if (order == null)
                {
                    return new ContentResult() { Content = "FAIL" + InvId };
                }

                decimal payedTotal = Convert.ToDecimal(OutSum, new CultureInfo("en-US"));

                // если не совпадают суммы
                if (order.Sum != payedTotal)
                {
                    return new ContentResult() { Content = "FAIL" + InvId };
                }

                // всё ОК
                // Если обработчик OnResult почему-то не отработал и заказ ещё не отмечен как оплаченный
                order.Status = 1;
                DB.SubmitChanges();

                return RedirectToAction("Index", "Home");
            }

            return new ContentResult() { Content = "FAIL" + InvId };
        }
        public ActionResult Fail(decimal OutSum, int InvId)
        {
            var order = DB.MoneyTransactions.FirstOrDefault(x => x.ID == InvId);
            if (order != null && order.Sum == OutSum)
            {
                order.Status = -1;
                DB.SubmitChanges();
            }
            return new ContentResult() { Content = "Произошла ошибка при оплате" };
        }
    }
}