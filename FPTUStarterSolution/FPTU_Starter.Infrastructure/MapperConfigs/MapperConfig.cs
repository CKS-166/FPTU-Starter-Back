using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
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
        }

        public void MappingProject()
        {
            CreateMap<Project, ProjectAddRequest>().ReverseMap();
            CreateMap<ProjectPackage, PackageAddRequest>().ReverseMap();
            CreateMap<ProjectPackage, PackageViewResponse>().ReverseMap();
            CreateMap<Project, ProjectViewResponse>()
                .ForMember(des => des.PackageViewResponses, src => src.MapFrom(x => x.Packages))
                
                .ReverseMap();
                

          
        }
    }
}
