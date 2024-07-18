using System;
using GraphQL.Server;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PushMessages.Core;
using VirtoCommerce.PushMessages.Core.BackgroundJobs;
using VirtoCommerce.PushMessages.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;
using VirtoCommerce.PushMessages.Data.BackgroundJobs;
using VirtoCommerce.PushMessages.Data.Extensions;
using VirtoCommerce.PushMessages.Data.Handlers;
using VirtoCommerce.PushMessages.Data.MySql;
using VirtoCommerce.PushMessages.Data.PostgreSql;
using VirtoCommerce.PushMessages.Data.Repositories;
using VirtoCommerce.PushMessages.Data.Services;
using VirtoCommerce.PushMessages.Data.SqlServer;
using VirtoCommerce.PushMessages.ExperienceApi;
using VirtoCommerce.PushMessages.ExperienceApi.Authorization;
using VirtoCommerce.PushMessages.ExperienceApi.Extensions;
using VirtoCommerce.PushMessages.ExperienceApi.Handlers;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Infrastructure;

namespace VirtoCommerce.PushMessages.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<PushMessageOptions>().BindConfiguration("PushMessages").ValidateDataAnnotations();

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

        serviceCollection.AddTransient<IFcmTokenService, FcmTokenService>();
        serviceCollection.AddTransient<IFcmTokenSearchService, FcmTokenSearchService>();
        serviceCollection.AddSingleton<FcmPushMessageRecipientChangedEventHandler>();

        serviceCollection.AddSingleton<MemberChangedEventHandler>();
        serviceCollection.AddSingleton<PushMessageChangedEventHandler>();

        serviceCollection.AddSingleton<IPushMessageJobService, PushMessageJobService>();

        // GraphQL
        var assemblyMarker = typeof(AssemblyMarker);
        var graphQlBuilder = new CustomGraphQLBuilder(serviceCollection);
        graphQlBuilder.AddGraphTypes(assemblyMarker);
        serviceCollection.AddMediatR(assemblyMarker);
        serviceCollection.AddAutoMapper(assemblyMarker);
        serviceCollection.AddSchemaBuilders(assemblyMarker);
        serviceCollection.AddDistributedMessageService(Configuration);
        serviceCollection.AddSingleton<XapiPushMessageRecipientChangedEventHandler>();
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
        permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "Push Messages", ModuleConstants.Security.Permissions.AllPermissions);

        // Apply migrations
        using var serviceScope = serviceProvider.CreateScope();
        using var dbContext = serviceScope.ServiceProvider.GetRequiredService<PushMessagesDbContext>();
        dbContext.Database.Migrate();

        // Register event handlers
        appBuilder.RegisterEventHandler<MemberChangedEvent, MemberChangedEventHandler>();
        appBuilder.RegisterEventHandler<PushMessageChangedEvent, PushMessageChangedEventHandler>();
        appBuilder.RegisterEventHandler<PushMessageRecipientChangedEvent, XapiPushMessageRecipientChangedEventHandler>();

        appBuilder.UseFirebaseCloudMessaging(ModuleInfo.Id);
        appBuilder.UsePushMessageJobs();
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}
