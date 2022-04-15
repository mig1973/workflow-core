using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using WorkflowCore.Interface;

namespace WorkflowCore.Sample18
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices();

            //start the workflow host
            var host = serviceProvider.GetService<IWorkflowHost>();
            host.RegisterWorkflow<ActivityWorkflow, MyData>();
            host.Start();

            Console.WriteLine("Starting workflow...");

            var workflowId = host.StartWorkflow("activity-sample", new MyData { Request = "Spend $1,000,000" }).Result;

            var approval = host.GetPendingActivity("get-approval", "worker1", TimeSpan.FromMinutes(1)).Result;

            if (approval != null)
            {                
                Console.WriteLine("Approval required for " + approval.Parameters);
                host.SubmitActivitySuccess(approval.Token, "John Smith");
            }

            Console.ReadLine();
            host.Stop();
        }

        private static IServiceProvider ConfigureServices()
        {
            //setup dependency injection
            IServiceCollection services = new ServiceCollection();
            //services.AddWorkflow();
            //services.AddWorkflow(x => x.UseMongoDB(@"mongodb://localhost:27017", "workflow"));

            //Note: for SqlServer the Server=.  does not seem to work when running on localhost,
            //so replace the . with (localdb)\MSSQLLocalDB and it should work
            //services.AddWorkflow(x => x.UseSqlServer(@"Server=.;Database=WorkflowCore;Trusted_Connection=True;", true, true));
            services.AddWorkflow(x => x.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=WorkflowCore2;Trusted_Connection=True;", true, true));
            //services.AddWorkflow(x => x.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=WorkflowCore;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False", true, true));
            //"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=WorkflowCore;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
            
            //services.AddWorkflow(x => x.UsePostgreSQL(@"Server=127.0.0.1;Port=5432;Database=workflow;User Id=postgres;", true, true));
            services.AddLogging(cfg => 
            {
                cfg.AddConsole();
                cfg.AddDebug();
            });

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
