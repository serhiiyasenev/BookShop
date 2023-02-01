﻿using AutoMapper;
using BusinessLayer.Enums;
using BusinessLayer.Models.Outbound;
using DataAccessLayer.DTO;

namespace BusinessLayer.Profiles
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<ProductInbound, ProductDto>()
                .ForMember(dto => dto.Id, opt => opt.Ignore())
                .ForMember(dto => dto.Name, opt => opt.MapFrom(inb => inb.Name))
                .ForMember(dto => dto.Description, opt => opt.MapFrom(inb => inb.Description))
                .ForMember(dto => dto.Author, opt => opt.MapFrom(inb => inb.Author))
                .ForMember(dto => dto.Price, opt => opt.MapFrom(inb => inb.Price))
                .ForMember(dto => dto.ImageUrl, opt => opt.MapFrom(inb => inb.ImageUrl));

            CreateMap<ProductDto, ProductOutbound>()
                .ForMember(outb => outb.Id, opt => opt.MapFrom(dto => dto.Id))
                .ForMember(outb => outb.Name, opt => opt.MapFrom(dto => dto.Name))
                .ForMember(outb => outb.Description, opt => opt.MapFrom(dto => dto.Description))
                .ForMember(outb => outb.Author, opt => opt.MapFrom(dto => dto.Author))
                .ForMember(outb => outb.Price, opt => opt.MapFrom(dto => dto.Price))
                .ForMember(outb => outb.ImageUrl, opt => opt.MapFrom(dto => dto.ImageUrl));

            CreateMap<BookingInbound, BookingDto>()
                .ForMember(dto => dto.Id, opt => opt.Ignore())
                .ForMember(dto => dto.Products, opt => opt.MapFrom(inb => inb.Products))
                .ForMember(dto => dto.DeliveryAddress, opt => opt.MapFrom(inb => inb.DeliveryAddress))
                .ForMember(dto => dto.DeliveryDate, opt => opt.MapFrom(inb => inb.DeliveryDate))
                .ForMember(dto => dto.CreatedDate, opt => opt.MapFrom(inb => inb.CreatedDate))
                .ForMember(dto => dto.Status, opt => opt.MapFrom(inb => (int)inb.Status));

            CreateMap<BookingDto, BookingOutbound>()
                .ForMember(outb => outb.Id, opt => opt.MapFrom(dto => dto.Id))
                .ForMember(outb => outb.Products, opt => opt.MapFrom(dto => dto.Products))
                .ForMember(outb => outb.DeliveryAddress, opt => opt.MapFrom(dto => dto.DeliveryAddress))
                .ForMember(outb => outb.DeliveryDate, opt => opt.MapFrom(dto => dto.DeliveryDate))
                .ForMember(outb => outb.CreatedDate, opt => opt.MapFrom(dto => dto.CreatedDate))
                .ForMember(outb => outb.Status, opt => opt.MapFrom(dto => (BookingStatus)dto.Status));
        }
    }
}