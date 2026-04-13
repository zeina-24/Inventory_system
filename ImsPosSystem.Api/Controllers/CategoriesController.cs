using ImsPosSystem.Application.DTOs.Catalogue;
using ImsPosSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace ImsPosSystem.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ISubcategoryService _subcategoryService;

    public CategoriesController(ICategoryService categoryService, ISubcategoryService subcategoryService)
    {
        _categoryService    = categoryService;
        _subcategoryService = subcategoryService;
    }

    // ── Categories ────────────────────────────────────────────────────────────

    /// <summary>Get all categories</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        => Ok(await _categoryService.GetAllCategoriesAsync());

    /// <summary>Get a single category by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Create a new category</summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetCategory), new { id = result.CategoryId }, result);
    }

    /// <summary>Update an existing category</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        => Ok(await _categoryService.UpdateCategoryAsync(id, dto));

    /// <summary>Delete a category (blocked if it has linked data)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return NoContent();
    }

    // ── Subcategories (nested under categories) ────────────────────────────────

    /// <summary>Get all subcategories for a category</summary>
    [HttpGet("{categoryId:int}/subcategories")]
    public async Task<ActionResult<IEnumerable<SubcategoryDto>>> GetSubcategories(int categoryId)
        => Ok(await _subcategoryService.GetSubcategoriesByCategoryAsync(categoryId));
}

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SubcategoriesController : ControllerBase
{
    private readonly ISubcategoryService _service;

    public SubcategoriesController(ISubcategoryService service) => _service = service;

    /// <summary>Get all subcategories</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubcategoryDto>>> GetAll()
        => Ok(await _service.GetAllSubcategoriesAsync());

    /// <summary>Get a subcategory by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SubcategoryDto>> GetById(int id)
    {
        var result = await _service.GetSubcategoryByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Create a subcategory</summary>
    [HttpPost]
    public async Task<ActionResult<SubcategoryDto>> Create([FromBody] CreateSubcategoryDto dto)
    {
        var result = await _service.CreateSubcategoryAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.SubcategoryId }, result);
    }

    /// <summary>Update a subcategory</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<SubcategoryDto>> Update(int id, [FromBody] UpdateSubcategoryDto dto)
        => Ok(await _service.UpdateSubcategoryAsync(id, dto));

    /// <summary>Delete a subcategory (blocked if linked to products)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteSubcategoryAsync(id);
        return NoContent();
    }
}
