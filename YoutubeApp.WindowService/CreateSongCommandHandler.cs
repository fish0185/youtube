using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using JustSaying.Messaging.MessageHandling;

using YoutubeApp.Domain;

namespace YoutubeApp.WindowService
{
    public class CreateSongCommandHandler : IHandlerAsync<CreateSongCommand>
    {
        public async Task<bool> Handle(CreateSongCommand message)
        {
            Console.WriteLine(message.Url);
            return true;
        }
    }
}
