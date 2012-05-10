using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Diagnostics;
using NancySelfHost.Auth;
using TinyIoC;

namespace NancySelfHost
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get
            {
                return new DiagnosticsConfiguration { Password = @"123" };
            }
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            //base.ConfigureApplicationContainer(container);

            IUserDatabase userDatabase = new UserDatabase();
            GuidUserMapper guidUserMapper = new GuidUserMapper(userDatabase);

// ReSharper disable RedundantTypeArgumentsOfMethod
            container.Register<IUserDatabase>(userDatabase);
// ReSharper restore RedundantTypeArgumentsOfMethod
            container.Register(guidUserMapper);
            container.Register<IUserMapper>(guidUserMapper);
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("assets", @"assets"));
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            var formsAuthConfiguration =
                            new FormsAuthenticationConfiguration
                            {
                                RedirectUrl = "~/login",
                                UserMapper = container.Resolve<IUserMapper>(),
                            };

            FormsAuthentication.Enable(pipelines, formsAuthConfiguration);
        }
    }
}
