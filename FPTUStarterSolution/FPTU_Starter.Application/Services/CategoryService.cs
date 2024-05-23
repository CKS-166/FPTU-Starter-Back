using AutoMapper;
using FPTU_Starter.Application.IRepository;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.CategoryDTO;
using FPTU_Starter.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ResultDTO<string>> CreateCate(CategoryAddRequest request)
        {
            Category cate = _mapper.Map<Category>(request);
            await _unitOfWork.CategoryRepository.AddAsync(cate);
            await _unitOfWork.CommitAsync();
            return ResultDTO<string>.Success("", "Add Sucessfully");
        }

        public async Task<ResultDTO<List<CategoryViewResponse>>> ViewAllCates()
        {
            IEnumerable<Category> cates = _unitOfWork.CategoryRepository.GetAll();
            IEnumerable<CategoryViewResponse> categoryViewResponses = _mapper.Map<IEnumerable<CategoryViewResponse>>(cates);
            return ResultDTO<List<CategoryViewResponse>>.Success(categoryViewResponses.ToList(),"");
        }
    }
}
