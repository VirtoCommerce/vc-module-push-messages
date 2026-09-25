using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
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

/// <summary>Serves members by id. Ids not in the map are simply absent from the result.</summary>
internal sealed class FakeMemberService : IMemberService
{
    private readonly IDictionary<string, Member> _members;

    public FakeMemberService(IDictionary<string, Member> members)
    {
        _members = members;
    }

    public Task<Member[]> GetByIdsAsync(string[] memberIds, string responseGroup = null, string[] memberTypes = null)
    {
        var result = memberIds
            .Where(_members.ContainsKey)
            .Select(x => _members[x])
            .ToArray();

        return Task.FromResult(result);
    }

    public Task<Member> GetByIdAsync(string memberId, string responseGroup = null, string memberType = null)
    {
        return Task.FromResult(_members.TryGetValue(memberId, out var member) ? member : null);
    }

    public Task SaveChangesAsync(Member[] members) => Task.CompletedTask;

    public Task DeleteAsync(string[] ids, string[] memberTypes = null) => Task.CompletedTask;
}

/// <summary>
/// Answers the two searches the audience funnel makes: by MemberId (children of an organization)
/// and by Keyword (the audience query). Returns everything in a single batch.
/// </summary>
internal sealed class FakeMemberSearchService : IMemberSearchService
{
    private readonly IDictionary<string, IList<Member>> _childrenByParentId;
    private readonly IDictionary<string, IList<Member>> _membersByKeyword;

    public FakeMemberSearchService(
        IDictionary<string, IList<Member>> childrenByParentId = null,
        IDictionary<string, IList<Member>> membersByKeyword = null)
    {
        _childrenByParentId = childrenByParentId ?? new Dictionary<string, IList<Member>>();
        _membersByKeyword = membersByKeyword ?? new Dictionary<string, IList<Member>>();
    }

    public Task<MemberSearchResult> SearchMembersAsync(MembersSearchCriteria criteria)
    {
        IList<Member> found = [];

        if (!string.IsNullOrEmpty(criteria.MemberId) && _childrenByParentId.TryGetValue(criteria.MemberId, out var children))
        {
            found = children;
        }
        else if (!string.IsNullOrEmpty(criteria.Keyword) && _membersByKeyword.TryGetValue(criteria.Keyword, out var matched))
        {
            found = matched;
        }

        return Task.FromResult(new MemberSearchResult
        {
            TotalCount = found.Count,
            Results = found,
        });
    }

    public Task<IList<Member>> SearchAllAsync(MembersSearchCriteria criteria)
    {
        return Task.FromResult<IList<Member>>([]);
    }
}

/// <summary>Records the criteria it was handed and returns a canned result.</summary>
internal sealed class RecordingAudienceService : IPushMessageAudienceService
{
    private readonly PushMessageAudienceResult _result;

    public RecordingAudienceService(PushMessageAudienceResult result)
    {
        _result = result;
    }

    public PushMessageAudienceCriteria LastCriteria { get; private set; }

    public Task<PushMessageAudienceResult> ResolveAsync(
        PushMessageAudienceCriteria criteria,
        string messageId = null,
        ISet<string> excludedUserIds = null)
    {
        LastCriteria = criteria;

        return Task.FromResult(_result);
    }
}
