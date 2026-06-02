using AutoMapper;
using UnifyNow.Forest.Data.SQL;
using UnifyNow.Forest.Services;

namespace UnifyNow.Forest
{
    public class AutoMappings : Profile
    {
        public AutoMappings()
        {
            CreateMap<Preset, PresetModel>().ReverseMap();

            //CreateMap<Person, EmployeeImageModel>().ReverseMap();
            //CreateMap<PersonImage, ImageModel>().ReverseMap();
        }

    }
}

