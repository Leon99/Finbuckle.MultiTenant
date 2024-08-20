using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BasePathStrategySample.Pages;

public class IndexModel : PageModel
{
    readonly ILogger<IndexModel> _logger;

    public TenantInfo? TenantInfo { get; private set; }

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }
    
    public void OnGet()
    {
        TenantInfo = HttpContext.GetMultiTenantContext<TenantInfo>()?.TenantInfo;
    }
}