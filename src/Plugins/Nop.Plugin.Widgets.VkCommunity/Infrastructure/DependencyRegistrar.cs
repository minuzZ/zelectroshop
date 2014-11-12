using Autofac;
using Autofac.Core;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Widgets.VkCommunity.Controllers;

namespace Nop.Plugin.Widgets.VkCommunity.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            //we cache presentation models between requests
            builder.RegisterType<WidgetsVkCommunityController>()
                .WithParameter(ResolvedParameter.ForNamed<ICacheManager>("nop_cache_static"));
        }

        public int Order
        {
            get { return 2; }
        }
    }
}
