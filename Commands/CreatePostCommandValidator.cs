using FluentValidation;

namespace MyFirstApi.Commands;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(c => c.Title).NotEmpty().MaximumLength(255);
        RuleFor(c => c.Content).NotEmpty().MaximumLength(1000);
    }
}
