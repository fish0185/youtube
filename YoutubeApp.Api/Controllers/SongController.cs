using YoutubeApp.Api.Model;
using System.Threading.Tasks;
using JustSaying;
using Microsoft.AspNetCore.Mvc;
using YoutubeApp.Domain;

namespace YoutubeApp.Api.Controllers
{
    

    [Route("api/[controller]")]
    public class SongController : Controller
    {
        private readonly IHaveFulfilledPublishRequirements _publisher;

        public SongController(IHaveFulfilledPublishRequirements publisher)
        {
            _publisher = publisher;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateSongRequest request)
        {
            if (request.Url == null || !request.Url.Authority.Contains("youtube.com"))
            {
                return this.BadRequest("Uri is invalid");
            }

            await _publisher.PublishAsync(new CreateSongCommand
                                              {
                                                  Url = request.Url.ToString(),
                                                  Email = request.Email
            });

            return this.Ok();
        }
    }
}