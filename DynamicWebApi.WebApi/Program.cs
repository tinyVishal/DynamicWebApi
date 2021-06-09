/* Copyright Chetan N Mandhania */
using DynamicWebApi.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using static tiny.Extensions;
Host.CreateDefaultBuilder(args).ConfigureAppConfiguration((h, c) => { c.Build(); }).ConfigureTinyLogger().ConfigureWebHostDefaults(w => { w.UseStartup<Startup>(); w.UseIISIntegration(); }).Build().Run();
