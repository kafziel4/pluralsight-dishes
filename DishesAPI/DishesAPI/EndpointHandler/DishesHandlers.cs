using System.Security.Claims;
using AutoMapper;
using DishesAPI.DbContexts;
using DishesAPI.Entities;
using DishesAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DishesAPI.EndpointHandler
{
    public static class DishesHandlers
    {
        public static async Task<Ok<IEnumerable<DishDto>>> GetDishesAsync(
            DishesDbContext dishesDbContext,
            ClaimsPrincipal claimsPrincipal,
            IMapper mapper,
            ILogger<DishDto> logger,
            string? name)
        {
            Console.WriteLine($"User authenticated? {claimsPrincipal.Identity?.IsAuthenticated}");

            logger.LogInformation("Getting the dishes...");

            return TypedResults.Ok(mapper.Map<IEnumerable<DishDto>>(await dishesDbContext.Dishes
                .Where(d => name == null || d.Name.Contains(name))
                .ToListAsync()));
        }

        public static async Task<Results<NotFound, Ok<DishDto>>> GetDishByIdAsync(
            DishesDbContext dishesDbContext, IMapper mapper, Guid dishId)
        {
            var dishEntity = await dishesDbContext.Dishes.FindAsync(dishId);
            if (dishEntity == null)
                return TypedResults.NotFound();

            return TypedResults.Ok(mapper.Map<DishDto>(dishEntity));
        }

        public static async Task<Results<NotFound, Ok<DishDto>>> GetDishByNameAsync(
            DishesDbContext dishesDbContext, IMapper mapper, string dishName)
        {
            var dishEntity = await dishesDbContext.Dishes
                .FirstOrDefaultAsync(d => d.Name == dishName);
            if (dishEntity == null)
                return TypedResults.NotFound();

            return TypedResults.Ok(mapper.Map<DishDto>(dishEntity));
        }

        public static async Task<CreatedAtRoute<DishDto>> CreateDishAsync(
            DishesDbContext dishesDbContext, IMapper mapper, DishForCreationDto dishForCreationDto)
        {
            var dishEntity = mapper.Map<Dish>(dishForCreationDto);
            dishesDbContext.Add(dishEntity);
            await dishesDbContext.SaveChangesAsync();

            var dishToReturn = mapper.Map<DishDto>(dishEntity);

            return TypedResults.CreatedAtRoute(
                dishToReturn,
                "GetDish",
                new { dishId = dishToReturn.Id });
        }

        public static async Task<Results<NotFound, NoContent>> UpdateDishAsync(
            DishesDbContext dishesDbContext, IMapper mapper, Guid dishId, DishForUpdateDto dishForUpdateDto)
        {
            var dishEntity = await dishesDbContext.Dishes.FindAsync(dishId);
            if (dishEntity == null)
                return TypedResults.NotFound();

            mapper.Map(dishForUpdateDto, dishEntity);

            await dishesDbContext.SaveChangesAsync();

            return TypedResults.NoContent();
        }

        public static async Task<Results<NotFound, NoContent>> DeleteDishAsync(
            DishesDbContext dishesDbContext, Guid dishId)
        {
            var dishEntity = await dishesDbContext.Dishes.FindAsync(dishId);
            if (dishEntity == null)
                return TypedResults.NotFound();

            dishesDbContext.Dishes.Remove(dishEntity);
            await dishesDbContext.SaveChangesAsync();
            return TypedResults.NoContent();
        }
    }
}