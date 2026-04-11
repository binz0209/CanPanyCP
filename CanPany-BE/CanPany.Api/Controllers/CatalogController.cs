using CanPany.Application.Interfaces;
using CanPany.Domain.Entities;
using CanPany.Shared.Common;
using CanPany.Shared.Common.I18n;
using Microsoft.AspNetCore.Mvc;

namespace CanPany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly ISkillService _skillService;
    private readonly ICategoryService _categoryService;
    private readonly ILocationService _locationService;
    private readonly IExperienceLevelService _experienceLevelService;

    public CatalogController(
        ISkillService skillService,
        ICategoryService categoryService,
        ILocationService locationService,
        IExperienceLevelService experienceLevelService)
    {
        _skillService = skillService;
        _categoryService = categoryService;
        _locationService = locationService;
        _experienceLevelService = experienceLevelService;
    }

    [HttpGet("skills")]
    public async Task<IActionResult> GetSkills()
    {
        var skills = await _skillService.GetAllAsync();
        return Ok(ApiResponse.CreateSuccess(skills));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(ApiResponse.CreateSuccess(categories));
    }

    [HttpGet("locations")]
    public async Task<IActionResult> GetLocations()
    {
        var locations = await _locationService.GetAllAsync();
        return Ok(ApiResponse.CreateSuccess(locations));
    }

    [HttpGet("experience-levels")]
    public async Task<IActionResult> GetExperienceLevels()
    {
        var levels = await _experienceLevelService.GetAllAsync();
        return Ok(ApiResponse.CreateSuccess(levels));
    }
}
