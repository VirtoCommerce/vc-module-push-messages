using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;
using GeneralSettings = VirtoCommerce.PushMessages.Core.ModuleConstants.Settings.General;

namespace VirtoCommerce.PushMessages.Tests;

/// <summary>Returns a single batch of tokens, then empties so paging terminates.</summary>
internal sealed class FakeFcmTokenSearchService : IFcmTokenSearchService
{
    private readonly IList<FcmToken> _tokens;

    public FakeFcmTokenSearchService(IList<FcmToken> tokens)
    {
        _tokens = tokens;
    }

    public Task<FcmTokenSearchResult> SearchAsync(FcmTokenSearchCriteria criteria, bool clone = true)
    {
        return Task.FromResult(new FcmTokenSearchResult
        {
            TotalCount = _tokens.Count,
            Results = _tokens,
        });
    }
}

/// <summary>Records the ids passed to DeleteAsync so the test can assert pruning.</summary>
internal sealed class RecordingFcmTokenService : IFcmTokenService
{
    public List<string> DeletedIds { get; } = new();

    public Task DeleteAsync(IList<string> ids, bool softDelete = false)
    {
        DeletedIds.AddRange(ids);
        return Task.CompletedTask;
    }

    public Task<IList<FcmToken>> GetAsync(IList<string> ids, string responseGroup = null, bool clone = true)
    {
        return Task.FromResult<IList<FcmToken>>(new List<FcmToken>());
    }

    public Task SaveChangesAsync(IList<FcmToken> models)
    {
        return Task.CompletedTask;
    }
}

/// <summary>Resolves the BatchSize setting from its descriptor default (50).</summary>
internal sealed class FakeSettingsManager : ISettingsManager
{
    public Task<ObjectSettingEntry> GetObjectSettingAsync(string name, string objectType = null, string objectId = null)
    {
        return Task.FromResult(new ObjectSettingEntry(GeneralSettings.BatchSize));
    }

    public Task<IEnumerable<ObjectSettingEntry>> GetObjectSettingsAsync(IEnumerable<string> names, string objectType = null, string objectId = null)
    {
        return Task.FromResult<IEnumerable<ObjectSettingEntry>>(new List<ObjectSettingEntry>());
    }

    public Task SaveObjectSettingsAsync(IEnumerable<ObjectSettingEntry> objectSettings)
    {
        return Task.CompletedTask;
    }

    public Task RemoveObjectSettingsAsync(IEnumerable<ObjectSettingEntry> objectSettings)
    {
        return Task.CompletedTask;
    }

    // ISettingsRegistrar members — not exercised by these tests.
    public IEnumerable<SettingDescriptor> AllRegisteredSettings => new List<SettingDescriptor>();

    public void RegisterSettings(IEnumerable<SettingDescriptor> settings, string moduleId = null) { }

    public void RegisterSettingsForType(IEnumerable<SettingDescriptor> settings, string typeName) { }

    public IEnumerable<SettingDescriptor> GetSettingsForType(string typeName) => new List<SettingDescriptor>();

    public IDictionary<string, string[]> GetSettingTypeAssignments() =>
        new Dictionary<string, string[]>();
}
