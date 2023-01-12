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



    [Fact]
    public async Task Test_PutItem_ReturnsBadRequest_IfItemIsInvalid()
    {
        // Arrange
        var controller = new ItemController(_context, _mapper);

        var putItem = new ItemDto { Id = 2, Name = "" };

        // Act
        var result = await controller.PutItem(2, putItem);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestResult>(result);
        
    }
    
}