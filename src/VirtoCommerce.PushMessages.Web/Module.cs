using System;
using GraphQL.Server;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.ExperienceApiModule.Core.Extensions;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;
using VirtoCommerce.Platform.Core.Bus;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PushMessages.Core;
using VirtoCommerce.PushMessages.Core.Events;
using VirtoCommerce.PushMessages.Core.Services;
using VirtoCommerce.PushMessages.Data.MySql;
using VirtoCommerce.PushMessages.Data.PostgreSql;
using VirtoCommerce.PushMessages.Data.Repositories;
using VirtoCommerce.PushMessages.Data.Services;
using VirtoCommerce.PushMessages.Data.SqlServer;
using VirtoCommerce.PushMessages.ExperienceApi;
using VirtoCommerce.PushMessages.ExperienceApi.Authorization;
using VirtoCommerce.PushMessages.ExperienceApi.Extensions;
using VirtoCommerce.PushMessages.ExperienceApi.Handlers;

namespace VirtoCommerce.PushMessages.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContext<PushMessagesDbContext>(options =>
        {
            var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
            var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

            switch (databaseProvider)
            {
                case "MySql":
                    options.UseMySqlDatabase(connectionString);
                    break;
                case "PostgreSql":
                    options.UsePostgreSqlDatabase(connectionString);
                    break;
                default:
                    options.UseSqlServerDatabase(connectionString);
                    break;
            }
        });

        serviceCollection.AddTransient<IPushMessagesRepository, PushMessagesRepository>();
        serviceCollection.AddTransient<Func<IPushMessagesRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<IPushMessagesRepository>());

        serviceCollection.AddTransient<IPushMessageService, PushMessageService>();
        serviceCollection.AddTransient<IPushMessageSearchService, PushMessageSearchService>();

        serviceCollection.AddTransient<IPushMessageRecipientService, PushMessageRecipientService>();
        serviceCollection.AddTransient<IPushMessageRecipientSearchService, PushMessageRecipientSearchService>();

        // GraphQL
        var assemblyMarker = typeof(AssemblyMarker);
        var graphQlBuilder = new CustomGraphQLBuilder(serviceCollection);
        graphQlBuilder.AddGraphTypes(assemblyMarker);
        serviceCollection.AddMediatR(assemblyMarker);
        serviceCollection.AddAutoMapper(assemblyMarker);
        serviceCollection.AddSchemaBuilders(assemblyMarker);
        serviceCollection.AddDistributedMessageService(Configuration);
        serviceCollection.AddTransient<PushMessageSendingEventHandler>();
        serviceCollection.AddSingleton<IAuthorizationHandler, PushMessagesAuthorizationHandler>();
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        var serviceProvider = appBuilder.ApplicationServices;

        // Register settings
        var settingsRegistrar = serviceProvider.GetRequiredService<ISettingsRegistrar>();
        settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

        // Register permissions
        var permissionsRegistrar = serviceProvider.GetRequiredService<IPermissionsRegistrar>();
        permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "PushMessages", ModuleConstants.Security.Permissions.AllPermissions);

        // Apply migrations
        using var serviceScope = serviceProvider.CreateScope();
        using var dbContext = serviceScope.ServiceProvider.GetRequiredService<PushMessagesDbContext>();
        dbContext.Database.Migrate();

        var handlerRegistrar = appBuilder.ApplicationServices.GetService<IHandlerRegistrar>();
        handlerRegistrar.RegisterHandler<PushMessageSendingEvent>((message, _) => appBuilder.ApplicationServices.GetService<PushMessageSendingEventHandler>().Handle(message));
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}
