using FPTU_Starter.Application.ViewModel;
using FPTU_Starter.Application.ViewModel.InteractionDTO;
using FPTU_Starter.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTU_Starter.Application.Services.IService
{
    public interface IInteractionService
    {
        Task<List<Like>> GetAll();
        Task<List<Comment>> GetAllComment();
        Task<ResultDTO<LikeResponse>> LikeProject(LikeRequest likeRequest);
        Task<ResultDTO<Comment>> CommentProject(CommentRequest request);
    }
}
