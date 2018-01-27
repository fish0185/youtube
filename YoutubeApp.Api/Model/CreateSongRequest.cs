using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YoutubeApp.Api.Model
{
    public class CreateSongRequest
    {
        public Uri Url { get; set; }
        public string Email { get; set; }
    }
}
