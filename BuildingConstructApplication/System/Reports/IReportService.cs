﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.MaterialStore;
using ViewModels.Pagination;
using ViewModels.Response;

namespace Application.System.Reports
{
    public interface IReportService
    {
        Task<BasePagination<List<ReportProductDTO>>> GetAllReportProduct(PaginationFilter filter, int? storeID);

        Task<BaseResponse<bool>> ReportProduct(ReportRequestDTO report);
    }
}
