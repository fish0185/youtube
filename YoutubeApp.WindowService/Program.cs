using System;

using Amazon;

using JustSaying;

using Microsoft.Extensions.Logging.Abstractions;

using YoutubeApp.Domain;

namespace YoutubeApp.WindowService
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateMeABus.WithLogging(new NullLoggerFactory())
                .InRegion(RegionEndpoint.APSoutheast2.SystemName)
                .WithSqsTopicSubscriber()
                .IntoQueue("CreateSongs")
                .WithMessageHandler<CreateSongCommand>(new CreateSongCommandHandler())
                .StartListening();
            Console.WriteLine("youtube processor started....");
            Console.ReadLine();
        }
    }
}
