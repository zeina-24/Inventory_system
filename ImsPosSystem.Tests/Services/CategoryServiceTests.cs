using AutoMapper;
using ImsPosSystem.Application.DTOs.Catalogue;
using ImsPosSystem.Application.Interfaces;
using ImsPosSystem.Application.Mappings;
using ImsPosSystem.Domain.Entities;
using ImsPosSystem.Infrastructure.Services;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;

namespace ImsPosSystem.Tests.Services;

/// <summary>
/// Unit tests for <see cref="CategoryService"/>.
/// All dependencies (IUnitOfWork, IGenericRepository) are mocked with Moq,
/// so these tests run without any database connection.
/// </summary>
[TestFixture]
public class CategoryServiceTests
{
    // ── SUT & collaborator mocks ────────────────────────────────────────────
    private Mock<IUnitOfWork> _uowMock;
    private Mock<IGenericRepository<Category>> _categoryRepoMock;
    private Mock<IGenericRepository<Subcategory>> _subcategoryRepoMock;
    private Mock<IGenericRepository<Product>> _productRepoMock;
    private IMapper _mapper;
    private CategoryService _sut;

    [SetUp]
    public void SetUp()
    {
        _uowMock             = new Mock<IUnitOfWork>();
        _categoryRepoMock    = new Mock<IGenericRepository<Category>>();
        _subcategoryRepoMock = new Mock<IGenericRepository<Subcategory>>();
        _productRepoMock     = new Mock<IGenericRepository<Product>>();

        // Wire the UoW mock to return our repo mocks
        _uowMock.Setup(u => u.Categories).Returns(_categoryRepoMock.Object);
        _uowMock.Setup(u => u.Subcategories).Returns(_subcategoryRepoMock.Object);
        _uowMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        // Build a real AutoMapper using the production MappingProfile
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _sut = new CategoryService(_uowMock.Object, _mapper);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GetAllCategoriesAsync
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GetAllCategoriesAsync_ReturnsAllMappedDtos()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { CategoryId = 1, CategoryCode = "ELEC", CategoryName = "Electronics",  IsActive = true,  CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { CategoryId = 2, CategoryCode = "FURN", CategoryName = "Furniture",    IsActive = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _categoryRepoMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(categories);

        // Act
        var result = (await _sut.GetAllCategoriesAsync()).ToList();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].CategoryCode, Is.EqualTo("ELEC"));
        Assert.That(result[1].CategoryName, Is.EqualTo("Furniture"));
        Assert.That(result[1].IsActive, Is.False);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GetCategoryByIdAsync
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public async Task GetCategoryByIdAsync_ExistingId_ReturnsMappedDto()
    {
        // Arrange
        var category = new Category
        {
            CategoryId   = 42,
            CategoryCode = "HW",
            CategoryName = "Hardware",
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        };

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(42))
            .ReturnsAsync(category);

        // Act
        var result = await _sut.GetCategoryByIdAsync(42);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CategoryId, Is.EqualTo(42));
        Assert.That(result.CategoryCode, Is.EqualTo("HW"));
    }

    [Test]
    public async Task GetCategoryByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _sut.GetCategoryByIdAsync(9999);

        // Assert
        Assert.That(result, Is.Null);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  CreateCategoryAsync — guard clauses
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public void CreateCategoryAsync_DuplicateCategoryCode_ThrowsInvalidOperationException()
    {
        // Arrange — code already exists
        _categoryRepoMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true); // first call = code check → true means duplicate

        var dto = new CreateCategoryDto("ELEC", "Electronics New", true);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sut.CreateCategoryAsync(dto));
    }

    [Test]
    public void CreateCategoryAsync_DuplicateCategoryName_ThrowsInvalidOperationException()
    {
        // Arrange — code is unique, but name is a duplicate
        var callCount = 0;
        _categoryRepoMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount > 1; // first call (code) → false, second call (name) → true
            });

        var dto = new CreateCategoryDto("NEW01", "Electronics", true);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sut.CreateCategoryAsync(dto));
    }

    [Test]
    public async Task CreateCategoryAsync_ValidDto_ReturnsNewCategoryDto()
    {
        // Arrange — no duplicates
        _categoryRepoMock
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>()))
            .ReturnsAsync(false);

        _categoryRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);

        _uowMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var dto = new CreateCategoryDto("SPORT", "Sports", true);

        // Act
        var result = await _sut.CreateCategoryAsync(dto);

        // Assert
        Assert.That(result.CategoryCode, Is.EqualTo("SPORT"));
        Assert.That(result.CategoryName, Is.EqualTo("Sports"));
        Assert.That(result.IsActive, Is.True);

        // Verify the entity was actually added and saved
        _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  DeleteCategoryAsync — referential-integrity guards
    // ════════════════════════════════════════════════════════════════════════

    [Test]
    public void DeleteCategoryAsync_HasLinkedSubcategories_ThrowsInvalidOperationException()
    {
        // Arrange
        var category = new Category { CategoryId = 1, CategoryCode = "ELEC", CategoryName = "Electronics", IsActive = true };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);
        _subcategoryRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Subcategory, bool>>>())).ReturnsAsync(true);
        _productRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(false);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sut.DeleteCategoryAsync(1));
    }

    [Test]
    public void DeleteCategoryAsync_HasLinkedProducts_ThrowsInvalidOperationException()
    {
        // Arrange
        var category = new Category { CategoryId = 2, CategoryCode = "FURN", CategoryName = "Furniture", IsActive = true };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(category);
        _subcategoryRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Subcategory, bool>>>())).ReturnsAsync(false);
        _productRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(true);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sut.DeleteCategoryAsync(2));
    }

    [Test]
    public void DeleteCategoryAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _sut.DeleteCategoryAsync(999));
    }

    [Test]
    public async Task DeleteCategoryAsync_NoLinkedEntities_RemovesAndSaves()
    {
        // Arrange — orphan category, safe to delete
        var category = new Category { CategoryId = 5, CategoryCode = "MISC", CategoryName = "Miscellaneous", IsActive = true };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(category);
        _subcategoryRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Subcategory, bool>>>())).ReturnsAsync(false);
        _productRepoMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(false);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act — should complete without throwing
        await _sut.DeleteCategoryAsync(5);

        // Assert
        _categoryRepoMock.Verify(r => r.Remove(category), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
