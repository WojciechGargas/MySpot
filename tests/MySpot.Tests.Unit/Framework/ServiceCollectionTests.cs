using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;
using MySpot.Api.Services;
using Shouldly;

namespace MySpot.Tests.Unit.Framework;

public class ServiceCollectionTests
{
    [Fact]
    public void test()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IMessanger, Messanger>()
        .AddTransient<IMessanger, Messanger2>();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var messenger = serviceProvider.GetServices<IMessanger>();
    }
    
    private interface IMessanger
    {
        void Send();
    }
    
    private class Messanger : IMessanger
    {
        private readonly Guid _id = Guid.NewGuid();
        public void Send()
        {
            Console.WriteLine($"Sending a message...{_id}");
        }
    }
    
    private class Messanger2 : IMessanger
    {
        private readonly Guid _id = Guid.NewGuid();
        public void Send()
        {
            Console.WriteLine($"Sending a message...{_id}");
        }
    }

}