using LEAPBot.Contracts;
using LEAPBot.Domain.Contracts;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LEAPBot.IoC
{
    public class RuntimeRegistry : Registry
    {
        public RuntimeRegistry()
        {
            Scan(x => {

                x.AssembliesAndExecutablesFromApplicationBaseDirectory();

                x.WithDefaultConventions();
            });

            For<ISettingsReader>().Singleton().Use<SettingsReader>();
        }
    }
}