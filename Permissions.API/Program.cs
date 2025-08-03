
using AutoMapper;
using Confluent.Kafka;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Permissions.Application.Elastic;
using Permissions.Application.Kafka;
using Permissions.Application.Mapper;
using Permissions.Application.Permissions.Commands;
using Permissions.Domain.Interfaces;
using Permissions.Infrastructure.Elastic;
using Permissions.Infrastructure.Kafka;
using Permissions.Infrastructure.Persistance;
using Permissions.Infrastructure.Repositories;
using Serilog;

namespace Permissions.API
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                                .ReadFrom.Configuration(builder.Configuration)
                                .CreateLogger();

            builder.Host.UseSerilog();


            builder.Services.AddDbContext<PermissionDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
            builder.Services.AddScoped<IPermissionTypeRepository, PermissionTypeRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IPermissionElasticService, PermissionElasticService>();
            builder.Services.AddScoped<IPermissionKafkaService, PermissionKafkaService>();

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<RequestPermissionCommand>());

            builder.Services.AddSingleton(sp =>
            {
                var elasticUri = builder.Configuration.GetValue<string>("ElasticSearch:Uri");
                var settings = new ElasticsearchClientSettings(new Uri(elasticUri));
                return new ElasticsearchClient(settings);
            });

            builder.Services.AddSingleton<IProducer<Null, string>>(sp => {
                var config = new ProducerConfig
                {
                    BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "kafka:9092"
                };
                return new ProducerBuilder<Null, string>(config).Build();
            });

            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });


            builder.Services.AddControllers();
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
          

            var app = builder.Build();


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            //app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
