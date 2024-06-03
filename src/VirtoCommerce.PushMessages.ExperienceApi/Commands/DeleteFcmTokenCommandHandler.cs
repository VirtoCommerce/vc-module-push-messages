using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands;

public class DeleteFcmTokenCommandHandler : IRequestHandler<DeleteFcmTokenCommand, bool>
{
    public Task<bool> Handle(DeleteFcmTokenCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
