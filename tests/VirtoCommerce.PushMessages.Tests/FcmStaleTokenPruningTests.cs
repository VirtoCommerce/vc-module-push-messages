using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging.Abstractions;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;
using VirtoCommerce.PushMessages.Data.Handlers;
using Xunit;
using GeneralSettings = VirtoCommerce.PushMessages.Core.ModuleConstants.Settings.General;

namespace VirtoCommerce.PushMessages.Tests;

/// <summary>
/// VCST-5210 — when FCM rejects a registration token with a terminal per-token
/// error (Unregistered / InvalidArgument = HTTP 404, token no longer valid),
/// the handler must prune that token from storage so it is not retried forever.
/// Transient errors and successful sends must NOT delete any token.
/// </summary>
[Trait("Category", "Unit")]
public class FcmStaleTokenPruningTests
{
    [Fact]
    public async Task TerminalTokenError_DeletesOffendingToken_VCST5210()
    {
        // Arrange: three stored tokens; FCM rejects the middle one as Unregistered (404).
        var tokens = new[]
        {
            new FcmToken { Id = "id-ok", Token = "token-ok", UserId = "u1" },
            new FcmToken { Id = "id-dead", Token = "token-dead", UserId = "u1" },
            new FcmToken { Id = "id-also-ok", Token = "token-also-ok", UserId = "u1" },
        };

        var tokenService = new RecordingFcmTokenService();
        var handler = new TestableHandler(
            new FakeFcmTokenSearchService(tokens),
            tokenService,
            new FakeSettingsManager(),
            new[]
            {
                new FcmSendResult { IsSuccess = true },
                new FcmSendResult { IsSuccess = false, ErrorCode = MessagingErrorCode.Unregistered },
                new FcmSendResult { IsSuccess = true },
            });

        // Act
        await handler.SendMessageAsync(NewMessage(), Recipients("u1"));

        // Assert: exactly the dead token's id was deleted.
        Assert.Single(tokenService.DeletedIds);
        Assert.Equal("id-dead", tokenService.DeletedIds[0]);
    }

    [Fact]
    public async Task InvalidArgumentTokenError_DeletesOffendingToken_VCST5210()
    {
        var tokens = new[]
        {
            new FcmToken { Id = "id-bad", Token = "token-bad", UserId = "u1" },
        };

        var tokenService = new RecordingFcmTokenService();
        var handler = new TestableHandler(
            new FakeFcmTokenSearchService(tokens),
            tokenService,
            new FakeSettingsManager(),
            new[] { new FcmSendResult { IsSuccess = false, ErrorCode = MessagingErrorCode.InvalidArgument } });

        await handler.SendMessageAsync(NewMessage(), Recipients("u1"));

        Assert.Single(tokenService.DeletedIds);
        Assert.Equal("id-bad", tokenService.DeletedIds[0]);
    }

    [Fact]
    public async Task TransientTokenError_DoesNotDeleteToken_VCST5210()
    {
        var tokens = new[]
        {
            new FcmToken { Id = "id-transient", Token = "token-transient", UserId = "u1" },
        };

        var tokenService = new RecordingFcmTokenService();
        var handler = new TestableHandler(
            new FakeFcmTokenSearchService(tokens),
            tokenService,
            new FakeSettingsManager(),
            new[] { new FcmSendResult { IsSuccess = false, ErrorCode = MessagingErrorCode.Unavailable } });

        await handler.SendMessageAsync(NewMessage(), Recipients("u1"));

        Assert.Empty(tokenService.DeletedIds);
    }

    [Fact]
    public async Task AllTokensSucceed_DeletesNothing_VCST5210()
    {
        var tokens = new[]
        {
            new FcmToken { Id = "id-1", Token = "token-1", UserId = "u1" },
            new FcmToken { Id = "id-2", Token = "token-2", UserId = "u1" },
        };

        var tokenService = new RecordingFcmTokenService();
        var handler = new TestableHandler(
            new FakeFcmTokenSearchService(tokens),
            tokenService,
            new FakeSettingsManager(),
            new[]
            {
                new FcmSendResult { IsSuccess = true },
                new FcmSendResult { IsSuccess = true },
            });

        await handler.SendMessageAsync(NewMessage(), Recipients("u1"));

        Assert.Empty(tokenService.DeletedIds);
    }

    private static PushMessage NewMessage() => new() { Id = "msg-1", ShortMessage = "hello" };

    private static IList<PushMessageRecipient> Recipients(params string[] userIds) =>
        userIds.Select(x => new PushMessageRecipient { UserId = x }).ToList();

    /// <summary>
    /// Overrides the Firebase network seam so the test controls per-token outcomes
    /// without constructing the non-public FirebaseAdmin response types.
    /// </summary>
    private sealed class TestableHandler : FcmPushMessageRecipientChangedEventHandler
    {
        private readonly IList<FcmSendResult> _results;

        public TestableHandler(
            IFcmTokenSearchService fcmTokenSearchService,
            IFcmTokenService fcmTokenService,
            ISettingsManager settingsManager,
            IList<FcmSendResult> results)
            : base(null, fcmTokenSearchService, fcmTokenService, settingsManager, NullLogger<FcmPushMessageRecipientChangedEventHandler>.Instance)
        {
            _results = results;
        }

        protected override Task<IList<FcmSendResult>> SendEachForMulticastAsync(MulticastMessage firebaseMessage)
        {
            return Task.FromResult(_results);
        }
    }
}
