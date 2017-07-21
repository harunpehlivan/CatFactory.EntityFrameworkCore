﻿using System;
using System.Collections.Generic;
using CatFactory.CodeFactory;
using CatFactory.DotNetCore;
using CatFactory.OOP;

namespace CatFactory.EfCore
{
    public class DbMapperClassDefinition : CSharpClassDefinition
    {
        public DbMapperClassDefinition(EfCoreProject project)
        {
            Name = project.Database.GetDbEntityMapperName();

            BaseClass = "EntityMapper";

            var lines = new List<ILine>();

            if (project.Settings.UseMefForEntitiesMapping)
            {
                Namespaces.Add("System.Composition.Hosting");
                Namespaces.Add("System.Reflection");

                lines.Add(new CommentLine("// Get current assembly"));
                lines.Add(new CodeLine("var currentAssembly = typeof(StoreDbContext).GetTypeInfo().Assembly;"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine("// Create assemblies array"));
                lines.Add(new CodeLine("var assemblies = new[] {{ currentAssembly }};"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine("// Get configuration for container from current assembly"));
                lines.Add(new CodeLine("var configuration = new ContainerConfiguration().WithAssembly(currentAssembly);"));
                lines.Add(new CodeLine());

                lines.Add(new CommentLine("// Create container for exports"));
                lines.Add(new CodeLine("using (var container = configuration.CreateContainer())"));
                lines.Add(new CodeLine("{{"));
                lines.Add(new CommentLine(1, "// Get all definitions that implement IEntityMap interface"));
                lines.Add(new CodeLine(1, "Mappings = container.GetExports<IEntityMap>();"));
                lines.Add(new CodeLine("}}"));
            }
            else
            {
                Namespaces.Add("System.Collections.Generic");

                lines.Add(new CodeLine("Mappings = new List<IEntityMap>()"));

                lines.Add(new CodeLine("{{"));

                for (var i = 0; i < project.Database.Tables.Count; i++)
                {
                    var item = project.Database.Tables[i];

                    lines.Add(new CodeLine(1, "new {0}(){1}", item.GetMapName(), i == project.Database.Tables.Count - 1 ? String.Empty : ","));
                }

                lines.Add(new CodeLine("}};"));
            }

            Constructors.Add(new ClassConstructorDefinition()
            {
                Lines = lines
            });
        }
    }
}
