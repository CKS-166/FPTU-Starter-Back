using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FPTU_Starter.Application.ViewModel.CategoryDTO;
using FPTU_Starter.Application.ViewModel.CategoryDTO.SubCategoryDTO;
using FPTU_Starter.Application.ViewModel.ProjectDTO;
using FPTU_Starter.Application.ViewModel.ProjectDTO.ProjectImage;
using FPTU_Starter.Application.ViewModel.ProjectDTO.ProjectPackageDTO;
using FPTU_Starter.Application.ViewModel.ProjectDTO.RewardItemDTO;
using FPTU_Starter.Application.ViewModel.UserDTO;
using FPTU_Starter.Domain.Entity;

namespace FPTU_Starter.Infrastructure.MapperConfigs
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            MappingProject();
            MappingUserProfile();
            MappingCategory();
        }

        public void MappingProject()
        {
            CreateMap<RewardItem, RewardItemAddRequest>().ReverseMap();
            CreateMap<RewardItem,RewardItemViewResponse>().ReverseMap();
            CreateMap<ProjectPackage, PackageAddRequest>().ReverseMap();
            CreateMap<ProjectAddRequest,Project>()
                .ForMember(des => des.Packages , src => src.MapFrom(x => x.Packages)).ReverseMap();
            CreateMap<ProjectPackage, PackageViewResponse>().ReverseMap();
            CreateMap<Project, ProjectViewResponse>()
                .ForMember(des => des.PackageViewResponses, src => src.MapFrom(x => x.Packages))
                .ForMember(des => des.ProjectOwnerName , src => src.MapFrom(x => x.ProjectOwner.AccountName))
                .ForMember(des => des.OwnerId, src => src.MapFrom(x => x.ProjectOwner.Id))
                .ForMember(des => des.CategoryName , src => src.MapFrom(x => x.Category.Name))
                .ForMember(des => des.StoryImages, src => src.MapFrom(x => x.Images))
                .ReverseMap();
            CreateMap<ProjectImage,ProjectImageAddRequest>().ReverseMap();
            CreateMap<ProjectImage,ProjectImageViewResponse>().ReverseMap();
        }

        public void MappingUserProfile()
        {
            CreateMap<ApplicationUser, UserInfoResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.AccountName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserPhone, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.UserBirthDate, opt => opt.MapFrom(src => src.DayOfBirth))
                .ForMember(dest => dest.UserAddress, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.UserGender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.UserAvatarUrl, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.UserBgAvatarUrl, opt => opt.MapFrom(src => src.BackgroundAvatar))
                .ReverseMap();
        }
        
        public void MappingCategory()
        {
            CreateMap<Category,CategoryAddRequest>().ReverseMap().ForMember(des => des.SubCategories 
            ,src => src.MapFrom(x => x.SubCategories));
            CreateMap<Category,CategoryViewResponse>().ReverseMap();
            CreateMap<SubCategory,SubCategoryAddRequest>().ReverseMap();    
            CreateMap<SubCategory,SubCategoryViewResponse>().ReverseMap();
        }
    }
}
