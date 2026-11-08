using MediatR;

namespace MyFirstApi.Commands;

public record DeletePostCommand(int Id) : IRequest<Unit>;
