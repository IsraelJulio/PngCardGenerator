using Microsoft.AspNetCore.Mvc;
using PngCardGenerator.Api.Models;
using PngCardGenerator.Api.Services;

namespace PngCardGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplatesController : ControllerBase
{
    private readonly CardTemplateService _cardTemplateService;

    public TemplatesController(CardTemplateService cardTemplateService)
    {
        _cardTemplateService = cardTemplateService;
    }

    [HttpPost]
    [RequestSizeLimit(30_000_000)]
    public async Task<ActionResult<CardTemplateDetailsDto>> Create([FromBody] CardTemplateUpsertRequest request, CancellationToken cancellationToken)
    {
        var validation = Validate(request);
        if (validation is not null)
            return validation;

        var created = await _cardTemplateService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<ActionResult<List<CardTemplateSummaryDto>>> GetAll(CancellationToken cancellationToken)
    {
        return await _cardTemplateService.ListAsync(cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CardTemplateDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var template = await _cardTemplateService.GetAsync(id, cancellationToken);
        return template is null ? NotFound() : Ok(template);
    }

    [HttpPut("{id:guid}")]
    [RequestSizeLimit(30_000_000)]
    public async Task<ActionResult<CardTemplateDetailsDto>> Update(Guid id, [FromBody] CardTemplateUpsertRequest request, CancellationToken cancellationToken)
    {
        var validation = Validate(request);
        if (validation is not null)
            return validation;

        var updated = await _cardTemplateService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _cardTemplateService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private static ActionResult? Validate(CardTemplateUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return new BadRequestObjectResult("Template name is required.");

        if (request.Width <= 0 || request.Height <= 0)
            return new BadRequestObjectResult("Width and Height must be greater than zero.");

        if (request.Width > 2500 || request.Height > 3500)
            return new BadRequestObjectResult("The requested image is too large.");

        if (request.Layers.Count == 0)
            return new BadRequestObjectResult("At least one layer is required.");

        return null;
    }
}
