// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Internal;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Strategies;

public class HostStrategy : IMultiTenantStrategy
{
    readonly string regex;

    public HostStrategy(string template)
    {
        // New in 2.1, match whole domain if just "__tenant__".
        if (template == Constants.TenantToken)
        {
            template = template.Replace(Constants.TenantToken, "(?<key>.+)");
        }
        else
        {
            // Check for valid template.
            // Template cannot be null or whitespace.
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new MultiTenantException("Template cannot be null or whitespace.");
            }

            // Wildcard "*" must be only occur once in template.
            if (Regex.Match(template, @"\*(?=.*\*)").Success)
            {
                throw new MultiTenantException("Wildcard \"*\" must be only occur once in template.");
            }

            // Wildcard "*" must be only token in template segment.
            if (Regex.Match(template, @"\*[^\.]|[^\.]\*").Success)
            {
                throw new MultiTenantException("\"*\" wildcard must be only token in template segment.");
            }

            // Wildcard "?" must be only token in template segment.
            if (Regex.Match(template, @"\?[^\.]|[^\.]\?").Success)
            {
                throw new MultiTenantException("\"?\" wildcard must be only token in template segment.");
            }

            template = template.Trim().Replace(".", @"\.");
            string wildcardSegmentsPattern = @"(\.[^\.]+)*";
            string singleSegmentPattern = @"[^\.]+";
            if (template.Substring(template.Length - 3, 3) == @"\.*")
            {
                template = template.Substring(0, template.Length - 3) + wildcardSegmentsPattern;
            }

            wildcardSegmentsPattern = @"([^\.]+\.)*";
            template = template.Replace(@"*\.", wildcardSegmentsPattern);
            template = template.Replace("?", singleSegmentPattern);
            template = template.Replace(Constants.TenantToken, @"(?<key>[^\.]+)");
        }

        this.regex = $"^{template}$";
    }

    /// <inheritdoc />
    public Task<string> GetKeyAsync(object context)
    {
        if (context is not HttpContext httpContext)
            throw new MultiTenantException(null,
                new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

        var host = httpContext.Request.Host;

        if (host.HasValue == false)
            return Task.FromResult<string?>(null);

        string? key = null;

        var match = Regex.Match(host.Host, regex,
            RegexOptions.ExplicitCapture,
            TimeSpan.FromMilliseconds(100));

        if (match.Success)
        {
            key = match.Groups["key"].Value;
        }

        return Task.FromResult(key);
    }
}