﻿using FPTU_Starter.Application.IRepository;
using FPTU_Starter.Application.Services.IService;
using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.CategoryDTO.SubCategoryDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.Services
{
    public class SubCategoryManagementService : ISubCategoryManagmentService
    {
        public Task<ResultDTO<string>> CreateSubCates(List<SubCategoryAddRequest> subCategoryAddRequests)
        {
            throw new NotImplementedException();
        }
    }
}
