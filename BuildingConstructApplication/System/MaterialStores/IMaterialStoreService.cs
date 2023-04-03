﻿using Data.Entities;
using ViewModels.MaterialStore;
using ViewModels.Pagination;
using ViewModels.Response;

namespace Application.System.MaterialStores
{
    public interface IMaterialStoreService
    {
        Task<BasePagination<List<MaterialStoreDTO>>> GetList(PaginationFilter filter);

        Task<BasePagination<List<MaterialStoreDTO>>> Search(PaginationFilter filter, string keyword);
        Task<bool> CreateProduct(ProductDTO request);
        Task<BasePagination<List<ProductStoreDTO>>> GetAllProductStore(PaginationFilter filter,bool isAll,int? storeID);
        Task<BaseResponse<ProductDetailDTO>> GetProductDetail(int productId);
        Task<bool> DeleteProduct(int productID);
        Task<BaseResponse<ProductStoreDTO>> UpdateProduct(UpdateProductDTO request);
        Task<BaseResponse<List<MaterialStoreStatisticDTO>>> GetBillStatistic();
    }
}
