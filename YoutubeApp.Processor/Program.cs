using System;
using Amazon;
using JustSaying;
using Microsoft.Extensions.Logging.Abstractions;
using YoutubeApp.Domain;

namespace YoutubeApp.Processor
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateMeABus.WithLogging(new NullLoggerFactory())
                .InRegion(RegionEndpoint.APSoutheast2.SystemName)
                .WithSqsTopicSubscriber()
                .IntoQueue("CreateSongs.Live")
                .ConfigureSubscriptionWith(
                    c =>
                        {
                            c.MessageRetentionSeconds = 300;
                            c.VisibilityTimeoutSeconds = 180;
                        })
                .WithMessageHandler<CreateSongCommand>(new CreateSongCommandHandler())
                .StartListening();
            Console.WriteLine("Youtube processor started...");
            Console.ReadLine();
        }
    }
}