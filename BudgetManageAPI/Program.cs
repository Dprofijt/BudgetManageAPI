
using BudgetManageAPI.Data;
using BudgetManageAPI.Repositories;
using BudgetManageAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace BudgetManageAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton(typeof(IRepository<>), typeof(CashFlowRepository<>));

            builder.Services.AddScoped<MLService>();

            builder.Services.AddDbContext<DBContextBudgetManageAPI>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("BudgetMoneyDB")));



            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
