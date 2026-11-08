مرجع الشامل والدليل السريع لـ IRequest و MediatR و CQRS1. المفاهيم الأساسية: الفرق بين النمط القديم والنمط الحديثلماذا انتقلنا من الـ Services التقليدية إلى IRequest؟النمط القديم (Traditional N-Tier):كان يتم جمع كافة الأساليب والخدمات الخاصة بالكيان (مثل Create, Update, GetById, Delete, ResetPassword) داخل كلاس واحد كبير يُدعى UserService أو ProductService.المشكلة: كبر حجم الملف مع الوقت (يتجاوز ألف سطر)، صعوبة التعديل، وتضخم الـ Constructor بالاعتمادات (Dependencies).النمط الحديث (Vertical Slice Architecture مع IRequest):يتم فصل كل عملية (Feature) في ملف ومُنفذ (Handler) مستقل تماماً عن باقي العمليات.الميزة: كود أسهل في القراءة، عزْل التغييرات لمنع كسر أي ميزة أخرى، وسهولة الاختبار (Unit Testing).2. جدول الفروق الجوهرية (المصطلحات والمفاهيم)المفهومالمضمون والدورالسلوك والمخرجات المتبعة في الشركاتDTOحاوية بيانات صامتة: تنقل البيانات بين Frontend و Backend عبر الشبكة.لا تمتلك أي Business Logic، وتُستخدم للمدخلات أو المخرجات.Commandأمر تغيير: طلب تعديل بيانات (INSERT, UPDATE, DELETE).ترجع 201 Created مع الـ ID أو الـ DTO عند الإنشاء، وتُرجع 204 No Content عند التعديل والمسح.Queryطلب استعلام: قراءة بيانات فقط دون أي تغيير في قواعد البيانات.تُرجع دائماً 200 OK محمّلة بـ DTO البيانات، وتُنفذ باستخدام AsNoTracking() للسرعة.IRequest<T>واجهة MediatR للـ Request: تُحول الكلاس إلى طلب رسمياً ليُنفذه Handler واحد.تُطبق نمط One-to-One (طلب واحد ينتظر إجابة واحدة).INotificationواجهة الأحداث (Events): إشعار النظام بوقوع حدث جانبي.تُطبق نمط One-to-Many (حدث يُنفذه أكثر من Handler بدون انتظار رد مباشر).3. الهيكل المعتمد لتنظيم الملفات (Vertical Slice Structure)تُفضل معظم الشركات تنظيم الملفات حسب الميزة (Feature) داخل مشروع الـ Application Layer:Plaintextsrc/
└── Application/
    └── Features/
        └── Products/
            ├── Commands/
            │   └── CreateProduct/
            │       ├── CreateProductCommand.cs      // يحوي IRequest و Response DTO
            │       └── CreateProductHandler.cs      // يحوي اللوجيك المباشر
            └── Queries/
                └── GetProductById/
                    ├── GetProductByIdQuery.cs      // يحوي IRequest الاستعلام
                    ├── GetProductByIdHandler.cs    // يحوي كود القراءة السريع
                    └── ProductDetailsDto.cs         // كلاس الـ DTO الخاص بالنتيجة
4. الأمثلة البرمجية المتكاملة (Clean Architecture + Repository Pattern)أولاً: كود الـ Command (إنشاء منتج - CreateProduct)1. الـ Command والـ Response DTOC#using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

// الـ Command يعمل كـ DTO للمدخلات ويحمل الواجهة IRequest
public record CreateProductCommand(string Name, decimal Price, int CategoryId) 
    : IRequest<ProductCreatedDto>;

// الـ DTO المخصص للنتيجة التي ستعود للفرونت إند
public record ProductCreatedDto(int Id, string Name, decimal Price);
2. الـ Handler (باستخدام الـ Repository)C#using MediatR;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Features.Products.Commands.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductCreatedDto>
{
    private readonly IProductRepository _productRepository;

    public CreateProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductCreatedDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            CategoryId = request.CategoryId
        };

        await _productRepository.AddAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);

        return new ProductCreatedDto(product.Id, product.Name, product.Price);
    }
}
ثانياً: كود الـ Query (استعلام قراءة - GetProductById)1. الـ Query والـ Response DTOC#using MediatR;

namespace Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(int ProductId) : IRequest<ProductDetailsDto>;

public record ProductDetailsDto(int Id, string Name, decimal Price, string CategoryName);
2. الـ Handler (قراءة سريعة)C#using MediatR;
using Microsoft.EntityFrameworkCore;
using Application.Common.Interfaces;

namespace Application.Features.Products.Queries.GetProductById;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDetailsDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        // استخدام AsNoTracking مع Select المباشر لأعلى أداء
        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == request.ProductId)
            .Select(p => new ProductDetailsDto(p.Id, p.Name, p.Price, p.Category.Name))
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null) 
            throw new KeyNotFoundException("المنتج غير موجود");

        return product;
    }
}
ثالثاً: كود الـ Controller (النظيف)C#using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Queries.GetProductById;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator) => _mediator = mediator;

    // Command Endpoint (201 Created + Data)
    [HttpPost]
    public async Task<ActionResult<ProductCreatedDto>> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // Query Endpoint (200 OK + Data)
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailsDto>> GetById(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        return Ok(result);
    }
}
5. قواعد ذهبية للاستخدام في المشاريعلا تُرجع الـ Entity الخاصة بالداتابيز أبداً: قم دائماً بتحويل النتائج إلى DTO قبل إرجاعها من الـ Handler.الـ Command لا ينبغي أن يحتوي على قراءة مطولة: افصل عمليات الاستعلام الكبيرة في Query مستقل.التسجيل الآلي في Program.cs: يُسجل MediatR كلمة واحدة للوصول لكل الـ Handlers في المشروع:C#builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(IApplicationDbContext).Assembly));
