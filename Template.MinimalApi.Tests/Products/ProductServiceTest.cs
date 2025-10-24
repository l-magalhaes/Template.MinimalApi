using FluentAssertions;
using Moq;
using Template.MinimalApi.Application.Products;
using Template.MinimalApi.Domain.Abstractions;
using Template.MinimalApi.Domain.Entities;
using Template.MinimalApi.Domain.Repositories;


namespace Template.MinimalApi.Tests.Products
{
    public class ProductServiceTest
    {
        private readonly Mock<IProductRepository> _products = new();
        private readonly Mock<IUnitOfWork> _uow = new();

        private ProductService CreateSut() => new(_products.Object, _uow.Object);

        private static Product MakeProduct(Guid? id = null, string name = "Item X", decimal price = 123.45m)
        {
            var p = new Product(name, price);
            if (id.HasValue)
            {
                p.GetType().GetProperty("Id")?.SetValue(p, id.Value);
            }
            return p;
        }

        [Fact]
        public async Task CreateAsync_DeveCriar_QuandoNomeNaoExiste()
        {
            var sut = CreateSut();
            var dto = new ProductDtos.Create("Produto A", 10m);

            _products.Setup(x => x.NameExistsAsync(dto.Name, null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

            var result = await sut.CreateAsync(dto);

            result.Should().NotBeNull();
            result.Name.Should().Be("Produto A");
            result.Price.Should().Be(10m);

            _products.Verify(x => x.AddAsync(It.Is<Product>(p => p.Name == "Produto A" && p.Price == 10m),
                                             It.IsAny<CancellationToken>()), Times.Once);
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_DeveLancar_QuandoNomeJaExiste()
        {
            var sut = CreateSut();
            var dto = new ProductDtos.Create("Duplicado", 20m);

            _products.Setup(x => x.NameExistsAsync(dto.Name, null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

            var act = async () => await sut.CreateAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Product name 'Duplicado' already exists.");

            _products.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetAsync_DeveRetornarDto_QuandoEncontrado()
        {
            var sut = CreateSut();
            var id = Guid.NewGuid();
            var entity = MakeProduct(id, "P1", 9.9m);

            _products.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(entity);

            var result = await sut.GetAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Name.Should().Be("P1");
            result.Price.Should().Be(9.9m);
        }

        [Fact]
        public async Task GetAsync_DeveRetornarNull_QuandoNaoEncontrado()
        {
            var sut = CreateSut();
            var id = Guid.NewGuid();

            _products.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Product?)null);

            var result = await sut.GetAsync(id);

            result.Should().BeNull();
        }


        [Fact]
        public async Task GetPagedAsync_DeveMapearItensECalcularTotalPages()
        {
            var sut = CreateSut();
            var query = new ProductDtos.Query(2, 3, "abc", "name", false);
            var items = new List<Product>
        {
            MakeProduct(Guid.NewGuid(), "A", 1m),
            MakeProduct(Guid.NewGuid(), "B", 2m),
            MakeProduct(Guid.NewGuid(), "C", 3m),
        };
            const int total = 10; 

            _products.Setup(x => x.GetPagedAsync(query.Page, query.PageSize, query.Search, query.SortBy, query.Desc, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((items, total));

            var result = await sut.GetPagedAsync(query);

            result.Items.Should().HaveCount(3);
            result.Page.Should().Be(2);
            result.PageSize.Should().Be(3);
            result.TotalItems.Should().Be(10);
            result.TotalPages.Should().Be(4);

            result.Items.Select(i => i.Name).Should().BeEquivalentTo(new[] { "A", "B", "C" });
        }

        [Fact]
        public async Task GetPagedAsync_PageSizeZero_DeveTratarComoUmParaTotalPages()
        {
            var sut = CreateSut();
            var q = new ProductDtos.Query(1, 0, null, null, false);
            var items = new List<Product> { MakeProduct(Guid.NewGuid(), "A", 1m) };
            const int total = 10;

            _products.Setup(x => x.GetPagedAsync(q.Page, q.PageSize, q.Search, q.SortBy, q.Desc, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((items, total));

            var result = await sut.GetPagedAsync(q);

            result.TotalPages.Should().Be(10);
            result.PageSize.Should().Be(0); 
            result.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task UpdateAsync_DeveRetornarNull_QuandoNaoEncontrado()
        {
            var sut = CreateSut();
            var id = Guid.NewGuid();
            var dto = new ProductDtos.Update("Novo", 11m);

            _products.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Product?)null);

            var result = await sut.UpdateAsync(id, dto);

            result.Should().BeNull();
            _products.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_DeveLancar_QuandoNomeJaExiste()
        {
            var sut = CreateSut();
            var id = Guid.NewGuid();
            var entity = MakeProduct(id, "Antigo", 5m);
            var dto = new ProductDtos.Update("Duplicado", 99m);

            _products.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(entity);

            _products.Setup(x => x.NameExistsAsync(dto.Name, id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(true);

            var act = async () => await sut.UpdateAsync(id, dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Product name 'Duplicado' already exists.");

            _products.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_DeveAtualizarESalvar_QuandoValido()
        {
            var sut = CreateSut();
            var id = Guid.NewGuid();
            var entity = MakeProduct(id, "Antigo", 5m);
            var dto = new ProductDtos.Update("Novo", 99m);

            _products.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(entity);

            _products.Setup(x => x.NameExistsAsync(dto.Name, id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(false);

            var result = await sut.UpdateAsync(id, dto);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Name.Should().Be("Novo");
            result.Price.Should().Be(99m);

            _products.Verify(x => x.Update(It.Is<Product>(p => p.Name == "Novo" && p.Price == 99m)), Times.Once);
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DeveRetornarFalse_QuandoNaoEncontrado()
        {
            var sut = CreateSut();
            var id = Guid.NewGuid();

            _products.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Product?)null);

            var ok = await sut.DeleteAsync(id);

            ok.Should().BeFalse();
            _products.Verify(x => x.Remove(It.IsAny<Product>()), Times.Never);
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_DeveRemoverESalvar_QuandoEncontrado()
        {
            var sut = CreateSut();
            var id = Guid.NewGuid();
            var entity = MakeProduct(id, "A", 1m);

            _products.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(entity);

            var ok = await sut.DeleteAsync(id);

            ok.Should().BeTrue();
            _products.Verify(x => x.Remove(It.Is<Product>(p => p == entity)), Times.Once);
            _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}