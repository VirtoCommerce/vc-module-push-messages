using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands;

public class AddFcmTokenCommandHandler : IRequestHandler<AddFcmTokenCommand, bool>
{
    public Task<bool> Handle(AddFcmTokenCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
