using ECommerceStore.Models;
using ECommerceStore.Repository;

namespace ECommerceStore.Services;

public class ReportService
{
    private readonly ReportRepository _repository;

    public ReportService(ReportRepository repository)
    {
        _repository = repository;
    }

    public Task<List<ProductSalesSummary>> GetProductSalesSummaryAsync()
    {
        return _repository.GetProductSalesSummaryAsync();
    }
}