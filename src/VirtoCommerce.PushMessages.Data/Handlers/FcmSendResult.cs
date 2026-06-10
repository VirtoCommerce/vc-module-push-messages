using FirebaseAdmin.Messaging;

namespace VirtoCommerce.PushMessages.Data.Handlers;

/// <summary>
/// Per-token outcome of a Firebase multicast send, aligned by index with the
/// tokens passed to <see cref="FirebaseMessaging.SendEachForMulticastAsync(MulticastMessage)"/>.
/// Decouples the dead-token pruning logic from the FirebaseAdmin response types
/// (<c>BatchResponse</c>/<c>SendResponse</c> have no public constructors), so the
/// behavior is unit-testable via a seam.
/// </summary>
public class FcmSendResult
{
    public bool IsSuccess { get; set; }

    public MessagingErrorCode? ErrorCode { get; set; }
}
