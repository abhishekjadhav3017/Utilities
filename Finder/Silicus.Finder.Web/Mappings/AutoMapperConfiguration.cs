﻿using System.Diagnostics.CodeAnalysis;
using AutoMapper;

namespace Silicus.Finder.Web.Mappings
{
    [ExcludeFromCodeCoverage]
    public class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.Initialize(x =>
                {
                    x.AddProfile<DomainToViewModelMappingProfile>();
                    x.AddProfile<ViewModelToDomainMappingProfile>();
                });
        }
    }
}