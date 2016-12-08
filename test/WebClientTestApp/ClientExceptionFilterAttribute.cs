using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace WebClientTestApp
{
    public class ClientExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override Task OnExceptionAsync(ExceptionContext context)
        {
            return base.OnExceptionAsync(context);
        }
    }
}
