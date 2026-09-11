using KPW.Application.DTOs.Species;
using KPW.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KPW.Api.Controllers;

[ApiController]
[Route("species-breeds")]
[Route("api/species-breeds")]
[Authorize]
public class SpeciesBreedController : ControllerBase
{
    private readonly ISpeciesBreedConfigService _configService;
    private readonly ICurrentUserService _currentUserService;

    public SpeciesBreedController(
        ISpeciesBreedConfigService configService,
        ICurrentUserService currentUserService)
    {
        _configService = configService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<SpeciesBreedConfigDto>> GetConfig(CancellationToken cancellationToken)
    {
        var config = await _configService.GetConfigAsync(cancellationToken);
        return Ok(config);
    }

    [HttpPut]
    [Authorize(Roles = "SysAdmin,ClinicAdmin,Physiotherapist")]
    public async Task<ActionResult<SpeciesBreedConfigDto>> UpdateConfig(
        [FromBody] UpdateSpeciesBreedConfigRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.Species == null || request.Species.Count == 0)
        {
            return BadRequest(new { message = "At least one species must be configured." });
        }

        var modifiedBy = _currentUserService.Email ?? _currentUserService.UserId?.ToString() ?? "Admin";
        var result = await _configService.UpdateConfigAsync(request.Species, modifiedBy, cancellationToken);
        return Ok(result);
    }

    [HttpPost("reset")]
    [Authorize(Roles = "SysAdmin,ClinicAdmin,Physiotherapist")]
    public async Task<ActionResult<SpeciesBreedConfigDto>> ResetConfig(CancellationToken cancellationToken)
    {
        var modifiedBy = _currentUserService.Email ?? _currentUserService.UserId?.ToString() ?? "Admin";
        var result = await _configService.ResetToDefaultsAsync(modifiedBy, cancellationToken);
        return Ok(result);
    }
}
