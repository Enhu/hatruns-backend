using AutoMapper;
using HatCommunityWebsite.DB;
using HatCommunityWebsite.Service.Dtos;
using HatCommunityWebsite.Service.Responses;
using HatCommunityWebsite.Service.Responses.Data;

namespace HatCommunityWebsite.Service.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<SubmissionDto, Run>();
            CreateMap<Run, SubmissionDto>();
            CreateMap<SubmissionResponse, Run>();
            CreateMap<UpdateSubmissionDto, Run>();
            CreateMap<UserDataResponse, User>();
            CreateMap<User, UserDataResponse>();
            CreateMap<RunData, Run>();
        }
    }
}