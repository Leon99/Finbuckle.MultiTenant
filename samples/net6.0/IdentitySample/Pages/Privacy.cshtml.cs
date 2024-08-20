﻿using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentitySample.Pages;

public class PrivacyModel : PageModel
{
    readonly ILogger<PrivacyModel> _logger;

    public PrivacyModel(ILogger<PrivacyModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}