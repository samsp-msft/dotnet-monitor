﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Diagnostics.Monitoring.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.UnitTestApp.Scenarios
{
    internal sealed class AspNetScenario
    {
        public static Command Command()
        {
            Command command = new(TestAppScenarios.AspNet.Name);
            command.SetHandler(ExecuteAsync);
            return command;
        }

        public static async Task ExecuteAsync(InvocationContext context)
        {
            context.ExitCode = await ScenarioHelpers.RunWebScenarioAsync<Startup>(async logger =>
            {
                await ScenarioHelpers.WaitForCommandAsync(TestAppScenarios.AspNet.Commands.Continue, logger);

                return 0;
            }, context.GetCancellationToken());
        }

        private sealed class Startup
        {
            public static void ConfigureServices(IServiceCollection services)
            {
                services.AddControllers();
            }

            public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                app.UseRouting();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/", Responses.Ok);
                    endpoints.MapGet("/privacy", Responses.Ok);
                    endpoints.MapGet("/slowresponse", Responses.SlowResponseAsync);
                });
            }

            public static class Responses
            {
                public static IResult Ok() => Results.Ok();

                public static async Task<IResult> SlowResponseAsync()
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));

                    return Results.Ok();
                }
            }
        }
    }
}
