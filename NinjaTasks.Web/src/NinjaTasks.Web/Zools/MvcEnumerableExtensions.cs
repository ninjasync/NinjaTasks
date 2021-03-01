using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;

namespace NinjaTasks.Web.Zools
{
    public static class MvcEnumerableExtensions
    {
        public static IActionResult FirstOr404<T>(this IEnumerable<T> e)
        {
            var val = e.FirstOrDefault();
            if(val == null)
                return new HttpNotFoundResult();
            return new JsonResult(val);
        }
    }
}
