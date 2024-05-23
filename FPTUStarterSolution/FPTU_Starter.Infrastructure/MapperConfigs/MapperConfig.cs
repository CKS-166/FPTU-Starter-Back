using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FPTU_Starter.Application.ViewModel.CategoryDTO;
using FPTU_Starter.Application.ViewModel.ProjectDTO;
using FPTU_Starter.Application.ViewModel.ProjectDTO.ProjectPackageDTO;
using FPTU_Starter.Domain.Entity;

namespace FPTU_Starter.Infrastructure.MapperConfigs
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            MappingProject();
            MappingCategory();
        }

        public void MappingProject()
        {
            CreateMap<ProjectPackage, PackageAddRequest>().ReverseMap();
            CreateMap<ProjectAddRequest,Project>()
                .ForMember(des => des.Packages , src => src.MapFrom(x => x.Packages)).ReverseMap();
            CreateMap<ProjectPackage, PackageViewResponse>().ReverseMap();
            CreateMap<Project, ProjectViewResponse>()
                .ForMember(des => des.PackageViewResponses, src => src.MapFrom(x => x.Packages))
                .ForMember(des => des.ProjectOwnerName , src => src.MapFrom(x => x.ProjectOwner.AccountName))
                .ForMember(des => des.OwnerId, src => src.MapFrom(x => x.ProjectOwner.Id))
                .ForMember(des => des.CategoryName , src => src.MapFrom(x => x.Category.Name))  
                .ReverseMap();
        }

        public void MappingCategory()
        {
            CreateMap<Category,CategoryAddRequest>().ReverseMap();
            CreateMap<Category,CategoryViewResponse>().ReverseMap();
        }
    }
}
