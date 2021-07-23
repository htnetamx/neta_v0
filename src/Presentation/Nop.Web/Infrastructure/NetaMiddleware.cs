using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Services.Stores;

namespace Nop.Web.Infrastructure
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class NetaMiddleware
    {
        private readonly IStoreService _storeService;
        private readonly RequestDelegate _next;

        public NetaMiddleware(RequestDelegate next, IStoreService storeService)
        {
            _next = next;
            _storeService = storeService;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (string.IsNullOrWhiteSpace(httpContext.Request.Host.Host))
                await _next(httpContext);
            else
            {
                var stores = await _storeService.GetAllStoresAsync();
                var store = stores.Where(v => v.Hosts.Contains(httpContext.Request.Host.Host)).Any();
                if (!store)
                {
                    httpContext.Response.Redirect("https://api.whatsapp.com/send/?phone=525574174213&text=Hola%21+Cu%C3%A1l+es+la+nueva+liga+de+netamx&app_absent=0");
                    //httpContext.Response.Redirect("https://wa.me/525574174213?text=Hola!%20Cuál%20es%20la%20nueva%20liga%20de%20netamx", permanent: true);
                    return;
                }
                else
                {
                    await _next(httpContext);
                }
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class NetaMiddlewareExtensions
    {
        public static IApplicationBuilder UseNetaMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<NetaMiddleware>();
        }
    }
}
