﻿using System;
using Owin;
using System.Web.Http;
using Microsoft.Practices.Unity;
using Unity.WebApi;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ToDoList.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            config.DependencyResolver = new UnityDependencyResolver(SetupContainer());

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "Api",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            app.UseStaticFiles(new StaticFileOptions
                {
                    FileSystem = new PhysicalFileSystem(@"./www"),
                }
            );

            app.Use<RedirectRootMiddleware>();

            app.UseWebApi(config);

            app.MapSignalR();
        }

        public class RedirectRootMiddleware : OwinMiddleware
        {
            public RedirectRootMiddleware(OwinMiddleware next) : base(next)
            {
            }

            public async override Task Invoke(IOwinContext context)
            {
                if (context.Request.Path.Value.Length <= 1)
                {
                    context.Response.StatusCode = 301;
                    context.Response.Headers.Set("Location", "/index.html");
                }
                else
                {
                    context.Response.Headers.Set("Cache-Control", "no-cache");
                    await Next.Invoke(context);
                }
            }
        }

        private IUnityContainer SetupContainer()
        {
            // evtl. weiterer Service, der den Hub kennt
            var todoService = new ToDoService();

            var container = new UnityContainer();
            container.RegisterInstance<IToDoService>(todoService);

            return container;
        }
    }
}
