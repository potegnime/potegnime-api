using Microsoft.AspNetCore.Authorization;
using PotegniMe.DTOs.Error;
using PotegniMe.DTOs.Search;
using PotegniMe.DTOs.TorrentScrape;
using PotegniMe.Services.SearchService;

namespace PotegniMe.Controllers
{
    [Route("search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        // Fields
        private readonly ISearchService _searchService;
        private readonly IConfiguration _configuration;

        // Constructor
        public SearchController(ISearchService searhService, IConfiguration configuration)
        {
            _searchService = searhService;
            _configuration = configuration;
        }

        [HttpGet, Authorize]
        public async Task<ActionResult> Search([FromQuery] SearchRequestDto searchRequsetDto)
        {
            try
            {
                // Query is the only necceaary parameter
                if (searchRequsetDto.Query == null)
                {
                    return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "Izraz za iskanje je obvezen" });
                }

                // TODO
                // Cache the categories and providers to reduce the number of requests to the scraper PotegniMe

                // Check if provider exists
                if (searchRequsetDto.Source != null)
                {
                    IList<string> providers = await _searchService.GetProvidersAsync();
                    if (!providers.Contains(searchRequsetDto.Source))
                    {
                        return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "Neveljaven vnos vira" });
                    }

                    // Check if category for the provider exists
                    if (searchRequsetDto.Category != null)
                    {
                        IDictionary<string, List<string>> categories = await _searchService.GetCategoriesAsync();
                        List<string> providerCategories = categories[searchRequsetDto.Source];

                        if (!providerCategories.Contains(searchRequsetDto.Category))
                        {
                            return BadRequest(new ErrorResponseDto { ErrorCode = 1, Message = "Neveljaven vnos kategorije" });
                        }
                    }
                }

                ScrapedTorrentsResponseDto result = await _searchService.GetScrapedTorrentsAsync(searchRequsetDto);
                return Ok(result);
            }
            catch (TorrentScraperException)
            {
                return StatusCode(503, new ErrorResponseDto { ErrorCode = 1, Message = "Napaka pri iskanju torrentov iz drugih virov" });
            }
            catch (NotFoundException)
            {
                return StatusCode(404);
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet("categories"), Authorize]
        public async Task<ActionResult> GetCategories()
        {
            try
            {
                IDictionary<string, List<string>> categories = await _searchService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (TorrentScraperException)
            {
                return StatusCode(503, new ErrorResponseDto { ErrorCode = 1, Message = "Napaka pri iskanju torrentov iz drugih virov" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }

        [HttpGet("providers"), Authorize]
        public async Task<ActionResult> GetProviders()
        {
            try
            {
                IList<string> providers = await _searchService.GetProvidersAsync();
                return Ok(providers);
            }
            catch (TorrentScraperException)
            {
                return StatusCode(503, new ErrorResponseDto { ErrorCode = 1, Message = "Napaka pri iskanju torrentov iz drugih virov" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ErrorResponseDto { ErrorCode = 2, Message = e.Message });
            }
        }
    }
}
