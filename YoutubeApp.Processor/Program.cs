using System;
using System.Configuration;
using Amazon;
using Autofac;
using JustSaying;
using JustSaying.Messaging.MessageHandling;
using Microsoft.Extensions.Logging.Abstractions;
using YoutubeApp.Domain;

namespace YoutubeApp.Processor
{
    class Program
    {
        static void Main(string[] args)
        {
            var contianerBuilder = new ContainerBuilder();
            contianerBuilder.RegisterType<CreateSongCommandHandler>().AsImplementedInterfaces();
            var container = contianerBuilder.Build();

            CreateMeABus.WithLogging(new NullLoggerFactory())
                .InRegion(RegionEndpoint.APSoutheast2.SystemName)
                .WithNamingStrategy(
                    () => new Naming(ConfigurationManager.AppSettings["Env"]))
                .WithSqsTopicSubscriber()
                .IntoQueue("CreateSongs")
                .ConfigureSubscriptionWith(
                    c =>
                        {
                            c.MessageRetentionSeconds = 300;
                            c.VisibilityTimeoutSeconds = 180;
                        })
                .WithMessageHandler<CreateSongCommand>(new HandlerResolver(container))
                .StartListening();
            Console.WriteLine("Youtube processor started...");
            Console.ReadLine();
        }
    }

    public class HandlerResolver : IHandlerResolver
    {
        private readonly IContainer _container;

        public HandlerResolver(IContainer container)
        {
            _container = container;
        }

        public IHandlerAsync<CreateSongCommand> ResolveHandler(HandlerResolutionContext context)
        {
            return new CreateSongCommandHandler();
        }

        public IHandlerAsync<T> ResolveHandler<T>(HandlerResolutionContext context)
        {
            return _container.Resolve<IHandlerAsync<T>>();
        }
    }
}