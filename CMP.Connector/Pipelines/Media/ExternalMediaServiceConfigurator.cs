using Microsoft.Extensions.DependencyInjection;
using Sitecore.Abstractions;
using Sitecore.DependencyInjection;
using Sitecore.Resources.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMP.Connector.Pipelines.Media
{
    public class ExternalMediaServiceConfigurator : IServicesConfigurator
    {
        //Register External Media manager
        public void Configure(IServiceCollection serviceCollection)
        {
            // this is replacing Sitecore's default binding of BaseMediaManager to ExternalMediaManager
            serviceCollection.AddSingleton<BaseMediaManager>(s =>
                new ExternalMediaManager(
                    new DefaultMediaManager(
                        s.GetService<BaseFactory>(),
                        s.GetService<MediaProvider>()
                    )));
        }
    }
}