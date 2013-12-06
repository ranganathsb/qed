﻿using Owin;
using System.Security.Principal;
using OwinExtensions;

namespace qed
{
    public static partial class Functions
    {
        public static void ConfigureBuilder(IAppBuilder builder)
        {
            var forbidIfSignedIn = new MiddlewareFunc(next => env =>
            {
                var principal = env.Get<IPrincipal>("server.User");
                if (principal != null && principal.Identity.IsAuthenticated)
                {
                    env.SetStatusCode(403);
                    return env.GetCompleted();
                }

                return next(env);
            });

            builder.Use(ContentType.Create());

            builder.Use(Mustache.Create(
                templateRootDirectoryName: "MustacheTemplates",
                layoutTemplateName: "_layout"));

            builder.Use(DispatcherMiddleware.Create(dispatcher =>
            {
                dispatcher.Get("/", Handlers.GetHome);
                dispatcher.Post("/events/push", Handlers.PostPushEvent);
                dispatcher.Post("/events/force", Handlers.PostForceEvent);
                dispatcher.Get("/forms/sign-up", forbidIfSignedIn, Handlers.GetSignUpForm);
                dispatcher.Get("/{owner}/{name}", Handlers.GetBuilds);
                dispatcher.Get("/{owner}/{name}/builds/{id}", Handlers.GetBuild);
                dispatcher.Post("/{owner}/{name}/builds", Handlers.PostBuild);
            }));
        }
    }
}
