using MediatR;
using MyFirstApi.Repositories;

namespace MyFirstApi.Commands;

public class DeletePostHandler(IPostRepository postRepository) : IRequestHandler<DeletePostCommand, Unit>
{
    public async Task<Unit> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        await postRepository.DeleteAsync(request.Id, cancellationToken);
        await postRepository.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
