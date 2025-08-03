using AutoMapper;
using Permissions.Application.DTOs;
using Permissions.Domain.Entities;
using Permissions.Shared.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permissions.Application.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            
            CreateMap<Permission, PermissionDto>();
          
            CreateMap<Permission, PermissionDocument>()
                .ForMember(dest => dest.PermissionId, opt => opt.MapFrom(src => src.Id));

            CreateMap<PermissionDocument, PermissionDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src=> src.PermissionId));

        }
    }
}
