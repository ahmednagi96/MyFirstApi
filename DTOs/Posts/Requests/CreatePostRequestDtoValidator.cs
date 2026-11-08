using FluentValidation;

namespace MyFirstApi.DTOs.Posts.Requests;

public class CreatePostRequestDtoValidator : AbstractValidator<CreatePostRequestDto>
{
    public CreatePostRequestDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .Length(2, 255);

        RuleFor(x => x.Content)
            .NotEmpty()
            .Length(2, 1000);
    }
}
