using Microsoft.AspNetCore.Mvc;
using PngCardGenerator.Api.Models;
using PngCardGenerator.Api.Services;

namespace PngCardGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly CardRendererService _cardRendererService;
    private readonly CardTemplateService _cardTemplateService;

    public CardsController(CardRendererService cardRendererService, CardTemplateService cardTemplateService)
    {
        _cardRendererService = cardRendererService;
        _cardTemplateService = cardTemplateService;
    }

    [HttpPost("render")]
    [RequestSizeLimit(30_000_000)]
    public async Task<IActionResult> Render([FromBody] CardRenderRequest request, CancellationToken cancellationToken)
    {
        if (request.Width <= 0 || request.Height <= 0)
            return BadRequest("Width and Height must be greater than zero.");

        if (request.Width > 2500 || request.Height > 3500)
            return BadRequest("The requested image is too large.");

        if (request.Layers.Count == 0)
            return BadRequest("At least one layer is required.");

        var bytes = await _cardRendererService.RenderAsync(request, cancellationToken);
        return File(bytes, "image/png", $"card-{DateTime.UtcNow:yyyyMMddHHmmss}.png");
    }

    [HttpPost("render-from-template/{templateId:guid}")]
    public async Task<IActionResult> RenderFromTemplate(Guid templateId, CancellationToken cancellationToken)
    {
        var request = await _cardTemplateService.BuildRenderRequestAsync(templateId, cancellationToken);
        if (request is null)
            return NotFound("Template not found.");

        var bytes = await _cardRendererService.RenderAsync(request, cancellationToken);
        await _cardTemplateService.RegisterGeneratedCardAsync(templateId, $"template-{templateId}", request, cancellationToken);
        return File(bytes, "image/png", $"card-template-{templateId:N}-{DateTime.UtcNow:yyyyMMddHHmmss}.png");
    }

    [HttpGet("template/classic")]
    public IActionResult ClassicTemplate()
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "classic-template.json");

        if (!System.IO.File.Exists(templatePath))
            return NotFound("Template not found.");

        return PhysicalFile(templatePath, "application/json");
    }
}
