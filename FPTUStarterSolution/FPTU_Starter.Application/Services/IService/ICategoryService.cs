using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.CategoryDTO;
using FPTU_Starter.Application.ViewModel.CategoryDTO.SubCategoryDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.Services.IService
{
    public interface ICategoryService
    {
        Task<ResultDTO<List<CategoryViewResponse>>> ViewAllCates();
        Task<ResultDTO<string>> CreateCate(CategoryAddRequest request);
        Task<ResultDTO<List<SubCategoryViewResponse>>> ViewSubCates(Guid cateId);
    }
}
