using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAISearchScenarios.Services;
using static OpenAISearchScenarios.Services.SearchService;

namespace OpenAISearchScenarios.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly SearchService _searchService;
        
        public IndexModel(ILogger<IndexModel> logger, SearchService searchService)
        {
            _logger = logger;
            _searchService = searchService;
        }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public SearchResults? SearchResults { get; set; }

        public async Task OnGet()
        {
            if (!String.IsNullOrWhiteSpace(Search))
            {
                SearchResults = await _searchService.Search(Search);
            }
        }
    }
}