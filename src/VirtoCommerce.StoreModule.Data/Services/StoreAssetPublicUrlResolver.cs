using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VirtoCommerce.StoreModule.Core;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.StoreModule.Data.Services;

/// <summary>
/// Default <see cref="IStoreAssetPublicUrlResolver"/>. When <see cref="Store.AssetPublicUrl"/> is set,
/// a relative URL is combined with it and an absolute URL is rebased onto it (preserving path, query
/// and fragment). When <see cref="StoreAssetsOptions.KnownAssetHosts"/> is not empty, only absolute
/// URLs whose host is listed there are rebased; other hosts are treated as external and returned as-is.
/// Registered as a singleton.
/// </summary>
public class StoreAssetPublicUrlResolver : IStoreAssetPublicUrlResolver
{
    // Hosts are compared case-insensitively (StringComparer.OrdinalIgnoreCase).
    private readonly HashSet<string> _knownAssetHosts;
    private readonly ILogger<StoreAssetPublicUrlResolver> _logger;

    public StoreAssetPublicUrlResolver(IOptions<StoreAssetsOptions> options, ILogger<StoreAssetPublicUrlResolver> logger)
    {
        _logger = logger;
        _knownAssetHosts = BuildKnownHosts(options.Value.KnownAssetHosts);
    }

    public virtual string GetAbsoluteUrl(Store store, string url)
    {
        ArgumentNullException.ThrowIfNull(store);

        // null/empty is returned as-is
        if (string.IsNullOrEmpty(url))
        {
            return url;
        }

        // No store override configured => unchanged behavior.
        var assetPublicUrl = store.AssetPublicUrl;
        if (string.IsNullOrEmpty(assetPublicUrl))
        {
            return url;
        }

        // data:/blob: are returned as-is
        if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        if (IsAbsolute(url, out var uri))
        {
            // When known hosts are configured, replace only listed hosts; other hosts are external.
            if (_knownAssetHosts.Count > 0 && !_knownAssetHosts.Contains(uri.Host))
            {
                return url;
            }

            return RebaseHost(assetPublicUrl, url);
        }

        return CombineUrl(assetPublicUrl, url);
    }

    /// <summary>
    /// Combines the store asset base URL (a full URL or bare host, optionally with a base path)
    /// with a relative asset path.
    /// </summary>
    protected virtual string CombineUrl(string assetPublicUrl, string relativeUrl)
    {
        return $"{NormalizeBaseUrl(assetPublicUrl).TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
    }

    /// <summary>
    /// Replaces scheme/host/port of <paramref name="absoluteUrl"/> with those of
    /// <paramref name="baseUrl"/> (a full URL or bare host, optionally with a base path),
    /// preserving the original path, query and fragment.
    /// On parse failure, logs a warning and returns <paramref name="absoluteUrl"/> unchanged.
    /// </summary>
    protected virtual string RebaseHost(string assetPublicUrl, string absoluteUrl)
    {
        try
        {
            if (!IsAbsolute(absoluteUrl, out var source) ||
                !Uri.TryCreate(NormalizeBaseUrl(assetPublicUrl), UriKind.Absolute, out var baseUri))
            {
                _logger.LogWarning(
                    "Cannot rebase asset URL '{AssetUrl}' onto the store asset public URL '{AssetPublicUrl}': the base URL is not a valid absolute URL. The original URL is returned as-is.",
                    absoluteUrl, assetPublicUrl);
                return absoluteUrl;
            }

            var basePath = baseUri.AbsolutePath.TrimEnd('/');
            var combinedPath = basePath + source.AbsolutePath;

            var builder = new UriBuilder
            {
                Scheme = baseUri.Scheme,
                Host = baseUri.Host,
                Port = baseUri.IsDefaultPort ? -1 : baseUri.Port,
                Path = combinedPath,
                Query = source.Query.TrimStart('?'),
                Fragment = source.Fragment.TrimStart('#'),
            };

            return builder.Uri.AbsoluteUri;
        }
        catch (UriFormatException ex)
        {
            _logger.LogWarning(ex,
                "Failed to rebase asset URL '{AssetUrl}' onto the store asset public URL '{AssetPublicUrl}'. The original URL is returned as-is.",
                absoluteUrl, assetPublicUrl);
            return absoluteUrl;
        }
    }

    private HashSet<string> BuildKnownHosts(IEnumerable<string> knownAssetHosts)
    {
        var hosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in (knownAssetHosts ?? []).Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            // Accept a bare host/domain or a full URL.
            if (Uri.TryCreate(NormalizeBaseUrl(entry.Trim()), UriKind.Absolute, out var uri))
            {
                hosts.Add(uri.Host);
            }
            else
            {
                _logger.LogWarning(
                    "Invalid host entry '{KnownAssetHost}' in the {ConfigSection} configuration section. The entry is ignored; expected a bare host (cdn.example.com) or an absolute URL.",
                    entry, $"{StoreAssetsOptions.SectionName}:{nameof(StoreAssetsOptions.KnownAssetHosts)}");
            }
        }

        return hosts;
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        return baseUrl.Contains("://", StringComparison.Ordinal) ? baseUrl : $"https://{baseUrl}";
    }

    private static bool IsAbsolute(string value, out Uri uri)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
