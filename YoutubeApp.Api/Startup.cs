using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon;
using Amazon.Runtime;

using JustSaying;
using JustSaying.AwsTools;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YoutubeApp.Domain;

namespace YoutubeApp.Api
{
    public class Startup
    {
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            CreateMeABus.DefaultClientFactory = () =>
                new DefaultAwsClientFactory(new BasicAWSCredentials("AKIAJONLG4V3AOB42JJQ", "9v50jmdLXEQDCbeG/F4L5yKu5ouPQ0+3YoymwXP2"));

            var publisher = CreateMeABus.WithLogging(_loggerFactory)
                .InRegion(RegionEndpoint.APSoutheast2.SystemName)
                .WithNamingStrategy(
                    () => new Naming(Configuration["Env"]))
                .ConfigurePublisherWith(
                    c =>
                    {
                        c.PublishFailureReAttempts = 3;
                        c.PublishFailureBackoffMilliseconds = 50;
                    })
                .WithSnsMessagePublisher<CreateSongCommand>(
                    c =>
                    {
                        c.HandleException = (e) =>
                                             {
                                                    // log exception
                                                    return false;
                                             };
                    });

            services.AddSingleton(publisher);
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
