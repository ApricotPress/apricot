using Apricot.Build.Tasks;
using Cake.Frosting;

return new CakeHost()
    .UseContext<BuildContext>()
    .Run(args);
