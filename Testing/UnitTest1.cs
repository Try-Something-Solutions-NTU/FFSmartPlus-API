using System.Net;
using Application.Item;
using Application.Unit;
using AutoMapper;
using Domain;
using FFsmartPlus.Controllers;
using FluentValidation.Results;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;


namespace Testing;

public class AdminControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly FridgeAppContext _context;
    private readonly IMapper _mapper;

    public AdminControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _context = new FridgeAppContext();
        _mapper = factory.Services.GetService<IMapper>();
    }


    
    
}